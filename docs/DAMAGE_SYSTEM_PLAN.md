# Pindorama — sistema híbrido de dano e morte

**Status:** plano de aplicação; nada implementado no jogo ainda.  
**Objetivo:** definir a máquina de estados, a integração com o que já existe e
um prompt melhorado para uma IA de implementação.

## 1. O que o SANABI faz

Referências públicas:

- [SANABI — Controls and mechanisms (gemelog)](https://gemelog.jp/en/games/sanabi/basics/)
- [SANABI — Walkthrough (gemelog)](https://gemelog.jp/en/games/sanabi/walkthrough/)

O SANABI tem:

- vida/health e máquinas de cura;
- dano não-letal com invulnerabilidade curta e possibilidade de dash
  direcional após o hit;
- perigos que continuam letais mesmo com vida: lasers, red zones, certos
  ataques;
- checkpoints e reset de trecho.

O Pindorama **não tem vida** hoje: todo perigo chama
`LevelController.RespawnPlayer()`. A adaptação proposta é:

| SANABI | Pindorama |
|---|---|
| Health | **contador de falhas consecutivas** |
| Dano + i-frames | **Hit + invulnerabilidade + knockback** |
| Dash pós-hit | **knockback + liberação dos ganchos** |
| Laser/red zone letal | **LethalDamage** (abismo, laser, kill zone) |
| Checkpoint/heal | **safe ground + tempo sem dano** |
| Morte | **LethalData ou falhas >= limite** |

Não copiar a fórmula do SANABI: o Pindorama não tem dash normal; o gancho e o
wall cling precisam ser cancelados de forma limpa no hit.

## 2. Estado atual do Pindorama

Componentes relevantes:

- `PlayerController2D`: movimento, gravidade, brace, pulo, interações.
- `DualHookController`: dois ganchos, input suspenso em QTE/estilingue,
  `ReleaseAll`, `ResetAirResources`.
- `HookHand`: joint/corda, `ForceDetach`, `PrimaryCommand`.
- `WallClingController`: wall cling/climb, `ExitCling`, `ClimbDirection`.
- `EnemyBase`: puxão, amarelo (`Time.timeScale` direto), QTE, contato.
- `LevelController`: respawn/teleporte, `R`.
- `LaserHazard`, `LethalHazard`, `EnemyProjectile`, `KillZone`: hoje chamam
  respawn diretamente.
- `HookHud`: status dos ganchos/boost.
- `PlayerCharacterVisual`: corpo/pose; sem hurt/death próprios.

### Problemas a resolver

1. Todo dano hoje é morte/respawn; não existe hit não-letal.
2. O amarelo escreve `Time.timeScale` diretamente; um slowmo de hit pode
   conflitar.
3. Não existe estado central de “morto/hit”; cada sistema reage por conta
   própria.
4. Hooks, wall cling, enemy interaction e brace precisam ser cancelados na
   ordem certa para não sobrar joint/constraint/input travado.
5. Respawn atual não reseta contador, i-frames, slowmo nem input suspenso de
   forma centralizada.

## 3. Máquina de estados proposta

Estados do jogador para dano:

```text
Normal
  -> HitStun        (dano não-letal, falhas < limite)
  -> Dead           (dano letal ou falhas >= limite)

HitStun
  -> Recovering     (hitstun acabou)

Recovering
  -> Normal         (recuperou: tempo seguro ou safe ground)
  -> HitStun        (novo dano não-letal)
  -> Dead           (dano letal ou falhas >= limite)

Dead
  -> Respawning     (delay de morte, tempo real)

Respawning
  -> Normal         (LevelController reposicionou e resetou)
```

### Regras de transição

**Normal → HitStun**

- Recebe `DamageType.NonLethal`;
- não está em i-frames;
- `failures + 1 < maxFailures`;
- incrementa `failures`;
- cancela ganchos, wall cling, enemy interaction e brace;
- aplica knockback;
- ativa hitstun, i-frames e slowmo curto.

**Normal/Recovering → Dead**

- Recebe `DamageType.Lethal`; ou
- `failures + 1 >= maxFailures`;
- não há “segunda chance”: morte imediata;
- limpa timeScale, ganchos, input e física;
- agenda respawn.

**HitStun → Recovering**

- `hitStunDuration` em tempo real terminou;
- input volta, mas hooks continuam suspensos até o fim do hitstun/lock curto;
- i-frames continuam por `invulnerabilityDuration`.

**Recovering → Normal**

- `timeSinceLastHit >= recoveryTime` (ex.: 2,5 s) **ou**
- `safeGroundTime >= safeGroundRecovery` (ex.: 0,35 s);
- reseta `failures = 0`;
- limpa HUD de falhas.

**Dead → Respawning**

- `deathDelay` em tempo real (ex.: 0,6 s);
- pode tocar animação/VFX de morte;
- sem input, sem gancho, sem física de movimento.

**Respawning → Normal**

- `LevelController.RespawnPlayer()` reposiciona;
- `PlayerDamageController.ResetAfterRespawn()`:
  - failures = 0;
  - estado = Normal;
  - i-frames de spawn (ex.: 1 s);
  - hooks `ResetAirResources` + input liberado;
  - wall cling forçado a sair;
  - `TimeScaleService.ClearAll()`;
  - Rigidbody velocity zero, constraints/gravity restaurados.

## 4. Valores iniciais sugeridos

| Parâmetro | Valor |
|---|---:|
| `maxFailures` | 3 |
| `hitStunDuration` | 0,18 s |
| `invulnerabilityDuration` | 0,9 s |
| `hitSlowScale` | 0,35 |
| `hitSlowDuration` | 0,08 s real |
| `knockbackForce` | 12 |
| `recoveryTime` | 2,5 s |
| `safeGroundRecovery` | 0,35 s |
| `deathDelay` | 0,6 s |
| `spawnInvulnerability` | 1 s |

São valores de protótipo; devem ficar no Inspector.

## 5. Ordem de reset no hit

A ordem importa para não deixar joint, constraint ou input preso:

1. `PlayerDamageController` entra em `HitStun`/`Dead` e trava input.
2. `DualHookController.SetInputSuspended(true)`.
3. `DualHookController.ReleaseAll(DetachReason.Hit)`.
4. `WallClingController.ForceExit()` (método novo, público).
5. Se houver `EnemyBase`/QTE ativo, cancelar a interação:
   - `PlayerController2D.EndEnemyInteraction()`;
   - `EnemyBase.CancelInteraction()`.
6. Restaurar `gravityScale`, constraints e `rb.simulated` se algum sistema
   tiver mexido.
7. Aplicar knockback no `Rigidbody2D`:
   ```csharp
   Vector2 dir = ((Vector2)player.position - impactPoint).normalized;
   playerBody.linearVelocity = dir * knockbackForce;
   ```
   Se o impacto for no centro, usar `-velocity` ou `-facing`.
8. `TimeScaleService.Push("Hit", hitSlowScale, hitSlowDuration)`.
9. Tocar flash/camera shake; não alterar `Time.fixedDeltaTime`.

No respawn:

1. `TimeScaleService.ClearAll()`.
2. `DualHookController.SetInputSuspended(false)`.
3. `DualHookController.ResetAirResources()`.
4. `WallClingController.ForceExit()`.
5. `PlayerController2D` restaura controle/gravidade/constraints.
6. `LevelController` teleporta.
7. `PlayerDamageController.ResetAfterRespawn()`.

## 6. TimeScale sem bugs

Criar um **`TimeScaleService`** central. Ninguém deve escrever
`Time.timeScale` diretamente.

```csharp
public static class TimeScaleService
{
    public static void Push(string owner, float scale, float unscaledDuration = -1f);
    public static void Pop(string owner);
    public static void ClearAll();
    public static float Current { get; }
}
```

Regras:

- calcular o menor scale ativo (slow mais forte vence);
- timers em `Time.unscaledDeltaTime`;
- `duration < 0` = infinito (usado pelo amarelo);
- ao remover o último modificador, voltar a **1**;
- death/respawn chama `ClearAll()`;
- o amarelo deixa de escrever `Time.timeScale` e passa a usar
  `Push("Yellow", 0.25f)`/`Pop("Yellow")`;
- QTE continua usando `Time.unscaledTime`;
- nenhum sistema deve alterar `Time.fixedDeltaTime`;
- `OnDisable`/`OnDestroy` remove o próprio owner.

Exemplo de conflito:

```text
Yellow ativo: 0.25
Hit entra: 0.35 (menor = 0.25; hit não acelera o jogo)
Yellow termina: hit ainda ativo -> 0.35
Hit termina: 1
```

## 7. Física e gancho

- **Não** usar `rb.simulated = false` para hit/morte; usar um lock de controle
  (`PlayerController2D.SetControlEnabled(false)`) e manter o Rigidbody
  simulado.
- No hit, o knockback é uma velocidade definida **depois** de soltar os
  ganchos; o joint não pode puxar de volta no mesmo frame.
- No death, velocidade zero e controle desligado; não desativar o collider.
- No respawn, zerar velocidade e restaurar constraints/gravity antes de
  teleportar.
- Wall cling e enemy interaction não podem continuar ativos no frame do hit.
- `HookHand.ForceDetach` já notifica `IHookReceiver`; usar isso para cancelar
  o QTE/amarelo.
- Após o hit, `ResetAirResources` ao recuperar/respawnar para não deixar
  cooldown de gancho “preso”.

## 8. Perigos letais × não-letais

Migrar os perigos para um componente único, por exemplo `DamageOnContact`:

```csharp
public enum DamageType { NonLethal, Lethal }

public struct DamageInfo
{
    public DamageType type;
    public Vector2 sourcePoint;
    public float knockback;
    public GameObject source;
}
```

Sugestão de configuração:

| Perigo | Tipo |
|---|---|
| Abismo / KillZone | Lethal |
| Laser | Lethal |
| Kill zone marcada | Lethal |
| Espinhos | NonLethal |
| Projétil de torreta | NonLethal |
| Contato com inimigo | NonLethal |
| Falha de QTE | NonLethal (knockback) |

Todos chamam `PlayerDamageController.ApplyDamage(info)`, nunca
`LevelController.RespawnPlayer()` diretamente.

## 9. HUD e feedback

- Pips de falhas: 3 pontos; preenche a cada hit; zera na recuperação.
- Flash branco/vermelho no hit; flash de morte.
- Slowmo curto; sem congelar a UI.
- Texto opcional: `RECUPERANDO...` ou `ESTÁVEL`.
- Não mostrar “vida” se o sistema é contador de falhas.

## 10. Testes de aceite

1. Cair no abismo → morte imediata e respawn; contador zera.
2. Laser → morte imediata, mesmo com i-frames.
3. Espinho → hit, solta ganchos, knockback, slowmo curto, 1 falha.
4. Terceiro hit consecutivo → morte.
5. Ficar 2,5 s sem dano → contador zera.
6. Ficar 0,35 s em chão seguro → contador zera.
7. Hit durante QTE amarelo → cancela QTE, restaura `Time.timeScale`, solta
   hooks e não deixa input travado.
8. Hit durante wall cling → sai da parede, sem ficar preso.
9. Hit durante brace → solta os dois ganchos e sai do brace.
10. Morrer com dois ganchos → nenhum joint/constraint sobra.
11. Respawn após morte → input, ganchos, gravidade e timeScale normais.
12. Hits durante i-frames não incrementam falhas.
13. UI e timers continuam funcionando durante slowmo.

## 11. Prompt melhorado para a IA de implementação

Copie o bloco abaixo. Ele assume que a IA tem acesso ao projeto Unity.

```text
Atue como engenheiro Unity sênior no projeto Pindorama (Unity 6, 2D, física
Rigidbody2D, Input Manager legado).

CONTEXTO
O jogo é um platformer 2D de precisão com dois ganchos, swing boost, wall
cling/climb, caixas, laser, inimigos e QTE. Hoje NÃO existe vida: LaserHazard,
LethalHazard, EnemyProjectile, EnemyBase e KillZone chamam
LevelController.RespawnPlayer() diretamente. O inimigo amarelo escreve
Time.timeScale diretamente. O jogador tem PlayerController2D,
DualHookController, HookHand, WallClingController e PlayerCharacterVisual.

OBJETIVO
Implementar dano híbrido:
1. Perigos Lethal (abismo, laser, kill zone) matam imediatamente e resetam o
   trecho/fase.
2. Perigos NonLethal (espinhos, projétil, contato) causam Hit: soltam todos os
   ganchos, cancelam wall cling/enemy interaction/brace, aplicam knockback na
   direção oposta ao impacto, ativam hitstun curto e um slowmo de ~0.08 s real
   a 0.35x.
3. Cada hit não-letal incrementa um contador de falhas. Com 3 falhas
   consecutivas, o jogador morre.
4. O contador zera após 2.5 s sem dano OU 0.35 s em chão seguro.
5. Dano letal ignora i-frames; dano nonlethal é ignorado durante i-frames.

ARQUIVOS/CLASSES
Leia e integre com:
- PlayerController2D.cs
- DualHookController.cs
- HookHand.cs
- WallClingController.cs
- EnemyBase.cs
- LevelController.cs
- LaserHazard.cs / LethalHazard.cs / EnemyProjectile.cs
- HookHud.cs / PlayerCharacterVisual.cs
- GAME_RULES.md

IMPLEMENTE
- DamageType.cs / DamageInfo.cs
- PlayerDamageController.cs
- TimeScaleService.cs
- DamageOnContact.cs (ou equivalente)
- PlayerDamageHud.cs (pips + flash)
- Pequenas APIs públicas em WallClingController e EnemyBase para cancelar
  interação/forçar saída.

MÁQUINA DE ESTADOS
Normal -> HitStun: NonLethal, fora de i-frames, failures+1 < 3.
HitStun -> Recovering: fim do hitstun.
Recovering -> Normal: recoveryTime ou safe ground.
Normal/Recovering -> Dead: Lethal OU failures+1 >= 3.
Dead -> Respawning: deathDelay em tempo real.
Respawning -> Normal: LevelController reposiciona e reset central.

ORDEM DO HIT
1. trava controle e input;
2. DualHookController.SetInputSuspended(true);
3. DualHookController.ReleaseAll(DetachReason.Hit);
4. WallClingController.ForceExit();
5. cancela EnemyBase/QTE e EndEnemyInteraction();
6. restaura gravityScale/constraints;
7. rb.linearVelocity = direção oposta * knockback;
8. TimeScaleService.Push("Hit", 0.35f, 0.08f);
9. flash/camera shake.

TIMESCALE
- Ninguém escreve Time.timeScale diretamente.
- TimeScaleService com owners, menor scale ativo vence, timers em
  Time.unscaledDeltaTime, ClearAll no death/respawn.
- Amarelo usa Push("Yellow", 0.25f)/Pop("Yellow").
- QTE continua em tempo não escalado.
- Não alterar Time.fixedDeltaTime.

REGRAS
- Não usar rb.simulated=false em hit/morte.
- Não chamar LevelController.RespawnPlayer() diretamente de perigos.
- Não deixar joint, constraint, input suspenso ou timeScale alterado após
  hit/morte/respawn.
- Respawn reseta failures, i-frames, hooks, wall cling, enemy interaction,
  timeScale, constraints e HUD.
- Preservar tuning de movimento/gancho; não reescrever o jogo.
- Sem arte nova; usar placeholders de hurt/death no visual atual.

TESTES
Implemente ou descreva verificações para:
abismo, laser, espinho, projétil, contato, 3 hits, recuperação por tempo,
recuperação por chão, hit durante QTE amarelo, hit durante wall cling, hit
durante brace, morte com dois ganchos, respawn, i-frames, HUD e timescale.

ENTREGA
Código compilável, mudanças mínimas e explicadas, e um resumo dos testes.
```

## 12. Observações de integração

- O `EnemyBase` já usa `Time.timeScale` para o amarelo; esse é o ponto mais
  delicado. Migrar antes de adicionar o hit slowmo.
- `LevelController.RespawnPlayer()` deve deixar de ser chamado por perigos e
  passar a ser chamado apenas pelo `PlayerDamageController` e pelo reset `R`.
- O `PlayerCharacterVisual` atual usa placeholders; não é necessário criar
  hurt/death agora.
- O HUD existente pode ganhar os pips sem trocar a arte.
- A ordem de execução importa: o hit deve cancelar os sistemas antes de
  aplicar velocidade; o joint não pode reconciliar no mesmo frame.
