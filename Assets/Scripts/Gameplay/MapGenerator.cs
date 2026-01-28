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

        // 타일 정보
        int totalTiles = bounds.width * bounds.height;
        Vector3Int[] positions = new Vector3Int[totalTiles];
        TileBase[] tileArray = new TileBase[totalTiles];

        int index = 0;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                positions[index] = new Vector3Int(x, y, 0);

                // 테두리
                if (x == bounds.xMin || x == bounds.xMax - 1 || y == bounds.yMin || y == bounds.yMax - 1)
                {
                    tileArray[index] = selectedTheme.wallTile;
                }
                // 바닥
                else
                {
                    tileArray[index] = selectedTheme.floorTile;
                }

                index++;
            }
        }

        // 한 번에 생성
        tilemap.SetTiles(positions, tileArray);
    }

    public void ClearMap()
    {
        if (tilemap != null)
        {
            tilemap.ClearAllTiles();
        }
    }
}