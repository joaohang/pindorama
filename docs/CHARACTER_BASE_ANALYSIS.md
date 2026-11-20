# Análise — personagem do projeto `jogoTeste` como base

**Fonte analisada:** `jogoTeste/New Unity Project`  
**Unity:** 6000.6.6f1 (mesma versão do Pindorama)  
**Data:** 2026-11-20

## Veredito

**Sim, é possível usar como base visual**, principalmente os sprites 80×80 e as
animações de **Idle, Run, Jump e Hang**.

**Não é um plug-and-play**, e não recomendo usar os scripts do projeto como
base de código. O Pindorama já tem um sistema de gancho duplo, wall cling e
puzzle mais avançado. O que vale a pena trazer é:

- o personagem 80×80;
- os clipes `Cyberpunk_Idle`, `Cyberpunk_Run`, `Cyberpunk_Jump`;
- as 4 poses `Hang`;
- o conceito de `Player Pose` separado do corpo;
- os offsets de muzzle das poses.

O que **não existe** e precisa ser criado:

- wall cling/wall climb;
- poses para dois ganchos simultâneos;
- hurt/death/respawn;
- pose dedicada de queda/land;
- pose de boost.

## O que existe no projeto

### Personagem 80×80 — o candidato principal

`Assets/GrapplingHook/Art/Cyberpunk City 2/Sprites/Player/`

| Estado | Frames | Canvas | PPU | Pivô |
|---|---:|---:|---:|---|
| Idle | 4 | 80×80 | 100 | 0.5, 0.5 |
| Run | 8 | 80×80 | 100 | 0.5, 0.5 |
| Jump | 7 | 80×80 | 100 | 0.5, 0.5 |
| Hang | 4 | 80×80 | 100 | ~0.46875, 0.36875 |

Frames:

- `Idle1–4`
- `Run1–8`
- `Jump1–7`
- `Hang1–4`

O personagem tem cabelo azul, roupa vermelha e detalhes neon. O estilo é
cyberpunk colorido, um pouco mais “arcade” que o graybox atual do Pindorama,
mas combina com a direção industrial/cyberpunk.

### Personagem 32×38 — não recomendo

`Assets/GrapplingHook/Art/OriginalPlayer/`

- Idle 6 frames;
- Run 6 frames;
- Jump 2 frames;
- Shoot 3 frames;
- Hurt 2 frames.

É um personagem **laranja/astronauta**, com estilo e escala diferentes do
personagem 80×80. Misturar os dois quebra a identidade visual. Use apenas como
referência de quantidade de estados, não como base.

### Animações

`Assets/GrapplingHook/Animations/`

- `Cyberpunk_Idle.anim` — 4 frames, 8 fps, loop;
- `Cyberpunk_Run.anim` — 8 frames, 12 fps;
- `Cyberpunk_Jump.anim` — 7 frames, 12 fps;
- `GrappleHold.anim` — pose única de 0.15 s;
- `GrappleAirLaunch.anim` / `GrappleGroundLaunch.anim`;
- `Cyberpunk_Player.overrideController` — troca os clipes do controller base
  pelos clipes 80×80.

O controller usa os parâmetros:

- `Speed` (float);
- `isGrounded` (bool);
- `Attack` (trigger);
- `IsBusy` (bool).

### Apresentação do gancho

`GrapplingHookPresentation.cs` é a parte mais interessante para copiar como
**conceito**:

- o corpo normal é escondido;
- um `Player Pose` separado mostra as poses `Hang`;
- `Hang2`/`Hang3` são usadas para desenhar/mirar;
- `Hang4` é a pose presa;
- cada pose tem um `Muzzle` com offset próprio;
- ao soltar, o corpo volta ao Animator normal.

Isso resolve o problema de trocar sprites com **pivôs diferentes** (Idle 0.5,0.5
vs. Hang 0.46875,0.36875) sem o personagem “pular” de posição.

### Tileset do projeto fonte

`tileset.png` é 720×128 e contém 7 sprites irregulares:

| Sprite | Tamanho |
|---|---:|
| tileset_0 | 161×98 |
| tileset_1 | 66×45 |
| tileset_2 | 66×45 |
| tileset_3 | 87×98 |
| tileset_4 | 139×98 |
| tileset_5 | 40×88 |
| tileset_6 | 87×45 |

