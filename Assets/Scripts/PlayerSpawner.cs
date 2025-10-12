using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    // 덮어씌울 캐릭터 템플릿
    [Header("Player Template")]
    [SerializeField] private GameObject playerTemplate;

    void Start()
    {
        // 캐릭터 ID 불러오기
        string characterIdToLoad = GameSession.SelectedCharacterId;

        if (string.IsNullOrEmpty(characterIdToLoad))
        {
            Debug.LogError("Character Load failed - Check ID");
            return;
        }

        // 캐릭터 데이터 불러오기
        var (data, sprite) = DataManager.LoadCharacter(characterIdToLoad);

        if (data == null || sprite == null)
        {
            Debug.LogError($"Character Load failed - Check Data: {characterIdToLoad}");
            return;
        }

        // 프리팹 생성
        GameObject playerInstance = Instantiate(playerTemplate, Vector3.zero, Quaternion.identity);

        // 능력치 적용
        PlayerController playerController = playerInstance.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.Initialize(data, sprite);
            Debug.Log($"'{data.className}' Character Load Succeed");
        }
    }
}