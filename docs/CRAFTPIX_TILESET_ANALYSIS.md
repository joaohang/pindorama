# CraftPix 32×32 Platformer Tileset — análise e integração

**Pasta analisada:** `craftpix-net-141897-free-prototype-2d-platformer-32x32-pixel-tileset`  
**Status:** configurado no projeto. Veja
[`CRAFTPIX_SETUP.md`](CRAFTPIX_SETUP.md) para abrir a cena e usar a palette.

> **Atenção importante:** os PNGs de `1 Tiles`, `2 TilePrefabs` e `4 BackTiles`
> têm **rótulos de tamanho desenhados dentro da imagem** (`32`, `45°`, `27°`,
> `32×288` etc.). Eles são um **kit de layout/protótipo**, não arte final.
> Use-os como guia de colisão/level design, ou esconda o grupo de rótulos nos
> PSDs e exporte os tiles limpos. Não importe esses PNGs diretamente achando
> que são tiles de produção.
>
> `3 Pipes` e `5 Objects` (portas, botões, alavancas, baús, espinhos etc.)
> estão limpos e podem ser usados diretamente depois de recortados.

## 1. O que tem na pasta

- **1 Tiles:** 64 tiles 32×32 + 6 folhas `Tileset1–6` de 256×256.
- **2 TilePrefabs:** plataformas prontas horizontais/verticais, redondas e
  quadradas; 6 variantes de cor.
- **3 Pipes:** 21 peças 32×32 + 6 `PipeSet` de 224×96.
- **4 BackTiles:** 36 tiles 32×32 + 6 `BackTileset` de 192×192.
- **5 Objects:** portas, botões, alavancas, baús, escadas, cordas, espinhos,
  alvos e boneco de treino; 6 variantes de cor.
- **PSD:** fontes editáveis.
- **Licença:** `Licens.txt` aponta para
  <https://craftpix.net/file-licenses/>.

## 2. Escala — encaixe perfeito

| Item | Valor |
|---|---:|
| Tile | **32×32 px** |
| Pindorama | **1 unidade = 32 px** |
| PPU | **32** |
| Plataforma padrão | 1 unidade |
| Jogador visível | ~1,74 unidade |

O tileset foi desenhado exatamente na mesma escala do Pindorama. Isso é a maior
vantagem dele.

## 3. As 6 cores

Cores dominantes amostradas:

| Folha | Cor principal | Hex | Uso sugerido |
|---|---|---|---|
| `Tileset1` | verde | `#3A936A`, `#54B55C` | **parede escalável** |
| `Tileset2` | azul | `#4C7097`, `#7CA1FF` | tecnologia/portal/fundo |
| `Tileset3` | cinza | `#888795`, `#CECED3` | **estrutura comum** |
| `Tileset4` | roxo | `#AA51B8`, `#E171CF` | **fundo interdimensional** |
| `Tileset5` | tan/amarelo | `#BC9364`, `#DCBE72` | **plataforma móvel / caixas** |
| `Tileset6` | vermelho | `#CE3C3A`, `#F88858` | **perigo / laser / kill zone** |

Contorno escuro comum: `#0C0A25`. Isso ajuda a manter a leitura sobre o fundo
escuro do Pindorama.

### Regra de cor (importante)

Não usar as seis cores como plataformas equivalentes. A cor deve comunicar a
regra:

```text
cinza  = piso/parede comum
verde  = escalável
vermelho = perigo
tan    = móvel
roxo   = fundo/portal
azul   = tecnologia/portal
```

Se o verde for usado como plataforma comum, o jogador vai tentar escalar.
Se o vermelho for usado como chão, o jogador vai evitar pular.

## 4. O que aproveitar primeiro

### 4.1 Prefabs de plataforma (como layout)

`2 TilePrefabs` tem rótulos de tamanho baked. Use como **molde de colisão**:
posicione o prefab, copie o tamanho para um `BoxCollider2D` e depois troque o
visual por um tile limpo/PSD. Não use o PNG diretamente como arte final.

`2 TilePrefabs`:

- `h1x3`, `h1x5`, `h1x7`, `h1x9` — plataformas horizontais de 3, 5, 7 e 9 tiles;
- `h1x3_2`, etc. — metade da altura (0,5 unidade);
- `v3x1`, `v5x1`, `v7x1` — paredes verticais;
- `r1–r4` — plataformas redondas;
- `s2–s4` — quadradas;
- `Prefabs1–6` — folhas de referência.

