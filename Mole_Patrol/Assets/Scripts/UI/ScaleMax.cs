using UnityEngine;

public class ScaleMax : MonoBehaviour
{
    void Start()
    {
        ScaleToCanvas.ScaleToFitCanvas(GetComponent<RectTransform>());
    }
}
