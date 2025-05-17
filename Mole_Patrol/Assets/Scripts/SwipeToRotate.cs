using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeToRotate : MonoBehaviour
{
    

    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private Vector2 touchDelta;
    private bool isTouchingSphere = false;

    public float rotationSpeed = 5f;

    public static bool turnOffRotation = false;


    void Update()
    {
        if (Input.touchCount > 0 && !turnOffRotation)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isTouchingSphere = CheckIfTouchHitsSphere(touch.position);
                    if (isTouchingSphere)
                    {
                        touchStartPos = touch.position;
                    }
                    break;

                case TouchPhase.Moved:
                    if (isTouchingSphere)
                    {
                        touchEndPos = touch.position;
                        touchDelta = touchEndPos - touchStartPos;

                        float rotationX = touchDelta.x * rotationSpeed * Time.deltaTime;
                        float rotationY = touchDelta.y * rotationSpeed * Time.deltaTime;

                        transform.Rotate(Vector3.up, -rotationX, Space.World);
                        transform.Rotate(Vector3.right, rotationY, Space.World);

                        touchStartPos = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                    isTouchingSphere = false;
                    break;
            }
        }
    }

    private bool CheckIfTouchHitsSphere(Vector2 touchPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject clickedObject = hit.collider.gameObject;
            if(clickedObject.tag == "Triangle")
            {
                return true;
            }
        }
        return false;
    }
}
