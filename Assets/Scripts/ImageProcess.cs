using UnityEngine;

public static class ImageProcess
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

    //전처리(이진화, 크롭, 중앙 정렬
    public static Texture2D PreprocessSketch(Texture2D original)
    {
        // 이진화 (threshold)
        Texture2D binarized = BinarizeImage(original);

        // 그림 영역만 자르기 (boundingRect)
        Rect boundingBox = FindDrawingBounds(binarized);
        Texture2D cropped = CropTexture(binarized, boundingBox);

        // 캔버스 중앙에 배치 (ResizeTexture)
        Texture2D centered = CenterOnCanvas(cropped, 128);

        // 중간 생성물 삭제
        Object.Destroy(binarized);
        Object.Destroy(cropped);

        return centered;
    }

    // cv.threshold
    public static Texture2D BinarizeImage(Texture2D source)
    {
        Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);
        Color[] pixels = source.GetPixels();

        float threshold = 0.5f; // 흰색 배경, 검은색 선

        for (int i = 0; i < pixels.Length; i++)
        {
            float gray = pixels[i].grayscale;
            pixels[i] = gray > threshold ? Color.white : Color.black;
        }

        result.SetPixels(pixels);
        result.Apply();
        return result;
    }

    // cv.findContours + cv.boundingRect
    public static Rect FindDrawingBounds(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        int width = texture.width;
        int height = texture.height;

        int minX = width, maxX = 0, minY = height, maxY = 0;
        bool foundPixel = false;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = pixels[y * width + x];
                // 검은색 픽셀 찾기
                if (pixel.grayscale < 0.5f)
                {
                    foundPixel = true;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (!foundPixel)
        {
            // 그림이 없는 경우
            return new Rect(0, 0, width, height);
        }

        // Padding
        int padding = 10;
        minX = Mathf.Max(0, minX - padding);
        maxX = Mathf.Min(width - 1, maxX + padding);
        minY = Mathf.Max(0, minY - padding);
        maxY = Mathf.Min(height - 1, maxY + padding);

        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    // Texture를 Bounding Box 기준으로 자르기
    public static Texture2D CropTexture(Texture2D source, Rect cropRect)
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
    public static Texture2D CenterOnCanvas(Texture2D drawing, int canvasSize)
    {
        Texture2D canvas = new Texture2D(canvasSize, canvasSize, TextureFormat.RGB24, false);

        // 캔버스를 흰색으로 채움
        Color[] whitePixels = new Color[canvasSize * canvasSize];
        for (int i = 0; i < whitePixels.Length; i++)
            whitePixels[i] = Color.white;
        canvas.SetPixels(whitePixels);

        // 그림 리사이즈 Image.NEAREST
        float scale = Mathf.Min((float)canvasSize / drawing.width, (float)canvasSize / drawing.height);
        int newWidth = Mathf.RoundToInt(drawing.width * scale);
        int newHeight = Mathf.RoundToInt(drawing.height * scale);

        // 회색 픽셀 생성 방지
        Texture2D resized = ResizeTextureNearestNeighbor(drawing, newWidth, newHeight);

        // 중앙 위치 계산
        int offsetX = (canvasSize - newWidth) / 2;
        int offsetY = (canvasSize - newHeight) / 2;

        // 캔버스에 그림 배치
        Color[] drawingPixels = resized.GetPixels();
        canvas.SetPixels(offsetX, offsetY, newWidth, newHeight, drawingPixels);
        canvas.Apply();

        // 중간 생성물 삭제
        Object.Destroy(resized);

        return canvas;
    }

    // Resize Texture 회색 픽셀 생성을 방지
    public static Texture2D ResizeTextureNearestNeighbor(Texture2D source, int newWidth, int newHeight)
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
