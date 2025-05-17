using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerCameraControllerPrefab;
    [SerializeField] private Transform worldCenterPoint;

    private static bool hostCameraCreated = false;

    //public override void OnNetworkSpawn()
    //{
    //    base.OnNetworkSpawn();

    //    if (IsOwner)
    //    {
    //        // Check if we're the host and if a host camera has already been created
    //        if (IsHost && hostCameraCreated)
    //        {
    //            Debug.Log("Host camera already exists, skipping camera creation.");
    //            return;
    //        }

    //        // Find world center if not assigned
    //        if (worldCenterPoint == null)
    //        {
    //            GameObject centerObj = GameObject.FindGameObjectWithTag("WorldCenter");
    //            if (centerObj != null)
    //                worldCenterPoint = centerObj.transform;
    //        }

    //        // Check for existing camera controllers to prevent duplicates
    //        PlayerCameraController[] existingControllers = FindObjectsOfType<PlayerCameraController>();
    //        foreach (var controller in existingControllers)
    //        {
    //            if (controller.OwnerClientId == OwnerClientId)
    //            {
    //                Debug.Log($"Camera controller for player {OwnerClientId} already exists!");
    //                cameraController = controller;
    //                return;
    //            }
    //        }

    //        // Spawn camera controller
    //        GameObject controllerObject = Instantiate(playerCameraControllerPrefab);
    //        cameraController = controllerObject.GetComponent<PlayerCameraController>();

    //        // Network spawn the camera controller
    //        NetworkObject networkObject = controllerObject.GetComponent<NetworkObject>();
    //        if (networkObject != null)
    //        {
    //            networkObject.SpawnWithOwnership(OwnerClientId);
    //        }
    //        else
    //        {
    //            Debug.LogError("NetworkObject component missing on player camera controller prefab!");
    //        }

    //        if (IsHost)
    //        {
    //            hostCameraCreated = true;
    //        }
    //    }
    //}

    //public override void OnNetworkDespawn()
    //{
    //    // Cleanup
    //    if (cameraController != null && cameraController.gameObject != null)
    //    {
    //        Destroy(cameraController.gameObject);

    //        if (IsHost)
    //        {
    //            hostCameraCreated = false;
    //        }
    //    }

    //    base.OnNetworkDespawn();
    //}
}