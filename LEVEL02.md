# Level 02 — guia da cena

Cena: `Assets/_Project/Scenes/Level02.unity`

Todos os elementos estão **na hierarquia da cena** (nada de GameBootstrap).
Você pode mover, redimensionar e trocar referências pelo Inspector.

## Hierarquia

```text
Level02
├── Player             (PlayerBodyAlignment)
│   ├── HandRed        (HookHand + HookRopeView)
│   ├── HandBlue       (HookHand + HookRopeView)
│   └── Visual         (SpriteRenderer que o collider segue)
├── Main Camera        (CameraFollow)
├── LevelController
├── BoxSyncPuzzle
├── LASER 1
├── BOX RED 2
├── BOX BLUE 1
├── Pit Red / Pit Blue
├── Bola Amarela
├── BolaPontoA / BolaPontoB / TrilhoBola
├── Espinhos
├── Ancora_*           (11 barras de swing)
├── Plataforma*        (plataformas e paredes)
├── SpawnPoint
├── HUD / AimView / SpeedLines
└── ...
```

## Colisores e triggers

| Objeto | Collider | Trigger? | Rigidbody2D | Script |
|---|---|---|---|---|
| Plataformas / paredes | `BoxCollider2D` | Não | — | — |
| Âncoras de swing | `BoxCollider2D` | **Sim** | — | `HookAnchor` (Swing) |
| Caixas | `BoxCollider2D` | Não | **Kinematic**, Freeze Rotation Z | `HookAnchor` (Fixed), `HookPullable`, `BoxIdentity` |
| Marcações (pits) | `BoxCollider2D` | **Sim** | — | `BoxSnapTarget` |
| Laser | `BoxCollider2D` | **Sim** | — | `LaserHazard` |
| Espinhos / queda | `BoxCollider2D` | **Sim** | — | `LethalHazard` |
| Bola Amarela | `CircleCollider2D` | Não | **Kinematic** | `HookAnchor` (Swing), `MovingAnchor` |
| Jogador | `CapsuleCollider2D` | Não | Dynamic | `PlayerController2D`, `DualHookController` |

### Regras importantes

- **Caixas**: `Rigidbody2D` Kinematic + `constraints = Freeze Rotation Z`. O
  `HookPullable` move por `MovePosition`; o `BoxIdentity` centraliza a caixa no
  `SnapPoint` e desliga os colliders quando ela encaixa.
- **Pits**: o `BoxSnapTarget` só aceita a cor configurada. Caixa errada dispara
  `WrongBoxEntered` e o puzzle falha.
- **Laser**: `LaserHazard.SetActive(false)` desliga beam, glow e collider de uma
  vez. O `BoxSyncPuzzle` chama isso permanentemente no sucesso.
- **Bola Amarela**: o `DistanceJoint2D` do gancho usa o `Rigidbody2D` da bola
  como `connectedBody`; por isso o jogador é transportado junto.
- **Espinhos**: um único `BoxCollider2D` trigger cobre a faixa; os sprites dos
  espinhos são apenas visuais.
- **Collider do jogador**: o componente `PlayerBodyAlignment` no `Player` lê os
  `bounds` do `SpriteRenderer` do filho `Visual` e ajusta automaticamente o
  `CapsuleCollider2D` (tamanho, offset e direção). Ele roda também no editor:
  se você mover ou redimensionar a imagem, o collider acompanha. Não edite o
  offset/tamanho do collider na mão — o componente sobrescreve para manter o
  alinhamento. Para reajustar manualmente, use o menu de contexto do componente
  **“Ajustar collider à imagem”**.

## Como o puzzle funciona

1. Arraste `BOX RED 2` para `Pit Red` e `BOX BLUE 1` para `Pit Blue`.
2. Se as duas marcações estiverem ocupadas **ao mesmo tempo** (ou dentro de
   `simultaneousWindow`), o `BoxSyncPuzzle` resolve e desliga o `LASER 1`.
3. Se apenas uma estiver ocupada além da janela, ou se uma caixa entrar na
   marcação errada, o puzzle falha: as duas caixas explodem e voltam aos spawns.
4. O laser **não** volta a ligar depois de resolvido.

Campos ajustáveis no `BoxSyncPuzzle`:

- `simultaneousWindow` (padrão 3 s): tempo de tolerância com uma caixa colocada.
- `failureCooldown` (0,75 s): evita falhas repetidas em sequência.
- `targets`, `boxes`, `lasersToDisable`: arrays editáveis no Inspector.

## Como criar mais caixas/marcações

1. Duplique uma caixa existente e mude `BoxIdentity.Color` + o `HookAnchor`.
2. Duplique um pit e mude `BoxSnapTarget.AcceptedColor`.
3. Adicione os novos componentes nos arrays do `BoxSyncPuzzle`.
4. Mantenha o `SnapPoint` como filho do pit, no centro desejado.

## Ajustes de gameplay

- `HookSettings` (asset em `Assets/_Project/Data`): alcance, corda, swing,
  boost, etc.
- `HookPullable`: `pullStepDistance`, `pullStopDistance`, `pullSpeed`.
- `MovingAnchor`: `pointA`, `pointB`, `speed`.
- `LevelController`: spawn, player, hooks, puzzle; `R` reseta o jogador e as
  caixas (o laser resolvido permanece desligado).

## Controles

Os mesmos do protótipo:

- `A`/`D` mover, mouse mirar, LMB/RMB ganchos, `Espaço` pular/soltar,
  `Shift` boost, `Ctrl` soltar, `R` reset.