Isso **não é um tileset 32×32**. É um conjunto de plataformas/blocos
hand-painted em roxo/teal. Serve como referência de clima, mas não substitui o
tileset 32×32 planejado no `TILESET_BRIEF.md`.

### Scripts do projeto fonte

`GrapplingHook2D` e `PlayerTestController` usam:

- **Input System**;
- **URP**;
- um único gancho;
- lógica de física diferente do Pindorama.

Não recomendo portar esses scripts. O Pindorama já tem:

- dois ganchos com cores;
- disparo espelhado;
- wall cling;
- boost tangencial;
- puxáveis e puzzle;
- câmera/controle legacy.

Portar o `GrapplingHook2D` seria um retrocesso.

## Compatibilidade técnica

### Escala — muito boa

O prefab fonte usa:

- sprite 80×80;
- PPU 100;
- escala 3 no root.

Isso resulta em um canvas visual de:

```text
80 / 100 × 3 = 2,4 unidades
```

Mas o personagem **opaco** do Idle tem apenas ~23×58 px:

```text
23 / 100 × 3 = 0,69 unidade de largura
58 / 100 × 3 = 1,74 unidade de altura
```

O capsule do Pindorama é **0,8 × 1,8**. Ou seja, o personagem visível encaixa
quase perfeitamente no collider atual.

| Item | Fonte | Pindorama |
|---|---:|---:|
| Altura visível idle | 1,74 u | 1,8 u |
| Largura visível idle | 0,69 u | 0,8 u |
| Canvas do sprite | 2,4 u | — |
| PPU | 100 | 32 (mundo) |
| Escala do visual | 3 | 1 (root) |

**Conclusão:** use o visual em um filho com escala 3; mantenha o capsule
0,8×1,8. Não use o root scale 3 inteiro, senão o collider e a física mudam.

### Conflito com `PlayerBodyAlignment`

O Pindorama tem `PlayerBodyAlignment`, que ajusta o capsule ao `bounds` do
`SpriteRenderer`. Com um sprite 80×80, o bounds é **2,4×2,4**, não o personagem
opaco (0,69×1,74). Isso criaria um capsule gigante.

Soluções, em ordem de preferência:

1. **Desativar/remover `PlayerBodyAlignment`** no player com esse visual e
   deixar o capsule fixo em 0,8×1,8.
2. Alterar o componente para aceitar um `Vector2 colliderSizeOverride`.
3. Fazer um bake/crop dos sprites 80×80 para o bounding box opaco e reimportar
   com pivô ajustado. Dá mais trabalho e muda os offsets de muzzle.

Para o protótipo, a opção 1 é a mais rápida.

### Alinhamento dos pés

O sprite é 80×80 com pivô central. Em escala 3/PPU100, o canvas tem 2,4 u e o
fundo opaco fica no fundo do canvas. O capsule do Pindorama tem 1,8 u.

Sugestão:

- filho `Visual` com escala 3;
- `localPosition.y ≈ +0,3` para alinhar o pé do sprite ao fundo do capsule;
- testar no Scene view;
- manter o `Muzzle`/mãos fora do `Visual`, para a corda não herdar a escala.

### Pivôs diferentes entre Idle e Hang

Idle/Run/Jump usam pivô `0.5, 0.5`; Hang usa `0.46875, 0.36875`. Se trocar o
sprite no mesmo `SpriteRenderer`, o personagem desloca.

Solução recomendada: copiar a estrutura do projeto fonte:

```text
Player
├── BodyVisual          (SpriteRenderer + Animator: Idle/Run/Jump)
└── PoseVisual          (SpriteRenderer separado: Hang1–4, desligado por padrão)
    └── Muzzle
```

Quando o gancho prende ou o jogador está em swing, desliga `BodyVisual` e liga
`PoseVisual`. Ao soltar, inverte.

## O que dá para aproveitar

