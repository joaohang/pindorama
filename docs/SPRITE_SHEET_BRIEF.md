# Pindorama — Sprite Sheet Brief

**Objetivo:** contexto completo para uma IA gerar o sprite sheet do personagem
jogável do Pindorama.  
**Estado atual:** visual 80×80 importado do projeto `jogoTeste`, com Idle/Run/
Jump/Hang. Wall cling/climb ainda usam placeholders estáticos.  
**Referência de design:** SANABI, sem copiar personagem, arte ou animações.

## 1. O jogo

Pindorama é um platformer 2D de precisão e momentum, em pixel art cyberpunk
industrial, com:

- dois ganchos coloridos: **vermelho (LMB)** e **azul (RMB)**;
- swing com física, corda, boost tangencial;
- wall cling e wall climb;
- caixas puxáveis, puzzle, laser e bola móvel;
- inimigos amarelos (estilingue) e QTE azul/vermelho.

O personagem precisa ser legível em movimento rápido, com silhueta clara e
poses exageradas. A corda e os VFX são desenhados pelo motor; **não** devem ser
baked no sprite.

## 2. Personagem atual

Referência visual para anexar ao prompt:
[`images/player_current_reference.png`](images/player_current_reference.png).

- Canvas: **80×80 px**;
- PPU: **100**;
- personagem visível: ~**23×58 px** no Idle;
- direção base: **olhando para a direita** (o jogo espelha em X);
- paleta atual (amostrada do Idle1):

| Uso | Hex |
|---|---|
| Contorno/quase preto | `#050912` |
| Cabelo azul escuro | `#2C47BE` / `#424187` |
| Cabelo azul claro | `#4A8CD8` |
| Pele | `#B15C51` / `#A096D1` |
| Branco/creme | `#FCFCFC` |
| Calça vermelho-escura | `#750C18` / `#872F3B` |
| Laranja/vermelho vivo | `#EC5A0C` / `#C32210` |

Manter a identidade: cabelo azul, roupa vermelha/laranja, detalhes claros e
pequenos acentos neon. Não redesenhar o personagem como outra pessoa.

## 3. Análise do SANABI

Referências públicas consultadas:

