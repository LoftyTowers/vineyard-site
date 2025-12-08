# Coding Patterns (language-agnostic)

## When to add a seam
| Trigger | Reach for |
|---|---|
| Crossing an IO boundary (HTTP, DB, queue, filesystem) | **Port + Adapter** |
| Two or more real variants needed now or soon | **Strategy** or **Factory** |
| Optional concern toggled by feature flag/plugin | **Decorator** |
| Integration owned by another team/vendor | **Port + Adapter** |

Always cite the trigger in your PR/commit when you introduce a seam; otherwise prefer the simplest DevKit-style implementation.

## Strategy
Use when behaviour varies by type/rule/flag. Avoid if only one variant and no near-term need.
See: examples/dotnet/patterns/strategy/PricingStrategies.cs

## Factory
Use when creation depends on config/env and is likely to change.
See: examples/dotnet/patterns/factory/PaymentGatewayFactory.cs

## Decorator
Use for cross-cuts: logging, caching, metrics, auth.
See: examples/dotnet/patterns/decorator/CachingCatalog.cs

## Ports & Adapters
Wrap external I/O; keep domain ignorant of transport/provider.

## DI vs Static vs Factory
- **Default**: constructor DI for collaborators (gateways, repos, clocks, loggers, validators).
- **Static**: only for **pure functions** (no IO, no state). Prefer modules with static methods or extension methods.
- **Factory**: use when selection varies per call (strategy keyed by type/tenant/feature flag).
