# 🔷 Block Engine v2.2.0 — Official README

> **One File. Every Runtime.**  
> Block is a lightweight, high-performance **Polyglot Orchestration Engine** built on C#. It allows developers to compose, mix, and chain Python, JavaScript (Node.js), Lua, PHP, PowerShell, SQLite, and compiled runtimes inside a single plain text file—while automatically managing variable state propagation across runtimes.

---

## 🌟 Why Block?

In traditional software architecture, using Python for data analysis, Node.js for web tools, and PowerShell for system automation requires splitting logic across separate scripts or microservices. Maintaining inter-process communication (IPC) through temporary files or REST APIs adds unnecessary overhead.

**Block solves this cleanly:**
- 🧩 **Single-File Simplicity**: Organize multi-language workflows using clear `<py>`, `<js>`, `<lua>`, `<php>`, `<ps>`, and `<sql>` tags inside a readable file.
- 🔗 **Automatic State Propagation**: Variables defined in one stage (e.g. Python arrays) are automatically passed forward to subsequent stages (e.g. Node.js or Lua).
- ⚡ **Native Execution**: Block invokes real system interpreters directly, keeping full compatibility with native package managers (`pip`, `npm`, `composer`, `luarocks`).

---

## 📦 Edition Comparison Matrix

| Feature / Edition | 🍃 **Block Lite** (`.blkl`) | ⚡ **Block Standard** (`.blk`) | 🚀 **Block+ (Flagship)** (`.blkp`) |
|---|---|---|---|
| **Target Use Cases** | Lightweight scripting & Web tools | Everyday polyglot automation | Enterprise-grade orchestration & compiled runtimes |
| **Supported Languages** | Python, JS, HTML | Python, JS, Lua, PHP, PS, SQL, HTML | Full matrix (incl. C, C++, Go, Rust, Java, etc.) |
| **State Propagation** | Basic variables | Full dynamic JSON state pipeline | Advanced types & object state serialization |
| **Auto-Package Setup** | — | — | Built-in Winget runtime auto-installation |
| **Executable Name** | `block-lite.exe` | `block.exe` | `block-plus.exe` |

---

## 💻 Quick Start Example

Create a file named `demo.blkp`:

```html
# 1. Initialize data in Python
<py>
pipeline_name = "Block Engine v2.2.0 Demo"
raw_data = [10, 20, 30, 40, 50]
python_sum = sum(raw_data)
status = "HEALTHY"
print(f"[Python] Processed: sum = {python_sum}")
</py>

# 2. Transform array in JavaScript (Node.js)
<js>
var transformed = raw_data.map(x => x * 2);
console.log("[Node.js] Doubled Array:", transformed.join(", "));
</js>

# 3. Compute Fibonacci sequence in Lua
<lua>
function fib(n)
    if n <= 1 then return n end
    return fib(n-1) + fib(n-2)
end
lua_fib = fib(10)
print("[Lua 5.4] Fibonacci Fib(10) =", lua_fib)
</lua>

# 4. Generate SHA-256 security signature in PHP
<php>
$signature = hash('sha256', $pipeline_name);
echo "[PHP 8] SHA-256 Signature: " . substr($signature, 0, 16) . "...\n";
</php>

# 5. Summarize multi-language pipeline in Python
<py>
print("\n" + "=" * 50)
print(f"🎉 Pipeline Complete: {pipeline_name}")
print(f"   • Python Sum  : {python_sum}")
print(f"   • Lua Math    : Fib(10) = {lua_fib}")
print(f"   • PHP Hash    : {signature[:16]}...")
print("=" * 50)
</py>
```

### 🎯 Execution Command:
```cmd
block-plus demo.blkp
```

---

## 🛠️ CLI Command Reference

```bash
# 1. Print Engine Version
block-plus --version

# 2. Execute a Block Script
block-plus path/to/script.blkp

# 3. Launch Background REST API Server
block-plus serve --port 8080

# 4. Open Interactive Security Configuration CLI
block-plus --settings
```

---

## 💾 Installers & Portable Packages

To suit various deployment requirements, Block provides both an automated installer and zero-installation portable packages:

- 🖥️ **Universal All-in-One GUI Installer (`BlockSetup.exe`)**:
  Features a sleek dark-themed GUI that lets users select between **Block Lite**, **Block Standard**, or **Block+ Flagship Edition**. Automatically configures Windows system `%PATH%` environment variables, maps `.blkl` / `.blk` / `.blkp` file extensions, and provides one-click Winget runtime pre-installation.
- 📦 **Independent Portable Packages (Portable ZIPs)**:
  - `block-lite.zip` (Contains `block-lite.exe`)
  - `block.zip` (Contains `block.exe`)
  - `block-plus.zip` (Contains `block-plus.exe`)

---

## 🛡️ Enterprise Security & Hardening

Block Engine v2.2.0 incorporates comprehensive static and dynamic security controls:
1. 🔒 **Strict Loopback Binding**: API Server and HTTP listeners bind exclusively to `127.0.0.1` / `localhost` to prevent unauthorized remote network access.
2. 🔑 **Mandatory X-Api-Token Header**: All REST endpoints enforce token validation and return `403 Forbidden` on missing or invalid tokens.
3. 📂 **Path Canonicalization & Junction Protection**: `Path.GetFullPath` resolves Reparse Points and symbolic links to prevent sandbox boundary escapes.
4. 🛡️ **Subprocess Output Buffer Limits**: Caps stdout/stderr process output at 10MB to mitigate Out-Of-Memory (OOM) Denial of Service attacks.
5. 📱 **Acode DOM Sandbox Isolation**: Live preview embeds `sandbox="allow-scripts allow-modals"` to isolate parent origin permissions and `localStorage`.

---

## 🔌 Ecosystem & Tools

- 🧩 **VS Code Extension** (`block-language-2.2.0.vsix`): Polyglot syntax highlighting, code snippets, and live HTML preview.
- 📱 **Acode Mobile Plugin** (`acode-plugin-block.zip`): Full Block language support on Android devices.
- 🖥️ **Windows One-Click Installer** (`BlockSetup.exe`): Automated system PATH configuration and file extension association.

---

## 🌐 Official Resources

- 🔗 **Official Website**: [https://block-io.blockengine.workers.dev/](https://block-io.blockengine.workers.dev/)
- 📄 **Documentation & Wiki**: [https://block-io.blockengine.workers.dev/wiki.html](https://block-io.blockengine.workers.dev/wiki.html)

---
*© 2026 Block Engine Team. Licensed under the MIT License.*
