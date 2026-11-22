#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TwinHook.Editor
{
    /// <summary>
    /// Cria palettes limpas, somente com tiles 32x32, a partir das PNGs
    /// individuais em Art/Pallet/Tilesets. Não altera as palettes antigas.
    /// Menu: Pindorama > Palettes > Criar palettes 32x32 limpas.
    /// </summary>
    public static class CleanTilePaletteSetup
    {
        private const string ArtRoot = "Assets/_Project/Art/Pallet";
        private const string SourceRoot = ArtRoot + "/Tilesets";
        private const string TileRoot = ArtRoot + "/CleanTiles";

        private static readonly string[] Names =
            { "tl", "t", "tr", "l", "c", "r", "bl", "b", "br", "single" };

        [MenuItem("Pindorama/Palettes/Criar palettes 32x32 limpas")]
        public static void Create()
        {
            AssetDatabase.Refresh();
            if (AssetDatabase.IsValidFolder(TileRoot))
                AssetDatabase.DeleteAsset(TileRoot);
            AssetDatabase.CreateFolder(ArtRoot, "CleanTiles");

            Dictionary<string, Tile> grey = CreateTheme("GreyClean", "Grey", "gray");
            Dictionary<string, Tile> green = CreateTheme("GreenClean", "Green", "green");

            CreatePalette("CleanGreyPalette", grey);
            CreatePalette("CleanGreenPalette", green);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("CleanTilePaletteSetup: palettes limpas criadas.");
        }

        private static Dictionary<string, Tile> CreateTheme(string prefix,
                                                            string folder,
                                                            string themeName)
        {
            string tileFolder = TileRoot + "/" + folder;
            AssetDatabase.CreateFolder(TileRoot, folder);

            Dictionary<string, Tile> result = new Dictionary<string, Tile>();
            foreach (string n in Names)
            {
                string texturePath = SourceRoot + "/" + prefix + "_" + n + ".png";
                if (!File.Exists(texturePath))
                {
                    Debug.LogWarning("CleanTilePaletteSetup: falta " + texturePath);
                    continue;
                }

                ConfigureTexture(texturePath);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
                if (sprite == null)
                {
                    Debug.LogWarning("CleanTilePaletteSetup: sprite não importado " + texturePath);
                    continue;
                }

                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                tile.color = Color.white;
                tile.colliderType = Tile.ColliderType.Grid;

                string assetPath = tileFolder + "/tile_" + themeName + "_" + n + ".asset";
                AssetDatabase.CreateAsset(tile, assetPath);
                result[n] = tile;
            }
            return result;
        }

        private static void ConfigureTexture(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32f;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 64;
            importer.SaveAndReimport();
        }

        private static void CreatePalette(string name, Dictionary<string, Tile> themeTiles)
        {
            string path = ArtRoot + "/" + name + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                AssetDatabase.DeleteAsset(path);

            GameObject root = new GameObject(name);
            Grid grid = root.AddComponent<Grid>();
            grid.cellSize = Vector3.one;

            GameObject layer = new GameObject("Layer");
            layer.transform.SetParent(root.transform);
            Tilemap tilemap = layer.AddComponent<Tilemap>();
            TilemapRenderer renderer = layer.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = 0;

            for (int i = 0; i < Names.Length; i++)
            {
                if (themeTiles.TryGetValue(Names[i], out Tile tile))
                {
                    tilemap.SetTile(new Vector3Int(i % 5, i / 5, 0), tile);
                }
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            AssetDatabase.ImportAsset(path);

            // Tile Palette procura um subasset GridPalette dentro do prefab.
            GridPalette gridPalette = ScriptableObject.CreateInstance<GridPalette>();
            gridPalette.cellSizing = GridPalette.CellSizing.Automatic;
            gridPalette.hideFlags = HideFlags.None;
            AssetDatabase.AddObjectToAsset(gridPalette, path);
            AssetDatabase.ImportAsset(path);
        }
    }
}
#endif
