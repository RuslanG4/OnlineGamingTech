using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Atmosphere : MonoBehaviour
{
    public float gravity = -10;
    public float radius = 3;
    public List<GameObject> objectsOnPlanet = new List<GameObject>();
    // Start is called before the first frame update

    void Start()
    {
        gatherObjectsInAtmosphere();
    }


    private void FixedUpdate()
    {

       // gatherObjectsInAtmosphere();
        foreach (GameObject obj in objectsOnPlanet)
        {

            if( obj != null)
            {
              
                applyGravity(obj);
                applyCorrectionRotation(obj);
                
            }
            
           


        }
    }

    // Update is called once per frame
    void Update()
    {

    }


    //public void updateAtmosphere()
    //{gatherObjectsInAtmosphere();
    //}

    public void gatherObjectsInAtmosphere()
    {
        Vector3 planetCenter = transform.position;

        Collider[] colliders = Physics.OverlapSphere(planetCenter, radius);

        foreach (Collider col in colliders)
        {
            if (col.gameObject != gameObject) // Avoid detecting itself
            {
                if (!objectsOnPlanet.Contains(col.gameObject))
                {
                    if( col.gameObject.GetComponent<Enemy>() != null)
                    {
                        objectsOnPlanet.Add(col.gameObject);
                    }
                    
                }
                
            }
        }

    }




    void applyGravity(GameObject body)
    {
        Vector3 gravityUp = (body.transform.position - transform.position).normalized;

        body.GetComponent<Rigidbody>().AddForce(gravityUp * gravity, ForceMode.Acceleration); //apply gravity atop player

    }

    public static void applyCorrectionRotation(GameObject body)
    {
        Vector3 gravityUp = body.transform.position.normalized;
        Vector3 bodyUp = body.transform.up; //player up vector

        Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp) * body.transform.rotation; //rotate towards the centre of the planet
        body.transform.rotation = Quaternion.Slerp(body.transform.rotation, targetRotation, 10 * Time.deltaTime);
    }

    public Vector3 getGravityDirection(Rigidbody rb)
    {
        Vector3 gravityDirection = (rb.transform.position - transform.position).normalized;

        return gravityDirection;
    }

    void OnDrawGizmos()
    {

        Gizmos.color = Color.blue;
        Vector3 center = transform.position;
        Gizmos.DrawWireSphere(center, radius);


       
    }

    void OnDrawGizmosSelected()
    {
        foreach (GameObject obj in objectsOnPlanet)
        {

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, obj.transform.position);
        }
    }

}
