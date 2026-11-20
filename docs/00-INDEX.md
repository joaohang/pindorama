# Pindorama — Índice de documentos

## Comece por aqui

- [`GAME_RULES.md`](GAME_RULES.md) — **Mechanics / Rules Specification**,
  fonte canônica das regras jogáveis (movimento, gancho, parede, caixas,
  laser, bola e inimigos).

## Documentos existentes

- [`../README.md`](../README.md) — onboarding, cenas e controles.
- [`../LEVEL02.md`](../LEVEL02.md) — level design e regras específicas da
  Level02.
- [`../ENEMIES.md`](../ENEMIES.md) — design e histórico do subsistema de
  inimigos.

## Planejados

- `TDD.md` — Technical Design Document (classes, eventos, integração).
- `01-GDD.md` — visão, pilares e loop.
- `04-LEVELS/` — um documento por fase.
- `05-BALANCE.xlsx` — planilha de tuning.
- `06-ADR/` — decisões de arquitetura/design.

## Convenções

- Regras usam IDs (`R-HOOK-01`, `R-WALL-03`, `R-QTE-02`) para facilitar bugs,
  testes e prompts.
- Status: `[IMP]` implementado, `[PROT]` protótipo, `[PLAN]` planejado.
- Regras são escritas de forma testável; detalhes de código ficam no TDD.
- Ao mudar uma regra, atualize o changelog em `GAME_RULES.md`.
