using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject meshObj;
    public GameObject effectObj;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Explonsion());
    }

    IEnumerator Explonsion()
    {
        yield return new WaitForSeconds(3);
        meshObj.SetActive(false);
        effectObj.SetActive(true);
        rb = GetComponent<Rigidbody>();
        rb.AddExplosionForce(100,Vector3.up,7f);
    }
   

}
