# Skeleton backend

.NET 10 (LTS) modular monolith. Minimal APIs, no mediator. No database or auth wired yet. Add them deliberately.

## Commands
- Build: `dotnet build -c Release` (warnings are errors)
- Test: `dotnet test --solution Skeleton.slnx` (Microsoft.Testing.Platform, opted in via `global.json`)
- Coverage: append `--coverlet --coverlet-output-format cobertura --results-directory TestResults`
- Format: `dotnet format` (CI runs `dotnet format --verify-no-changes`)
- Run: `dotnet run --project src/Skeleton.Host` → http://localhost:5080/scalar (Development only)
- Container: `dotnet publish src/Skeleton.Host -c Release -t:PublishContainer`

## Layout
- `src/Skeleton.Host`: composition root only (`Program.cs`, observability, CORS and rate limiting). No business logic.
- `src/Skeleton.SharedKernel`: `IModule`, `Result<T>`/`Error`, `Error.ToProblem()`. Must not depend on Host or modules.
- `src/Modules/<Name>/Skeleton.Modules.<Name>`: module implementation. `<Name>Module : IModule` is the only entry point.
- `src/Modules/<Name>/Skeleton.Modules.<Name>.Contracts`: public DTOs and `I<Name>ModuleApi` for other modules.
- `tests/`: unit tests per module, `Skeleton.Host.IntegrationTests` (WebApplicationFactory), `Skeleton.ArchitectureTests` (module boundaries).

## Conventions
- One file per feature in `Features/`: request record, handler class, static endpoint class with `Map(IEndpointRouteBuilder)`.
- Request records are `public` (the .NET 10 validation generator ignores non-public types); everything else in a module is `internal`.
- Each module's `Register` calls `services.AddValidation()` and registers its own handlers.
- Expected failures return `Result<T>` with an `Error` (NotFound/Validation/Conflict/Failure) → `error.ToProblem()`. Throw only for bugs.
- Modules talk to each other only through `*.Contracts`. Architecture tests enforce this.
- Add new modules to the `modules` array in `Program.cs` explicitly (no assembly scanning).
- Package versions live only in `Directory.Packages.props`. Commit `packages.lock.json` changes.
- Mark deliberate shortcuts with `// NOTE:` naming the limit and the upgrade path.
