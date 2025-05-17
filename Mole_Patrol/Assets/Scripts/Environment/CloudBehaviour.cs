using System.Collections;
using UnityEngine;

public class CloudBehaviour : MonoBehaviour
{
    private Renderer modelRenderer;
    private Material modelMaterial;
    public float fadeDuration = 4f;
    public Transform planet; // Reference to the planet
    public float speed = 8f; // Speed of cloud movement

    private Vector3 orbitAxis;

    void Start()
    {
        modelRenderer = GetComponentInChildren<Renderer>();
        modelMaterial = modelRenderer.material;

        // Ensure the material supports transparency
        modelMaterial.SetFloat("_Mode", 2); // 2 = Fade mode
        modelMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        modelMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        modelMaterial.SetInt("_ZWrite", 0);
        modelMaterial.DisableKeyword("_ALPHATEST_ON");
        modelMaterial.EnableKeyword("_ALPHABLEND_ON");
        modelMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        modelMaterial.renderQueue = 3000;

        if (planet == null)
        {
            planet = GameObject.Find("Planet").transform;
        }

        orbitAxis = Random.onUnitSphere;

        StartCoroutine(FadeIn());
    }

    private void Update()
    {
        Atmosphere.applyCorrectionRotation(gameObject);
        if (planet == null) return;

        transform.RotateAround(planet.position, orbitAxis, speed * Time.deltaTime);

    }

    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = modelMaterial.color;
        color.a = 0f; // Start fully transparent
        modelMaterial.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            modelMaterial.color = color;
            yield return null;
        }

        yield return new WaitForSeconds(3);
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color color = modelMaterial.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            modelMaterial.color = color;
            yield return null;
        }

        Destroy(gameObject);
    }
}