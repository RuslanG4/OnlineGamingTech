using UnityEngine;

public class ExclamationRotation : MonoBehaviour
{
    public float rotationSpeed = 80f; 
    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
