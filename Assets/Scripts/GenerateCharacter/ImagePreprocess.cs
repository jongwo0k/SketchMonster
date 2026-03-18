using UnityEngine;

public static class ImagePreprocess
{
    // Buffer
    private static Color32[] workBuffer;
    private static Color32[] tempBuffer = new Color32[128 * 128];
    private static Color32[] finalBuffer = new Color32[128 * 128];

    // Gaussian Kernel (5x5)
    private static readonly int[] gaussianKernel = new int[]
    {
        1,  4,  7,  4, 1,
        4, 16, 26, 16, 4,
        7, 26, 41, 26, 7,
        4, 16, 26, 16, 4,
        1,  4,  7,  4, 1
    };

    // 배경 제거 (특정 색 범위 투명화 alpha=0)
    public static Texture2D RemoveBackground(Texture inputTexture)
    {
        Texture2D sourceTexture = ToTexture2D(inputTexture);
        Color32[] pixels = sourceTexture.GetPixels32();

        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 p = pixels[i];
            // R, B값은 높고 G값은 낮은, R-G 차이와 B-G 차이가 큰 색상 (Magenta 배경 제거 용도로 사전 학습)
            int rg_diff = p.r - p.g;
            int bg_diff = p.b - p.g;

            if (rg_diff >= 50 && bg_diff >= 50)
            {
                pixels[i].a = 0;
            }
        }

