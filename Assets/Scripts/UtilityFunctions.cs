using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class FadeAudioSource
{
    public static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        yield break;
    }
}

public static class UtilityFunctions
{

    public static int HexToDec(string hex)
    {
        int dec = System.Convert.ToInt32(hex, 16);
        return dec;
    }
    public static float HexToFloatNormalized(string hex)
    {
        return HexToDec(hex) / 255f;
    }
    public static Color StringToColor(string hexString)
    {
        float red = HexToFloatNormalized(hexString.Substring(0, 2));
        float green = HexToFloatNormalized(hexString.Substring(2, 2));
        float blue = HexToFloatNormalized(hexString.Substring(4, 2));
        float alpha = 1f;
        if (hexString.Length >= 8)
        {
            alpha = HexToFloatNormalized(hexString.Substring(6, 2));
        }

        return new Color(red, green, blue, alpha);
    }

    public static bool ContainsLayer(this LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }
}
