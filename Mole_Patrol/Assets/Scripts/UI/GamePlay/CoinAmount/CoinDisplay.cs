using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI coinAmount;
    void Start()
    {
        coinAmount = transform.GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        coinAmount.text = GameManager.Instance.GetCoinCount().ToString();
    }
}
