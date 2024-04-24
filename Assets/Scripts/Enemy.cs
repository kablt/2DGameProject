using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    public int maxHealth;
    public int cureHealth;

    Rigidbody rb;
    BoxCollider BoxCollider;
    Material mat;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        BoxCollider = GetComponent<BoxCollider>();
        mat = GetComponent<MeshRenderer>().material;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            cureHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec));

            Debug.Log("Melee :" + cureHealth);
        }
        else if(other.tag=="Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            cureHealth-= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);
            StartCoroutine(OnDamage(reactVec));
            Debug.Log("Bullet : " + cureHealth);
        }
    }

    IEnumerator OnDamage(Vector3 reactVec)
    {
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if(cureHealth>0)
        {
            mat.color = Color.white;
        }
        else
        {
            mat.color = Color.gray;
            gameObject.layer = 12;

            reactVec = reactVec.normalized;
            reactVec += Vector3.up;
            rb.AddForce(reactVec * 5, ForceMode.Impulse);

            Destroy(gameObject, 4);
        }
    }
}