- [SANABI Controls Guide — gemelog](https://gemelog.jp/en/games/sanabi/basics/)
- [SANABI Walkthrough — gemelog](https://gemelog.jp/en/games/sanabi/walkthrough/)
- [Famitsu — matéria sobre SANABI](https://www.famitsu.com/news/202301/16289191.html)

O SANABI tem, entre outras coisas:

- corrida, pulo, queda;
- **wall climb** para cima/baixo com `W/S`;
- **wall jump**;
- chain/grappling hook com mira, attach e swing;
- **dash enquanto preso** na chain;
- **pull** para âncora/teto;
- **charged dash**;
- agarrar e arremessar inimigos;
- espada/combate;
- hurt/ morte.

O Pindorama compartilha a fantasia de mobilidade vertical e gancho, mas tem
regras próprias:

- **dois** ganchos, não um;
- swing boost no lugar de dash/charged dash;
- wall cling/climb;
- inimigos QTE/estilingue;
- caixas/puzzle;
- sem espada, sem charged dash, sem controle de robô, sem veículo.

## 4. O que já existe × o que falta

### Já existe no sprite 80×80

| Animação | Frames |
|---|---:|
| Idle | 4 |
| Run | 8 |
| Jump | 7 |
| Hang (gancho) | 4 |

### Faltando para o Pindorama

Prioridade **P0** = necessário para a jogabilidade atual.

| Animação | Frames ideais | Prioridade |
|---|---:|---|
| Wall cling idle | 2 | P0 |
| Wall climb up | 6 | P0 |
| Wall climb down | 6 | P0 |
| Wall jump | 3 | P0 |
| Fall loop | 2 | P0 |
| Land | 2 | P0 |
| Hook aim forward | 1 | P0 |
| Hook aim up | 1 | P0 |
| Hook aim down | 1 | P0 |
| Hook fire | 2 | P0 |
| Hook attached — single | 2 | P0 |
| Hook attached — dual | 2 | P0 |
| Brace (dois ganchos) | 2 | P0 |
| Swing boost | 2 | P0 |
| Pull (puxável) | 2 | P0 |
| Hurt | 2 | P0 |
| Death | 4 | P0 |
| Respawn | 2 | P0 |

### P1 — inimigos e interações

| Animação | Frames | Prioridade |
|---|---:|---|
| Yellow: grudar | 1 | P1 |
| Yellow: mirar | 2 | P1 |
| Yellow: lançar | 2 | P1 |
| QTE: struggle | 2 | P1 |
| QTE: success | 1 | P1 |
| Pull-to-enemy / impacto | 2 | P1 |

### P2 — polimento

- turn/skid: 2;
- idle look up/down: 2;
- ledge grab: 2 (mecânica ainda não existe);
- ceiling movement: 2 (mecânica ainda não existe);
- dash: 2 (mecânica ainda não existe).

### O que **não** pedir agora

- espada/ataque corpo a corpo;
- charged dash;
- controle de robô;
- veículo;
- natação;
- escalada de escada;
- animações de SANABI copiadas.

## 5. Especificação técnica do sheet

- Cada célula: **80×80 px**;
- padding entre células: **2 px transparentes**;
- fundo transparente;
- personagem **facing right** em todos os frames;
- pivô consistente: **center (0.5, 0.5)**;
- pés alinhados ao mesmo baseline (parte inferior do corpo, ~4 px acima da
  borda inferior);
- mesma escala, mesma paleta, mesma espessura de linha;
- sem corda, sem VFX, sem sombra, sem cenário, sem UI, sem texto;
- sem motion blur;
- 1 sprite por célula, sem frames duplicados;
- animações em linhas separadas ou arquivos separados;
- taxa: 8–12 fps; run 12 fps; wall 8 fps; hook 10 fps.

### Formato de entrega

Preferência: **um PNG por grupo**, não um único sheet gigante:

```text
player_locomotion.png    idle, run, jump, fall, land, turn
player_wall.png          wall cling, climb up/down, wall jump
player_hook.png          aim, fire, attached, dual, brace, boost, pull
player_combat.png        hurt, death, respawn, yellow, QTE
```

Cada sheet com **8 colunas**, células 80×80 + 2 px de gutter.  
Entregar também:

```text
manifest.csv
```

com colunas:

```text
animation,frame,column,row,fps,loop,pivot_x,pivot_y,notes
```

### Muzzle/hand markers

Se a IA conseguir, informar no manifest a posição aproximada da mão direita e
esquerda em cada frame de hook, em pixels locais (0–80). Isso ajuda a alinhar a
corda do Pindorama, que é desenhada pelo motor.

## 6. Regras de pose

### Locomoção

- Idle: respiração sutil, arma/hook device abaixado.
- Run: passada ampla, cabelo/capa com atraso, silhueta legível.
- Jump: antecipação, subida, apex, queda; não usar a mesma pose para tudo.
- Fall: braços abertos, pernas separadas, silhueta de “queda”.
- Land: crouch curto com absorção; 2 frames.
- Turn: inclinação lateral, pé de pivô.

### Parede

- Wall cling: corpo de lado, quadril perto, **2 mãos + 2 pés**.
- Climb up: alternância diagonal (mão oposta ao pé), 6 frames.
- Climb down: joelhos mais dobrados, movimento controlado.
- Wall jump: 3 frames — compressão, impulso, saída.

### Gancho

- Aim forward/up/down: braço da mão correspondente estendido; o outro braço
  equilibra.
- Fire: recuo curto do braço, corpo inclina levemente.
- Attached single: corpo pendulado, braço firme, pés para trás.
- Attached dual: dois braços abertos, peito exposto, corpo em Y.
- Brace: corpo compacto, dois braços puxando, pés firmes.
- Boost: corpo estendido na tangente, cabelo/capa arrastando.
- Pull: corpo inclina na direção do objeto, braços puxando.

### Dano

- Hurt: recuo, cabeça/costas, 2 frames; flash é VFX, não sprite.
- Death: 4 frames de colapso.
- Respawn: 2 frames de recuperação.

### Inimigos

- Yellow: postura de “grudado”, mira com mouse e lançamento.
- QTE: corpo tensionado, braço preso, reação de sucesso.

## 7. Prompt principal (inglês, para gerador de imagem)

Use este bloco como prompt mestre. Anexe os sprites atuais de Idle/Run/Jump/Hang
como referência de personagem.

```text
You are a senior 2D pixel-art animator creating an original playable-character
sprite sheet for a precision platformer.

PROJECT
Pindorama is a fast 2D momentum platformer inspired by SANABI, with two
color-coded grappling hooks (red/blue), swinging, wall cling/climb, pullable
boxes, and hook-based enemy interactions. Do not copy SANABI characters, art,
logos, or exact animations.

CHARACTER
Use the attached 80x80 sprites as the exact character reference: blue hair,
red/orange pants, pale face, dark cyberpunk outfit, small neon accents. Keep
the same identity, proportions, palette, and line weight. Do not redesign the
character.

CANVAS AND FORMAT
Each frame is exactly 80x80 px on a transparent background. Character faces
RIGHT in every frame (the game mirrors X for left). Center pivot at 0.5,0.5.
Keep the feet on a consistent baseline about 4 px above the bottom edge. Use
2 px transparent padding between cells. Do not draw ropes, VFX, shadows,
backgrounds, UI, text, labels, or grid lines.

STYLE
Crisp pixel art, 1-pixel-ish dark outline where appropriate, limited palette,
readable silhouette, no anti-aliasing outside the silhouette, no motion blur.
Palette anchors: #050912, #2C47BE, #4A8CD8, #B15C51, #A096D1, #FCFCFC,
#750C18, #872F3B, #EC5A0C, #C32210.

DELIVERABLE
Produce separate sprite sheets, each 8 columns wide, with 80x80 cells:
1. player_locomotion.png: idle 4, run 8, jump 8, fall 2, land 2, turn 2.
2. player_wall.png: wall cling 2, wall climb up 6, wall climb down 6,
   wall jump 3.
3. player_hook.png: aim forward 1, aim up 1, aim down 1, fire 2,
   attached single 2, attached dual 2, brace 2, swing boost 2, pull 2.
4. player_combat.png: hurt 2, death 4, respawn 2, yellow stick 1,
   yellow aim 2, yellow launch 2, QTE struggle 2, QTE success 1.
Also provide manifest.csv with animation, frame, column, row, fps, loop,
pivot_x, pivot_y, and notes.

POSE RULES
- Wall cling: side-on, hips close, two hands and two feet in contact.
- Wall climb: diagonal limb alternation, 6 frames per direction.
- Wall jump: compress, push off, release; body away from wall.
- Hook aim: extend the corresponding arm forward/up/down; other arm balances.
- Hook fire: short recoil, torso lean.
- Single hook attached: body pendulum, one arm taut, legs trailing.
- Dual hook attached: both arms out, chest open, body in a Y shape.
- Brace: compact body, both arms pulling, feet planted.
- Swing boost: stretched body along the tangent, hair/cloth trailing.
- Pull: torso leaning toward the pulled object.
- Hurt: recoil and head snap; no flash baked in.
- Death: 4-stage collapse.
- Yellow: stuck, aim, launch.
- QTE: tension, struggle, success.

CONSISTENCY
Keep the same character in every cell. No duplicate idle frames. No extra
characters. No perspective or isometric view. No rope. No VFX. No text.
Animation must loop cleanly where marked loop.
```

## 8. Batch prompts

Image generators lose consistency with 70+ frames. Gere em quatro lotes e use
o primeiro como referência dos próximos.

### Lote A — locomoção

```text
Create player_locomotion.png for the attached 80x80 pixel-art character:
idle 4 frames, run 8 frames, jump 8 frames (anticipation, rise, apex, fall),
fall 2 frames, land 2 frames, turn 2 frames. 8 columns, 80x80 cells, 2 px
transparent padding, transparent background, facing right, center pivot,
consistent feet baseline. No ropes, VFX, background, text, or UI.
Match the attached character exactly.
```

### Lote B — parede

```text
Create player_wall.png for the attached 80x80 pixel-art character:
wall cling 2 frames, wall climb up 6 frames, wall climb down 6 frames,
wall jump 3 frames. Side-on wall poses with two hands and two feet in contact,
diagonal limb alternation, hips close to the wall. 8 columns, 80x80 cells,
2 px padding, transparent background, facing right. No wall drawn, no rope,
no VFX, no text.
```

### Lote C — gancho

```text
Create player_hook.png for the attached 80x80 pixel-art character:
aim forward 1, aim up 1, aim down 1, fire 2, attached single 2, attached dual 2,
brace 2, swing boost 2, pull 2. Keep both wrist hook devices visible but do not
draw ropes. Single-hook poses in pendulum lean; dual-hook pose with both arms
open; brace compact; boost stretched. 8 columns, 80x80 cells, 2 px padding,
transparent background, facing right.
```

### Lote D — dano e inimigos

```text
Create player_combat.png for the attached 80x80 pixel-art character:
hurt 2, death 4, respawn 2, yellow-enemy stick 1, yellow aim 2,
yellow launch 2, QTE struggle 2, QTE success 1. Expressive silhouettes,
no blood, no gore, no baked VFX. 8 columns, 80x80 cells, 2 px padding,
transparent background, facing right. Match the attached character exactly.
```

## 9. Negative prompt

```text
no rope, no grappling line, no VFX, no glow, no speed lines, no background,
no scenery, no UI, no text, no labels, no grid lines, no shadow, no floor,
no extra characters, no SANABI character, no SANABI logo, no copied assets,
no isometric, no top-down, no 3D, no photorealism, no blurry pixels,
no anti-aliasing outside the silhouette, no motion blur, no inconsistent
proportions, no palette shift, no different outfit, no different hair,
no duplicate frames, no missing limbs, no extra limbs
```

## 10. Critérios de aceite

- [ ] 80×80 por frame, transparente.
- [ ] personagem olhando para a direita.
- [ ] pés no mesmo baseline.
- [ ] mesma identidade/paleta do personagem atual.
- [ ] wall cling com 4 contatos.
- [ ] climb up/down com 6 frames e alternância diagonal.
- [ ] wall jump com 3 fases.
- [ ] hook aim/fire/attach/dual/brace/boost/pull legíveis.
- [ ] hurt/death sem VFX baked.
- [ ] sem corda, sem fundo, sem texto.
- [ ] manifest.csv consistente.
- [ ] loops limpos.
