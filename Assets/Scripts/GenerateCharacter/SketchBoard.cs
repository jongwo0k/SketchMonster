using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class SketchBoard : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // 인스펙터 변수
    // 그림판 영역
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI resultText;
    private RawImage drawingArea;

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

    // 그림판 변수
    private Texture2D texture;
    private Color32 penColor = new Color32(0, 0, 0, 255);
    private Color32 eraserColor = new Color32(255, 255, 255, 255);
    private Color32 currentColor;
    private Vector2 lastMousePosition;                  // 이전 프레임의 마우스 위치
    private bool isDrawing = false;
    private bool isSubmitted = false;

    // 능력치에 사용될 변수
    private float timer;                                // remainTime
    private int strokeCount = 0;

    // 그림판 모드
    private enum DrawMode
    {
        None,
        Pen,
        Eraser
    }
    private DrawMode currentMode = DrawMode.Pen;

    private GenerationManager generationManager;

    // Buffer
    private Color32[] pixelBuffer;
    private bool isDirty = false;
    private bool isInitialized = false;

    void Start()
    {
        if (!TryInitializeGenerationManager()) return;

        drawingArea = GetComponent<RawImage>();

        InitializeUI();
        InitializeListeners();

        // UI Layout 계산 완료 후 Texture 생성
        StartCoroutine(InitAfterLayout());
    }

    // Apply 일괄 적용
    void LateUpdate()
    {
        if (texture == null) return;

        if (isDirty)
        {
            texture.SetPixels32(pixelBuffer);
            texture.Apply();
            isDirty = false;
        }
    }

    // ------------------------------ 초기화 ---------------------------------
    private void OnEnable()
    {
        if (!isInitialized) return;

        ResetBoard();
    }

    private bool TryInitializeGenerationManager()
    {
        generationManager = GenerationManager.Instance;
        if (generationManager == null)
        {
            Debug.LogError("GenerationManager not found");
            enabled = false; // 스크립트 비활성화
            return false;
        }
        return true;
    }

    private void InitializeUI()
    {
        bottomPanel.SetActive(false);
        resultText.gameObject.SetActive(false);
        timer = GameConfig.Data.sketchDuration;
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

        isInitialized = true;
    }

    // ------------------------------ 입력 처리 ---------------------------------
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isSubmitted || timer <= 0) return;
        if (currentMode == DrawMode.None) return;

        isDrawing = true;
        if (currentMode == DrawMode.Pen) strokeCount++;
        lastMousePosition = GetLocalPoint(eventData);

        DrawLine(lastMousePosition, lastMousePosition);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDrawing) return;
        if (isSubmitted || timer <= 0) return;

        Vector2 localPoint = GetLocalPoint(eventData);
        DrawLine(lastMousePosition, localPoint);
        lastMousePosition = localPoint;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDrawing = false;
    }

    private Vector2 GetLocalPoint(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            drawingArea.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        return localPoint;
    }

    // ------------------------------ 그리기 ---------------------------------

    // 펜 굵기 만큼 점으로 그리기
    private void DrawDot(int x, int y)
    {
        int sqrBrushSize = brushSize * brushSize;
        for (int i = -brushSize; i < brushSize; i++)
        {
            for (int j = -brushSize; j < brushSize; j++)
            {
                if (i * i + j * j < sqrBrushSize)
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
        timer = GameConfig.Data.sketchDuration;
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
        generationManager.StartCharacterCreation(texture, strokeCount, remainSeconds);
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