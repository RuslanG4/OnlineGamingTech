using System;
using UnityEngine;

public class UIDataManager : MonoBehaviour
{
    // Define the event
    public event Action<float> OnBuildingUIClosed;

    // Method to trigger the event
    public void TriggerBuildingUIClosed(float time)
    {
        OnBuildingUIClosed?.Invoke(time);
    }
}
