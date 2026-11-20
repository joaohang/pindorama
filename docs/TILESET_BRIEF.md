# Pindorama — Brief de Tileset

**Status:** contexto para outra IA / artista  
**Versão:** 0.1  
**Objetivo:** servir como briefing completo para uma IA montar o tileset do
Pindorama sem quebrar a escala, a legibilidade ou as regras de colisão do jogo.

> Use este documento como contexto. Se for gerar imagem, use a seção
> “Prompt pronto” no final. Se for montar na Unity, use as seções técnicas.

## 1. O jogo em uma frase

Pindorama é um platformer 2D de precisão e momentum, inspirado em Sanabi, com
dois ganchos coloridos (vermelho e azul), swing, wall cling e combate por
gancho em um mundo industrial/cyberpunk escuro.

## 2. Como o jogo é visto

- Câmera **ortográfica 2D**, side-scroller.
- `orthographicSize = 9.5`.
- Tela de referência: **16:9 / 1920×1080**.
- Área visível aproximada: **33,8 × 19 unidades**.
- Sem perspectiva isométrica, sem top-down, sem 3D.
- O tileset é **frontal**, feito para ser montado em Tilemap.

## 3. Escala e grid

| Item | Valor |
|---|---:|
| Tamanho do tile | **32 × 32 px** |
| 1 unidade Unity | **1 tile = 32 px** |
| Pixels por unidade (PPU) | **32** |
| Jogador | ~0,8 × 1,8 unidades (~26 × 58 px) |
| Plataforma padrão | 1 unidade de espessura (~32 px) |
| Parede vertical | 1 unidade de largura |
| Corredor típico | 4–8 unidades |
| Altura de parede | 8–16 unidades |

**Regra de ouro:** não mude a escala. Um tile = 1 unidade. O jogador precisa
caber em corredores de 3–4 tiles e ser maior que um tile de altura.

## 4. Direção de arte

### Referência de clima

- Megastrutura industrial/cyberpunk;
- aço escuro, concreto, painéis, cabos, grades, ventiladores;
- neon contido em ciano, vermelho, azul e amarelo;
- sensação de lugar hostil, antigo e tecnológico;
- **inspirado em Sanabi, mas original** — não copiar assets, logo ou layouts.

### Regras de leitura

1. **Legibilidade acima de detalhe.** O jogador, os ganchos e os perigos
   precisam ser lidos em 0,2 s.
2. **Superfície caminhável clara:** a borda de cima do chão/plataforma deve
   ter um rim light de 1–2 px.
3. **Parede escalável = teal:** lista vertical/chevrons/brilho ciano.
4. **Parede comum = sem teal:** aço/cinza, sem listra de escalada.
5. **Perigo = vermelho:** espinhos, laser, listras de alerta.
6. **Fundo com baixo contraste:** não competir com o player.
7. **Nada de ruído atrás da corda do gancho:** manter áreas limpas.
8. **Daltonismo:** não depender só de cor; usar padrão (chevrons teal vs.
   listras diagonais vermelhas).

### Paleta base

| Uso | Hex |
|---|---|
| Fundo profundo | `#0B0F14` / `#121A22` |
| Aço escuro | `#2A3440` |
| Aço médio | `#3E4C5A` |
| Aço claro / rim | `#6E7F8C` |
| Concreto | `#4A5563` |
| Teal escalável | `#2BD9C4` |
| Teal glow | `#69FFF0` |
| Vermelho gancho/perigo | `#FF4D5A` |
| Vermelho escuro | `#B32435` |
| Azul gancho | `#4DA6FF` |
| Azul escuro | `#1E5FA8` |
| Amarelo bola | `#FFD24A` |
| Amarelo escuro | `#B8860B` |
| Branco/creme | `#E6F1F2` |

Usar **5–7 valores por material**. Evitar gradientes fotográficos; o jogo é
graybox com estética neon.

## 5. Famílias de tiles

### 5.1 Estrutura comum (não escalável)

Chão, plataformas, teto, paredes comuns.

- Base: aço escuro/concreto.
- Topo: rim light claro.
- Faces: painéis, parafusos, rachaduras, faixas de advertência.
- **Sem teal**.
- Layer Unity: `Default`.

### 5.2 Estrutura escalável

Mesma geometria da comum, mas com assinatura visual teal.

