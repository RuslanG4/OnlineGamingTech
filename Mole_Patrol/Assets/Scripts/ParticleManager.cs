using System.Collections;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public GameObject deathParticlePrefab;
    public GameObject hitParticle;
    public GameObject pickUpParticle;
    public GameObject explosionParticle;
    public GameObject towerPlaceParticle;
    private static ParticleManager _instance;

    private GameObject planetRef;
    public static ParticleManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ParticleManager>();
                if (_instance == null)
                {
                    GameObject singleton = new GameObject("ParticleManager");
                    _instance = singleton.AddComponent<ParticleManager>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        planetRef = GameObject.Find("Planet");

        if (planetRef == null)
        {
            StartCoroutine(WaitForPlanetReference());
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private IEnumerator WaitForPlanetReference()
    {
        yield return null;

        planetRef = GameObject.Find("Planet");

        if (planetRef != null)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            StartCoroutine(WaitForPlanetReference());
        }
    }
    public void PlayPickUpParticle(Vector3 _pos)
    {
        PlayParticle(_pos, pickUpParticle);
    }

    public void PlayExplosionParticle(Vector3 _pos)
    {
        PlayParticle(_pos, explosionParticle);

        //// Align particles
        //foreach (Transform child in explosionInstance.transform)
        //{
        //    child.rotation = Quaternion.LookRotation(normal);
        //}
    }
    public void PlayTowerPlaceParticle(Vector3 _pos)
    {
        PlayParticle(_pos, towerPlaceParticle);
    }
    public void PlayHitParticle(Vector3 _pos)
    {
        PlayParticle(_pos, hitParticle);
    }
    public void PlayDeathParticle(Vector3 _pos)
    {
        PlayParticle(_pos, deathParticlePrefab);
    }

    private void PlayParticle(Vector3 _pos, GameObject prefab)
    {
        GameObject particleEffect = Instantiate(prefab, _pos, Quaternion.identity, planetRef.transform);

        ParticleSystem particleSystem = particleEffect.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Play();
        }

        Destroy(particleEffect, Mathf.Max(particleSystem.main.duration, 0.1f));
    }
}

