using System;
using TMPro;
using UnityEngine;

public class UpdateWaveCount : MonoBehaviour
{
    public static UpdateWaveCount Instance { get; private set; }

    public event Action<int> OnWaveCountUpdated;

    [SerializeField] private TextMeshProUGUI waveText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void UpdateWaveText(int waveNumber)
    {
        OnWaveCountUpdated?.Invoke(waveNumber);
    }

    private void Start()
    {
        OnWaveCountUpdated += UpdateText;
    }

    private void UpdateText(int waveNumber)
    {
        if (waveText != null)
        {
            waveText.text = "Round: " + waveNumber;
        }
    }
}