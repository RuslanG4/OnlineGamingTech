using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public static float SpawnRate = 6f; // 6 seconds
    private GameObject[] triangleObjects; 


    void Start()
    {
        // Start the delayed spawning process
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {

        yield return new WaitUntil(() => GameObject.Find("Planet") != null);

        triangleObjects = GameObject.FindGameObjectsWithTag("Triangle");

        if (triangleObjects.Length == 0)
        {
            Debug.LogError("No GameObjects with the 'Triangle' tag found in the scene.");
            yield break;
        }

        InvokeRepeating("SpawnEnemy", 0f, SpawnRate);
    }

    void SpawnEnemy()
    {
        int randomIndex = Random.Range(0, triangleObjects.Length);
        GameObject randomTriangle = triangleObjects[randomIndex];

        PlaceEnemy(randomTriangle);
    }

    public void PlaceEnemy(GameObject triangle)
    {
        if (triangle == null)
        {
            Debug.LogError("Triangle not initialized.");
            return;
        }

        Vector3 position = triangle.transform.Find("centroid").transform.position;

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, position.normalized);

        Instantiate(enemyPrefab, position, rotation, triangle.transform);
        //GameManager.Instance.UpdateEnemySpawned();
    }
}
