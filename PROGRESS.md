# Progress: dotnet-modular-skeleton
Last updated: 2026-10-09

## Where we stopped
PROGRESS.md tracking added on `chore/progress-tracking` → push + PR it, then branch protection on `main`.

## Next (action points, in order)
- [ ] Branch protection on `main` (required checks: "Build and test", "Analyze C#")
- [ ] Decide GHCR package visibility (starts private)
- [ ] Fix local Docker daemon (`docker info` panicked) and run the container once
- [ ] Optional: pack the template as a NuGet package (install without cloning)

## Waiting on
- `gh` token missing `read:packages` scope: me, since 2026-10-07

## To discuss
- (none)

## Decisions
- 2026-10-07: .NET 10 LTS modular monolith (IModule + per-module Contracts); no DB, auth or mediator yet. Why: minimal skeleton, add deliberately.
- 2026-10-07: xUnit v3 on MTP + Shouldly, NetArchTest for boundaries, SDK container publish (no Dockerfile), CPM with lock files. Why: built-in/boring tooling.
- 2026-10-07: README.md excluded from generated products; product docs live in CLAUDE.md. Why: template rename mangles README prose.

## Done
- 2026-10-09: Added PROGRESS.md, `@PROGRESS.md` import, excluded PROGRESS.md from template output, PROGRESS.md, CLAUDE.md, .template.config/template.json, verification: JSON validated, build/tests not run (no code change)
- 2026-10-07: Skeleton merged (PR #1, 639db05); repo marked GitHub template (`dotnet new modular-skeleton`), CI green, image published to ghcr.io/adamtalleh/dotnet-modular-skeleton
