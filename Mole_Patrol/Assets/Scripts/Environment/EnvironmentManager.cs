using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance { get; private set; }
    private Transform planetTransform;
    public GameObject cloudPrefab;
    public float spawnInterval = 5f;

    private List<Transform> planetChildren = new List<Transform>();

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

        StartCoroutine(FindPlanet());
    }

    IEnumerator FindPlanet()
    {
        yield return new WaitUntil(() => GameObject.Find("Planet") != null);

        planetTransform = GameObject.Find("Planet").transform;

        if (planetTransform != null)
        {
            foreach (Transform child in planetTransform)
            {
                Transform centroid = child.Find("centroid").transform;
                planetChildren.Add(centroid);
            }
        }
    }


    void Start()
    {
        StartCoroutine(SpawnClouds());
    }

    IEnumerator SpawnClouds()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (planetChildren.Count > 0)
            {
                Transform randomChild = planetChildren[Random.Range(0, planetChildren.Count)];
                Vector3 spawnPosition = randomChild.position + randomChild.up * 3f; // Adjust height above child

                Quaternion spawnRotation = Quaternion.LookRotation(randomChild.up, randomChild.forward);

                Instantiate(cloudPrefab, spawnPosition, spawnRotation, randomChild.transform.parent.transform.parent);
            }
        }
    }

}
