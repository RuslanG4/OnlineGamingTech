using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomingScript : MonoBehaviour
{
    public Camera cam;
    public float zoomSpeed;
    public float scrollSpeed;
    public float minZoom;
    public float maxZoom;

    private void Awake()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogError("Camera component not found on " + gameObject.name, this);
            this.enabled = false;
            return;
        }

        cam.fieldOfView = 75;
    }

    void Update()
    {
        HandleTouchZoom();
        HandleMouseScrollZoom();
    }

    void HandleTouchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            float prevDistance = (touch1.position - touch1.deltaPosition - (touch2.position - touch2.deltaPosition)).magnitude;
            float currentDistance = (touch1.position - touch2.position).magnitude;

            float zoomChange = (prevDistance - currentDistance) * zoomSpeed;

            ApplyZoom(zoomChange);
        }
    }

    void HandleMouseScrollZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            ApplyZoom(-scrollInput * scrollSpeed);
        }
    }

    void ApplyZoom(float zoomChange)
    {
        if (cam == null) return;

        if (cam.orthographic)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + zoomChange, minZoom, maxZoom);
        }
        else
        {
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView + zoomChange, minZoom * 5, maxZoom * 5);
        }
    }
}