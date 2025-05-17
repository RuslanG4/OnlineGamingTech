using UnityEngine;

public class CactusRotation : MonoBehaviour
{
    void Start()
    {
        float randomYRotation = Random.Range(0f, 360f);

        transform.localRotation = Quaternion.Euler(0f, randomYRotation, 0f);
    }
}
