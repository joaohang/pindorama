# Pindorama — protótipo de ganchos (Unity)

Protótipo jogável das mecânicas da especificação v0.3, montado em código
(`GameBootstrap`). Não há arte final: tudo é graybox com sprites sólidos.

## Como abrir e jogar

1. Abra o **Unity Hub** → **Add project from disk** → selecione esta pasta
   (`pindorama`). Use **Unity 6000.6.6f1** (Unity 6).
2. Abra a cena **`Assets/_Project/Scenes/Main.unity`**.
3. Aperte **Play**.

A cena já está no Build Settings. Tudo (chão, paredes, âncoras, blocos, jogador,
câmera, HUD) é criado em tempo de execução pelo `GameBootstrap`.

## Controles

| Ação | Teclado/mouse | Gamepad |
|---|---|---|
| Mover | `A` / `D` (ou setas) | stick esquerdo |
| Mirar | mouse | — (mouse) |
| Gancho vermelho | botão esquerdo | `LB` |
| Gancho azul | botão direito | `RB` |
| Pular / soltar ganchos | `Espaço` | `A` |
| Soltar sem pular | `Ctrl` esquerdo | `B` |
| Reset da sala | `R` | — |

## Mecânicas testáveis na sala

- **Corredor de swing:** várias âncoras no teto (barras largas de 2,5 u,
  vermelhas/azuis/neutras) ao longo da sala, mais duas laterais. O gancho
  prende, dá um **puxão inicial limitado** na direção da âncora e permite
  balançar; A/D acelera o pêndulo.
- **Segure para ficar preso:** em âncora de swing, o gancho só continua preso
  enquanto o botão daquela mão estiver **segurado**. Soltar o botão solta o
  gancho na hora, preservando o momentum.
- **Suspensão heroica:** enquanto há um gancho de swing ativo, a gravidade cai
  de 4,5 para 3,4. No brace (2 ganchos) ela vai a zero.
- **Air control:** A/D continuam acelerando no ar e aplicam força tangencial no
  pêndulo; o limite de velocidade no gancho é 26 u/s.
- **Salto de gancho:** pendurado, `Espaço` solta a corda e dá um **impulso
  vertical** (preservando o momentum horizontal), como um pulo de balanço.
- **Alcance limitado (8 u):** a âncora alta (dourada, no alto) fica fora do
  alcance parado — é preciso pular para o gancho alcançar.
- **Corda com limite de esticamento** e colisão com o chão: se a corda estica
  demais, a velocidade para fora é cortada; se o jogador ainda encosta no chão,
  o collider continua segurando (não atravessa).
- **Disparo espelhado:** mire para a direita e aperte LMB+RMB juntos — vermelho
  vai para a direita, azul para a esquerda.
- **Puxável (bloco vermelho/azul):** com **um** gancho preso, o jogador **não
  trava**: continua andando/caindo normalmente. Cada aperto no botão daquela mão
  arrasta o bloco um passo; o botão **nunca solta**. `Espaço`/`Ctrl` solta.
- **Braço (2 ganchos):** quando os **dois** ganchos estão presos, o jogador para
  no ar/chão (gravidade suspensa). É o estado estável para mirar e puxar.
  Soltar um dos ganchos devolve o movimento.
- **Puxão espelhado:** com as duas mãos em puxáveis, aperte os dois botões.
- **Queda:** cair no fundo faz reset. `R` reseta a sala.

## Notas técnicas

- O protótipo usa o **Input Manager legado** (`UnityEngine.Input`) para abrir e
  jogar sem instalar pacotes. A especificação pede o **Input System** para o
  jogo final; a troca é só na camada de input (`PlayerController2D` e
  `DualHookController`).
- A câmera é um follow simples (`CameraFollow`), substituível por Cinemachine.
- O HUD é `OnGUI` (sem TextMeshPro) para não exigir pacotes extras.
- Compilação validada em **Unity 6000.6.6f1** (batch mode, sem erros).
