# Pindorama — prompt do sprite sheet do protagonista

Prompt único para gerar o **sprite sheet jogável** do protagonista.  
Não é uma folha de turnaround/design: são frames de animação em células 80×80.

## Prompt

```text
Create a complete 2D pixel-art SPRITE SHEET for the playable protagonist of
"Pindorama", a cyberpunk precision-platformer. This is a gameplay sprite sheet,
NOT a character turnaround, NOT a concept-art sheet, and NOT a single
illustration. Every cell must be one animation frame.

CHARACTER
A fictional Black Brazilian cyborg in his late 50s, loosely inspired by the
historical revolutionary Carlos Marighella, but fictionalized: no literal
portrait, no real-person likeness, no copied logo or existing game asset.
He is a prisoner of an interdimensional techno-dictatorship: lined face,
short receding gray hair, thick gray mustache, tired but defiant eyes, old
scars, one organic eye and one cracked amber optical implant. Prison-grade
implants replace parts of his spine, neck, and both forearms, with visible
flesh-to-metal seams, cable ports, serial stamps, and improvised repairs.
He wears a torn orange prison jumpsuit with a faded inmate number, a scavenged
dark ballistic vest, heavy boots, a broken ankle shackle, a cybernetic collar
with faint portal glyphs, and a holographic ID tag. The silhouette must read
as a prisoner-cyborg, not a generic chrome android.

NO HANDS — THIS IS CRITICAL
The character has NO HANDS. Do not draw fingers, palms, wrists, or human hands.
Both arms end in mechanical weapon sockets. The two weapons are permanently
integrated into the forearms and must be visible in every frame.

LEFT ARM — RED CHAIN-SCYTHE
A curved war-scythe blade mounted on a mechanical forearm socket. Worn red
painted metal, dark steel edge, exposed chain links, a compact cable spool,
bolts, and a short tether. It is a weapon, not a farming scythe: aggressive,
hooked, readable, and able to catch/pull like a grappling blade. No hand holds
it; the weapon grows out of the arm.

RIGHT ARM — CYAN MAGNETIC HAMMER
A heavy cybernetic forearm ending in a compact magnetic hammer head with copper
coils, cyan-electric energy, yellow-and-black warning stripes, pistons, and a
retractable chain. The head is bulky and asymmetric. No hand holds it; the
hammer is the end of the arm. It can clamp to metal and deliver a magnetic slam.

The two weapons must be instantly distinguishable in silhouette: a thin,
curved red scythe on one side; a heavy, cyan, rectangular hammer on the other.
Do not give him a gun, sword, normal hook, or human hands.

CANVAS AND FORMAT
Each frame is exactly 80x80 px on a transparent background.
Character faces RIGHT in every frame; the game mirrors X for left.
Center pivot at 0.5,0.5.
Keep the feet on a consistent baseline about 4 px above the bottom edge.
Use 2 px transparent padding between cells.
No ropes, long chains, VFX, shadows, backgrounds, UI, text, labels, or grid
lines. The scythe/hammer may have only their short integrated chain mounts;
the long grappling cable is drawn by the game engine.

STYLE
Crisp 2D pixel art, sharp pixel clusters, readable silhouette, limited palette,
slightly exaggerated weapons and forearms, no anti-aliasing outside the
silhouette, no motion blur, no 3D, no photorealism.
Palette anchors: prison orange #C65A2E/#8E3A1F; rust; gunmetal #2A3440;
near-black #050912; bone white #FCFCFC; deep red #750C18/#C32210; electric cyan
#2BD9C4/#4A8CD8; warning yellow #FFD24A. Use neon only as accent.

SPRITE SHEET CONTENT
Produce four separate PNG sheets, each 8 columns wide, with 80x80 cells:

1. player_locomotion.png
   idle 4, run 8, jump 8 (anticipation/rise/apex/fall), fall 2, land 2, turn 2.

2. player_wall.png
   wall cling 2, wall climb up 6, wall climb down 6, wall jump 3.

3. player_weapons.png
   scythe aim forward 1, scythe aim up 1, scythe aim down 1, scythe fire 2,
   scythe attached 2, hammer aim 2, hammer slam 3, hammer attached 2,
   dual-weapon stance 2, brace 2, swing boost 2, pull 2.

4. player_combat.png
   hurt 2, death 4, respawn 2, yellow-enemy stick 1, yellow aim 2,
   yellow launch 2, QTE struggle 2, QTE success 1.

If the generator cannot produce all four, produce them in this order:
locomotion, wall, weapons, combat. Use the first sheet as the exact character
reference for the next ones.

POSE RULES
- Run: wide stride, scythe trailing, hammer counterbalancing.
- Jump: anticipation, rise, apex, fall; weapons follow the body.
- Wall cling: side-on, hips close, two feet and the two weapon forearms in
  contact; no hands.
- Wall climb: diagonal limb alternation, 6 frames per direction.
- Wall jump: compress, push off, release; body away from the wall.
- Scythe aim: curve the blade toward the target; no hand, no wrist.
- Scythe fire: arm recoil, blade/chain extended.
- Hammer slam: heavy overhead arc, cyan coil flare, body braced.
- Dual stance: scythe and hammer both visible, chest open.
- Brace: compact body, both weapon arms pulling, feet planted.
- Swing boost: stretched body along the tangent, weapons trailing.
- Pull: torso leaning toward the pulled object.
- Hurt: recoil and head snap; no blood, no baked flash.
- Death: 4-stage collapse.
- QTE/yellow: tension, struggle, launch.

CONSISTENCY
Same character, same outfit, same two weapons in every cell.
No extra arms, no missing arms, no human hands, no swords, no guns.
No duplicate frames. No labels inside the image.
Deliver a manifest.csv with:
animation, frame, column, row, fps, loop, pivot_x, pivot_y, notes.
```

## Resumo em português

- É um **sprite sheet jogável**, não uma folha de design/turnaround.
- O personagem é um **ciborgue prisioneiro**, inspirado em Carlos Marighella,
  mas fictício e sem likeness literal.
- **Não tem mãos**: os dois antebraços terminam em armas integradas.
- **Braço esquerdo:** foice de guerra vermelha, com corrente/cabo curto.
- **Braço direito:** martelo magnético ciano, pesado, com bobinas e pistões.
- Sem arma de fogo, sem espada, sem gancho comum, sem mãos humanas.
- Frames 80×80, fundo transparente, olhando para a direita, pivô central,
  pés no mesmo baseline, 2 px de padding.
- Quatro sheets: locomoção, parede, armas e combate.
- Sem corda longa, VFX, texto, fundo ou UI.

## Observação

A foice ocupa o papel de arma/gancho convencional do braço esquerdo; o martelo
magnético ocupa o braço direito. Se você quiser inverter as cores/lados, é só
trocar “left/right” no prompt.
