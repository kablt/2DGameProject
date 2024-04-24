using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{

    public enum Type {A,B,C};
    public Type enemyType;
    public int maxHealth;
    public int cureHealth;
    public Transform target;
    public BoxCollider mealarea;
    public bool isChase;
    public bool isAttack;

    Rigidbody rb;
    BoxCollider BoxCollider;
    Material mat;
    NavMeshAgent nav;
    Animator anim;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        BoxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<MeshRenderer>().material;
        nav = GetComponentInChildren<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        Invoke("ChaseStart", 2);
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("IsWalk", true);
    }
    void Update()
    {
        if (nav.enabled)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase; // !isChase == false;    
        }
    }



    void FreezeVelocity()
    {
        if(isChase)
        {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        }
    }

    void Targerting()
    {
        float targetRadius = 0f;
        float targetRange = 0f;

        switch(enemyType)
        {
            case Type.A:
                targetRadius = 1.5f;
                targetRange = 3f;
                break;
            case Type.B:
                targetRadius = 1f;
                targetRange = 12f;
                break;
            case Type.C:
                targetRadius = 1.5f;
                targetRange = 3f;
                break;
        }

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

        if(hits.Length > 0 && !isAttack)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {

        isChase = false;
        isAttack = true;
        anim.SetBool("IsAttack", true);

        switch(enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f);
                mealarea.enabled = true;
                yield return new WaitForSeconds(1f);
                mealarea.enabled = false;

                yield return new WaitForSeconds(1f);
                break;
            case Type.B:
                yield return new WaitForSeconds(0.1f);
                rb.AddForce(transform.forward * 20, ForceMode.Impulse);
                mealarea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rb.velocity = Vector3.zero;
                mealarea.enabled = false;

                yield return new WaitForSeconds(2f);

                break;
            case Type.C:

                break;
        }
        isAttack = false;
        isChase=true;
        anim.SetBool("IsAttack", false);
    }

   void FixedUpdate()
    {
        Targerting();
        FreezeVelocity(); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            cureHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec, false));

            Debug.Log("Melee :" + cureHealth);
        }
        else if(other.tag=="Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            cureHealth-= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);
            StartCoroutine(OnDamage(reactVec,false));
            Debug.Log("Bullet : " + cureHealth);
        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        cureHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec,true));
    }

    IEnumerator OnDamage(Vector3 reactVec, bool IsGrenade)
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
            gameObject.layer = 12; // øµªÛ¿∫ 14
            isChase = false;
            nav.enabled = false;
            anim.SetTrigger("doDie");
            if(IsGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;
                rb.freezeRotation = false;
                rb.AddForce(reactVec * 5, ForceMode.Impulse);
                rb.AddTorque(reactVec * 15, ForceMode.Impulse);
            }
            else
            {
            reactVec = reactVec.normalized;
            reactVec += Vector3.up;
            rb.AddForce(reactVec * 5, ForceMode.Impulse);
            }

            Destroy(gameObject, 4);
        }
    }
}
