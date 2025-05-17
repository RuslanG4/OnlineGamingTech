using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyPopUp : MonoBehaviour
{
    public float floatSpeed = 10f;
    public float fadeSpeed = 1f;
    private TextMeshProUGUI textMesh;
    private Color textColor;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        textColor = textMesh.color;
    }

    void Update()
    {
        transform.position += new Vector3(0, floatSpeed, 0);
        textColor.a -= fadeSpeed * Time.deltaTime;
        textMesh.color = textColor;

        if (textColor.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}