| Recurso | Aproveitar? | Como |
|---|---|---|
| Sprites 80×80 | ✅ | copiar para `Assets/_Project/Art/Player/` |
| Idle/Run/Jump | ✅ | usar clips existentes ou recriar |
| Hang 1–4 | ✅ | pose de swing/gancho |
| Muzzle offsets | ✅ | adaptar para uma mão |
| `Player Pose` separado | ✅ | resolve pivôs |
| Animações 32×38 | ❌ | estilo diferente |
| Scripts de hook/player | ❌ | Pindorama já tem sistema melhor |
| Tileset 720×128 | ⚠️ | só referência de clima; não é 32×32 |
| URP/materiais | ❌ | Pindorama é built-in |
| Input System | ❌ | Pindorama usa Input Manager |

## O que falta para o Pindorama

### 1. Wall Cling / Wall Climb

Não existe nenhuma pose de parede no projeto fonte. `Hang` é pose de corda,
não de parede.

Para o Pindorama, seriam necessários:

- `WallCling_Idle` (loop, 2–4 frames);
- `WallClimb_Up` (loop, 4–6 frames);
- `WallClimb_Down` (loop, 4–6 frames);
- `WallJump` (2–3 frames).

Sem isso, o wall cling funcionaria, mas com o personagem “pendurado no ar” sem
animação adequada.

### 2. Dois ganchos

As poses `Hang2/3/4` mostram **um** gancho. O Pindorama tem dois ganchos
simultâneos.

Opções:

1. usar `Hang` para o gancho principal e desenhar a segunda corda saindo da
   outra mão (funcional, mas visualmente aproximado);
2. criar poses com dois braços/duas cordas;
3. usar o corpo normal + duas cordas, sem pose de hang, para o disparo duplo.

Para o protótipo, a opção 1 é aceitável.

### 3. Hurt / morte / respawn

O set 80×80 não tem hurt/death. O set 32×38 tem, mas é outro personagem.

Soluções:

- flash branco/vermelho + partículas no dano;
- criar 2–3 frames de hurt no mesmo estilo;
- usar uma pose de jump/queda como placeholder.

### 4. Queda e aterrissagem

`Jump1–7` cobre subida/queda de forma genérica. Não há um loop de queda
separado. Para o Pindorama, o ideal é:

- `Fall` (loop 2 frames);
- `Land` (1–2 frames).

Pode usar `Jump5–7` como placeholder.

### 5. Boost

Não há pose de Swing Boost. Usar `Hang3`/`Hang4` + VFX é suficiente no
protótipo.

## Arquitetura de port recomendada

### Estrutura do Player

```text
Player (capsule 0.8×1.8, PlayerController2D, DualHookController)
├── VisualRoot (scale 3, offset y ≈ 0.3)
│   ├── BodyVisual (SpriteRenderer + Animator: Idle/Run/Jump)
│   └── PoseVisual (SpriteRenderer: Hang1–4, desligado)
├── HandRed (muzzle)
├── HandBlue (muzzle)
└── ...
```

Não copiar o prefab inteiro: ele traz `PlayerTestController`, `PlayerHealth`,
`EnemyHealth`, `GrapplingHook2D`, colliders e layers do projeto fonte. Copie
**somente os SpriteRenderers, Animator e clips**.

### Bridge de animação

Criar um `PlayerAnimatorBridge` no Pindorama:

- lê `PlayerController2D.Speed`, `IsGrounded`, `IsBraced`,
  `IsInEnemyInteraction`;
- lê `WallClingController.IsClinging`;
- lê `DualHookController.AnyAttached` / `RedHand.IsSwing` / `BlueHand.IsSwing`;
- define:
  - `Speed` (float);
  - `isGrounded` (bool);
  - `IsBusy` (bool);
  - `Hang` (bool);
  - `WallCling` (bool);
  - `WallClimb` (float);
  - `Hurt` (trigger);
  - `Dead` (bool).

O Animator do corpo toca Idle/Run/Jump. O `PoseVisual` é ligado pelo bridge
quando há swing/hang, como no projeto fonte.

### Muzzle e corda

O Pindorama já tem `HandRed`/`HandBlue` como mãos. Para o visual novo:

- manter as mãos no root do player, não dentro do `VisualRoot` com escala 3;
- usar `PoseMuzzleOffsets` por frame de Hang, como no projeto fonte;
- para duas mãos, criar dois offsets ou usar as mãos fixas;
- a corda do `HookRopeView` continua usando `Hand.VisualAttachPoint`.

