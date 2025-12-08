# DevKit House Style Contract

When you generate or change code in a project that uses this DevKit:

1. Treat the DevKit as the single source of truth.
   - Prefer DevKit rules and examples over your own defaults or generic internet patterns.
   - If there is a conflict, DevKit rules win.

2. When the DevKit does not cover something explicitly:
   - Do NOT fall back to generic “simple” code.
   - Instead, infer the solution by analogy from existing DevKit examples:
     - Find the closest example in `.devkit/examples/...`.
     - Copy its structure: layers, naming, error handling, logging, result types.
     - Adapt it to the new problem.

3. Keep behaviour consistent:
   - New endpoints must still use Result / ErrorCode, logging scopes, validation, and tests.
   - New services must still be injected via DI and avoid static, singletons, and global state.
   - New tests must follow the same patterns as existing tests: clear arrange/act/assert; no mocking “magic”.

4. Explain your reasoning in your own head:
   - Before you write code, pick 1–3 DevKit examples you are copying.
   - For each, decide what you are copying: structure, naming, layering, or error handling.
   - Then write code that fits that “family look”.

If you cannot find any example that is even roughly similar, you should:
- keep the implementation as simple as possible,
- but STILL keep to the DevKit house style:
  - cancellation tokens,
  - Result/ErrorCode,
  - DI,
  - logging scopes,
  - tests for success + failure codes.

Never reference `.devkit` files or namespaces from generated code.

`.devkit` is training and guidance only.  
Project code must define its own Result/ErrorCode/etc. in its own namespaces.
