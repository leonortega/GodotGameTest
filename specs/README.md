# Retro Platformer Spec Repository

This repository is Godot-targeted and AI-friendly.

## Goals
- Keep requirements grouped by bounded game context.
- Keep requirements atomic, with one main behavior per spec file.
- Make every requirement testable with explicit acceptance criteria.
- Keep traceability across specs, contracts, examples, and BDD scenarios.
- Align preproduction decisions to a practical Godot 4.x workflow for a 2D retro platformer.

## Structure
- `features/`: Human-readable behavioral specs by game context.
- `contracts/`: Machine-readable JSON Schema contracts for state and result payloads.
- `examples/`: Input/output fixtures used by tests and AI generation.
- `bdd/`: Gherkin feature files for executable acceptance scenarios.
- `templates/`: Authoring templates.
- `references/`: External engine docs and repository references used to shape the specs.
- `glossary.md`: Shared domain terms.
- `decisions/`: Decision tables for ambiguous gameplay behavior.
- `traceability/`: Spec ID and mapping rules.

## Authoring Rules
1. One spec file per atomic behavior.
2. Use stable IDs: `<CONTEXT>.<AREA>.<NNN>` (example: `PLAYER.MOVEMENT.001`).
3. Use `shall` for normative requirements.
4. Include positive and edge scenarios.
5. Add data contracts when a data shape matters for tests or runtime integration.
6. Godot-facing specs may name engine systems when required for scene, input, camera, save, or content-authoring decisions.
7. Reference external repositories for patterns only; do not copy Mario IP, copyrighted assets, or third-party code into product requirements.

## Current Version
- Baseline: `v1.0`
- Product: `Super Pixel Quest`
- Target Engine: `Godot 4.x`
