using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SelectionRing : MonoBehaviour
{
    public float rotationSpeed = 100f; 
    public static SelectionRing Instance { get; private set; }

    public Material transparentMaterial;
    public Material rangeCutOffMaterial;

    private List<GameObject> selectedTriangles = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

    }

    public void SelectTower(GameObject tower)
    {
        Tower currentTower = tower.GetComponent<Tower>();

        if (!currentTower.isInitialised)
        {
            Debug.LogWarning("Tower not initialised yet — delaying SelectTower.");

            // Delay selection until Initialise() finishes
            currentTower.OnInitialised += (t) =>
            {
                Debug.Log("Delayed SelectTower executing after Initialise.");
                SelectTower(t.gameObject); // Retry selection
            };

            return;
        }

        // Now it's safe to access towerData
        TowerData data = currentTower.GetTowerData();
        if (data == null)
        {
            Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] SelectionRing: SelectTower(): data == null");
            return;
        }

        transform.position = tower.transform.position;
        transform.rotation = tower.transform.rotation;
        transform.parent = tower.transform.parent;
        gameObject.SetActive(true);

        UpdateRange(tower.transform.position, data.Range);

        if (data.TowerType == "Sniper")
        {
            UpdateCutOffRange(tower.transform.position, tower.GetComponent<SniperTower>().cutOffRange);
        }
    }


    // Method to deselect the tower
    public void DeselectTower()
    {
        gameObject.SetActive(false); // Hide the selection ring

        ClearRange(); // clears range
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

    }

    void UpdateCutOffRange(Vector3 towerPosition, float range)
    {
        foreach (GameObject triangle in GameManager.Instance.allTriangles)
        {
            if (Vector3.Distance(triangle.transform.Find("centroid").transform.position, towerPosition) <= range)
            {
                MeshRenderer renderer = triangle.GetComponent<MeshRenderer>();

                Material[] materials = renderer.materials;

                   Material[] newMaterials = new Material[2];
                   newMaterials[0] = materials[0];
                   newMaterials[1] = rangeCutOffMaterial;

                   renderer.materials = newMaterials;
                
            }
        }
    }
    void UpdateRange(Vector3 towerPosition, float range)
    {
        foreach (GameObject triangle in GameManager.Instance.allTriangles)
        {
            if (Vector3.Distance(triangle.transform.Find("centroid").transform.position, towerPosition) <= range)
            {
                HighlightTriangle(triangle);
            }
            else
            {
                ResetTriangle(triangle);
            }
        }
    }

    void HighlightTriangle(GameObject triangle)
    {
        MeshRenderer renderer = triangle.GetComponent<MeshRenderer>();

        if (renderer != null)
        {
            Material[] materials = renderer.materials;
            if (materials.Length == 1)
            {
                Material[] newMaterials = new Material[2];
                newMaterials[0] = materials[0];
                newMaterials[1] = transparentMaterial;

                renderer.materials = newMaterials;
            }
        }

        selectedTriangles.Add(triangle);
    }

    void ClearRange()
    {
        foreach (GameObject triangle in selectedTriangles)
        {
            ResetTriangle(triangle);
        }
        selectedTriangles.Clear();
    }
    void ResetTriangle(GameObject triangle)
    {
        MeshRenderer renderer = triangle.GetComponent<MeshRenderer>();

        if (renderer != null)
        {
            Material[] materials = renderer.materials;

            if (materials.Length > 1)
            {
                Material[] newMaterials = new Material[materials.Length - 1];

                for (int i = 0; i < materials.Length - 1; i++)
                {
                    newMaterials[i] = materials[i];
                }

                renderer.materials = newMaterials;
            }
        }
    }
}
