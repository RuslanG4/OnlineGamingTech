using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform planet;
    public float rotationSpeed = 0.2f;
    public float distance = 27f;
    public float minYAngle = -45f;
    public float maxYAngle = 45f;

    private float yaw = 0f;
    private float pitch = 0f;
    private Vector2 lastInputPosition;
    private bool isDragging = false;
    public bool turnOffRotation = false;

    private void Awake()
    {
        if (planet == null)
        {
            GameObject planetObject = GameObject.FindGameObjectWithTag("Planet");
            if (planetObject != null)
            {
                planet = planetObject.transform;
            }
            else
            {
                Debug.LogError("Planet object not found! Make sure your planet GameObject is tagged 'Planet'.", this);
                this.enabled = false;
                return;
            }
        }

        Vector3 offset = transform.position - planet.position;
        if (offset.magnitude > 0.01f)
        {
            yaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
            Vector3 flatOffset = new Vector3(offset.x, 0, offset.z);
            pitch = Vector3.SignedAngle(flatOffset, offset, Vector3.Cross(flatOffset, Vector3.up));
            pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);
        }
        else
        {
            yaw = 0f;
            pitch = 0f;
            Quaternion initialRotation = Quaternion.Euler(pitch, yaw, 0);
            transform.position = planet.position + initialRotation * (Vector3.back * distance);
        }
    }

    void Update()
    {
        if (planet == null) return;

        // Touch input (mobile)

        if (turnOffRotation) return;

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                lastInputPosition = touch.position;
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 delta = touch.deltaPosition;
                yaw += delta.x * rotationSpeed;
                pitch -= delta.y * rotationSpeed;
                pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        // Mouse input (desktop)
        else if (Input.GetMouseButtonDown(0)) // LEFT mouse button now
        {
            lastInputPosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 delta = currentMousePosition - lastInputPosition;
            lastInputPosition = currentMousePosition;

            yaw += delta.x * rotationSpeed * 0.5f;
            pitch -= delta.y * rotationSpeed * 0.5f;
            pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    void LateUpdate()
    {
        if (planet == null) return;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 position = planet.position + rotation * (Vector3.back * distance);

        transform.position = position;
        transform.LookAt(planet);
    }
}