Uso: são a forma mais rápida de montar uma sala sem Tilemap. Coloque o prefab,
adicione `BoxCollider2D` (ou `CircleCollider2D` nos redondos) e ajuste a layer.

### 4.2 Tilesets

`1 Tiles/Tileset3` (cinza) como base; `Tileset1` (verde) para paredes
escaláveis; `Tileset6` (vermelho) para perigo; `Tileset5` (tan) para movimento;
`Tileset4` (roxo) para fundo.

Mas, de novo: os PNGs de `Tileset1–6` têm os rótulos. Para usar como arte,
primeiro limpe os rótulos no `PSD/Tileset.psd`. Para o protótipo, o melhor é
usar o desenho apenas como **referência de escala e silhueta**.

### 4.3 Pipes e BackTiles

- `3 Pipes` — **limpos**, decoração industrial, sem collider por padrão.
- `4 BackTiles` — **com rótulos**; use como referência ou limpe no PSD.

Os Pipes resolvem parte do fundo vazio e combinam com a prisão interdimensional.
Para o fundo, uma alternativa é usar `BackTiles` limpos do PSD ou criar um
tilemap escuro simples.

### 4.4 Objetos

| Objeto | Uso |
|---|---|
| `Door.png` / `Door2.png` | porta de saída/transição (frames) |
| `Button.png` / `Button2.png` | botão de puzzle |
| `Lever.png` | alavanca (frames) |
| `Chest.png` | baú/recompensa (frames) |
| `Spikes1–3.png` | espinhos; usar com `LethalHazard` NonLethal |
| `Target.png` | alvo de treino de gancho |
| `TrainingDummy.png` | tutorial de combate |
| `Ladder1–6.png` | **não usar ainda**: não há mecânica de escada |
| `Rope1–3.png` | decoração; a corda do gancho é procedural |

As folhas `Objects1–6` são atlas de referência; para usar, recorte os sprites
individuais (porta, alavanca etc.) em vez de importar a folha inteira.

## 5. Limpando os rótulos (se quiser usar a arte)

Os PSDs são a melhor saída:

1. abra `PSD/Tileset.psd` no Photoshop/GIMP;
2. localize o grupo/camada dos textos de medida;
3. oculte o grupo;
4. exporte cada tile 32×32 como PNG transparente;
5. repita para `PSD/TilePrefabs.psd` e `PSD/BackTiles.psd`.

Se os rótulos estiverem **rasterizados** na camada da arte, será necessário
pintar por cima ou recriar o tile. Nesse caso, use o pack como referência de
layout e a arte final do `TILESET_BRIEF.md`.

Para o protótipo, há um caminho mais rápido: use os PNGs rotulados apenas para
**bloquear a geometria** (com o `SpriteRenderer` desligado), copie os tamanhos
para os colliders e use os objetos/pipes limpos como decoração. Depois troque
os blocos por tiles limpos.

## 6. Slopes: cuidado

O tileset tem peças de **45° e 27°**. O Pindorama hoje usa:

- `Rigidbody2D` dinâmico;
- movimento horizontal;
- `IsGrounded` por círculo;
- colliders de caixa.

**Não existe suporte a slope.** Se usar o visual inclinado com collider de
caixa, o personagem vai flutuar ou atravessar. Duas opções:

1. **agora:** usar só tiles retos e prefabs horizontais/verticais;
2. **depois:** implementar um `SlopeCollider2D`/movimento em rampa e então usar
   os tiles de 45°/27°.

Para o protótipo, a opção 1 é a correta.

## 7. Configuração na Unity

### Importação

- Texture Type: **Sprite (2D and UI)**;
- Sprite Mode:
  - folhas `Tileset1–6`: **Multiple**;
  - tiles individuais/prefabs: **Single**;
- Pixels Per Unit: **32**;
- Filter Mode: **Point (no filter)**;
- Compression: **None**;
- Generate Mip Maps: **off**;
- Alpha Is Transparency: **on**;
- Pivot:
  - tiles: **Center**;
  - prefabs de plataforma: **Center** (ou Bottom Center, se preferir alinhar
    pelo pé);
- Grid By Cell Size: **32×32** para as folhas 256×256;
- Extrude Edges: 0.

### Tilemaps separados

| Tilemap / objeto | Layer | Collider |
|---|---|---|
| `Tilemap_Common` | Default | TilemapCollider2D + CompositeCollider2D |
| `Tilemap_Climbable` | **ClimbableWall** | TilemapCollider2D + CompositeCollider2D |
| `Tilemap_Hazard` | Default | Trigger, sem composite |
| `Tilemap_Background` | Default | nenhum |
| `Pipes/Props` | Default | nenhum (ou BoxCollider se for cobertura) |

