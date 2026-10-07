# dotnet-modular-skeleton

A reusable **.NET 10 (LTS) modular-monolith backend** to start every new product from. It ships with tests, CI/CD and container publishing already working, so it's ready to ship from the first commit.

It deliberately includes **no database, no auth and no mediator**. Each product adds those to suit its own needs. The skeleton only contains plumbing that every product needs.

> This README documents the template itself and is **not** copied into generated products.
> Products get `CLAUDE.md`, which describes the stack, commands and conventions.

## What's inside

| Concern | How | Package |
|---|---|---|
| Architecture | Host + modules (`IModule`), each module = implementation project + public `Contracts` project | — |
| Endpoints | Minimal APIs, one file per feature (`Features/<Feature>.cs`: request, handler, endpoint) | — |
| Validation | .NET 10 built-in (`AddValidation()` + DataAnnotations) → 400 ProblemDetails | — |
| Errors | `Result<T>` / `Error` → RFC 7807 ProblemDetails; unhandled exceptions → 500 ProblemDetails | — |
| Health | `/health/live` (no checks), `/health/ready` (all registered checks) | — |
| API docs | OpenAPI at `/openapi/v1.json` and Scalar UI at `/scalar`, Development only | Microsoft.AspNetCore.OpenApi, Scalar.AspNetCore |
| Observability | OpenTelemetry traces, metrics and logs; OTLP export when `OTEL_EXPORTER_OTLP_ENDPOINT` is set | OpenTelemetry.* (5) |
| Hardening | CORS from config, per-IP rate limiting, HSTS + HTTPS redirect outside Dev | — |
| Build rules | Central Package Management, lock files, nullable, warnings as errors, `latest-recommended` analyzers, `.editorconfig` | — |
| Tests | xUnit v3 on Microsoft.Testing.Platform, Shouldly, WebApplicationFactory, NetArchTest module-boundary rules, coverlet coverage | xunit.v3, Shouldly, Microsoft.AspNetCore.Mvc.Testing, NetArchTest.Rules, coverlet.MTP |
| Container | SDK container publish (no Dockerfile), `noble-chiseled-extra`, non-root, port 8080 | — |
| CI/CD | GitHub Actions: format → build → test + coverage → template check → image to GHCR; CodeQL; Dependabot | — |

## Create a new product

**Option A: `dotnet new` (renames everything)**

```bash
git clone https://github.com/AdamTalleh/dotnet-modular-skeleton
dotnet new install ./dotnet-modular-skeleton
dotnet new modular-skeleton -n Acme.Shop        # creates ./Acme.Shop with Acme.Shop.* projects and namespaces
```

`Skeleton` is replaced everywhere: project and folder names, namespaces, the solution, and the container name (lowercased, `acme.shop`).

**Option B: GitHub "Use this template"**, then rename `Skeleton` to your product name yourself. Option A is quicker.

After creating it:
1. `git init`, push to a new GitHub repo. CI runs straight away. On the first restore NuGet creates `packages.lock.json` files; commit them.
2. Delete the Sample module when you no longer need it as a reference: remove `src/Modules/Sample`, `tests/*Sample.Tests`, their entries in the `.slnx`, the project reference and `new SampleModule()` line in the Host, and the Sample assertions in the integration tests.
3. Add your DB, auth and modules.

## Add a module

1. Copy `src/Modules/Sample` to `src/Modules/<Name>` and rename the projects and namespaces.
2. Add both projects to the `.slnx`.
3. Reference the module (not its Contracts) from `Skeleton.Host.csproj` and add `new <Name>Module()` to the `modules` array in `Program.cs`.
4. Other modules may reference **only** `<Name>.Contracts`. The architecture tests fail the build otherwise.

## Run locally

```bash
dotnet run --project src/Skeleton.Host          # http://localhost:5080/scalar
dotnet test --solution Skeleton.slnx
dotnet format                                  # fix formatting before pushing
```

Container (needs Docker running):

```bash
dotnet publish src/Skeleton.Host -c Release -t:PublishContainer
docker run -p 8080:8080 skeleton:latest
```

## Configuration

| Setting | Default | Notes |
|---|---|---|
| `Cors:AllowedOrigins` | `[]` | Empty = cross-origin requests refused |
| `RateLimiting:PermitLimit` / `WindowSeconds` | 100 / 60 | Per client IP, per instance. Validated at startup |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | unset | Set to enable OTLP export (e.g. `http://otel-collector:4317`) |
| `OTEL_SERVICE_NAME` | assembly name | Standard OTel env var |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | unset | Set `true` behind a reverse proxy/ingress so client IP and scheme are correct |

## CI/CD

| Workflow | Trigger | Does |
|---|---|---|
| `ci.yml` → **build** | PRs, push to `main`, `v*.*.*` tags | locked restore (fails on known vulnerable packages), format check, Release build, tests + Cobertura artifact, template check |
| `ci.yml` → **container** | push to `main` / tags, after build passes | publishes `ghcr.io/<owner>/<repo>` tagged `sha-<short>` + `latest` (main) or `1.2.3` (tag `v1.2.3`) |
| `codeql.yml` | PRs to `main`, push to `main`, weekly | CodeQL C# security analysis |
| `dependabot.yml` | weekly | NuGet + GitHub Actions updates (OpenTelemetry grouped) |

**Release:** `git tag v1.2.3 && git push origin v1.2.3`.

**Recommended repo settings** (GitHub → Settings):
- *Branches → Add rule for `main`*: require a pull request, require the status checks **Build and test** and **Analyze C#**, block force pushes.
- *Packages*: after the first image publish, set the GHCR package visibility (it starts private).

## Gotchas

- **Request types must be `public`.** The .NET 10 validation source generator silently skips non-public types, so validation would not run.
- **Every module that maps endpoints calls `services.AddValidation()`.** The generator only sees endpoints in its own assembly.
- **Tests use Microsoft.Testing.Platform** (`global.json` → `test.runner`). Use `dotnet test --solution …` / `--project …`; VSTest-only options like `--collect` don't apply.
- **Using order isn't enforced** in `.editorconfig`, because renaming the template reorders usings. Unused usings still fail the build.
