using UnityEngine;

public class PlayerSpawner : MonoBehaviour

{
    [Header("Player Template")]
    [SerializeField] private GameObject playerTemplate;

    public GameObject SpawnPlayer()
    {
        Vector3 spawnPoint = new Vector3(0, -6, 0); // 중앙 오브젝트 아래

#if UNITY_WEBGL // Web 에선 DataManager대신 GameSession에서
        CharacterData data = GameSession.SelectedCharacterData;
        Sprite sprite = GameSession.SelectedCharacterSprite;
#else
        string characterIdToLoad = GameSession.SelectedCharacterId;
        var (data, sprite) = DataManager.LoadCharacter(characterIdToLoad);
#endif

        // 캐릭터 생성
        GameObject playerInstance = Instantiate(playerTemplate, spawnPoint, Quaternion.identity);

        // 능력치 적용
        PlayerController playerController = playerInstance.GetComponent<PlayerController>();

        playerController.Initialize(data, sprite);

        return playerInstance;
    }
}