Configuração dos sólidos:

```text
TilemapCollider2D.usedByComposite = true
CompositeCollider2D.geometryType = Polygons
Rigidbody2D.bodyType = Static
```

**Parede comum e escalável precisam ser objetos/Tilemaps separados**, porque
`Layer` é por GameObject.

### Sorting order sugerido

```text
BackTiles / Pipes de fundo   -20
Props de fundo               -10
Common / Climbable             0
Hazard                         2
Props interativos              3
Player                        20
Ropes/VFX                     30
HUD                          100
```

### Cor e clima

O CraftPix é **pastel/claro**; o Pindorama é escuro. Para encaixar:

- use cinza/roxo/azul como base;
- reforce verde/vermelho só onde há regra;
- escureça o fundo com `SpriteRenderer.color` (ex.: 0,55–0,75 de luminosidade);
- mantenha o contorno escuro `#0C0A25`, que já ajuda;
- não aplique tint aleatório em cada tile: use um tint por **layer/tema**.

## 8. Mapa de uso recomendado

```text
Piso e paredes comuns     -> Tileset3 (cinza)
Parede escalável          -> Tileset1 (verde) + layer ClimbableWall
Perigo/laser/kill zone    -> Tileset6 (vermelho)
Plataforma móvel / caixa  -> Tileset5 (tan/amarelo)
Fundo/portal              -> Tileset4 (roxo)
Tech/portais/detalhes     -> Tileset2 (azul)
Pipes                     -> PipeSet3 (cinza) ou PipeSet4 (roxo)
Objetos                   -> variante da cor do ambiente
```

## 9. Plano de aplicação no Pindorama

### Fase 1 — sala de teste

1. Limpar os rótulos dos PSDs ou importar os tiles apenas como placeholders
   de layout (com o `SpriteRenderer` de teste desligado).
2. Montar uma sala com:
   - chão cinza;
   - parede verde escalável com `ClimbableWall` e `HookAnchor`;
   - espinhos vermelhos com `LethalHazard` NonLethal;
   - plataforma tan móvel;
   - fundo roxo com BackTiles.
3. Testar escala, leitura, wall cling e gancho.

### Fase 2 — substituir o graybox da Level02

4. Trocar os blocos cinza por `Tilemap_Common`/prefabs cinza.
5. Trocar a parede teal por `Tileset1` verde.
6. Trocar o visual do laser/hazard por vermelho.
7. Adicionar BackTiles/Pipes atrás.

### Fase 3 — objetos

8. Porta de saída com `Door`.
9. Botão/alavanca para puzzle.
10. Baú/recompensa.
11. Alvos/boneco para tutorial.

### Fase 4 — slopes

12. Só depois de implementar movimento em rampa, usar 45°/27°.

## 10. O que não usar agora

- tiles de 45°/27° como colisão;
- escadas (sem mecânica);
- cordas (a corda é procedural);
- todos os 6 temas misturados na mesma sala;
- `Objects1–6` como um sprite único sem recorte;
- 16×32/32×16 em um Tilemap 32×32 sem prefab/offset.

## 11. Licença

`Licens.txt` aponta para:

<https://craftpix.net/file-licenses/>

Antes de publicar:

- confirme se a licença free permite uso comercial;
- mantenha o crédito se exigido;
- não redistribua os arquivos originais como asset pack;
- não use os sprites como marca/logo do jogo.

## 12. Resumo

**Vale como kit de layout/protótipo, não como arte final direta.** O tamanho
32×32/PPU32 encaixa perfeitamente e os prefabs aceleram o level design, mas os
PNGs de tiles/prefabs/backtiles têm rótulos de medida.

O melhor uso:

- **Pipes e Objects** (limpos): usar diretamente;
- **Tiles/Prefabs/BackTiles**: usar como molde de colisão/layout ou limpar os
  rótulos no PSD antes de importar;
- **cores**: cinza = comum; verde = escalável; vermelho = perigo; tan = móvel;
  roxo = fundo/portal; azul = tecnologia;
- **slopes**: evitar por enquanto;
- **assets**: manter layers/Tilemaps separados por função.

O maior ganho visual imediato é usar os **Pipes** como profundidade, os
**Objects** como portas/espinhos/botões e a paleta cinza + verde + vermelho
para comunicar a jogabilidade. A arte de tiles definitiva deve vir do
`TILESET_BRIEF.md` ou de uma exportação limpa dos PSDs.
