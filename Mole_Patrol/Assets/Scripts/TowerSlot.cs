using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using static TowerData;

public class TowerSlot : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public GameObject towerPrefab;
    public string towerName;
    private GameObject previewTower;
    private Camera mainCamera;
    private TowerPlacer buildingPlacer;
    private BuildingMenuSlot buildingMenuSlot;

    private int layerMask;

    void Awake()
    {
        int ignoreLayerIndex = LayerMask.NameToLayer("Environment");

        if (ignoreLayerIndex != -1)
        {
            layerMask = ~(1 << ignoreLayerIndex); // Invert the layer mask to exclude the "Environment" layer
        }
        else
        {
            Debug.LogWarning("Layer 'Environment' not found!");
        }

        buildingMenuSlot = GetComponent<BuildingMenuSlot>();
    }

    void Start()
    {
        mainCamera = Camera.main;
        buildingPlacer = FindObjectOfType<TowerPlacer>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Instantiate a preview of the building
        if (buildingMenuSlot.CanAfford())
        {
            previewTower = Instantiate(towerPrefab);

            previewTower.GetComponent<Collider>().enabled = false; // Disable collider while dragging
            previewTower.GetComponent<Tower>().Initialise(TowerFactory.GetTowerData(towerName, TowerData.TowerLevel.LevelOne));

            SwipeToRotate.turnOffRotation = true;
            GameObject.Find("Game Camera").GetComponent<CameraOrbit>().turnOffRotation = true;
            GamePlayUIController.Instance.BackButton();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (previewTower != null && buildingMenuSlot.CanAfford())
        {
            // Convert screen position to world position
            Ray ray = mainCamera.ScreenPointToRay(eventData.position);

            // Perform a raycast but ignore the "Environment" layer
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                if(hit.collider.gameObject.tag == "Triangle")
                {
                    GameObject triangle = hit.collider.gameObject;
                    Vector3 centroidPosition = triangle.transform.Find("centroid").transform.position;

                    Tower[] placedTowers = GameObject.Find("Planet").GetComponentsInChildren<Tower>();
                    bool canSlotIn = true;
                    for (int i = 0; i < placedTowers.Length; i++)
                    {

                        Vector3 directionToTower = placedTowers[i].transform.position - centroidPosition;
                        if(directionToTower.magnitude < 1)
                        {
                            canSlotIn = false;
                            break;
                        }
                    }

                    if(canSlotIn)
                    {
                        previewTower.transform.position = centroidPosition;
                        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, centroidPosition.normalized);
                        previewTower.transform.rotation = rotation;
                    }
                }
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (previewTower != null && buildingMenuSlot.CanAfford())
        {
            Ray ray = mainCamera.ScreenPointToRay(eventData.position);

            // Perform a raycast to find the position where the tower should be placed
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                Vector3 position = hit.collider.transform.Find("centroid").position;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, position.normalized);


                if (NetworkManager.Singleton.IsClient)
                {
                    Debug.Log($"Client {NetworkManager.Singleton.LocalClientId} is sending ServerRPC");
                    switch(buildingMenuSlot.buildingName)
                    {
                        case "Cannon":
                            GameplayMultiplayerManager.Instance.RequestPlaceCannonServerRpc(position, rotation);
                            break;
                        case "Sniper":
                            GameplayMultiplayerManager.Instance.RequestPlaceSniperServerRpc(position, rotation);
                            break;
                        case "Mortar":
                            GameplayMultiplayerManager.Instance.RequestPlaceMortarServerRpc(position, rotation);
                            break;
                        case "Plane":
                            GameplayMultiplayerManager.Instance.RequestPlacePlaneServerRpc(position, rotation);
                            break;
                        default:
                            Debug.LogError("Invalid tower type");
                            break;
                    }
                    // GameplayMultiplayerManager.Instance.RequestPlaceTowerServerRpc(position, rotation);  // Client will call this
                }
                else
                {
                    Debug.LogError("NetworkManager is not a client.");
                }

                    buildingMenuSlot.BuyBuilding();
                GameManager.Instance.buildingUIOpen = false;
            }

            Destroy(previewTower);
            SwipeToRotate.turnOffRotation = false;
            GameObject.Find("Game Camera").GetComponent<CameraOrbit>().turnOffRotation = false;
        }
    }
}
