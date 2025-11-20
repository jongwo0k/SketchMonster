using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    // 중앙 관리 지점, 모든 스크립트에서 접근 가능
    public static GameManager Instance { get; private set; }

    // AI 모델 관리
    [Header("Managers")]
    [SerializeField] private ModelManager modelManager;

    // UI Panel 관리 (그림판)
    [Header("UI Panels")]
    [SerializeField] private GameObject drawingPanel;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject resultPanel;

    // 캐릭터 선택창
    [Header("Character Select UI")]
    [SerializeField] private List<RawImage> characterResultImages;
    [SerializeField] private List<Button> characterSelectButtons;
    [SerializeField] public List<TextMeshProUGUI> characterStatTexts;

    // 생성된 이미지(캐릭터) 임시 저장
    private List<CharacterData> generatedCharacters;

    // 싱글톤 인스턴스 설정
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // -----------------  초기화  ----------------
    void Start()
    {
        InitModelManager();
        InitPanels();
    }

    private void InitModelManager()
    {
        modelManager.Initialize();
    }

    private void InitPanels()
    {
        drawingPanel.SetActive(true);
        loadingPanel.SetActive(false);
        resultPanel.SetActive(false);
    }
    // -------------------------------------------

    // 캐릭터 생성 (시작)
    public void StartCharacterCreation(Texture2D sketchTexture, int strokeCount, int remainSeconds)
    {
        GameSession.OriginalSketch = sketchTexture; // 원본 스케치 저장

        Debug.Log($"Sketch submission complete - Stroke Count: {strokeCount}, Remain Time: {remainSeconds} sec");
        StartCoroutine(CharacterCreationRoutine(sketchTexture, strokeCount, remainSeconds)); // 게임이 멈추는 것 방지
    }

    // 캐릭터 생성 (전체 과정)
    private IEnumerator CharacterCreationRoutine(Texture2D sketch, int strokeCount, int remainSeconds)
    {
        // UI 전환 (로딩 창 활용)
        drawingPanel.SetActive(false);
        loadingPanel.SetActive(true);

        // 초기화
        generatedCharacters = new List<CharacterData>();

        // 이미지 3장 생성
        for (int i = 0; i < 3; i++)
        {
            yield return GenerateCharacter(i, sketch, strokeCount, remainSeconds); // 순차적
        }

        // UI 전환 (로딩 창 활용)
        loadingPanel.SetActive(false);
        resultPanel.SetActive(true);

        Debug.Log("Image creation complete - Select Character");
    }

    // 캐릭터 생성 (실제 동작)
    private IEnumerator GenerateCharacter(int index, Texture2D sketch, int strokeCount, int remainSeconds)
    {
        // AI 모델이 생성
        int classIndex = modelManager.RunClassifier(sketch);
        string className = modelManager.classNames[classIndex];
        Texture generatedTexture = modelManager.RunGenerator(classIndex);

        // 배경 제거
        Texture2D finalCharacterTexture = ImageProcess.RemoveBackground(generatedTexture);

        // 능력치 부여
        CharacterData newCharacterData = CharacterStatCalculator.Calculate(className, strokeCount, remainSeconds);

        // 데이터, UI 업데이트
        generatedCharacters.Add(newCharacterData);
        characterResultImages[index].texture = finalCharacterTexture;
        RegisterCharacterSelectButton(index);

        // 선택창에 캐릭터 정보 표시 (이미지 하단)
        characterStatTexts[index].text = $"Rank: {newCharacterData.grade}\n" +
                                         $"HP: {newCharacterData.hp:F0}\n" +
                                         $"Attack: {newCharacterData.attack:F0}\n" +
                                         $"Speed: {newCharacterData.speed:F1}";

        Debug.Log($"Generated Character #{index + 1} - Grade: {newCharacterData.grade}");

        // 메모리 해제
        if (generatedTexture != null)
        {
            if (generatedTexture is RenderTexture rt)
            {
                rt.Release();
            }
            Destroy(generatedTexture);
        }
        yield return null;
    }

    // 캐릭터 선택 버튼
    private void RegisterCharacterSelectButton(int index)
    {
        characterSelectButtons[index].onClick.AddListener(() => OnCharacterSelected(index));
    }

    // 선택된 캐릭터 이미지와 데이터(능력치) 저장
    public void OnCharacterSelected(int index)
    {
        CharacterData selectedCharacter = generatedCharacters[index];
        Texture2D selectedImage = characterResultImages[index].texture as Texture2D;

        Debug.Log($"Selected Character: #{index + 1}");
        Debug.Log($"Class: {selectedCharacter.className}, Grade: {selectedCharacter.grade}");
        Debug.Log($"HP: {selectedCharacter.hp}, Attack: {selectedCharacter.attack}, Speed: {selectedCharacter.speed}");

        if (selectedImage != null)
        {
            DataManager.SaveCharacter(selectedCharacter, selectedImage);
            Debug.Log("Character data saved");
        }
        else
        {
            Debug.LogError("Failed save data");
        }

        // 선택된 캐릭터의 ID를 세션에 저장
        GameSession.SelectedCharacterId = selectedCharacter.characterId;

        // 선택되지 않은 캐릭터는 적으로 등장
        GameSession.EnemyTextures = new List<Texture2D>();
        for (int i = 0; i < characterResultImages.Count; i++)
        {
            if (i != index)
            {
                GameSession.EnemyTextures.Add(characterResultImages[i].texture as Texture2D);
            }
        }

        // 비활성화 (셋 중 하나만 선택 가능)
        foreach (var button in characterSelectButtons)
        {
            button.interactable = false;
        }

        // 게임 Scene 진입
        SceneManager.LoadScene("GameScene");
    }
}