using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public Transform planetCenter;
    public float spinSpeed = 30f; 

    private GoldMineTower towerRef;

    private float goldAmount = 50;

    private void Awake()
    {
        planetCenter = GameObject.Find("Planet").transform;

        towerRef = transform.parent.GetComponent<GoldMineTower>();

        Vector3 upDirection = (transform.position - planetCenter.position).normalized;

        transform.rotation = Quaternion.LookRotation(transform.forward, upDirection);

        goldAmount = (float)towerRef.GetTowerData().GoldGeneration;

        InvokeRepeating(nameof(IncreaseGold), (float)towerRef.GetTowerData().GoldTimeInterval, (float)towerRef.GetTowerData().GoldTimeInterval); 
    }

    private void IncreaseGold()
    {
        goldAmount += (float)towerRef.GetTowerData().GoldGeneration;
        Debug.Log("GOLD AMOUNT" + goldAmount);
    }

    private void LateUpdate()
    {
        Vector3 upDirection = (transform.position - planetCenter.position).normalized;
        transform.rotation = Quaternion.LookRotation(transform.forward, upDirection);

        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
    }

    private void OnMouseDown()
    {
        GameManager.Instance.UpdateCoinCount((int)goldAmount);
        GameObject.FindFirstObjectByType<BuildingInformationHandler>().UpdateUI();
        if (towerRef != null)
        {
            towerRef.Reset();
        }

        ParticleManager.Instance.PlayPickUpParticle(gameObject.transform.position);

        Destroy(gameObject);
    }
}
