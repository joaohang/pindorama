"""Gera cartões-conceito das poses ausentes do personagem cyberpunk.

Não altera assets nem o jogo: serve apenas para revisão de pose/silhueta.
Fonte das sprites: projeto local jogoTeste (Cyberpunk City 2 / Ansimuz).
"""

from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import math

ROOT = Path(__file__).resolve().parents[1]
SRC = Path(r"C:\Users\joaov\OneDrive\Documentos\deepseek-harness\default-workspace\jogoTeste\New Unity Project\Assets\GrapplingHook\Art\Cyberpunk City 2\Sprites\Player")
OUT = ROOT / "docs" / "images" / "pose_concepts"
OUT.mkdir(parents=True, exist_ok=True)

BG = (10, 15, 21, 255)
CARD = (17, 25, 33, 255)
BORDER = (44, 58, 70, 255)
TEAL = (43, 217, 196, 255)
WHITE = (230, 241, 242, 255)
MUTED = (165, 182, 194, 255)
RED = (255, 77, 90, 255)
BLUE = (77, 166, 255, 255)
YELLOW = (255, 210, 74, 255)

F = ImageFont.truetype(r"C:\Windows\Fonts\arial.ttf", 15)
FB = ImageFont.truetype(r"C:\Windows\Fonts\arialbd.ttf", 22)
FT = ImageFont.truetype(r"C:\Windows\Fonts\arialbd.ttf", 34)
FS = ImageFont.truetype(r"C:\Windows\Fonts\arial.ttf", 12)

hang = [Image.open(SRC / "Hang" / f"Hang{i}.png").convert("RGBA") for i in range(1, 5)]
jump = [Image.open(SRC / "Jump" / f"Jump{i}.png").convert("RGBA") for i in range(1, 8)]
idle = [Image.open(SRC / "Idle" / f"Idle{i}.png").convert("RGBA") for i in range(1, 5)]

W, H = 1600, 1280
canvas = Image.new("RGBA", (W, H), BG)
d = ImageDraw.Draw(canvas)
d.text((36, 22), "PINDORAMA — POSES FALTANTES (CONCEPT)", font=FT, fill=WHITE)
d.text((36, 64), "Derivado dos sprites atuais; NÃO aplicado no jogo. Setas = contatos/direção.", font=F, fill=MUTED)
d.text((36, 86), "Referências públicas analisadas: Ruby Wall Slide & Hurt (Summer Engine) e Pixel Climber Girl (AI Spritesheet Maker).", font=FS, fill=MUTED)

# Base strip
y0 = 120
d.text((36, y0), "BASE ATUAL — Hang 1–4", font=FB, fill=TEAL)
x = 36
for i, im in enumerate(hang):
    thumb = im.resize((96, 96), Image.Resampling.NEAREST)
    canvas.alpha_composite(thumb, (x, y0 + 30))
    d.text((x, y0 + 130), f"Hang{i + 1}", font=FS, fill=MUTED)
    x += 120


def checker(card):
    dd = ImageDraw.Draw(card)
    for gx in range(0, card.width, 32):
        dd.line([gx, 0, gx, card.height], fill=(21, 30, 39, 255))
    for gy in range(0, card.height, 32):
        dd.line([0, gy, card.width, gy], fill=(21, 30, 39, 255))


def draw_wall(card, side):
    dd = ImageDraw.Draw(card)
    w = 54
    x = card.width - w if side == "right" else 0
    top = 44
    dd.rectangle([x, top, x + w, card.height], fill=(28, 43, 52, 255))
    for yy in range(top + 8, card.height, 28):
        dd.line([x, yy, x + w, yy], fill=(37, 56, 66, 255))
    edge = x if side == "right" else x + w - 3
    dd.rectangle([edge, top, edge + 3, card.height], fill=TEAL)
    for yy in range(top + 16, card.height, 36):
        if side == "right":
            dd.line([x - 14, yy, x - 4, yy + 8, x - 14, yy + 16], fill=TEAL, width=2)
        else:
            dd.line([x + w + 14, yy, x + w + 4, yy + 8, x + w + 14, yy + 16], fill=TEAL, width=2)


def marker(card, x, y, color=TEAL, r=4):
    dd = ImageDraw.Draw(card)
    dd.ellipse([x - r, y - r, x + r, y + r], outline=color, width=2)
    dd.line([x - r - 3, y, x + r + 3, y], fill=color, width=1)
    dd.line([x, y - r - 3, x, y + r + 3], fill=color, width=1)


def arrow(card, x1, y1, x2, y2, color=TEAL):
    dd = ImageDraw.Draw(card)
    dd.line([x1, y1, x2, y2], fill=color, width=2)
    a = math.atan2(y2 - y1, x2 - x1)
    for s in (-0.5, 0.5):
        dd.line([x2, y2, x2 - 9 * math.cos(a + s), y2 - 9 * math.sin(a + s)], fill=color, width=2)


def tint_sprite(im, color, alpha=90):
    overlay = Image.new("RGBA", im.size, color)
    # Preserva o alpha original: pinta só os pixels opacos.
    a = im.getchannel("A").point(lambda v: int(v * alpha / 255))
    overlay.putalpha(a)
    return Image.alpha_composite(im, overlay)