### Animator speed

O Pindorama corre a 7 u/s e pula a 16.8; o projeto fonte corre a 5 e pula a 9.

- `Animator.speed` do Run pode ser `0.8 + speed / 10`;
- Jump pode usar `1.0–1.3`;
- não deixar a animação “patinando” em alta velocidade.

## Plano de implementação em fases

### Fase 1 — placeholder jogável

1. Copiar 80×80 sprites + clips Idle/Run/Jump/Hang.
2. Montar `VisualRoot`/`BodyVisual`/`PoseVisual`.
3. Desativar `PlayerBodyAlignment` e fixar capsule 0.8×1.8.
4. Criar `PlayerAnimatorBridge` mínimo.
5. Usar Hang para swing; corpo normal para o resto.
6. Testar em `Level02` e `EnemyLab`.

### Fase 2 — wall cling

7. Criar/usar `WallCling_Idle` (pode ser Hang1 no começo).
8. Criar `WallClimb_Up/Down` (ou usar 2 frames de Hang).
9. Ligar `WallClingController` ao bridge.
10. Ajustar offset do visual na parede.

### Fase 3 — polimento

11. Hurt/death/respawn.
12. Fall/Land.
13. Poses de dois ganchos.
14. Boost.
15. Recolorir o personagem para a paleta do Pindorama, se necessário.

## Licença — atenção

O README do projeto fonte diz:

```text
Artwork & Assets: Ansimuz (Cyberpunk City 2)
https://ansimuz.itch.io/
```

Não há arquivo de licença do pacote de arte no projeto. Antes de usar:

- confirme se você tem licença para usar o pack no Pindorama;
- verifique se a licença permite **redistribuir** os sprites no repositório;
- se o repositório for público, não commite os PNGs de terceiros sem
  permissão;
- mantenha o crédito exigido pela licença.

O código do pacote parece ser de tutorial/integração; a arte é de terceiros.
Não trate o projeto fonte como licença livre só porque está no seu disco.

## Implementação aplicada no Pindorama

O personagem 80×80 foi aplicado como **visual runtime** em todas as cenas,
sem alterar os scripts de gameplay existentes.

- Sprites copiados para `Assets/_Project/Art/PlayerCyberpunk/`;
- clips copiados para `Assets/_Project/Animations/PlayerCyberpunk/`;
- `PlayerCyberpunk.controller` criado com estados Idle/Run/Jump;
- `PlayerCyberpunkVisual.prefab` em `Assets/_Project/Resources/`;
- `PlayerCharacterVisual` controla corpo/pose e facing;
- `PlayerController2D.Start()` instancia o prefab automaticamente;
- `PlayerBodyAlignment` é desativado e o capsule fica fixo em 0.8×1.8;
- o visual antigo (`Visual`) é desativado.

### Poses padrão para estados sem animação

| Estado | Visual usado |
|---|---|
| Idle | Idle 80×80 |
| Run | Run 80×80 |
| Jump/fall | Jump 80×80 |
| Swing/gancho | Hang4 |
| Brace/dois ganchos | Hang4 |
| Wall cling | Hang1 (placeholder) |
| Wall climb up/down | Hang1 (placeholder) |
| Wall jump | Jump (corpo) |
| Boost | Hang4 |
| Hurt/morte | corpo padrão |

Para trocar os placeholders depois, basta substituir os sprites/frames no
`PlayerCharacterVisual` e criar os clips definitivos.

### Como visualizar

Abra `Level02`, `EnemyLab` ou `Main` e dê Play. O prefab é instanciado no
`Start()` do `PlayerController2D`; no editor, o visual antigo pode continuar
aparecendo até entrar em Play.

## Recomendação final

**Use o personagem 80×80 como base visual/placeholder**, não os scripts.

O caminho mais seguro:

1. portar somente arte + clips;
2. manter o `PlayerController2D`, `DualHookController` e
   `WallClingController` do Pindorama;
3. criar um `PlayerAnimatorBridge`;
4. usar as poses Hang para swing;
5. criar depois as animações de parede e dois ganchos;
6. resolver a licença antes de publicar.

Isso deve render um salto visual grande com risco técnico baixo.
