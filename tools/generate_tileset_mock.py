"""Gera um mock procedural do tileset do Pindorama para validação visual.

Não é arte final: é um teste de escala, paleta, leitura e composição.
Saída em docs/images/.
"""

from __future__ import annotations

import math
import os
import random
from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "docs", "images")
os.makedirs(OUT, exist_ok=True)

# ---------------------------------------------------------------------------
# Paleta
# ---------------------------------------------------------------------------
BG = (8, 12, 17, 255)
BG2 = (12, 18, 25, 255)
STEEL_DARK = (26, 34, 44, 255)
STEEL = (42, 52, 64, 255)
STEEL2 = (62, 76, 90, 255)
STEEL3 = (110, 127, 140, 255)
CONCRETE = (74, 85, 99, 255)
TEAL = (43, 217, 196, 255)
TEAL_DARK = (20, 95, 88, 255)
TEAL_GLOW = (105, 255, 240, 255)
RED = (255, 77, 90, 255)
RED_DARK = (179, 36, 53, 255)
BLUE = (77, 166, 255, 255)
BLUE_DARK = (30, 95, 168, 255)
YELLOW = (255, 210, 74, 255)
YELLOW_DARK = (184, 134, 11, 255)
WHITE = (230, 241, 242, 255)
BLACK = (0, 0, 0, 255)

FONT_PATH = r"C:\Windows\Fonts\arial.ttf"
FONT_BOLD = r"C:\Windows\Fonts\arialbd.ttf"
FONT_MONO = r"C:\Windows\Fonts\consola.ttf"


def font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont:
    path = FONT_BOLD if bold else FONT_PATH
    try:
        return ImageFont.truetype(path, size)
    except OSError:
        return ImageFont.load_default()


# ---------------------------------------------------------------------------
# Tiles 32x32
# ---------------------------------------------------------------------------
TILE_NAMES = [
    "single", "center", "top", "bottom", "left", "right",
    "top_left", "top_right", "bottom_left", "bottom_right",
    "inner_tl", "inner_tr", "inner_bl", "inner_br",
]


def rgba(color, alpha=255):
    return (color[0], color[1], color[2], alpha)


