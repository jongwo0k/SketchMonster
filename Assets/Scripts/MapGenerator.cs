using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("Map")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private int width = 50;
    [SerializeField] private int height = 50;

    [Header("Tile")]
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase floorTile;
    // 배열로 수정해서 스테이지마다 다른 타일 랜덤 생성

    [ContextMenu("EDITOR: Generate Map")]
    public void GenerateMap()
    {
        int minX = -width / 2;
        int maxX = width / 2;
        int minY = -height / 2;
        int maxY = height / 2;

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                // 테두리
                if (x == minX || x == maxX - 1 || y == minY || y == maxY - 1)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                }
                // 바닥
                else
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                }
            }
        }
    }

    [ContextMenu("EDITOR: Destroy Map")]
    public void ClearMap()
    {
        if (tilemap != null)
        {
            tilemap.ClearAllTiles();
        }
    }
}