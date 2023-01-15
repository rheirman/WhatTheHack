using UnityEngine;

namespace WhatTheHack;

public static class TextureUtility
{
    public enum ImageFilterMode
    {
        Nearest = 0,
        Biliner = 1,
        Average = 2
    }

    public static Texture2D GetReadableTexture(this Texture2D texture)
    {
        // Create a temporary RenderTexture of the same size as the texture
        var tmp = RenderTexture.GetTemporary(
            texture.width,
            texture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        // Blit the pixels on texture to the RenderTexture
        Graphics.Blit(texture, tmp);
        // Backup the currently set RenderTexture
        var previous = RenderTexture.active;
        // Set the current RenderTexture to the temporary one we created
        RenderTexture.active = tmp;
        // Create a new readable Texture2D to copy the pixels to it
        var myTexture2D = new Texture2D(texture.width, texture.height);
        // Copy the pixels from the RenderTexture to the new Texture
        myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        myTexture2D.Apply();
        // Reset the active RenderTexture
        RenderTexture.active = previous;
        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(tmp);
        return myTexture2D;
    }

    public static Texture2D AddWatermark(this Texture2D background, Texture2D watermark)
    {
        watermark = ResizeTexture(watermark, ImageFilterMode.Average, background.width * 0.5f / watermark.width);
        var startX = background.width - watermark.width;
        var startY = background.height - watermark.height;
        for (var x = startX; x < background.width; x++)
        {
            for (var y = startY; y < background.height; y++)
            {
                var bgColor = background.GetPixel(x, y);
                var wmColor = watermark.GetPixel(x - startX, y - startY);

                var final_color = Color.Lerp(bgColor, wmColor, wmColor.a / 1.0f);

                background.SetPixel(x, y, final_color);
            }
        }

        background.Apply();
        Object.Instantiate(background);
        return background;
    }

    public static Texture2D ResizeTexture(Texture2D pSource, ImageFilterMode pFilterMode, float pScale)
    {
        //*** Variables
        int i;

        //*** Get All the source pixels
        var aSourceColor = pSource.GetPixels(0);
        var vSourceSize = new Vector2(pSource.width, pSource.height);

        //*** Calculate New Size
        float xWidth = Mathf.RoundToInt(pSource.width * pScale);
        float xHeight = Mathf.RoundToInt(pSource.height * pScale);

        //*** Make New
        var oNewTex = new Texture2D((int)xWidth, (int)xHeight, TextureFormat.RGBA32, false);

        //*** Make destination array
        var xLength = (int)xWidth * (int)xHeight;
        var aColor = new Color[xLength];

        var vPixelSize = new Vector2(vSourceSize.x / xWidth, vSourceSize.y / xHeight);

        //*** Loop through destination pixels and process
        var vCenter = new Vector2();
        for (i = 0; i < xLength; i++)
        {
            //*** Figure out x&y
            var xX = i % xWidth;
            var xY = Mathf.Floor(i / xWidth);

            //*** Calculate Center
            vCenter.x = xX / xWidth * vSourceSize.x;
            vCenter.y = xY / xHeight * vSourceSize.y;

            //*** Do Based on mode
            //*** Nearest neighbour (testing)
            if (pFilterMode == ImageFilterMode.Nearest)
            {
                //*** Nearest neighbour (testing)
                vCenter.x = Mathf.Round(vCenter.x);
                vCenter.y = Mathf.Round(vCenter.y);

                //*** Calculate source index
                var xSourceIndex = (int)((vCenter.y * vSourceSize.x) + vCenter.x);

                //*** Copy Pixel
                aColor[i] = aSourceColor[xSourceIndex];
            }

            //*** Bilinear
            else if (pFilterMode == ImageFilterMode.Biliner)
            {
                //*** Get Ratios
                var xRatioX = vCenter.x - Mathf.Floor(vCenter.x);
                var xRatioY = vCenter.y - Mathf.Floor(vCenter.y);

                //*** Get Pixel index's
                var xIndexTL = (int)((Mathf.Floor(vCenter.y) * vSourceSize.x) + Mathf.Floor(vCenter.x));
                var xIndexTR = (int)((Mathf.Floor(vCenter.y) * vSourceSize.x) + Mathf.Ceil(vCenter.x));
                var xIndexBL = (int)((Mathf.Ceil(vCenter.y) * vSourceSize.x) + Mathf.Floor(vCenter.x));
                var xIndexBR = (int)((Mathf.Ceil(vCenter.y) * vSourceSize.x) + Mathf.Ceil(vCenter.x));

                //*** Calculate Color
                aColor[i] = Color.Lerp(
                    Color.Lerp(aSourceColor[xIndexTL], aSourceColor[xIndexTR], xRatioX),
                    Color.Lerp(aSourceColor[xIndexBL], aSourceColor[xIndexBR], xRatioX),
                    xRatioY
                );
            }

            //*** Average
            else if (pFilterMode == ImageFilterMode.Average)
            {
                //*** Calculate grid around point
                var xXFrom = (int)Mathf.Max(Mathf.Floor(vCenter.x - (vPixelSize.x * 0.5f)), 0);
                var xXTo = (int)Mathf.Min(Mathf.Ceil(vCenter.x + (vPixelSize.x * 0.5f)), vSourceSize.x);
                var xYFrom = (int)Mathf.Max(Mathf.Floor(vCenter.y - (vPixelSize.y * 0.5f)), 0);
                var xYTo = (int)Mathf.Min(Mathf.Ceil(vCenter.y + (vPixelSize.y * 0.5f)), vSourceSize.y);

                //*** Loop and accumulate
                var unused = new Vector4();
                var oColorTemp = new Color();
                float xGridCount = 0;
                for (var iy = xYFrom; iy < xYTo; iy++)
                {
                    for (var ix = xXFrom; ix < xXTo; ix++)
                    {
                        //*** Get Color
                        oColorTemp += aSourceColor[(int)((iy * vSourceSize.x) + ix)];

                        //*** Sum
                        xGridCount++;
                    }
                }

                //*** Average Color
                aColor[i] = oColorTemp / xGridCount;
            }
        }

        //*** Set Pixels
        oNewTex.SetPixels(aColor);
        oNewTex.Apply();

        //*** Return
        return oNewTex;
    }
}