def make_tile(family: str, top=False, bottom=False, left=False, right=False,
              inner=None, variant=0) -> Image.Image:
    img = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    base = STEEL if family != "background" else BG2
    panel = STEEL2 if family != "background" else (18, 27, 36, 255)
    rim = STEEL3 if family != "background" else (35, 46, 58, 255)
    d.rectangle([0, 0, 31, 31], fill=base)

    # Painéis internos.
    if family != "background":
        d.rectangle([4, 9, 27, 10], fill=panel)
        d.rectangle([4, 21, 27, 22], fill=panel)
        d.line([9, 11, 9, 20], fill=STEEL_DARK)
        d.line([22, 11, 22, 20], fill=STEEL_DARK)
        # Parafusos.
        for x, y in ((3, 3), (28, 3), (3, 28), (28, 28)):
            d.point((x, y), fill=STEEL3)
        # Rachaduras leves.
        if variant % 2 == 0:
            d.line([12, 14, 14, 17, 13, 19], fill=STEEL_DARK)
        else:
            d.line([19, 25, 17, 27], fill=STEEL_DARK)
    else:
        # Fundo: painéis largos e escuros.
        d.rectangle([2, 6, 29, 14], fill=(15, 23, 31, 255))
        d.rectangle([2, 18, 29, 27], fill=(15, 23, 31, 255))
        d.line([2, 17, 29, 17], fill=(24, 35, 46, 255))

    # Assinatura teal da parede escalável.
    if family == "climbable":
        d.rectangle([1, 0, 4, 31], fill=(17, 70, 68, 255))
        d.line([2, 1, 2, 30], fill=TEAL)
        d.line([4, 1, 4, 30], fill=TEAL_DARK)
        # Chevrons.
        for y in range(7, 28, 8):
            d.line([7, y, 12, y + 4], fill=TEAL_GLOW)
            d.line([12, y + 4, 7, y + 8], fill=TEAL_GLOW)
        d.rectangle([1, 0, 4, 0], fill=TEAL)

    # Topo caminhável.
    if top:
        d.rectangle([0, 0, 31, 4], fill=CONCRETE)
        d.line([0, 0, 31, 0], fill=STEEL3)
        d.line([0, 4, 31, 4], fill=STEEL_DARK)
        if family == "climbable":
            d.line([0, 0, 31, 0], fill=TEAL_GLOW)
            d.line([0, 1, 31, 1], fill=TEAL)

    # Bordas.
    if bottom:
        d.line([0, 30, 31, 30], fill=STEEL_DARK)
        d.line([0, 31, 31, 31], fill=(13, 19, 26, 255))
    if left:
        d.line([0, 0, 0, 31], fill=STEEL2)
        d.line([1, 0, 1, 31], fill=STEEL_DARK)
    if right:
        d.line([31, 0, 31, 31], fill=STEEL2)
        d.line([30, 0, 30, 31], fill=STEEL_DARK)

    # Cantos internos: notch escuro para leitura no atlas.
    if inner:
        notch = 7
        if inner == "tl":
            d.polygon([(0, 0), (notch, 0), (0, notch)], fill=BG2)
        elif inner == "tr":
            d.polygon([(31, 0), (31 - notch, 0), (31, notch)], fill=BG2)
        elif inner == "bl":
            d.polygon([(0, 31), (notch, 31), (0, 31 - notch)], fill=BG2)
        elif inner == "br":
            d.polygon([(31, 31), (31 - notch, 31), (31, 31 - notch)], fill=BG2)

    # Ruído controlado.
    rnd = random.Random(hash((family, top, bottom, left, right, inner, variant)) & 0xFFFFFFFF)
    for _ in range(8):
        x = rnd.randrange(4, 28)
        y = rnd.randrange(5, 28)
        d.point((x, y), fill=STEEL_DARK if variant % 3 else STEEL3)

    return img


def tile_for(name: str, family: str) -> Image.Image:
    top = name in ("single", "top", "top_left", "top_right")
    bottom = name in ("single", "bottom", "bottom_left", "bottom_right")
    left = name in ("single", "left", "top_left", "bottom_left")
    right = name in ("single", "right", "top_right", "bottom_right")
    inner = None
    if name.startswith("inner_"):
        inner = name.split("_")[1]
        top = bottom = left = right = False
    return make_tile(family, top, bottom, left, right, inner, variant=len(name) + len(family))


