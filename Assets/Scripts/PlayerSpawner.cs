using UnityEngine;

public class PlayerSpawner : MonoBehaviour

{
    [Header("Player Template")]
    [SerializeField] private GameObject playerTemplate;

    public void SpawnPlayer()
    {
        Vector3 spawnPoint = new Vector3(0, -5, 0); // 중앙 오브젝트 아래
        string characterIdToLoad = GameSession.SelectedCharacterId;
        var (data, sprite) = DataManager.LoadCharacter(characterIdToLoad);

        // 캐릭터 생성
        GameObject playerInstance = Instantiate(playerTemplate, spawnPoint, Quaternion.identity);

        // 능력치 적용
        PlayerController playerController = playerInstance.GetComponent<PlayerController>();

        playerController.Initialize(data, sprite);
    }
}