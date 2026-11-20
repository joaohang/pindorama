# Pindorama — Índice de documentos

## Comece por aqui

- [`GAME_RULES.md`](GAME_RULES.md) — **Mechanics / Rules Specification**,
  fonte canônica das regras jogáveis (movimento, gancho, parede, caixas,
  laser, bola e inimigos).
- [`TILESET_BRIEF.md`](TILESET_BRIEF.md) — contexto, direção de arte,
  especificação técnica e prompt pronto para outra IA montar o tileset.
- [`CHARACTER_BASE_ANALYSIS.md`](CHARACTER_BASE_ANALYSIS.md) — análise do
  personagem do projeto `jogoTeste` como base visual e plano de port.
- [`SPRITE_SHEET_BRIEF.md`](SPRITE_SHEET_BRIEF.md) — análise do SANABI,
  poses faltantes e prompts para gerar o sprite sheet completo.
- [`CHARACTER_DESIGN_PROMPT.md`](CHARACTER_DESIGN_PROMPT.md) — prompt único
  para o sprite sheet do protagonista ciborgue (sem mãos; foice + martelo).

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
