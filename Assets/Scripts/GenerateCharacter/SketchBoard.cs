using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SketchBoard : MonoBehaviour
{
    // 인스펙터 변수
    // 그림판 영역
    [Header("UI Components")]
    [SerializeField] private RawImage drawingArea;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Top Buttons")]
    [SerializeField] private Button penButton;
    [SerializeField] private Button eraserButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button submitButtonTop;

    // 시간 종료 후 나타남
    [Header("Timeout Buttons")]
    [SerializeField] private GameObject bottomPanel;
    [SerializeField] private Button resetButtonBottom;
    [SerializeField] private Button submitButtonBottom;

    // 선 굵기는 CNN이 학습한 데이터와 유사해야 함 (크기 고정)
    [Header("Drawing Settings")]
    [SerializeField] private int canvasSize = 512;       // 그림판 크기 고정
    [SerializeField] private int brushSize = 8;          // 펜, 지우개
    [SerializeField] private float timerDuration = 20f;

    // 그림판 변수
    private Texture2D texture;
    private Color penColor = Color.black;
    private Color eraserColor = Color.white;
    private Color currentColor;
    private Vector2 lastMousePosition;                  // 이전 프레임의 마우스 위치
    private bool isDrawing = false;
    private bool isSubmitted = false;

    // 능력치에 사용될 변수
    private float timer;            // remainTime
    private int strokeCount = 0;
    
    // 그림판 모드
    private enum DrawMode { 
        None,
        Pen,
        Eraser
    }
    private DrawMode currentMode = DrawMode.Pen;

    // UI Raycast
    private GraphicRaycaster graphicRaycaster;
    private EventSystem eventSystem;
    private Canvas parentCanvas;
    private GameManager gameManager;

    // Buffer
    private Color32[] pixelBuffer;
    private bool isDirty = false;

    void Start()
    {
        if (!TryInitializeGameManager()) return;
        if (!TryInitializeCanvasComponents()) return;

        InitializeUI();
        InitializeListeners();

        // UI Layout 계산 완료 후 Texture 생성
        StartCoroutine(InitAfterLayout());
    }

    void Update()
    {
        if (isSubmitted || timer <= 0) return; // Timer 종료 or 제출
        HandleDrawingInput();
    }

    // Apply 일괄 적용
    void LateUpdate()
    {
        if(texture == null) return;

        if (isDirty)
        {
            texture.SetPixels32(pixelBuffer);
            texture.Apply();
            isDirty = false;
        }
    }

    // ------------------------------ 초기화 ---------------------------------
    private bool TryInitializeGameManager()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found");
            enabled = false; // 스크립트 비활성화
            return false;
        }
        return true;
    }

    private bool TryInitializeCanvasComponents()
    {
        // 캔버스
        parentCanvas = drawingArea.GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("drawingArea Error");
            enabled = false;
            return false;
        }

        // Raycast
        graphicRaycaster = parentCanvas.GetComponent<GraphicRaycaster>();
        if (graphicRaycaster == null)
        {
            Debug.LogError("graphicRaycaster Error");
            enabled = false;
            return false;
        }

        // Event
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("EventSystem not found");
            enabled = false;
            return false;
        }

        return true;
    }

    private void InitializeUI()
    {
        bottomPanel.SetActive(false);
        resultText.gameObject.SetActive(false);
        timer = timerDuration;
    }

    private void InitializeListeners()
    {
        // 버튼 별 메서드
        penButton.onClick.AddListener(SetPenMode);
        eraserButton.onClick.AddListener(SetEraserMode);
        clearButton.onClick.AddListener(ClearCanvas);
        submitButtonTop.onClick.AddListener(SubmitDrawing);
        resetButtonBottom.onClick.AddListener(ResetBoard);
        submitButtonBottom.onClick.AddListener(SubmitDrawing);
    }

    // UI Layout 계산 종료 후 Texture 초기화 (Start에서 한 프레임 대기)
    private IEnumerator InitAfterLayout()
    {
        Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame();

        texture = new Texture2D(canvasSize, canvasSize, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        drawingArea.texture = texture;

        pixelBuffer = new Color32[canvasSize * canvasSize];

        ClearCanvas();
        SetPenMode(); // 기본 모드 - Pen
        StartCoroutine(CountdownTimer());
    }

    // ------------------------------ 그리기 ---------------------------------
    private void HandleDrawingInput()
    {
        // 모드에 따라 카메라 조절
        Camera cam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;

        // 마우스, drawingArea 좌표 일치
        bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(drawingArea.rectTransform, Input.mousePosition, cam, out Vector2 localPoint);

        bool blockedByOtherUI = IsOnInteractiveUI();

        // 마우스 처음 눌렀을 때
        if (Input.GetMouseButtonDown(0) && isInside && currentMode != DrawMode.None && !blockedByOtherUI)
        {
            isDrawing = true;
            if (currentMode == DrawMode.Pen) strokeCount++;
            lastMousePosition = localPoint;
        }

        // 누른 상태로 드래그
        if (isDrawing && Input.GetMouseButton(0) && isInside)
        {
            DrawLine(lastMousePosition, localPoint);
            lastMousePosition = localPoint;
        }

        // 마우스에서 손을 뗌
        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }
    }

    // 버튼 클릭과 스케치 하려고 클릭한 경우 구분
    private bool IsOnInteractiveUI()
    {
        PointerEventData ped = new PointerEventData(eventSystem) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(ped, results);

        // Raycast 가장 위에 있는 UI 요소 기준
        foreach (var r in results)
        {
            GameObject go = r.gameObject;
            if (go == drawingArea.gameObject) return false;

            if (go.transform.IsChildOf(drawingArea.transform))
            {
                if (go.GetComponent<Selectable>() != null) return true;
                continue;
            }

            return true;
        }

        return false;
    }

    // 펜 굵기 만큼 점으로 그리기
    private void DrawDot(int x, int y)
    {
        for (int i = -brushSize; i < brushSize; i++)
        {
            for (int j = -brushSize; j < brushSize; j++)
            {
                if (new Vector2(i, j).magnitude < brushSize)
                {
                    int px = x + i;
                    int py = y + j;
                    if (px >= 0 && px < canvasSize && py >= 0 && py < canvasSize)
                    {
                        pixelBuffer[py * canvasSize + px] = currentColor;
                    }
                }
            }
        }
    }

    // 마우스 이동 시 점 끊김 보간
    private void DrawLine(Vector2 start, Vector2 end)
    { 
        Rect rect = drawingArea.rectTransform.rect;
        float displayWidth = rect.width;
        float displayHeight = rect.height;

        int x0 = (int)((start.x + displayWidth * 0.5f) * canvasSize / displayWidth);
        int y0 = (int)((start.y + displayHeight * 0.5f) * canvasSize / displayHeight);
        int x1 = (int)((end.x + displayWidth * 0.5f) * canvasSize / displayWidth);
        int y1 = (int)((end.y + displayHeight * 0.5f) * canvasSize / displayHeight);

        // 범위 체크
        x0 = Mathf.Clamp(x0, 0, canvasSize - 1);
        y0 = Mathf.Clamp(y0, 0, canvasSize - 1);
        x1 = Mathf.Clamp(x1, 0, canvasSize - 1);
        y1 = Mathf.Clamp(y1, 0, canvasSize - 1);

        float distance = Vector2.Distance(start, end);

        if (distance > 1f)
        {
            int steps = Mathf.CeilToInt(distance);
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                int lerpX = (int)Mathf.Lerp(x0, x1, t);
                int lerpY = (int)Mathf.Lerp(y0, y1, t);
                DrawDot(lerpX, lerpY);
            }
        }
        else
        {
            DrawDot(x1, y1);
        }

        isDirty = true;
    }

    // ----------------------------------------------------------------------------

    // 타이머 관리
    private IEnumerator CountdownTimer()
    {
        while (timer > 0 && !isSubmitted)
        {
            timer -= Time.deltaTime;
            timerText.text = $"Timer: {Mathf.CeilToInt(timer)}";
            yield return null;
        }

        if (!isSubmitted)
        {
            timerText.text = "Time Out";
            bottomPanel.SetActive(true);
        }
    }

    // 다시 그리기
    private void RestartTimer()
    {
        StopAllCoroutines();
        timer = timerDuration;
        bottomPanel.SetActive(false);
        StartCoroutine(CountdownTimer());
    }

    public void ResetBoard()
    {
        isSubmitted = false;
        ClearCanvas();
        RestartTimer();
        resultText.gameObject.SetActive(false);
    }

    // 버튼 클릭 (Public)
    public void SetPenMode()
    {
        currentMode = DrawMode.Pen;
        currentColor = penColor;
    }

    public void SetEraserMode()
    {
        currentMode = DrawMode.Eraser;
        currentColor = eraserColor;
    }

    public void ClearCanvas()
    {
        Color32 white = new Color32(255, 255, 255, 255);
        for (int i = 0; i < pixelBuffer.Length; i++)
            pixelBuffer[i] = white;

        strokeCount = 0;
        isDirty = true;
    }

    public void SubmitDrawing()
    {
        if (isSubmitted) return; // 중복 제출 방지

        isSubmitted = true;
        StopAllCoroutines();
        int remainSeconds = Mathf.CeilToInt(Mathf.Max(0, timer));
        timerText.text = "Submitted";

        // 최종 Apply
        texture.SetPixels32(pixelBuffer);
        texture.Apply();

        Debug.Log($"Stroke Count: {strokeCount}, Remain Time: {remainSeconds} sec");
        gameManager.StartCharacterCreation(texture, strokeCount, remainSeconds);
    }

    void OnDestroy()
    {
        if (texture != null)
        {
            Destroy(texture);
            texture = null;
        }
    }
}