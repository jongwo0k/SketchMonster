using UnityEngine;

public static class ImagePreprocess
{
    // Buffer
    private static Color32[] workBuffer;
    private static Color32[] tempBuffer;

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

        // 버퍼 초기화
        if (workBuffer == null || workBuffer.Length != totalPixels)
        {
            workBuffer = new Color32[totalPixels];
            tempBuffer = new Color32[totalPixels];
        }

        Color32[] pixels = original.GetPixels32();
        System.Array.Copy(pixels, workBuffer, totalPixels);

        ConvertToGrayscale(workBuffer, width, height);
        GaussianBlur(workBuffer, tempBuffer, width, height);
        BinarizeImage(workBuffer, width, height, 100);

        Rect bounds = FindDrawingBounds(workBuffer, width, height);

        int cropW = (int)bounds.width;
        int cropH = (int)bounds.height;
        Color32[] cropped = CropPixels(workBuffer, width, height, bounds);

        int maxSize = Mathf.Max(cropW, cropH);
        Color32[] centered = CenterOnCanvas(cropped, cropW, cropH, maxSize);

        Color32[] resized = ResizePixels(centered, maxSize, maxSize, 128, 128);

        // 최종 Texture
        Texture2D result = new Texture2D(128, 128, TextureFormat.RGB24, false);
        result.SetPixels32(resized);
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

    // Gaussian Blur (5x5 kernel)
    private static void GaussianBlur(Color32[] pixels, Color32[] temp, int width, int height)
    {
        // temp에 원본 복사
        System.Array.Copy(pixels, temp, pixels.Length);

        int kernelSize = 5;
        int halfKernel = kernelSize / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int sum = 0;
                int count = 0;

                for (int ky = -halfKernel; ky <= halfKernel; ky++)
                {
                    for (int kx = -halfKernel; kx <= halfKernel; kx++)
                    {
                        // 경계
                        int px = Mathf.Clamp(x + kx, 0, width - 1);
                        int py = Mathf.Clamp(y + ky, 0, height - 1);

                        sum += temp[py * width + px].r;
                        count++;
                    }
                }

                byte avg = (byte)(sum / count);
                pixels[y * width + x] = new Color32(avg, avg, avg, 255);
            }
        }
    }

    // 이진화 (threshold)
    private static void BinarizeImage(Color32[] pixels, int width, int height, int threshold)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            byte value = pixels[i].r > threshold ? (byte)255 : (byte)0;
            pixels[i] = new Color32(value, value, value, 255);
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
                if (pixels[y * width + x].r < 128)  // 검은 픽셀
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

        // Padding (끊김 방지)
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

    // Resize
    private static Color32[] ResizePixels(Color32[] source, int srcW, int srcH, int targetW, int targetH)
    {
        Color32[] result = new Color32[targetW * targetH];

        float ratioX = (float)srcW / targetW;
        float ratioY = (float)srcH / targetH;

        for (int y = 0; y < targetH; y++)
        {
            for (int x = 0; x < targetW; x++)
            {
                // 가장 가까운 픽셀 좌표 계산
                int srcX = Mathf.FloorToInt(x * ratioX);
                int srcY = Mathf.FloorToInt(y * ratioY);

                int srcIdx = srcY * srcW + srcX;
                int dstIdx = y * targetW + x;

                result[dstIdx] = source[srcIdx];
            }
        }

        return result;
    }
}