- Borda/chevrons/trilho vertical teal;
- brilho suave;
- não exagerar no brilho para não ofuscar o player;
- **Layer Unity: `ClimbableWall`**;
- **Tilemap separado** do comum (layer é por GameObject, não por tile).

### 5.3 Perigos

- Espinhos;
- emissores de laser;
- faixas de alerta;
- kill zone;
- **Trigger**, não colisão sólida.

### 5.4 Fundo

- Painéis distantes, tubulações, cabos, estruturas;
- dessaturado, baixo contraste;
- **sem collider**.

### 5.5 Props e decals

Separados do tilemap de colisão:

- canos, grades, ventiladores, monitores, luzes, cartazes, cabos, antenas;
- fumaça/vapor (opcional, com moderação);
- folhas de decalque transparentes.

### 5.6 Objetos do jogo (não fazem parte do tileset)

Estes já existem como objetos separados e **não** devem ser desenhados dentro
dos tiles:

- âncoras de gancho (vermelhas/azuis/neutras);
- Bola Amarela móvel;
- caixas vermelhas/azuis;
- laser;
- jogador;
- inimigos.

O tileset pode ter **suportes/encaixes visuais** para esses objetos, mas sem
substituí-los.

## 6. Conjunto mínimo de tiles

Para cada família de estrutura (comum e escalável), entregar:

### Borda / autotiling

- `single`
- `center`
- `top`
- `bottom`
- `left`
- `right`
- `top_left`
- `top_right`
- `bottom_left`
- `bottom_right`
- `inner_top_left`
- `inner_top_right`
- `inner_bottom_left`
- `inner_bottom_right`

### Variações

- 2–3 variações de `center` com detalhes diferentes;
- 1 variação “quebrada/danificada”;
- 1 variação com faixa de advertência;
- 1 pilar vertical;
- 1 plataforma fina (visual; colisão continua sólida).

### Autotiling

Preferência: **47-tile blob** para Rule Tile.

Fallback aceitável: **16-tile bitmask** (N/E/S/W) + 4 inner corners como
overlays.

Se a IA não conseguir gerar 47 tiles coerentes, entregar o conjunto de 14
tiles acima + overlays de canto. É melhor um conjunto pequeno e coerente do
que 47 tiles inconsistentes.

## 7. Especificação técnica da imagem

- Formato: **PNG RGBA**, fundo transparente.
- Tile: **32 × 32 px**.
- Atlas: grid com **padding de 1 px** entre tiles e **1 px de borda**.
- Sem anti-aliasing fora da silhueta (pixel art).
- Sem texto, UI, logo ou marca d'água.
- Sem perspectiva, sem isométrico.
- Sem sombras fortes que atravessem a borda do tile.
- Luz consistente vindo de **cima/esquerda**.
- Cada tile deve “fechar” perfeitamente com os vizinhos.
- Não colocar colisão na imagem; colisão é configurada na Unity.
- **Não usar assets, personagens, logo ou layouts de Sanabi.**

### Layout de atlas sugerido

```text
tileset_common.png       8 colunas × 6 linhas (48 slots)
tileset_climbable.png    8 × 6
tileset_hazard.png       8 × 2
tileset_background.png   8 × 6
props.png                atlas separado, tamanhos variados
palette.png              swatches
manifest.csv             nome, coluna, linha, máscara, colisão, layer
```

Cada célula do atlas: 32×32 + 1 px de padding. Não confiar em labels dentro da
imagem; entregar o `manifest.csv`.

## 8. Integração na Unity

### Importação

- Texture Type: **Sprite (2D and UI)**.
- Sprite Mode: **Multiple**.
- Pixels Per Unit: **32**.
- Filter Mode: **Point (no filter)** para pixel art.
- Compression: **None**.
- Generate Mip Maps: **off**.
- Alpha Is Transparency: **on**.
- Wrap Mode: **Clamp**.
- Extrude Edges: **0** (usar padding no atlas).
- Pivot dos tiles: **Center**.
- Pivot dos props: **Bottom Center** (quando fizer sentido).

### Tilemaps separados

| Tilemap | Layer | Collider |
|---|---|---|
| `Tilemap_Common` | `Default` | TilemapCollider2D + CompositeCollider2D |
| `Tilemap_Climbable` | `ClimbableWall` | TilemapCollider2D + CompositeCollider2D |
| `Tilemap_Hazard` | `Default` | Trigger (ou objetos separados) |
| `Tilemap_Background` | `Default` | Nenhum |
| `Tilemap_Props` | `Default` | Nenhum |

