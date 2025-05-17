using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveGenerator : MonoBehaviour
{
    // This method loads waves from the Resources folder and returns a deserialized WaveData object.
    public static WaveData LoadWavesFromJson()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("waves");

        if (jsonText == null)
        {
            Debug.LogError("Failed to load waves.json from Resources.");
            return null;
        }

        // Deserialize the JSON text into a WaveData object and return it
        return JsonUtility.FromJson<WaveData>(jsonText.text);
    }
}