        Texture2D resultTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false, false);
        resultTexture.SetPixels32(pixels);
        resultTexture.Apply();

        if (sourceTexture != inputTexture)
        {
            Object.Destroy(sourceTexture);
        }

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
        int width = original.width;
        int height = original.height;
        int totalPixels = width * height;

        // 버퍼 초기화 (원본 크기)
        if (workBuffer == null || workBuffer.Length != totalPixels)
        {
            workBuffer = new Color32[totalPixels];
        }

        Color32[] pixels = original.GetPixels32();
        System.Array.Copy(pixels, workBuffer, totalPixels);

        ConvertToGrayscale(workBuffer, width, height);
        Rect bounds = FindDrawingBounds(workBuffer, width, height);

        int cropW = (int)bounds.width;
        int cropH = (int)bounds.height;
        Color32[] cropped = CropPixels(workBuffer, width, height, bounds);

        int maxSize = Mathf.Max(cropW, cropH);
        Color32[] centered = CenterOnCanvas(cropped, cropW, cropH, maxSize);

        ResizePixels(centered, maxSize, maxSize, finalBuffer, 128, 128);
        GaussianBlur(finalBuffer, tempBuffer, 128, 128); // 블러 적용 순서 변경

        // 최종 Texture
        Texture2D result = new Texture2D(128, 128, TextureFormat.RGB24, false);
        result.SetPixels32(finalBuffer);
        result.Apply();

        return result;
    }

    // Grayscale 변환
    private static void ConvertToGrayscale(Color32[] pixels, int width, int height)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 p = pixels[i];
            byte gray = (byte)(p.r * 0.299f + p.g * 0.587f + p.b * 0.114f);
            pixels[i] = new Color32(gray, gray, gray, 255);
        }
    }

    // Gaussian Blur
    private static void GaussianBlur(Color32[] pixels, Color32[] temp, int width, int height)
    {
        System.Array.Copy(pixels, temp, pixels.Length);

        int halfKernel = 2; // 5x5
        int gaussianKernelSum = 273;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int sum = 0;

                for (int ky = -halfKernel; ky <= halfKernel; ky++)
                {
                    for (int kx = -halfKernel; kx <= halfKernel; kx++)
                    {
                        int px = Mathf.Clamp(x + kx, 0, width - 1);
                        int py = Mathf.Clamp(y + ky, 0, height - 1);

                        int kernelIdx = (ky + halfKernel) * 5 + (kx + halfKernel);
                        sum += temp[py * width + px].r * gaussianKernel[kernelIdx];
                    }
                }

                byte avg = (byte)(sum / gaussianKernelSum);
                pixels[y * width + x] = new Color32(avg, avg, avg, 255);
            }
        }
    }

    // BoundingBox (Min/Max)
    private static Rect FindDrawingBounds(Color32[] pixels, int width, int height)
    {
        int minX = width, maxX = 0, minY = height, maxY = 0;
        bool found = false;

        // 1회 스캔
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (pixels[y * width + x].r < 200) // 검은 픽셀
                {
                    found = true;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        // 그림이 없는 경우
        if (!found) return new Rect(0, 0, width, height);

        // Padding (끊김 방지, 여백)
        int pad = 10;
        minX = Mathf.Max(0, minX - pad);
        minY = Mathf.Max(0, minY - pad);
        int w = Mathf.Min(width - minX, maxX - minX + 1 + pad * 2);
        int h = Mathf.Min(height - minY, maxY - minY + 1 + pad * 2);

        return new Rect(minX, minY, w, h);
    }

    // Texture를 Bounding Box 기준으로 자르기
    private static Color32[] CropPixels(Color32[] source, int srcW, int srcH, Rect bounds)
    {
        int x = (int)bounds.x;
        int y = (int)bounds.y;
        int w = (int)bounds.width;
        int h = (int)bounds.height;

        Color32[] result = new Color32[w * h];

        for (int cy = 0; cy < h; cy++)
        {
            for (int cx = 0; cx < w; cx++)
            {
                int srcIdx = (y + cy) * srcW + (x + cx);
                int dstIdx = cy * w + cx;
                result[dstIdx] = source[srcIdx];
            }
        }

        return result;
    }

    // 중앙 정렬
    private static Color32[] CenterOnCanvas(Color32[] source, int srcW, int srcH, int targetSize)
    {
        Color32[] result = new Color32[targetSize * targetSize];

        // 캔버스를 흰색으로 채움
        Color32 white = new Color32(255, 255, 255, 255);
        for (int i = 0; i < result.Length; i++)
            result[i] = white;

        // 중앙 위치 계산
        int offsetX = (targetSize - srcW) / 2;
        int offsetY = (targetSize - srcH) / 2;

        // 중앙에 배치
        for (int y = 0; y < srcH; y++)
        {
            for (int x = 0; x < srcW; x++)
            {
                int srcIdx = y * srcW + x;
                int dstIdx = (offsetY + y) * targetSize + (offsetX + x);
                result[dstIdx] = source[srcIdx];
            }
        }

        return result;
    }

    // Bilinear Resize
    private static void ResizePixels(Color32[] source, int srcW, int srcH, Color32[] dest, int targetW, int targetH)
    {
        float ratioX = (float)srcW / targetW;
        float ratioY = (float)srcH / targetH;

        for (int y = 0; y < targetH; y++)
        {
            // src 좌표 (중심 기준)
            float srcY = (y + 0.5f) * ratioY - 0.5f;
            int y0 = Mathf.FloorToInt(srcY);
            int y1 = y0 + 1;
            float fy = srcY - y0;

            y0 = Mathf.Clamp(y0, 0, srcH - 1);
            y1 = Mathf.Clamp(y1, 0, srcH - 1);

            for (int x = 0; x < targetW; x++)
            {
                float srcX = (x + 0.5f) * ratioX - 0.5f;
                int x0 = Mathf.FloorToInt(srcX);
                int x1 = x0 + 1;
                float fx = srcX - x0;

                x0 = Mathf.Clamp(x0, 0, srcW - 1);
                x1 = Mathf.Clamp(x1, 0, srcW - 1);

                // 인접 픽셀
                float topLeft = source[y0 * srcW + x0].r; // grayscale, r만
                float topRight = source[y0 * srcW + x1].r;
                float bottomLeft = source[y1 * srcW + x0].r;
                float bottomRight = source[y1 * srcW + x1].r;

                // Bilinear 보간
                float top = topLeft + (topRight - topLeft) * fx;
                float bottom = bottomLeft + (bottomRight - bottomLeft) * fx;
                byte value = (byte)Mathf.Clamp(top + (bottom - top) * fy, 0f, 255f);

                dest[y * targetW + x] = new Color32(value, value, value, 255);
            }
        }
    }
}