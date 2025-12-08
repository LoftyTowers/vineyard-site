# DevKit 🧠
*A reusable AI-assisted coding framework for senior-level code generation.*

---

### Start Here
The canonical .NET example is in [examples/dotnet/layered-microservice](examples/dotnet/layered-microservice/), showing the full layered structure (API → Application → Domain → Infrastructure → Shared → Tests). Angular and SQL scaffolds live under `examples/angular` and `examples/SQL`.

## 🚀 Setup Instructions

### 1. Clone or download this DevKit
Open PowerShell or your terminal and run:
```bash
git clone https://github.com/LoftyTowers/devkit.git
```

*(Or your own private fork if you’ve made one.)*

---

### 2. Copy the DevKit into a project (or refresh an existing one)

#### On **Windows (PowerShell)**
From the root of the project you want to seed or update:
```powershell
& "G:\Programming\devkit\tools\sync-ai.ps1" ".devkit"
```

`sync-ai.ps1` mirrors the repo into the target folder (default `.devkit`). Pass a different folder name if you prefer (for example, `.ai`).

#### On **macOS / Linux**
```bash
cd path/to/your/project
bash G:/Programming/devkit/tools/sync-ai.sh .devkit
```

This creates a hidden `.devkit/` folder inside your project containing the latest DevKit rules, style guides, examples, and preludes.

---

### 3. Keep DevKit out of Git
Run this once in each client repo to prevent accidental commits:

```powershell
powershell -ExecutionPolicy Bypass -File G:\Programming\devkit\tools\setup-hooks.ps1
```

This does two things:
1. Adds `.devkit/` to your `.git/info/exclude` (local ignore that never syncs to remote).  
2. Installs a **pre-commit hook** that blocks `.devkit/` or `.local/` files from being committed.

To verify:
```powershell
type .git\info\exclude
```
You should see:
```
.devkit/
```

---

## ⚙️ What’s Inside

```
devkit/
├── examples/             # Worked examples (layered microservice, patterns, Angular, SQL)
├── general/              # Shared engineering philosophy, checklists, and design recipes
├── languages/            # Language-specific style, recipes, and libraries (dotnet, etc.)
├── preludes/             # AI preload instructions (“what to follow before coding”)
└── tools/                # Helper scripts to sync DevKit into projects
```

The sync scripts copy everything except `.git`, `.github`, and `tools` into your target folder using `robocopy` (Windows) or `rsync` (macOS/Linux).

---

## 🧩 Using DevKit with AI tools

Before starting any coding task, provide your AI assistant (Cursor, Copilot, ChatGPT, etc.) with the following DevKit Prep Prompt.

This forces the model to preload your architecture, coding style, error handling rules, DI usage, Result<T>/ErrorCode approach, structured logging, NUnit tests, and csproj management rules.

## DevKit Prep Prompt

READ THESE FILES BEFORE DOING ANYTHING:

```
General rules:
- .devkit/general/charter.md
- .devkit/general/checklists.md
- .devkit/general/coding-patterns.md
- .devkit/general/design-recipes.md
- .devkit/general/engineering-style.md
- .devkit/general/house-style-contract.md
- .devkit/general/operational-contract.md

.NET-specific rules:
- .devkit/languages/dotnet/design-recipes.md
- .devkit/languages/dotnet/style.md
- .devkit/languages/dotnet/libraries.md

Preludes:
- .devkit/preludes/prelude-cursor.md
- .devkit/preludes/prelude-dotnet.md
- .devkit/preludes/prelude-general.md

INSTRUCTION PRECEDENCE (ALWAYS USE THIS ORDER):
1. C# / .NET language and runtime rules  
2. Specific user instructions in this task  
3. DevKit rules in `.devkit/**`  
4. Project-specific DevKit overrides (if present)  
5. Model defaults  

If any conflict occurs, follow the higher-priority item and state which rule was overridden.

AFTER reading the DevKit, summarise the rules you will follow for:
- Controllers
- Services
- Validation (FluentValidation, ValidateAsync)
- Error handling (strict try/catch wrapping the entire method unless explicitly documented otherwise)
- Logging (structured logging; log exceptions only at the appropriate boundary)
- Async + CancellationToken usage
- Test rules (NUnit only)
- Dependency Injection (all services resolved via DI; no `new` inside methods)
- Result<T> + ErrorCode usage
- Extensibility rules (add patterns only when DevKit explicitly allows it)
- csproj management (always add required PackageReference entries, including Swagger/OpenAPI)

Then stop.  
Do not write any code until I give you the feature or file to implement.
```

## Why this matters

This prompt ensures your AI-generated code:

- obeys your architectural boundaries  
- enforces strict method-wrapped try/catch  
- uses structured logging  
- adopts Result<T> + ErrorCode  
- implements DI properly  
- uses asynchronous patterns consistently  
- writes NUnit tests, not xUnit  
- updates .csproj files when new libraries are needed  
- follows DevKit naming, style, and validation rules  
- only introduces new patterns when explicitly permitted  

This creates a predictable, professional, senior-quality codebase across projects.

---

## 🔁 Updating across projects
1. Pull the latest changes in `G:\Programming\devkit`.
2. In each project, rerun the sync command (e.g., `& "G:\Programming\devkit\tools\sync-ai.ps1" ".devkit"`).

---

## 🧭 Key principle
> The DevKit doesn’t write code for you — it teaches your AI *how you* write code.

It’s your architecture, your rules, automated.