# ---------------------------------------------------------------------------
# Atlas
# ---------------------------------------------------------------------------
def atlas_sheet(family: str) -> Image.Image:
    cols, rows = 7, 2
    cell, pad = 32, 1
    sheet = Image.new("RGBA", (cols * (cell + pad) + pad, rows * (cell + pad) + pad), (0, 0, 0, 0))
    for i, name in enumerate(TILE_NAMES):
        x = pad + (i % cols) * (cell + pad)
        y = pad + (i // cols) * (cell + pad)
        sheet.alpha_composite(tile_for(name, family), (x, y))
    return sheet


def atlas_card(family: str, title: str, scale: int = 4) -> Image.Image:
    cols, rows = 7, 2
    cell = 32 * scale
    label_h = 22
    gap = 6
    card = Image.new("RGBA", (cols * (cell + gap) + gap, 34 + rows * (cell + label_h + gap) + gap), BG)
    d = ImageDraw.Draw(card)
    d.text((gap, 6), title, font=font(18, True), fill=WHITE)
    sheet = atlas_sheet(family)
    for i, name in enumerate(TILE_NAMES):
        x = gap + (i % cols) * (cell + gap)
        y = 34 + (i // cols) * (cell + label_h + gap)
        tile = sheet.crop((1 + (i % cols) * 33, 1 + (i // cols) * 33,
                           1 + (i % cols) * 33 + 32, 1 + (i // cols) * 33 + 32))
        card.alpha_composite(tile.resize((cell, cell), Image.Resampling.NEAREST), (x, y))
        d.text((x + 2, y + cell + 2), name, font=font(11), fill=(170, 185, 195, 255))
    return card


# ---------------------------------------------------------------------------
# Props / hazards / background
# ---------------------------------------------------------------------------
def prop_pipe(w=80, h=24):
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rectangle([0, 6, w - 1, h - 7], fill=STEEL2)
    d.line([0, 6, w - 1, 6], fill=STEEL3)
    d.line([0, h - 7, w - 1, h - 7], fill=STEEL_DARK)
    for x in (4, w - 10):
        d.rectangle([x, 3, x + 6, h - 4], fill=STEEL)
        d.rectangle([x + 1, 5, x + 5, h - 6], fill=STEEL2)
    return im


def prop_grate():
    im = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rectangle([2, 2, 29, 29], fill=STEEL_DARK)
    for x in range(5, 29, 5):
        d.line([x, 4, x, 27], fill=STEEL2)
    d.rectangle([2, 2, 29, 29], outline=STEEL3)
    return im


def prop_monitor():
    im = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rectangle([3, 4, 28, 27], fill=STEEL_DARK)
    d.rectangle([6, 7, 25, 21], fill=(12, 60, 60, 255))
    d.line([8, 10, 22, 10], fill=TEAL)
    d.line([8, 13, 18, 13], fill=TEAL_DARK)
    d.point((24, 9), fill=TEAL_GLOW)
    d.rectangle([12, 24, 19, 27], fill=STEEL2)
    return im


def prop_light():
    im = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rectangle([10, 8, 21, 12], fill=STEEL2)
    d.rectangle([12, 12, 19, 16], fill=YELLOW)
    d.rectangle([14, 16, 17, 23], fill=STEEL_DARK)
    return im


def prop_fan():
    im = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.ellipse([3, 3, 28, 28], outline=STEEL3, width=2, fill=STEEL_DARK)
    cx, cy = 16, 16
    for a in range(0, 360, 90):
        r = math.radians(a)
        d.line([cx, cy, cx + math.cos(r) * 10, cy + math.sin(r) * 10], fill=STEEL2, width=4)
    d.ellipse([13, 13, 18, 18], fill=TEAL_DARK)
    return im


def prop_box(color):
    im = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rectangle([2, 2, 29, 29], fill=color)
    d.rectangle([2, 2, 29, 29], outline=WHITE)
    d.rectangle([6, 6, 25, 25], outline=rgba(color, 180))
    d.line([2, 2, 29, 29], fill=rgba(WHITE, 80))
    return im


def hazard_spikes():
    im = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rectangle([0, 24, 31, 31], fill=RED_DARK)
    for x in range(0, 32, 8):
        d.polygon([(x, 24), (x + 4, 4), (x + 8, 24)], fill=RED)
        d.line([x + 4, 4, x + 4, 20], fill=(255, 170, 170, 255))
    return im


def hazard_emitter():
    im = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rectangle([4, 4, 27, 27], fill=STEEL_DARK)
    d.rectangle([7, 7, 24, 24], fill=RED_DARK)
    d.ellipse([11, 11, 20, 20], fill=RED)
    d.ellipse([14, 14, 17, 17], fill=(255, 220, 220, 255))
    return im


def prop_cable(w=96, h=16):
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.line([0, h // 2, w - 1, h // 2], fill=STEEL_DARK, width=4)
    d.line([0, h // 2, w - 1, h // 2], fill=TEAL_DARK, width=1)
    for x in range(8, w, 24):
        d.rectangle([x, 2, x + 3, h - 3], fill=STEEL2)
    return im


def prop_warning():
    im = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.polygon([(16, 2), (30, 29), (2, 29)], fill=YELLOW)
    d.polygon([(16, 7), (25, 25), (7, 25)], fill=STEEL_DARK)
    d.line([16, 12, 16, 19], fill=YELLOW, width=2)
    d.point((16, 22), fill=YELLOW)
    return im


def props_card() -> Image.Image:
    items = [
        ("pipe", prop_pipe()), ("grate", prop_grate()),
        ("monitor", prop_monitor()), ("light", prop_light()),
        ("fan", prop_fan()), ("cable", prop_cable()),
        ("warning", prop_warning()), ("red box", prop_box(RED)),
        ("blue box", prop_box(BLUE)), ("spikes", hazard_spikes()),
        ("laser emitter", hazard_emitter()),
    ]
    cell_w, cell_h = 180, 100
    cols = 2
    rows = (len(items) + cols - 1) // cols
    card = Image.new("RGBA", (cols * cell_w + 20, rows * cell_h + 40), BG)
    d = ImageDraw.Draw(card)
    d.text((10, 6), "PROPS / HAZARDS (mock)", font=font(18, True), fill=WHITE)
    for i, (name, im) in enumerate(items):
        x = 10 + (i % cols) * cell_w
        y = 34 + (i // cols) * cell_h
        # Encaixa sem estourar.
        scale = min(1.0, 80 / max(im.width, im.height))
        w, h = max(1, int(im.width * scale)), max(1, int(im.height * scale))
        thumb = im.resize((w, h), Image.Resampling.NEAREST)
        card.alpha_composite(thumb, (x + (cell_w - w) // 2, y + 8))
        d.text((x + 8, y + 74), name, font=font(12), fill=(190, 205, 215, 255))
    return card


# ---------------------------------------------------------------------------
# Mock room 1088x612 (34x19.125 unidades @ 32 PPU)
# ---------------------------------------------------------------------------
W, H = 1088, 612


def draw_structure(img, x0, y0, cols, rows, family):
    for row in range(rows):
        for col in range(cols):
            top = row == 0
            bottom = row == rows - 1
            left = col == 0
            right = col == cols - 1
            tile = make_tile(family, top, bottom, left, right, variant=col + row)
            img.alpha_composite(tile, (x0 + col * 32, y0 + row * 32))


def glow_line(img, points, color, width=2, glow=6):
    overlay = Image.new("RGBA", img.size, (0, 0, 0, 0))
    d = ImageDraw.Draw(overlay)
    d.line(points, fill=rgba(color, 55), width=glow)
    d.line(points, fill=rgba(color, 120), width=max(2, width + 2))
    d.line(points, fill=color, width=width)
    img.alpha_composite(overlay)


def make_mock() -> Image.Image:
    img = Image.new("RGBA", (W, H), BG)
    d = ImageDraw.Draw(img)

    # Fundo: painéis, tubos e estrutura distante.
    d.rectangle([0, 0, W, H], fill=BG)
    for x in range(-40, W, 96):
        d.rectangle([x, 40, x + 78, 430], fill=(11, 17, 24, 255))
        d.rectangle([x + 6, 52, x + 72, 170], fill=(14, 21, 29, 255))
        d.line([x + 6, 180, x + 72, 180], fill=(20, 30, 40, 255))
    # Tubos horizontais distantes.
    for y in (80, 150, 320):
        d.line([0, y, W, y], fill=(18, 28, 37, 255), width=6)
        d.line([0, y, W, y], fill=(28, 41, 52, 255), width=1)
    # Cabos.
    for y in (105, 340):
        for x in range(0, W, 220):
            d.arc([x, y, x + 220, y + 80], 200, 340, fill=(25, 38, 48, 255), width=2)
    # Painéis de aviso distantes.
    for x in (180, 640, 880):
        d.rectangle([x, 360, x + 36, 390], fill=(18, 27, 36, 255))
        d.line([x + 4, 366, x + 32, 366], fill=(70, 55, 35, 255))
        d.line([x + 4, 372, x + 25, 372], fill=(70, 55, 35, 255))

    # Chão (4 tiles de altura).
    draw_structure(img, 0, 500, 34, 4, "common")
    # Parede esquerda comum.
    draw_structure(img, 0, 180, 1, 10, "common")
    # Parede direita escalável.
    draw_structure(img, 1056, 180, 1, 10, "climbable")

    # Plataformas.
    draw_structure(img, 224, 388, 7, 1, "common")
    draw_structure(img, 560, 324, 7, 1, "common")
    # Plataforma fina de serviço.
    draw_structure(img, 800, 420, 5, 1, "common")

    # Espinhos no chão.
    for x in range(480, 576, 32):
        img.alpha_composite(hazard_spikes(), (x, 468))

    # Laser vertical.
    glow_line(img, [(960, 190), (960, 500)], RED, width=3, glow=14)
    img.alpha_composite(hazard_emitter(), (944, 164))
    d.rectangle([946, 188, 974, 198], fill=STEEL_DARK)

    # Props.
    img.alpha_composite(prop_pipe(96, 24), (60, 300))
    img.alpha_composite(prop_grate(), (340, 356))
    img.alpha_composite(prop_monitor(), (610, 292))
    img.alpha_composite(prop_light(), (700, 270))
    img.alpha_composite(prop_fan(), (830, 260))
    img.alpha_composite(prop_warning(), (940, 330))
    img.alpha_composite(prop_cable(128, 16), (420, 300))

    # Caixas.
    img.alpha_composite(prop_box(RED), (250, 468))
    img.alpha_composite(prop_box(BLUE), (286, 468))

    # Âncora e corda.
    ax, ay = 700, 156
    glow_line(img, [(166, 455), (ax, ay)], RED, width=2, glow=8)
    d.ellipse([ax - 10, ay - 10, ax + 10, ay + 10], fill=RED_DARK, outline=WHITE)
    d.line([ax - 6, ay, ax, ay - 6, ax + 6, ay], fill=WHITE, width=2)
    d.line([ax, ay - 6, ax, ay + 6], fill=WHITE, width=2)

    # Bola Amarela.
    bx, by = 880, 236
    glow = Image.new("RGBA", img.size, (0, 0, 0, 0))
    gd = ImageDraw.Draw(glow)
    gd.ellipse([bx - 32, by - 32, bx + 32, by + 32], fill=rgba(YELLOW, 35))
    gd.ellipse([bx - 24, by - 24, bx + 24, by + 24], fill=rgba(YELLOW, 55))
    img.alpha_composite(glow)
    d.ellipse([bx - 17, by - 17, bx + 17, by + 17], fill=YELLOW, outline=(255, 245, 190, 255), width=2)
    d.line([bx, 150, bx, by - 17], fill=STEEL_DARK, width=2)

    # Jogador (26x58): silhueta clara, visor teal.
    px, py = 140, 442
    d.rounded_rectangle([px, py, px + 26, py + 58], radius=6, fill=(198, 225, 240, 255))
    d.rectangle([px + 5, py + 8, px + 21, py + 20], fill=STEEL_DARK)
    d.rectangle([px + 7, py + 10, px + 19, py + 16], fill=TEAL_GLOW)
    d.line([px + 6, py + 26, px + 20, py + 26], fill=STEEL3)
    d.line([px + 6, py + 30, px + 20, py + 30], fill=STEEL3)
    # Pernas.
    d.rectangle([px + 3, py + 44, px + 11, py + 58], fill=(150, 185, 205, 255))
    d.rectangle([px + 15, py + 44, px + 23, py + 58], fill=(150, 185, 205, 255))

    # Mira do gancho.
    d.line([px + 26, py + 22, 700, 156], fill=rgba(RED, 70), width=1)
    d.ellipse([696, 152, 704, 160], outline=WHITE)

    # Etiqueta de escala no canto (fora do "mundo").
    d.rectangle([0, 0, W, 28], fill=(0, 0, 0, 150))
    d.text((10, 5), "MOCK 34 x 19.1 unidades @ 32 PPU  |  player 26 x 58 px",
           font=font(13), fill=(200, 215, 225, 255))
    return img


# ---------------------------------------------------------------------------
# Composição final
# ---------------------------------------------------------------------------
def main():
    common_card = atlas_card("common", "COMMON STRUCTURE — 32x32 tiles (4x)")
    climb_card = atlas_card("climbable", "CLIMBABLE STRUCTURE — teal signature (4x)")
    props = props_card()
    mock = make_mock()

    common_card.save(os.path.join(OUT, "tileset_common_card.png"))
    climb_card.save(os.path.join(OUT, "tileset_climbable_card.png"))
    props.save(os.path.join(OUT, "tileset_props_card.png"))
    mock.save(os.path.join(OUT, "tileset_mock_room.png"))
    mock.resize((W * 2, H * 2), Image.Resampling.NEAREST).save(
        os.path.join(OUT, "tileset_mock_room_2x.png"))

    # Atlas nativo 32px para importar/testar na Unity, + manifesto simples.
    atlas_sheet("common").save(os.path.join(OUT, "tileset_common_atlas.png"))
    atlas_sheet("climbable").save(os.path.join(OUT, "tileset_climbable_atlas.png"))
    with open(os.path.join(OUT, "tileset_manifest.csv"), "w", encoding="utf-8") as f:
        f.write("family,tile,column,row,size,padding,collision,layer\n")
        for family, layer, collision in (
            ("common", "Default", "Grid"),
            ("climbable", "ClimbableWall", "Grid"),
        ):
            for i, name in enumerate(TILE_NAMES):
                f.write(f"{family},{name},{i % 7},{i // 7},32,1,{collision},{layer}\n")

    # Overview único para verificação rápida.
    canvas_w, canvas_h = 1900, 1760
    canvas = Image.new("RGBA", (canvas_w, canvas_h), BG)
    d = ImageDraw.Draw(canvas)
    d.text((40, 22), "PINDORAMA — TILESET MOCK", font=font(38, True), fill=WHITE)
    d.text((40, 70), "Validação de escala, paleta, leitura e composição — não é arte final",
           font=font(18), fill=(170, 190, 200, 255))

    canvas.alpha_composite(common_card, (40, 110))
    canvas.alpha_composite(climb_card, (40, 460))
    canvas.alpha_composite(props, (1070, 110))

    mock_w = int(W * 1.45)
    mock_h = int(H * 1.45)
    mock_scaled = mock.resize((mock_w, mock_h), Image.Resampling.NEAREST)
    canvas.alpha_composite(mock_scaled, (40, 800))
    d.text((40, 800 + mock_h + 10),
           "Mock 1088x612 (34x19.1 u) @ 32 PPU, upscaled 1.45x. "
           "Climbable = teal; common = cinza; hazard = vermelho.",
           font=font(15), fill=(170, 190, 200, 255))

    d.text((1070, 700), "CHECKLIST", font=font(20, True), fill=WHITE)
    lines = [
        "1 tile = 32 px = 1 unidade",
        "Player 26x58 px legível",
        "Topo do chão com rim light",
        "Parede teal = escalável",
        "Parede cinza = comum",
        "Perigo vermelho",
        "Fundo sem competir",
        "Sem slopes/one-way",
    ]
    for i, line in enumerate(lines):
        d.text((1070, 735 + i * 27), "• " + line, font=font(14), fill=(190, 205, 215, 255))

    canvas.save(os.path.join(OUT, "tileset_overview.png"))
    print("OK:", OUT)
    for name in ("tileset_overview.png", "tileset_common_card.png",
                 "tileset_climbable_card.png", "tileset_props_card.png",
                 "tileset_mock_room.png", "tileset_mock_room_2x.png"):
        path = os.path.join(OUT, name)
        print(name, os.path.getsize(path), "bytes")


if __name__ == "__main__":
    main()