Configuração dos tilemaps sólidos:

- `TilemapCollider2D.usedByComposite = true`;
- `CompositeCollider2D.geometryType = Polygons`;
- `Rigidbody2D.bodyType = Static`;
- Tiles de colisão: `Collider Type = Grid`;
- Tiles de fundo/props: `Collider Type = None`.

**Importante:** parede comum e parede escalável precisam ser **Tilemaps
separados**, porque `Layer` é por GameObject. O Wall Cling usa
`LayerMask` `ClimbableWall`.

### Ordem de sorting sugerida

```text
Background       -100
BackProps         -50
Common/Climbable    0
Hazard              5
Props front        10
Player             20
Ropes/VFX          30
HUD               100
```

### Regras de colisão

- Chão/plataforma: sólido, box;
- Parede escalável: sólida + layer `ClimbableWall`;
- Parede comum: sólida + layer `Default`;
- Laser/espinho: trigger;
- Fundo/props: sem collider;
- Sem slopes e sem one-way platforms no estado atual;
- Evitar tiles que sugiram rampas ou escadas com colisão diferente da caixa.

## 9. Como testar o tileset no jogo

Montar uma sala de teste com:

- chão de 32 tiles de largura;
- plataforma de 6×1 tiles;
- parede escalável de 1×8 tiles (teal);
- parede comum de 1×8 tiles (cinza) ao lado;
- espinhos;
- uma âncora de gancho;
- Bola Amarela;
- player.

Critérios:

1. O player (26×58 px) é legível contra o fundo.
2. A borda do chão é clara o bastante para saber onde pisar.
3. A parede teal é identificável como escalável sem HUD.
4. A parede cinza não parece escalável.
5. O laser/espinho é lido como perigo.
6. As emendas entre tiles não aparecem.
7. A corda do gancho não se perde no fundo.
8. A sala funciona em 1920×1080 e em 16:9.

## 10. Prompt pronto (para colar em outra IA)

### Versão em português

```text
Você é um artista técnico sênior de pixel art 2D para jogos de plataforma.

CONTEXTO
Estou criando Pindorama, um platformer 2D de precisão e momentum inspirado em
Sanabi, com dois ganchos coloridos (vermelho e azul), swing, wall cling e
combate por gancho. O visual é industrial/cyberpunk escuro, original, sem copiar
assets de Sanabi.

CÂMERA E ESCALA
Câmera ortográfica side-scroller, orthographicSize 9.5, referência 1920x1080.
1 tile = 1 unidade Unity = 32x32 px. PPU 32. Jogador ~26x58 px. Plataformas de
1 tile de espessura. Sem perspectiva, sem isométrico, sem 3D.

DIREÇÃO DE ARTE
Megastrutura industrial/cyberpunk: aço escuro, concreto, painéis, cabos,
grades, ventiladores, neon contido. Fundo escuro e dessaturado; superfícies
caminháveis com rim light claro; paredes escaláveis em teal com chevrons;
perigos em vermelho; detalhes com moderação para não atrapalhar a leitura.
Paleta: #0B0F14, #2A3440, #3E4C5A, #6E7F8C, #2BD9C4, #69FFF0, #FF4D5A,
#4DA6FF, #FFD24A, #E6F1F2.

ENTREGAR
1. Um atlas PNG RGBA transparente de tiles 32x32, com padding de 1 px, para
   estrutura comum (aço/cinza).
2. Um segundo atlas para estrutura escalável (mesma geometria, assinatura teal).
3. Um atlas de perigos (espinhos, laser, faixas de alerta).
4. Um atlas de fundo de baixo contraste.
5. Um atlas de props/decals separado (canos, grades, monitores, luzes).
6. Um manifest.csv com nome do tile, coluna, linha, máscara de vizinhança,
   colisão (Grid/None/Trigger) e layer.
7. Um mock 1920x1080 mostrando uma sala com player, chão, plataforma, parede
   escalável e parede comum.

REGRAS TÉCNICAS
Tiles 32x32, pixel art, sem anti-aliasing fora da silhueta, sem texto/UI/logo.
Luz consistente de cima/esquerda. Emendas perfeitas. Não desenhar personagens,
inimigos, âncoras, Bola Amarela, caixas ou laser dentro dos tiles. Não usar
assets ou layouts de Sanabi.

AUTOTILING
Preferência: 47-tile blob para Rule Tile. Se não for possível, entregue 14 tiles
básicos (single, center, top, bottom, left, right, 4 cantos externos e 4 cantos
internos) + overlays, e explique o mapeamento.

Antes de gerar, repita as premissas e a lista de entregáveis. Se algo for
ambíguo, pergunte.
```

