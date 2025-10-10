using UnityEngine;
using Unity.InferenceEngine;
using System.Collections.Generic;

public class ModelManager : MonoBehaviour
{
    // 인스펙터 변수
    [Header("Model Assets")]
    [SerializeField] private ModelAsset cnnModelAsset;
    [SerializeField] private ModelAsset ganModelAsset;

    // AI 모델 실행 엔진
    private Worker cnnWorker;
    private Worker ganWorker;

    // 사용 가능한 클래스 고정 (CNN이 사전 학습)
    public readonly Dictionary<int, string> classNames = new()
    {
        { 0, "Bird" },
        { 1, "Dog" },
        { 2, "Fish" }
    };

    // 모델 입력 값 크기
    private const int inputSize = 128;
    private const int latentDim = 100;
    private const int OutputImageSize = 64;

    // ONNX Layer 이름 (netron)
    private const string ganLatentInputName = "onnx::Reshape_0";
    private const string ganLabelInputName = "labels";
    private const string ganOutputName = "71";

    // 초기화, 모델 실행
    public void Initialize()
    {
        try
        {
            cnnWorker = new Worker(ModelLoader.Load(cnnModelAsset), BackendType.GPUCompute);
            ganWorker = new Worker(ModelLoader.Load(ganModelAsset), BackendType.GPUCompute);
            Debug.Log("ModelManager start");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"initialize() failed: {ex.Message}");
        }
    }

    // CNN이 입력받은 스케치 구별
    public int RunClassifier(Texture2D inputTexture) // 스케치는 Texture2D
    {
        using var inputTensor = TextureConverter.ToTensor(inputTexture, inputSize, inputSize, 1); // Sentis(-> inference engine)는 Tensor
        cnnWorker.Schedule(inputTensor);

        using var outputTensor = cnnWorker.PeekOutput() as Tensor<float>;
        if (outputTensor == null) // 안그리고 제출해도 생성?
        {
            Debug.LogError("CNN tensor null");
            return -1;
        }

        float[] outputData = outputTensor.DownloadToArray(); // GPU -> CPU
        int bestClassIndex = System.Array.IndexOf(outputData, Mathf.Max(outputData)); // 확률(가능성) 기준
        return bestClassIndex;
    }

    // GAN이 입력받은 클래스로 이미지 생성
    public Texture RunGenerator(int classIndex)
    {
        // 랜덤 노이즈 (z)
        using var latentTensor = new Tensor<float>(new TensorShape(1, latentDim));
        for (int i = 0; i < latentDim; i++)
            latentTensor[i] = UnityEngine.Random.Range(-1f, 1f);

        // CNN에게 받은 클래스
        using var labelTensor = new Tensor<int>(new TensorShape(1), new int[] { classIndex });

        ganWorker.SetInput(ganLatentInputName, latentTensor);
        ganWorker.SetInput(ganLabelInputName, labelTensor);
        ganWorker.Schedule();

        using var outputTensor = ganWorker.PeekOutput(ganOutputName) as Tensor<float>;
        if (outputTensor == null) // 생성 실패
        {
            Debug.LogError("GAN tensor null");
            return null;
        }

        float[] tensorData = outputTensor.DownloadToArray();

        // [-1, 1] -> [0, 1] 범위 변환 (색 밝기)
        for (int i = 0; i < tensorData.Length; i++)
        {
            tensorData[i] = (tensorData[i] + 1.0f) / 2.0f;
        }
        using var finalTensor = new Tensor<float>(outputTensor.shape, tensorData);

        // 이미지는 Texture
        var outputTexture = new RenderTexture(OutputImageSize, OutputImageSize, 0);
        TextureConverter.RenderToTexture(finalTensor, outputTexture);
        return outputTexture;
    }

    // 메모리 해제
    void OnDestroy()
    {
        cnnWorker?.Dispose();
        ganWorker?.Dispose();
    }
}