using UnityEngine;

public static class ImagePreprocess
{
    // 배경 제거 (특정 색 범위 투명화 alpha=0)
    public static Texture2D RemoveBackground(Texture inputTexture)
    {
        Texture2D sourceTexture = ToTexture2D(inputTexture);
        Color32[] pixels = sourceTexture.GetPixels32();

        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 p = pixels[i];
            // R, B값은 높고 G값은 낮은 색상 (Magenta 배경 제거 용도로 사전 학습)
            if (p.r >= 120 && p.b >= 120 && p.g <= 120)
            {
                pixels[i].a = 0;
            }
        }

        Texture2D resultTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false, false);
        resultTexture.SetPixels32(pixels);
        resultTexture.Apply();

        Object.Destroy(sourceTexture);

        return resultTexture;
    }

    // Texture 변환
    public static Texture2D ToTexture2D(Texture tex)
    {
        if (tex is Texture2D) return tex as Texture2D;

        RenderTexture currentActiveRT = RenderTexture.active; // GPU -> CPU
        RenderTexture.active = tex as RenderTexture;

        Texture2D tex2d = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false, false); // gamma=true(default), linear=false
        tex2d.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex2d.Apply();

        RenderTexture.active = currentActiveRT;
        return tex2d;
    }

    // 전처리
    public static Texture2D PreprocessSketch(Texture2D original)
    {
        Texture2D grayscale = ConvertToGrayscale(original);

        Texture2D blurred = GaussianBlur(grayscale, 5);

        Texture2D binarized = BinarizeImage(blurred, 100);

        Rect boundingBox = FindDrawingBounds(binarized);
        Texture2D cropped = CropTexture(binarized, boundingBox);

        Texture2D squared = CenterOnCanvas(cropped);

        Texture2D resized = ResizeTextureNearestNeighbor(squared, 128, 128);

        // 중간 생성물 삭제
        Object.Destroy(grayscale);
        Object.Destroy(blurred);
        Object.Destroy(binarized);
        Object.Destroy(cropped);
        Object.Destroy(squared);

        return resized;
    }

    // Grayscale 변환
    private static Texture2D ConvertToGrayscale(Texture2D source)
    {
        Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);
        Color[] pixels = source.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            float gray = pixels[i].grayscale;
            pixels[i] = new Color(gray, gray, gray);
        }

        result.SetPixels(pixels);
        result.Apply();
        return result;
    }

    // GaussianBlur (5x5 kernel)
    private static Texture2D GaussianBlur(Texture2D source, int kernelSize)
    {
        float sigma = 0.3f * ((kernelSize - 1) * 0.5f - 1) + 0.8f; // 5x5 kernel = 1.1 (GaussianBlur sigma 0)

        float[,] kernel = CreateGaussianKernel(kernelSize, sigma);

        Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);
        Color[] pixels = source.GetPixels();
        Color[] newPixels = new Color[pixels.Length];

        int width = source.width;
        int height = source.height;
        int halfKernel = kernelSize / 2;

        // kernel 적용
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float sum = 0f;
                float weightSum = 0f;

                for (int ky = -halfKernel; ky <= halfKernel; ky++)
                {
                    for (int kx = -halfKernel; kx <= halfKernel; kx++)
                    {
                        // 경계
                        int px = Mathf.Clamp(x + kx, 0, width - 1);
                        int py = Mathf.Clamp(y + ky, 0, height - 1);

                        float weight = kernel[ky + halfKernel, kx + halfKernel];
                        sum += pixels[py * width + px].grayscale * weight;
                        weightSum += weight;
                    }
                }

                float blurred = sum / weightSum;
                newPixels[y * width + x] = new Color(blurred, blurred, blurred);
            }
        }

        result.SetPixels(newPixels);
        result.Apply();
        return result;
    }

    // Gaussian Kernel 생성
    private static float[,] CreateGaussianKernel(int size, float sigma)
    {
        float[,] kernel = new float[size, size];
        int half = size / 2;
        float sum = 0f;

        for (int y = -half; y <= half; y++)
        {
            for (int x = -half; x <= half; x++)
            {
                float value = Mathf.Exp(-(x * x + y * y) / (2 * sigma * sigma));
                kernel[y + half, x + half] = value;
                sum += value;
            }
        }

        // 정규화
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                kernel[y, x] /= sum;
            }
        }

        return kernel;
    }

    // 이진화 (threshold)
    private static Texture2D BinarizeImage(Texture2D source, int threshold)
    {
        Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);
        Color[] pixels = source.GetPixels();

        float thresholdNormalized = threshold / 255f;

        for (int i = 0; i < pixels.Length; i++)
        {
            float gray = pixels[i].grayscale;
            pixels[i] = gray > thresholdNormalized ? Color.white : Color.black;
        }

        result.SetPixels(pixels);
        result.Apply();
        return result;
    }

    // findContours + boundingRect
    private static Rect FindDrawingBounds(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        int width = texture.width;
        int height = texture.height;

        bool[,] visited = new bool[height, width];
        int largestArea = 0;
        Rect largestBounds = new Rect(0, 0, width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!visited[y, x] && pixels[y * width + x].grayscale < 0.5f)
                {
                    // 연결된 영역의 바운딩 박스 계산
                    Rect bounds = FloodFill(pixels, visited, width, height, x, y);
                    int area = (int)(bounds.width * bounds.height);

                    // 가장 큰 영역만
                    if (area > largestArea)
                    {
                        largestArea = area;
                        largestBounds = bounds;
                    }
                }
            }
        }

        // 그림이 없는 경우
        if (largestArea == 0)
        {
            return new Rect(0, 0, width, height);
        }

        // Padding (끊김 방지)
        int pad = 10;
        int x_final = Mathf.Max(0, (int)largestBounds.x - pad);
        int y_final = Mathf.Max(0, (int)largestBounds.y - pad);
        int w_final = Mathf.Min(width - x_final, (int)largestBounds.width + pad * 2);
        int h_final = Mathf.Min(height - y_final, (int)largestBounds.height + pad * 2);

        return new Rect(x_final, y_final, w_final, h_final);
    }

    // 연결된 영역의 bounding box 계산
    private static Rect FloodFill(Color[] pixels, bool[,] visited, int width, int height, int startX, int startY)
    {
        System.Collections.Generic.Queue<Vector2Int> queue = new System.Collections.Generic.Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startY, startX] = true;

        int minX = startX, maxX = startX, minY = startY, maxY = startY;

        while (queue.Count > 0)
        {
            Vector2Int pos = queue.Dequeue();
            int x = pos.x;
            int y = pos.y;

            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);
            minY = Mathf.Min(minY, y);
            maxY = Mathf.Max(maxY, y);

            // 상하좌우
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];

                if (nx >= 0 && nx < width && ny >= 0 && ny < height &&
                    !visited[ny, nx] && pixels[ny * width + nx].grayscale < 0.5f)
                {
                    visited[ny, nx] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }

        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    // Texture를 Bounding Box 기준으로 자르기
    private static Texture2D CropTexture(Texture2D source, Rect cropRect)
    {
        int x = (int)cropRect.x;
        int y = (int)cropRect.y;
        int w = (int)cropRect.width;
        int h = (int)cropRect.height;

        Color[] pixels = source.GetPixels(x, y, w, h);
        Texture2D cropped = new Texture2D(w, h, TextureFormat.RGB24, false);
        cropped.SetPixels(pixels);
        cropped.Apply();

        return cropped;
    }

    // 중앙 정렬
    private static Texture2D CenterOnCanvas(Texture2D drawing)
    {
        int size = Mathf.Max(drawing.width, drawing.height);
        Texture2D square = new Texture2D(size, size, TextureFormat.RGB24, false);

        // 캔버스를 흰색으로 채움
        Color[] whitePixels = new Color[size * size];
        for (int i = 0; i < whitePixels.Length; i++)
            whitePixels[i] = Color.white;
        square.SetPixels(whitePixels);

        // 중앙 위치 계산
        int offsetX = (size - drawing.width) / 2;
        int offsetY = (size - drawing.height) / 2;

        // 캔버스에 그림 배치
        Color[] drawingPixels = drawing.GetPixels();
        square.SetPixels(offsetX, offsetY, drawing.width, drawing.height, drawingPixels);
        square.Apply();

        return square;
    }

    // Resize Texture 회색 픽셀 생성을 방지
    private static Texture2D ResizeTextureNearestNeighbor(Texture2D source, int newWidth, int newHeight)
    {
        Texture2D result = new Texture2D(newWidth, newHeight, TextureFormat.RGB24, false);

        float ratioX = (float)source.width / newWidth;
        float ratioY = (float)source.height / newHeight;

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                // 가장 가까운 픽셀 좌표 계산
                int sourceX = Mathf.FloorToInt(x * ratioX);
                int sourceY = Mathf.FloorToInt(y * ratioY);

                Color pixel = source.GetPixel(sourceX, sourceY);
                result.SetPixel(x, y, pixel);
            }
        }
        result.Apply();
        return result;
    }
}