using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private PlayerSpawner playerSpawner;

    // Map, Player 순서로 생성
    void Start()
    {
        mapGenerator.GenerateMap();
        playerSpawner.SpawnPlayer();
    }
}