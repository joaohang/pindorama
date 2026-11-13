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
| Caixas | `BoxCollider2D` | Não | **Dynamic** (gravidade), Freeze Rotation Z | `HookAnchor` (Fixed), `HookPullable` (modo dinâmico), `BoxIdentity` |
| Marcações (pits) | `BoxCollider2D` | **Sim** | — | `BoxSnapTarget` |
| Laser | `BoxCollider2D` | **Sim** | — | `LaserHazard` |
| Espinhos / queda | `BoxCollider2D` | **Sim** | — | `LethalHazard` |
| Bola Amarela | `CircleCollider2D` | Não | **Kinematic** | `HookAnchor` (Swing), `MovingAnchor` |
| Jogador | `CapsuleCollider2D` | Não | Dynamic | `PlayerController2D`, `DualHookController` |

### Regras importantes

- **Caixas**: `Rigidbody2D` **Dynamic** com gravidade e `constraints = Freeze
  Rotation Z`. O `HookPullable` está em **modo dinâmico**: o puxão aplica
  velocidade **horizontal** (não teleporta na diagonal), a gravidade e os
  colliders continuam valendo e a caixa cai quando sai da plataforma. O
  `BoxIdentity` centraliza a caixa no `SnapPoint` quando ela entra no pit.
  O modo kinematic antigo continua disponível em `HookPullable` para outros
  objetos (`useDynamicBody = false`).
- **Pits**: o `BoxSnapTarget` só aceita a cor configurada. Caixa errada dispara
  `WrongBoxEntered` e o puzzle falha.
- **Laser**: `LaserHazard.SetActive(false)` desliga beam, glow e collider de uma
  vez. O `BoxSyncPuzzle` chama isso permanentemente no sucesso.
- **Bola Amarela (âncora móvel com swing):** usa o mesmo `DistanceJoint2D` das
  plataformas, com `connectedBody` = `Rigidbody2D` da bola e
  **pivô no centro da bola** (`HookAnchor.AttachPoint`), não no ponto lateral
  do raio. O jogador **balança** e é carregado pelo movimento vertical; continua
  dinâmico, então colide com o chão e **não atravessa**. Soltar remove o joint.
- **Ponto de ancoragem:** para âncoras móveis (`MovingAnchor`), o pivô é o
  centro da âncora. Para âncoras estáticas, o gancho guarda o `hit.point` em
  coordenadas locais (`InverseTransformPoint`) e converte com `TransformPoint`
  a cada frame — a corda acompanha o movimento sem cortar o collider.
- **Espinhos**: um único `BoxCollider2D` trigger cobre a faixa; os sprites dos
  espinhos são apenas visuais.
- **Collider do jogador**: o componente `PlayerBodyAlignment` no `Player` lê os
  `bounds` do `SpriteRenderer` do filho `Visual` e ajusta automaticamente o
  `CapsuleCollider2D` (tamanho, offset e direção). Ele roda também no editor:
  se você mover ou redimensionar a imagem, o collider acompanha. Não edite o
  offset/tamanho do collider na mão — o componente sobrescreve para manter o
  alinhamento. Para reajustar manualmente, use o menu de contexto do componente
  **“Ajustar collider à imagem”**.

## Wall Cling (paredes escaláveis)

- Layer **`ClimbableWall`** (layer 8).
- `ParedeEsquerda` é toda escalável (teal).
- `ParedeDireita` foi dividida em **dois segmentos**:
  - `ParedeDireita_Grimpavel` (baixo, **teal**): layer `ClimbableWall` +
    `HookAnchor`; sobe/desce e aceita gancho.
  - `ParedeDireita_Comum` (cima, **cinza escuro**): layer `Default`, sem
    `HookAnchor`; não gruda e o gancho não prende.
- O `Player` tem o componente **`WallClingController`**.
- Detecção: dois `Raycast2D` (peito e cintura) na direção do movimento/face,
  exigindo o **mesmo collider** e normal horizontal. Não usa
  `OnCollisionEnter2D`.
- Entra no estado quando está no ar e:
  - pressiona na direção da parede ou tem velocidade horizontal para ela; ou
  - está sendo puxado por um gancho preso na parede.
- Ao agarrar: velocidade zerada, `gravityScale = 0`, `FreezePositionX`, solta
  ganchos, reseta cooldown de gancho/boost e trava o visual virado para a parede.
- **Subir/descer:** `W`/`S` ou `↑`/`↓` (stick vertical). Sobe devagar
  (`climbUpSpeed = 2`) e desce escorregando um pouco mais rápido
  (`climbDownSpeed = 3.2`). Sem input, fica parado.
- **Wall Jump:** `Espaço`/A → impulso diagonal para longe da parede
  (`normal * wallJumpForceX`, `wallJumpForceY`), restaurando a gravidade.
- **Soltar:** `Ctrl`/B → solta e cai. (`S`/`↓` agora serve para descer.)
- **Gancho na parede:** mire e dispare; assim que o gancho prende, o
  `WallClingController` sai do estado e o joint puxa o jogador.
- **Re-grab cooldown:** 0,2 s após pular/soltar/sair, para não grudar de novo no
  frame seguinte.
- **Gizmos:** com o Player selecionado, os dois raios/boxes de checagem aparecem
  em ciano (Scene view).

Para tornar outra parede escalável: coloque-a na layer `ClimbableWall` e, se
quiser que o gancho também prenda nela, adicione um `HookAnchor` (Neutral,
Swing). O menu **Pindorama → Configurar Wall Cling (Level02)** reaplica isso nas
paredes externas.

### Puxão dinâmico das caixas

- `HookPullable.useDynamicBody = true` (já configurado nas caixas da Level02).
- O puxão aplica apenas **velocidade horizontal**; a gravidade continua puxando
  a caixa para baixo. Ela respeita plataformas, paredes e o chão.
- **Segure o botão** para puxar continuamente; soltar para de puxar.
- `dynamicPullSpeed` (5): velocidade máxima do puxão.
- `dynamicPullAcceleration` (45): rapidez com que a caixa chega nessa velocidade.
- `dynamicPullOvershoot` (0,4): o alvo do puxão passa essa distância **além do
  jogador**, para a caixa atravessar o buraco em vez de parar antes dele.
- A caixa cai quando o centro dela passa da borda da plataforma. O trigger do
  pit precisa estar na **abertura do buraco** para pegar a caixa na queda.
- **Posicionamento:** fique do lado do buraco em que a caixa deve cair, com o
  buraco entre você e a caixa. O puxão sempre traz a caixa na sua direção.
- **Ao encaixar**, o `BoxIdentity` solta automaticamente qualquer gancho preso
  naquela caixa (`ReleaseHooks`), para o cabo não ficar pendurado no nada.
- Se quiser uma caixa que desliza sem gravidade (comportamento antigo), use
  `useDynamicBody = false`, `BoxIdentity.dynamicBody = false` e
  `Rigidbody2D.bodyType = Kinematic`.

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
