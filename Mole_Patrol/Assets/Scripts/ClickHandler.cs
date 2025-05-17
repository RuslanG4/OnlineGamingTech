using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickHandler : MonoBehaviour
{
    private GameObject PrevclickedObject;
    public LayerMask ignoreUILayerMask;
    void ResetColors(GameObject face)
    {
        MeshFilter meshFilter = face.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;

        if (mesh.colors.Length == 0)
        {
            Color[] defaultColors = new Color[mesh.vertices.Length];
            for (int i = 0; i < defaultColors.Length; i++)
            {
                defaultColors[i] = Color.white; 
            }
            mesh.colors = defaultColors; 
        }
        else
        {
            Color[] resetColors = new Color[mesh.colors.Length];
            for (int i = 0; i < resetColors.Length; i++)
            {
                resetColors[i] = Color.white; 
            }
            mesh.colors = resetColors; 
        }
    }

    void HandleTriangleClick(RaycastHit hit)
    {
        GameObject clickedObject = hit.collider.gameObject;

        if (PrevclickedObject != clickedObject)
        {
            if (PrevclickedObject != null)
            {
                ResetColors(PrevclickedObject);
            }
            PrevclickedObject = clickedObject;

        }

        MeshFilter meshFilter = clickedObject.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;

        if (mesh.colors.Length == 0)
        {
            Color[] defaultColors = new Color[mesh.vertices.Length];
            for (int i = 0; i < defaultColors.Length; i++)
            {
                defaultColors[i] = Color.white;
            }
            mesh.colors = defaultColors;
        }

        int triangleIndex = hit.triangleIndex;
        int baseIndex = triangleIndex * 3;

        Color[] currentColors = mesh.colors;

        currentColors[baseIndex] = Color.red;
        currentColors[baseIndex + 1] = Color.red;
        currentColors[baseIndex + 2] = Color.red;

        mesh.colors = currentColors;

    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                GameObject clickedObject = hit.collider.gameObject;

                if (clickedObject.CompareTag("Triangle"))
                {
                    HandleTriangleClick(hit);
                }
                else if (clickedObject.CompareTag("Tower") && !SwipeToRotate.turnOffRotation)
                {
                    Tower towerObject = clickedObject.GetComponent<Tower>();
                    if (towerObject == null)
                    {
                        Debug.Log("ClickHandler: Update(): towerobject == null");
                    }
                    GamePlayUIController.Instance.PassBuildingDetails(towerObject);
                    GamePlayUIController.Instance.SwitchMenuViews();
                    SelectionRing.Instance.SelectTower(clickedObject);
                }
            }
        }
    }
}