def card(title, note, base, wall_side=None, flip=False, tint=None, rotate=0,
         scale=1.5, arrows=None, markers=None, extra=None):
    c = Image.new("RGBA", (500, 300), CARD)
    checker(c)
    dd = ImageDraw.Draw(c)
    dd.rectangle([0, 0, 499, 299], outline=BORDER, width=1)
    title_x = 70 if wall_side == "left" else 12
    dd.text((title_x, 8), title, font=FB, fill=WHITE)
    dd.text((12, 270), note, font=FS, fill=MUTED)

    if wall_side:
        draw_wall(c, wall_side)

    im = base.copy()
    if flip:
        im = im.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
    if tint:
        im = tint_sprite(im, tint)
    if rotate:
        im = im.rotate(rotate, expand=True, resample=Image.Resampling.BICUBIC)
    if scale != 1.0:
        im = im.resize((int(im.width * scale), int(im.height * scale)),
                       Image.Resampling.NEAREST)

    if wall_side == "right":
        center_x = 420
    elif wall_side == "left":
        center_x = 80
    else:
        center_x = 250
    px = center_x - im.width // 2
    py = 150 - im.height // 2 + 4
    c.alpha_composite(im, (px, py))
    if markers:
        for m in markers:
            mx, my = m[0], m[1]
            mc = m[2] if len(m) > 2 else TEAL
            marker(c, mx, my, mc)
    if arrows:
        for a in arrows:
            arrow(c, *a)
    if extra:
        extra(c)
    return c


def add_dust(c):
    dd = ImageDraw.Draw(c)
    for x, y, r in [(190, 232, 7), (210, 240, 5), (232, 234, 6), (170, 242, 4)]:
        dd.ellipse([x - r, y - r, x + r, y + r], outline=(140, 120, 100, 180), width=2)


def speedlines(c):
    dd = ImageDraw.Draw(c)
    for yy in (100, 120, 140, 160, 180):
        dd.line([40, yy, 150, yy], fill=(100, 200, 210, 120), width=1)


def dual_ropes(c):
    dd = ImageDraw.Draw(c)
    dd.line([250, 130, 100, 40], fill=(255, 77, 90, 200), width=2)
    dd.line([250, 150, 400, 40], fill=(77, 166, 255, 200), width=2)
    dd.ellipse([94, 34, 106, 46], outline=RED, width=2)
    dd.ellipse([394, 34, 406, 46], outline=BLUE, width=2)


cards = []
cards.append(card("WALL CLING — DIREITA", "2 mãos + 2 pés; quadril perto. Base: Hang2.", hang[1],
                  wall_side="right", markers=[(440, 120), (440, 160), (440, 210), (420, 245)],
                  arrows=[(385, 165, 435, 165, TEAL)]))
cards.append(card("WALL CLING — ESQUERDA", "Espelho do anterior. Base: Hang2.", hang[1], wall_side="left", flip=True,
                  markers=[(60, 120), (60, 160), (60, 210), (80, 245)],
                  arrows=[(115, 165, 65, 165, TEAL)]))
cards.append(card("WALL CLIMB — SOBE", "Ciclo diagonal: mão oposta ao pé. Base: Jump4.", jump[3], wall_side="right",
                  markers=[(440, 115), (440, 205), (420, 245)],
                  arrows=[(320, 220, 320, 115, TEAL)]))
cards.append(card("WALL CLIMB — DESCE", "Controle excêntrico; joelhos dobrados. Base: Hang1.", hang[0], wall_side="right",
                  markers=[(440, 135), (440, 200), (420, 235)],
                  arrows=[(320, 110, 320, 220, TEAL)]))
cards.append(card("FALL — QUEDA", "Silhueta aberta; braços acima. Base: Jump6.", jump[5],
                  arrows=[(250, 70, 250, 255, (120, 200, 210, 180))]))
cards.append(card("LAND — ATERRISSAGEM", "Crouch, braços abertos, poeira. Base: Jump1.", jump[0],
                  scale=1.65, extra=add_dust))
cards.append(card("HURT — DANO", "Recuo + flash vermelho. Base: Jump3.", jump[2],
                  tint=RED, rotate=-8, markers=[(275, 135, RED)],
                  arrows=[(305, 150, 235, 195, RED)]))
cards.append(card("BOOST — PÊNDULO", "Corpo estendido; VFX ciano. Base: Hang4.", hang[3], extra=speedlines))
cards.append(card("DUAL HOOK — DOIS CABOS", "Hang4 + segunda corda; pose real exige arte nova.", hang[3],
                  extra=dual_ropes))

for idx, c in enumerate(cards):
    canvas.alpha_composite(c, (36 + (idx % 3) * 510, 300 + (idx // 3) * 300))

d.text((36, 1208), "Leitura de pose", font=FB, fill=TEAL)
d.text((210, 1214), "wall cling = 4 contatos + quadril perto; climb = alternância diagonal; hurt = recuo + flash; boost = extensão + VFX.", font=FS, fill=MUTED)
d.text((36, 1240), "Conceito derivado dos sprites atuais. Nada foi importado/aplicado no jogo.", font=FS, fill=YELLOW)

path = OUT / "missing_poses_concept.png"
canvas.convert("RGB").save(path)
print(path, path.stat().st_size)
