# CraftPix — configuração aplicada

## Abrir a cena de exemplo

Abra:

```text
Assets/_Project/Scenes/CraftPixDemo.unity
```

Ela já tem:

- chão, paredes e plataformas cinza;
- parede verde escalável na layer `ClimbableWall`;
- `HookAnchor` na parede;
- espinhos vermelhos com `LethalHazard` NonLethal;
- kill zone letal no fundo;
- player com câmera, `LevelController`, HUD de ganchos e HUD de falhas;
- porta, baú, botão, alvo, boneco de treino e pipes;
- fundo roxo escuro.

A cena está nas Build Settings.

## Tile Palette

A palette fica em:

```text
Assets/_Project/Art/CraftPix/Generated/CraftPixPalette.prefab
```

Abra:

```text
Window > 2D > Tile Palette
```

Se a palette não aparecer na lista, selecione o prefab acima e use **Open**.
Para pintar, selecione o Tilemap correto na hierarquia:

| Tilemap | Uso | Layer |
|---|---|---|
| `Common` | piso, paredes e plataformas cinza | Default |
| `Climbable` | parede verde escalável | `ClimbableWall` |
| `Background` | fundo roxo | Default |
| `Hazard` | faixa visual vermelha | Default |

### Nomes dos tiles

Cada tema tem 10 variantes:

```text
tl  top-left      t   top       tr  top-right
l   left          c   center    r   right
bl  bottom-left   b   bottom    br  bottom-right
single
```

Temas: `gray`, `green`, `blue`, `purple`, `tan`, `red`.

Exemplo:

```text
gray_t   -> topo de plataforma
gray_c   -> interior
gray_l   -> borda esquerda
gray_r   -> borda direita
gray_b   -> base
```

Use `single` para blocos isolados.

## Regras de colisão

- `Common` e `Climbable` têm `TilemapCollider2D` por tile.
- Não há `CompositeCollider2D` na cena de exemplo. Para mapas grandes, adicione
  `CompositeCollider2D` e configure `compositeOperation` no `TilemapCollider2D`.
- `Background` e `Hazard` não têm collider.
- Espinhos usam `BoxCollider2D` trigger + `LethalHazard`.
- Kill zone usa `BoxCollider2D` trigger + `LethalHazard` Lethal.

## Cores

| Tema | Uso |
|---|---|
| `gray` | estrutura comum |
| `green` | parede escalável |
| `red` | perigo/laser |
| `tan` | plataforma móvel/caixa |
| `purple` | fundo/portal |
| `blue` | tecnologia/portal |

Não use o verde como chão comum; ele comunica “escalável”.

## Como criar outro mapa

1. Duplique `CraftPixDemo.unity`.
2. Apague os tiles que não quer ou pinte por cima.
3. Use a palette nos Tilemaps `Common` e `Climbable`.
4. Mantenha a parede escalável na layer `ClimbableWall`.
5. Coloque `HookAnchor` nas paredes que aceitam gancho.
6. Coloque espinhos com `LethalHazard` NonLethal.
7. Coloque kill zones com `LethalHazard` Lethal.
8. Ajuste o spawn do `LevelController`.

## Reexecutar a configuração

Menu:

```text
Pindorama > CraftPix > Configurar tudo (v2)
```

Esse comando **recria** tiles, palette, player prefab e a cena de exemplo.
Portanto, se você já tiver pintado um mapa na `CraftPixDemo`, duplique a cena
antes de rodar.

## Assets

- Tilesets limpos: `Assets/_Project/Art/CraftPix/Generated/Tilesets/`
- Tile assets: `Assets/_Project/Art/CraftPix/Generated/Tiles/`
- Objetos: `Assets/_Project/Art/CraftPix/Objects/`
- Pipes: `Assets/_Project/Art/CraftPix/Pipes/`
- Créditos: `Assets/_Project/Art/CraftPix/CREDITS.txt`

Os PNGs originais de tiles/prefabs/backtiles tinham rótulos de medida; por isso
os tiles usados no projeto são uma **derivação 9-slice limpa** feita a partir da
paleta CraftPix. Os objetos e pipes são os originais limpos do pack.

## Dependência

Foi adicionado ao `Packages/manifest.json`:

```json
"com.unity.2d.tilemap": "1.0.0"
```

Isso habilita o Tile Palette/editor de Tilemap. Se o Unity pedir para resolver
pacotes, aceite.

## Licença

<https://craftpix.net/file-licenses/>

Confirme uso comercial e crédito antes de publicar. Não redistribua o pack
original como asset isolado.
