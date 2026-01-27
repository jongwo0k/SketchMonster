using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[System.Serializable]
public struct MapTheme
{
    public TileBase wallTile;
    public TileBase floorTile;
}
public class MapGenerator : MonoBehaviour
{
    [Header("Map")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private int width = 70;
    [SerializeField] private int height = 34;


    [Header("Tile")]
    [SerializeField] private List<MapTheme> mapTheme;

    public RectInt MapBounds
    {
        get
        {
            int minX = -width / 2;
            int minY = -height / 2;
            return new RectInt(minX, minY, width, height);
        }
    }

    public void GenerateMap()
    {
        RectInt bounds = MapBounds;
        MapTheme selectedTheme = mapTheme[Random.Range(0, mapTheme.Count)];
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                // Å×µÎ¸®
                if (x == bounds.xMin || x == bounds.xMax - 1 || y == bounds.yMin || y == bounds.yMax - 1)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), selectedTheme.wallTile);
                }
                // ¹Ù´Ú
                else
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), selectedTheme.floorTile);
                }
            }
        }
    }

    public void ClearMap()
    {
        if (tilemap != null)
        {
            tilemap.ClearAllTiles();
        }
    }
}