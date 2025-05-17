using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class PlanetGeneration : MonoBehaviour
{
    [Range(0, 5)] public int subdivisions = 3;

    private List<Vector3> vertices;
    private List<int> triangles;
    private Dictionary<long, int> middlePointIndexCache;

    public List<Vector3> centroids;
    public float scale = 2f;
    private GameObject planet;

    public GameObject prefab;

    public Material material1;
    public Material material2;

    public GameObject cactusOne;
    public GameObject cactusTwo;
    public GameObject deadGrass;
    public GameObject deadTree;

    private Unity.Mathematics.Random rng;

    private bool planetGenerated = false;
    private PlanetSync planetSync;

    void Start()
    {
        planet = new GameObject("Planet");
        planetSync = FindObjectOfType<PlanetSync>();
    }

    void Update()
    {
        if (!planetGenerated && planetSync != null && planetSync.SeedIsSet)
        {
            GenerateWithSeed(planetSync.seed.Value);
            planetGenerated = true;
        }
    }

    public void GenerateWithSeed(uint seed)
    {
        rng = new Unity.Mathematics.Random(seed);
        GeneratePlanet();
    }

    void GeneratePlanet()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        middlePointIndexCache = new Dictionary<long, int>();
        centroids = new List<Vector3>();

        CreateIcosahedron();
        Subdivide(subdivisions);
        CreateTriangleMeshes();
    }

    void CreateIcosahedron()
    {
        float t = (1f + Mathf.Sqrt(5f)) / 2f;

        vertices.AddRange(new Vector3[] {
            new Vector3(-1, t, 0).normalized * scale,
            new Vector3(1, t, 0).normalized * scale,
            new Vector3(-1, -t, 0).normalized * scale,
            new Vector3(1, -t, 0).normalized * scale,
            new Vector3(0, -1, t).normalized * scale,
            new Vector3(0, 1, t).normalized * scale,
            new Vector3(0, -1, -t).normalized * scale,
            new Vector3(0, 1, -t).normalized * scale,
            new Vector3(t, 0, -1).normalized * scale,
            new Vector3(t, 0, 1).normalized * scale,
            new Vector3(-t, 0, -1).normalized * scale,
            new Vector3(-t, 0, 1).normalized * scale
        });

        int[] icosahedronTriangles = {
            0, 11, 5,  0, 5, 1,  0, 1, 7,  0, 7, 10,  0, 10, 11,
            1, 5, 9,  5, 11, 4,  11, 10, 2,  10, 7, 6,  7, 1, 8,
            3, 9, 4,  3, 4, 2,  3, 2, 6,  3, 6, 8,  3, 8, 9,
            4, 9, 5,  2, 4, 11,  6, 2, 10,  8, 6, 7,  9, 8, 1
        };

        triangles.AddRange(icosahedronTriangles);
    }

    void Subdivide(int level)
    {
        for (int i = 0; i < level; i++)
        {
            List<int> newTriangles = new List<int>();
            middlePointIndexCache.Clear();

            for (int j = 0; j < triangles.Count; j += 3)
            {
                int a = GetMiddlePoint(triangles[j], triangles[j + 1]);
                int b = GetMiddlePoint(triangles[j + 1], triangles[j + 2]);
                int c = GetMiddlePoint(triangles[j + 2], triangles[j]);

                newTriangles.AddRange(new int[] {
                    triangles[j], a, c,
                    triangles[j + 1], b, a,
                    triangles[j + 2], c, b,
                    a, b, c
                });
            }

            triangles = newTriangles;
        }
    }

    int GetMiddlePoint(int p1, int p2)
    {
        long smallerIndex = Mathf.Min(p1, p2);
        long largerIndex = Mathf.Max(p1, p2);
        long key = (smallerIndex << 32) + largerIndex;

        if (middlePointIndexCache.TryGetValue(key, out int cachedIndex))
            return cachedIndex;

        Vector3 middle = (vertices[p1] + vertices[p2]).normalized * scale;
        int index = vertices.Count;
        vertices.Add(middle);

        middlePointIndexCache[key] = index;
        return index;
    }

    Vector3 CalculateCentroids(Vector3[] triangle)
    {
        return (triangle[0] + triangle[1] + triangle[2]) / 3f;
    }

    void CreateTriangleMeshes()
    {
        int triangleIndex = 0;
        for (int i = 0; i < triangles.Count; i += 3)
        {
            GameObject triangleObject = new GameObject("Triangle " + triangleIndex++);
            triangleObject.transform.SetParent(planet.transform);

            Mesh mesh = new Mesh();
            Vector3[] triangleVertices = {
                vertices[triangles[i]],
                vertices[triangles[i + 1]],
                vertices[triangles[i + 2]]
            };

            mesh.vertices = triangleVertices;
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 1) };
            mesh.RecalculateNormals();

            triangleObject.AddComponent<MeshFilter>().mesh = mesh;
            triangleObject.AddComponent<MeshRenderer>().materials = new Material[] {
                rng.NextInt(0, 2) == 0 ? material1 : material2
            };

            triangleObject.AddComponent<MeshCollider>().sharedMesh = mesh;
            triangleObject.tag = "Triangle";

            Vector3 pos = CalculateCentroids(triangleVertices);
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, pos.normalized);
            Instantiate(prefab, pos, rot, triangleObject.transform).name = "centroid";

            GenerateEnvironment(triangleVertices, triangleObject.transform);
            GenerateTrees(triangleVertices, triangleObject.transform);

            GameManager.Instance.allTriangles.Add(triangleObject);
        }
    }

    void GenerateEnvironment(Vector3[] triangleVertices, Transform parent)
    {
        float chance = rng.NextFloat();

        if (chance <= 0.15f)
        {
            Vector3 pos = GetRandomPointInTriangle(triangleVertices);
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, pos.normalized);
            Instantiate(GetRandomPrefab(), pos, rot, parent);
        }
    }

    void GenerateTrees(Vector3[] triangleVertices, Transform parent)
    {
        float chance = rng.NextFloat();

        if (chance <= 0.02f)
        {
            Vector3 pos = GetRandomPointInTriangle(triangleVertices);
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, pos.normalized);
            Instantiate(deadTree, pos, rot, parent);
        }
    }

    GameObject GetRandomPrefab()
    {
        GameObject[] prefabs = { cactusOne, cactusTwo, deadGrass };
        return prefabs[rng.NextInt(0, prefabs.Length)];
    }

    Vector3 GetRandomPointInTriangle(Vector3[] tri)
    {
        float r1 = rng.NextFloat();
        float r2 = rng.NextFloat();

        if (r1 + r2 > 1f)
        {
            r1 = 1f - r1;
            r2 = 1f - r2;
        }

        return tri[0] + r1 * (tri[1] - tri[0]) + r2 * (tri[2] - tri[0]);
    }
}
