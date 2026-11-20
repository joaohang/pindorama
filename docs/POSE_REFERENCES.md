# Poses faltantes — análise e conceitos

**Status:** revisão, **nada aplicado no jogo**  
**Data:** 2026-11-20

## Objetivo

Validar a leitura de pose e a silhueta das animações que o personagem 80×80
ainda não tem:

- wall cling;
- wall climb para cima/baixo;
- wall jump;
- fall;
- land;
- hurt;
- boost;
- pose de dois ganchos.

A imagem de revisão está em
[`images/pose_concepts/missing_poses_concept.png`](images/pose_concepts/missing_poses_concept.png).

## O que o sprite atual oferece

O pacote tem 23 sprites 80×80:

| Estado | Frames | Uso |
|---|---:|---|
| Idle | 4 | parado |
| Run | 8 | corrida |
| Jump | 7 | subida/queda/aterrissagem genérica |
| Hang | 4 | gancho: idle, saque, tiro, preso |

Observações:

- `Hang1` é uma pose vertical com o braço/corpo mais neutro.
- `Hang2` e `Hang3` têm braço estendido para o gancho.
- `Hang4` é a pose presa.
- `Jump1` é um crouch — bom ponto de partida para **land**.
- `Jump6` abre braços/pernas — bom ponto de partida para **fall**.
- `Jump3` tem o corpo inclinado — base para **hurt**.
- Não existe contato de mão/pé em parede; as poses Hang são de corda.

## Referências públicas analisadas

Não usei os arquivos dessas referências; usei apenas os princípios de pose.

1. **Ruby Wall Slide and Hurt Sprite Sheet** — Summer Engine  
   <https://www.summerengine.com/asset-store/ruby-wall-slide-and-hurt-sprite-sheet-6859a93e>  
   Lição: wall slide/hurt funcionam com silhueta de costas para a parede,
   quadril próximo, joelhos dobrados e recuo claro no hurt.

2. **Pixel Climber Girl** — AI Spritesheet Maker  
   <https://www.aispritesheetmaker.com/gallery/fe005880-455f-4304-8b75-8d13a3d87ca9>  
   Lição: ciclo de escalada com alternância diagonal (mão oposta ao pé),
   corpo compacto e leitura em 8 fps.

3. **Super Pixel Heroes / CraftPix** — referências gerais de plataforma  
   <https://craftpix.net/freebies/free-pixel-art-prototype-character-sprites>  
   Lição: poses de hurt/land precisam de exagero de silhueta para serem lidas
   em poucos frames.

> Essas páginas são referências de estudo, não assets para copiar. A licença
> de cada uma é diferente.

## Princípios de pose

### Wall cling

- Torso de lado, quase encostado na parede.
- **Duas mãos** e **dois pés** como pontos de contato.
- Quadril perto da parede, não pendurado para trás.
- Cabeça virada para a direção do movimento.
- Silhueta deve funcionar sem a corda.

### Wall climb up

- Alternância diagonal: mão direita + pé esquerdo, depois o inverso.
- Joelhos dobrados, quadril perto.
- 4–6 frames por ciclo.
- O corpo sobe, mas a cabeça continua legível.

### Wall climb down

- Controle excêntrico: joelhos mais dobrados, movimento mais lento.
- Mãos descem primeiro; pés procuram o próximo apoio.
- 4 frames são suficientes para um protótipo.

### Wall jump

- 2–3 frames: compressão, impulso, saída.
- Braços para trás no impulso e depois abertos.
- A silhueta precisa comunicar “para longe da parede”.

### Fall

- Silhueta aberta, braços acima/afastados.
- Pernas separadas; nada de pose compacta.
- Pode ser um loop de 2 frames + linhas de velocidade.

### Land

- Crouch com absorção; braços para fora.
- 1–2 frames.
- `Jump1` já serve como placeholder.

### Hurt

- Recuo do torso, cabeça para trás, braços reativos.
- Flash branco/vermelho + partículas.
- 2 frames bastam para o protótipo.

### Boost

- Corpo estendido na tangente; braço da corda firme.
- VFX ciano + speed lines.
- `Hang4` serve como base.

### Dois ganchos

- Braços abertos, peito exposto, duas cordas.
- Hang4 mostra só um gancho; a pose definitiva precisa de arte nova.
- Para o protótipo: Hang4 + segunda corda.

## Proposta de frames

| Estado | Frames | Prioridade |
|---|---:|---|
| Wall cling idle | 2 | alta |
| Wall climb up | 4 | alta |
| Wall climb down | 4 | alta |
| Wall jump | 2 | alta |
| Fall loop | 2 | média |
| Land | 1 | média |
| Hurt | 2 | média |
| Boost | 1 | baixa |
| Dual hook | 2 | baixa |

## O que a imagem mostra

A folha `missing_poses_concept.png` usa os sprites atuais como base e desenha
guias de contato/seta para validar:

- posição do corpo em relação à parede;
- quantidade de contatos;
- direção do movimento;
- silhueta de hurt/fall/land;
- leitura da corda dupla.

Ela **não é uma nova arte**: é um mock de pose derivado do personagem atual.
Nada foi importado no prefab nem no Animator.

## Próximo passo

Se a direção estiver aprovada:

1. desenhar/gerar os frames definitivos no mesmo estilo 80×80;
2. criar clips `WallCling`, `WallClimbUp`, `WallClimbDown`, `WallJump`, `Fall`,
   `Land`, `Hurt`;
3. trocar os placeholders do `PlayerCharacterVisual`;
4. testar em `Level02` e `EnemyLab`.

Se preferir, o próximo passo pode ser só **wall cling + climb**, que são os
mais importantes para a jogabilidade atual.
