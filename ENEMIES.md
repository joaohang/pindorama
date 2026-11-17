# Enemy Lab — análise de viabilidade, máquinas de estado e cena de validação

## Resposta curta

**Sim, dá para implementar.** A base de movimentação já existe e cobre boa parte
do prompt:

| Requisito | Status atual |
|---|---|
| Movimento fluido + momentum | ✅ |
| Gancho duplo, puxão, espelhado | ✅ |
| Swing Boost tangencial com cooldown | ✅ |
| Wall Cling + Wall Jump + drop + re-grab | ✅ |
| Caixas com snap, rotação travada, puzzle e laser | ✅ |
| Bola Amarela móvel | ✅ |
| Assistência de mira | ❌ (hoje é hitscan puro) |
| Inimigos, torreta, projéteis | ❌ (subsistema novo) |
| Estilingue amarelo + slow motion | ❌ |
| QTE azul/vermelho | ❌ |

O trabalho novo é concentrado em **inimigos e no contrato entre gancho e alvo**.

## Arquitetura recomendada

### 1. Contrato gancho → alvo

Hoje o `HookHand` conhece apenas `HookAnchor`. Para inimigos, adicionar uma
interface e notificar o alvo no attach/detach:

```csharp
public interface IHookReceiver
{
    void OnHookAttached(HookHand hand, Vector2 hitPoint);
    void OnHookDetached(HookHand hand, DetachReason reason);
}
```

No `HookHand.Attach`, após criar o joint/puxão:

```csharp
IHookReceiver receiver = anchor.GetComponent<IHookReceiver>();
receiver?.OnHookAttached(this, impactPoint);
```

No `ForceDetach`, antes de limpar o `anchor`:

```csharp
receiver?.OnHookDetached(this, reason);
```

Isso mantém o gancho genérico e deixa cada inimigo decidir o que fazer.

### 2. `EnemyBase`

Um componente base para todos os inimigos:

- `EnemyType` (`Normal`, `Turret`, `YellowSlingshot`, `BlueQte`, `RedQte`);
- cor/lado (Red/Blue/Neutral);
- vida, dano de contato, VFX de morte, respawn;
- referência ao `HookAnchor` filho;
- estado atual;
- `PlayerController2D` e `Rigidbody2D` do jogador;
- `DualHookController` para saber qual mão prendeu;
- campos de tuning (pull speed, stop distance, etc.);
- implementa `IHookReceiver`.

### 3. Estado do jogador durante interações com inimigo

Assim como o Wall Cling fez, é melhor o inimigo assumir o controle por um curto
período, mas com um **arbiter** para não brigar com gancho/parede:

- `PlayerInteractionState`: `Normal`, `Swinging`, `WallCling`, `EnemyStick`,
  `Launched`, `Dead`.
- O inimigo chama `player.BeginEnemyInteraction(...)` e
  `player.EndEnemyInteraction(...)`, que:
  - zeram velocidade;
  - suspendem gravidade/air control;
  - restauram no fim.
- Wall Cling, gancho e inimigo não devem escrever `linearVelocity` no mesmo
  frame sem coordenação.

### 4. Assistência de mira (opcional, mas recomendada)

`HookAimAssist`:

- procura `HookAnchor`/`EnemyBase` em um cone de ~25° e raio de ~9 u;
- escolhe o alvo mais próximo do centro do cone;
- ajusta `aimDirection` suavemente (`Slerp`/`Lerp`) por alguns graus;
- só atua se o alvo for válido para a mão que está mirando.

## Máquinas de estado

### Normal

```text
Idle/Patrol
   │ hook attach
   ▼
Hooked ── pull player até stopDistance
   │ contato/impacto
   ▼
Dead (VFX)
   │ hook released antes
   └──> Idle
```

- O gancho puxa o jogador até o inimigo (força/velocidade + joint).
- Ao encostar, inimigo morre; hook solta; jogador mantém momentum.
- Se soltar o botão antes, volta ao Idle e o inimigo fica vivo.

### Torreta de parede

```text
Idle/Aim
   │ timer
   ▼
Burst (N tiros)
   │
   ▼
Cooldown
   │
   └──> Aim (loop)
```

- Estados de tiro independentes do estado de gancho.
- `OnHookAttached` → `Hooked`; puxa o jogador; pode continuar atirando.
- Projétil: `EnemyProjectile` com velocidade, lifetime e dano/respawn.
- Recomendado: telegraph de 0.2–0.3 s antes da rajada.

### Inimigo amarelo (estilingue)

```text
Idle
  │ hook attach
  ▼
Pulling (puxa até perto)
  │ chegou
  ▼
Stuck (timeScale = 0.25, jogador parado, mira com mouse)
  │ solta botão do gancho
  ▼
Launch (direção da mira, atravessa e destrói o amarelo)
  │
  ▼
Dead + timeScale = 1
```

- Enquanto `Stuck`, usar `Time.unscaledDeltaTime` para input/UI.
- A direção do lançamento é `aimDirection` (ou vetor do jogador até o mouse).
- O lançamento deve ignorar colisão com o inimigo amarelo por um curto período
  (`Physics2D.IgnoreCollision` ou desabilitar o collider do inimigo).
- Se o jogador cancelar (drop), restaurar timeScale e soltar sem launch.

### Inimigos azul/vermelho (QTE)

```text
Idle
  │ hook attach
  ▼
Pulling
  │ chegou
  ▼
QteWindow (0.8–1.2 s, tempo real ou levemente desacelerado)
  │ input correto           │ input errado / timeout
  ▼                         ▼
Success (destrói)        Fail (knockback + dano + solta)
```

