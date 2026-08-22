using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace BlockEngine
{
    // A deliberately local-first project/package layer shared by Standard and Plus.
    // Packages are ordinary folders with a block.package.json manifest. No code is
    // downloaded or executed by the package commands themselves.
    public class BlockProjectManifest
    {
        public string name { get; set; }
        public string version { get; set; }
        public string engine { get; set; }
        public string entry { get; set; }
        public Dictionary<string, string> dependencies { get; set; }

        public BlockProjectManifest()
        {
            name = "my-block-project";
            version = "0.1.0";
            engine = "standard";
            entry = "main.blk";
            dependencies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public class BlockPackageManifest
    {
        public string name { get; set; }
        public string version { get; set; }
        public string main { get; set; }
        public string description { get; set; }

        public BlockPackageManifest()
        {
            name = "block-package";
            version = "0.1.0";
            main = "main.blk";
            description = "";
        }
    }

    public static class Ecosystem
    {
        public const string ProjectManifestName = "block.project.json";
        public const string PackageManifestName = "block.package.json";
        private static readonly JavaScriptSerializer Serializer =
            new JavaScriptSerializer { MaxJsonLength = (int)SecurityLimits.MaxJsonBytes };

        public static string FindProjectRoot(string startPath)
        {
            string candidate = string.IsNullOrEmpty(startPath) ? Environment.CurrentDirectory : startPath;
            candidate = Path.GetFullPath(candidate);
            if (File.Exists(candidate) || (!Directory.Exists(candidate) && Path.HasExtension(candidate)))
                candidate = Path.GetDirectoryName(candidate);

            DirectoryInfo current = new DirectoryInfo(candidate);
            while (current != null)
            {
                if (File.Exists(Path.Combine(current.FullName, ProjectManifestName)))
                    return current.FullName;
                current = current.Parent;
            }
            return Path.GetFullPath(candidate);
        }

        public static BlockProjectManifest LoadProject(string projectRoot)
        {
            string path = Path.Combine(Path.GetFullPath(projectRoot), ProjectManifestName);
            if (!File.Exists(path))
                throw new FileNotFoundException("No block.project.json found in project directory.", path);

            BlockProjectManifest manifest = Deserialize< BlockProjectManifest >(path);
            if (manifest == null) manifest = new BlockProjectManifest();
            if (string.IsNullOrWhiteSpace(manifest.name)) manifest.name = "my-block-project";
            if (string.IsNullOrWhiteSpace(manifest.version)) manifest.version = "0.1.0";
            if (string.IsNullOrWhiteSpace(manifest.engine)) manifest.engine = "standard";
            if (string.IsNullOrWhiteSpace(manifest.entry)) manifest.entry = "main.blk";
            if (manifest.dependencies == null)
                manifest.dependencies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return manifest;
        }

        public static BlockPackageManifest LoadPackage(string packageRoot)
        {
            string path = Path.Combine(Path.GetFullPath(packageRoot), PackageManifestName);
            if (!File.Exists(path)) return null;

            BlockPackageManifest manifest = Deserialize< BlockPackageManifest >(path);
            if (manifest == null) manifest = new BlockPackageManifest();
            if (string.IsNullOrWhiteSpace(manifest.name)) manifest.name = new DirectoryInfo(packageRoot).Name;
            if (string.IsNullOrWhiteSpace(manifest.version)) manifest.version = "0.1.0";
            if (string.IsNullOrWhiteSpace(manifest.main)) manifest.main = "main.blk";
            return manifest;
        }

        public static string ResolvePackageEntry(string currentScriptPath, string packageName,
            string requestedEntry, EngineConfig cfg)
        {
            ValidatePackageName(packageName);
            string projectRoot = FindProjectRoot(Path.GetDirectoryName(Path.GetFullPath(currentScriptPath)));
            BlockProjectManifest project = LoadProject(projectRoot);

            string relativePackage = null;
            if (project.dependencies != null)
                project.dependencies.TryGetValue(packageName, out relativePackage);
            if (string.IsNullOrWhiteSpace(relativePackage))
                relativePackage = Path.Combine("packages", packageName);

            string packageRoot = Path.GetFullPath(Path.Combine(projectRoot, relativePackage));
            if (!IsPathInSandbox(packageRoot, projectRoot))
                throw new UnauthorizedAccessException("Package path escapes the Block project root.");

            BlockPackageManifest package = LoadPackage(packageRoot);
            string entry = requestedEntry;
            if (string.IsNullOrWhiteSpace(entry)) entry = package != null ? package.main : "main.blk";
            if (string.IsNullOrWhiteSpace(entry)) entry = "main.blk";

            string fullEntry = Path.GetFullPath(Path.Combine(packageRoot, entry));
            if (!IsPathInSandbox(fullEntry, packageRoot))
                throw new UnauthorizedAccessException("Package entry escapes the package directory.");
            if (cfg != null && !IsPathInSandbox(fullEntry, cfg.SandboxDir))
                throw new UnauthorizedAccessException("Package entry escapes the configured sandbox directory.");
            if (!File.Exists(fullEntry))
                throw new FileNotFoundException("Package entry not found: " + fullEntry, fullEntry);
            return fullEntry;
        }

        public static void RunCli(string[] args)
        {
            string command = args.Length > 1 ? (args[1] ?? "").ToLowerInvariant() : "help";
            try
            {
                if (command == "init")
                {
                    string directory = args.Length > 2 ? args[2] : Environment.CurrentDirectory;
                    string name = args.Length > 3 ? args[3] : null;
                    InitProject(directory, name);
                }
                else if (command == "list")
                {
                    string directory = args.Length > 2 ? args[2] : Environment.CurrentDirectory;
                    ListProject(directory);
                }
                else if (command == "add")
                {
                    if (args.Length < 3) throw new ArgumentException("Usage: block ecosystem add <package-directory> [project-directory]");
                    string projectDirectory = args.Length > 3 ? args[3] : Environment.CurrentDirectory;
                    AddPackage(projectDirectory, args[2]);
                }
                else
                {
                    PrintHelp();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[Block Ecosystem] Error: " + ex.Message);
                Environment.ExitCode = 1;
            }
        }

        public static void InitProject(string directory, string projectName)
        {
            string root = Path.GetFullPath(directory ?? Environment.CurrentDirectory);
            Directory.CreateDirectory(root);
            string manifestPath = Path.Combine(root, ProjectManifestName);
            if (File.Exists(manifestPath))
            {
                Console.WriteLine("[Block Ecosystem] Project already exists: " + manifestPath);
                return;
            }

            BlockProjectManifest manifest = new BlockProjectManifest();
            if (!string.IsNullOrWhiteSpace(projectName)) manifest.name = projectName;
            else manifest.name = new DirectoryInfo(root).Name;
            WriteJson(manifestPath, manifest);
            Directory.CreateDirectory(Path.Combine(root, "packages"));

            string entryPath = Path.Combine(root, manifest.entry);
            if (!File.Exists(entryPath))
            {
                File.WriteAllText(entryPath,
                    "# Block project entry\n<py>\nprint(\"Hello from " + manifest.name + "\")\n</py>\n",
                    new UTF8Encoding(false));
            }
            Console.WriteLine("[Block Ecosystem] Initialized project: " + root);
            Console.WriteLine("[Block Ecosystem] Entry: " + manifest.entry);
        }

        public static void ListProject(string directory)
        {
            string root = FindProjectRoot(directory);
            BlockProjectManifest manifest = LoadProject(root);
            Console.WriteLine("[Block Ecosystem] " + manifest.name + " v" + manifest.version);
            Console.WriteLine("  Root: " + root);
            Console.WriteLine("  Engine: " + manifest.engine);
            Console.WriteLine("  Entry: " + manifest.entry);
            Console.WriteLine("  Dependencies:");
            if (manifest.dependencies == null || manifest.dependencies.Count == 0)
            {
                Console.WriteLine("    (none)");
                return;
            }
            foreach (KeyValuePair<string, string> dependency in manifest.dependencies)
                Console.WriteLine("    - " + dependency.Key + " -> " + dependency.Value);
        }

        public static void AddPackage(string projectDirectory, string packageDirectory)
        {
            string projectRoot = FindProjectRoot(projectDirectory);
            BlockProjectManifest project = LoadProject(projectRoot);
            string sourceRoot = Path.GetFullPath(packageDirectory);
            if (!Directory.Exists(sourceRoot))
                throw new DirectoryNotFoundException("Package directory not found: " + sourceRoot);
            if (ContainsReparsePoint(sourceRoot))
                throw new UnauthorizedAccessException("Package directory contains a reparse point and cannot be installed.");

            BlockPackageManifest package = LoadPackage(sourceRoot);
            string packageName = package != null && !string.IsNullOrWhiteSpace(package.name)
                ? package.name : new DirectoryInfo(sourceRoot).Name;
            ValidatePackageName(packageName);

            string packagesRoot = Path.Combine(projectRoot, "packages");
            string destination = Path.Combine(packagesRoot, packageName);
            if (Directory.Exists(destination) || File.Exists(destination))
                throw new IOException("Package is already installed: " + packageName);
            if (IsPathInSandbox(destination, sourceRoot))
                throw new UnauthorizedAccessException("Package destination cannot be inside its source directory.");
            Directory.CreateDirectory(packagesRoot);
            CopyDirectory(sourceRoot, destination);

            if (project.dependencies == null)
                project.dependencies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            project.dependencies[packageName] = Path.Combine("packages", packageName);
            WriteJson(Path.Combine(projectRoot, ProjectManifestName), project);
            Console.WriteLine("[Block Ecosystem] Added package " + packageName + " to " + project.name);
        }

        public static bool IsPathInSandbox(string fullPath, string sandboxRoot)
        {
            try
            {
                string root = Path.GetFullPath(sandboxRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string path = Path.GetFullPath(fullPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return path.Equals(root, StringComparison.OrdinalIgnoreCase)
                    || path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                    || path.StartsWith(root + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        private static bool ContainsReparsePoint(string path)
        {
            string current = Path.GetFullPath(path);
            while (!string.IsNullOrEmpty(current))
            {
                try
                {
                    if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) return true;
                }
                catch (FileNotFoundException) { }
                catch (DirectoryNotFoundException) { }
                string parent = Path.GetDirectoryName(current);
                if (string.IsNullOrEmpty(parent) || parent.Equals(current, StringComparison.OrdinalIgnoreCase)) break;
                current = parent;
            }
            return false;
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (string file in Directory.GetFiles(source))
            {
                if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) != 0)
                    throw new UnauthorizedAccessException("Package contains a reparse-point file: " + file);
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
            }
            foreach (string directory in Directory.GetDirectories(source))
            {
                if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
                    throw new UnauthorizedAccessException("Package contains a reparse-point directory: " + directory);
                CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
            }
        }

        private static void ValidatePackageName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 64)
                throw new ArgumentException("Package name must be 1-64 characters.");
            if (name == "." || name == ".." || name.EndsWith(".", StringComparison.Ordinal) || name.EndsWith(" ", StringComparison.Ordinal))
                throw new ArgumentException("Package name cannot be a relative path or end with a dot/space.");

            string deviceName = name.Split('.')[0];
            string[] reservedNames = { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
            foreach (string reserved in reservedNames)
            {
                if (string.Equals(deviceName, reserved, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Package name is a reserved Windows device name: " + name);
            }

            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (!(char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.'))
                    throw new ArgumentException("Package name contains an invalid character: " + name);
            }
        }

        private static T Deserialize<T>(string path)
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            return Serializer.Deserialize<T>(json);
        }

        private static void WriteJson(string path, object value)
        {
            string temp = path + ".tmp";
            File.WriteAllText(temp, Serializer.Serialize(value), new UTF8Encoding(false));
            if (!File.Exists(path))
            {
                File.Move(temp, path);
                return;
            }

            try
            {
                File.Replace(temp, path, null);
            }
            catch (Exception ex)
            {
                if (!(ex is PlatformNotSupportedException) && !(ex is IOException) && !(ex is UnauthorizedAccessException))
                    throw;
                // Keep the old manifest in place until the replacement contents
                // have been completely copied; never delete it first.
                File.Copy(temp, path, true);
                File.Delete(temp);
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Block Ecosystem commands:");
            Console.WriteLine("  block ecosystem init [directory] [name]");
            Console.WriteLine("  block ecosystem list [directory]");
            Console.WriteLine("  block ecosystem add <package-directory> [project-directory]");
            Console.WriteLine("  <use package=\"name\" /> loads packages/name/main.blk");
        }
    }
}