### Versão em inglês (para gerador de imagem)

```text
You are a senior 2D pixel-art technical artist for precision platformers.

PROJECT
Pindorama is a 2D precision/momentum platformer inspired by Sanabi, with two
color-coded grappling hooks (red/blue), swinging, wall cling, and hook combat.
Original industrial/cyberpunk visual identity; do not copy Sanabi assets.

CAMERA AND SCALE
Side-scrolling orthographic camera, orthographicSize 9.5, 1920x1080 reference.
1 tile = 1 Unity unit = 32x32 px. PPU 32. Player approx. 26x58 px. Platforms
1 tile thick. No perspective, no isometric, no 3D.

ART DIRECTION
Dark industrial megastructure: steel, concrete, panels, cables, grates, fans,
restrained neon. Low-contrast dark background; walkable surfaces with a bright
top rim; climbable walls in teal with chevrons; hazards red; detail must not
hurt gameplay readability. Palette: #0B0F14, #2A3440, #3E4C5A, #6E7F8C,
#2BD9C4, #69FFF0, #FF4D5A, #4DA6FF, #FFD24A, #E6F1F2.

DELIVERABLES
1. RGBA transparent 32x32 tile atlas with 1px padding for common steel/concrete
   structure.
2. A second atlas for climbable structure (same geometry, teal signature).
3. A hazard atlas (spikes, laser housings, warning stripes).
4. A low-contrast background atlas.
5. A separate props/decals atlas (pipes, grates, monitors, lights).
6. manifest.csv with tile name, column, row, neighbor mask, collision
   (Grid/None/Trigger), layer.
7. A 1920x1080 mock room with player, floor, platform, climbable wall, common
   wall.

TECHNICAL RULES
32x32 pixel art, no anti-aliasing outside silhouettes, no text/UI/logo.
Consistent top-left light. Seamless edges. Do not draw characters, enemies,
hook anchors, yellow ball, boxes, or laser inside the tiles. Do not use Sanabi
assets or layouts.

AUTOTILING
Preferred: 47-tile blob for Unity Rule Tiles. If not feasible, deliver 14 core
tiles (single, center, top, bottom, left, right, 4 outer corners, 4 inner
corners) plus overlays and explain the mapping.

Before generating, restate assumptions and deliverable list. Ask if anything is
ambiguous.
```

## 11. Negative prompt

```text
no characters, no player, no enemies, no UI, no text, no logo, no watermark,
no Sanabi assets, no copyrighted characters, no isometric, no top-down, no 3D,
no photorealistic, no blurry pixels, no anti-aliased edges, no JPEG artifacts,
no strong cast shadows crossing tile borders, no busy background noise,
no slopes/ramps, no one-way platforms, no ladders
```

## 12. Checklist de aceite do tileset

- [ ] 1 tile = 32 px e 1 unidade.
- [ ] Player de 26×58 px é legível.
- [ ] Chão e plataformas têm borda superior clara.
- [ ] Parede escalável é teal e distinta da comum.
- [ ] Parede comum não parece escalável.
- [ ] Perigos são vermelhos e/ou com padrão de alerta.
- [ ] Fundo não compete com player/corda.
- [ ] Emendas invisíveis.
- [ ] Atlas com padding e manifest.
- [ ] Sem texto/UI/personagens no tileset.
- [ ] Tilemaps separados para comum e escalável.
- [ ] Teste em 1920×1080.

## 13. O que não fazer

- Não desenhar o jogador, inimigos, âncoras, caixas, laser ou Bola Amarela
  dentro dos tiles.
- Não usar perspectiva.
- Não fazer rampas/escadas com colisão diferente de caixa.
- Não usar a mesma cor para parede escalável e comum.
- Não colocar detalhes brilhantes atrás da corda do gancho.
- Não copiar composição, personagens ou assets de Sanabi.
- Não gerar 47 tiles inconsistentes: um conjunto menor e coerente é melhor.