- O input correto é o botão da **cor do inimigo**:
  - vermelho → LMB;
  - azul → RMB.
- A janela usa `Time.unscaledDeltaTime` e mostra prompt no HUD.
- Acerto: inimigo morre, VFX, hook solta.
- Erro/timeout: jogador é rebatido para longe, perde vida/respawna, hook solta.

### Jogador (arbitragem)

```text
Normal ──hook──> Swinging
  │                 │
  ├──wall cling────> WallCling
  │
  ├──enemy hook────> EnemyStick (amarelo/QTE)
  │
  └──launch────────> Launched (invulnerável curto)
```

- Cada estado tem dono: WallCling, EnemyBase ou PlayerController2D.
- Ao entrar em `EnemyStick`, salvar `gravityScale`, constraints e
  `linearVelocity` para restaurar exatamente no fim.
- `Dead` cancela todos e chama `LevelController.RespawnPlayer()`.

## Cena sugerida: `EnemyLab.unity`

Layout horizontal, tudo visível e editável na Hierarchy (sem GameBootstrap):

```text
EnemyLab
├── Player
├── Main Camera
├── LevelController
├── HUD (EnemyHud)
├── PlataformaSpawn
├── Corredor de plataformas/âncoras
├── Sala 1: Inimigo Normal
│   ├── NormalEnemy
│   └── hook anchor filho
├── Sala 2: Torreta de Parede
│   ├── ParedeTorreta (ClimbableWall)
│   ├── TurretEnemy
│   └── ponto de tiro
├── Sala 3: Amarelo no ar
│   ├── YellowEnemy (collider trigger + HookAnchor)
│   └── plataforma de pouso
├── Sala 4: QTE Azul e Vermelho
│   ├── BlueQteEnemy
│   └── RedQteEnemy
└── paredes/limites
```

Sugestão de dimensões:

- sala de ~60 u de largura, 18 u de altura;
- spawn à esquerda, cada sala separada por 12–14 u;
- HUD mostra prompt do QTE, estado do amarelo e vida;
- `R` reseta a cena (LevelController);
- laddies/âncoras para testar aproximação.

## Configuração na Unity

### Layers/tags

- `Player`, `Enemy`, `EnemyProjectile`, `ClimbableWall`, `Hookable`.
- Inimigos com `BoxCollider2D` não-trigger; hitbox de gancho pode ser um
  `HookAnchor` filho com `BoxCollider2D` trigger.
- Projéteis em layer própria para colidir só com Player/chão.

### `EnemyBase` — campos

- `type`, `color`, `health`, `contactDamage`;
- `hookAnchor`, `spriteRenderer`, `deathVfx`;
- `pullSpeed`, `pullAcceleration`, `stopDistance`;
- `player`/`rb`/`hooks` (auto no `Awake`);
- `respawnPosition` (se a cena usar respawn).

### Torreta

- `burstCount`, `burstInterval`, `cooldown`, `telegraphTime`;
- `projectilePrefab`/`projectileSpeed`, `projectileDamage`;
- `firePoint`.

### Amarelo

- `slowTimeScale` (0.2–0.35);
- `stickDistance` (1.0–1.5);
- `launchSpeed` (18–26);
- `launchInvulnerability` (0.3 s).

### QTE

- `qteWindow` (0.8–1.2 s);
- `requiredHand` (Red/Blue);
- `knockbackForce` (8–14);
- `failDamage` (1).

### Puxão para inimigo

- Pode usar o próprio `DistanceJoint2D` do gancho + uma força/velocidade em
  direção ao alvo, parando a `stopDistance`.
- No código, `EnemyBase` aplica a força no `Rigidbody2D` do jogador enquanto
  `hooked` e `distance > stopDistance`.
- Não usar `MovePosition` no player; manter dinâmico para colisões.

## Plano de implementação

Fase 1 — base + cena:

1. `IHookReceiver` + eventos no `HookHand`.
2. `EnemyBase` + `NormalEnemy` + `TurretEnemy` + `EnemyProjectile`.
3. `EnemyLab` com salas 1 e 2.
4. `EnemyHud` mínimo.

Fase 2 — amarelo:

5. `YellowSlingshotEnemy` + time slow + launch.
6. Sala 3.

Fase 3 — QTE:

7. `QteEnemy` azul/vermelho + prompts.
8. Sala 4.

Fase 4 — polimento:

9. aim assist;
10. VFX/feedback, balanceamento, reset.

## Riscos e salvaguardas

- **Time.timeScale**: usar `unscaledDeltaTime` para input/UI e restaurar no
  `OnDisable`/morte.
- **Dois ganchos**: guardar qual mão prendeu o inimigo; só essa mão controla o
  estado.
- **Wall cling/gancho/inimigo**: centralizar a arbitragem de estado para não
  escrever velocidade/gravidade em dois lugares no mesmo frame.
- **Hook release**: notificar `OnHookDetached` também em jump/drop/reset.
- **Player collider**: manter dinâmico; não usar `simulated = false` em
  interações com inimigos.
- **QTE**: usar botão correspondente à cor; não usar o mesmo botão para tudo.
- **Projéteis**: layer/colisão seletiva para não acertar outros inimigos.

## Conclusão

O projeto já tem a parte mais difícil (movimento, gancho, boost, wall cling,
puzzle e bola móvel). O subsistema de inimigos é **viável** e modular, mas pede
uma cena e um contrato novo entre gancho e alvo. Recomendo validar primeiro um
**vertical slice** com inimigo normal + torreta + amarelo + um QTE, e só depois
balancear os quatro tipos juntos.
