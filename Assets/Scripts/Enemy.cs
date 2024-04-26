using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI; 

public class Enemy : MonoBehaviour
{

    public enum Type {A,B,C,D};
    public Type enemyType;
    public int maxHealth;
    public int cureHealth;
    public Transform target;
    public BoxCollider mealarea;
    public GameObject bullet;
    public bool isChase;
    public bool isAttack;
    public bool isDead;

    public Rigidbody rb;
    public BoxCollider boxCollider;
    public MeshRenderer[] meshs;
    public NavMeshAgent nav;
    public Animator anim;

    void Awake()
    {

        if(enemyType != Type.D)
        {
        Invoke("ChaseStart", 2);
        }
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("IsWalk", true);
    }
    void Update()
    {
        if (nav.enabled && enemyType != Type.D)
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
        if(!isDead && enemyType != Type.D)
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
                    targetRadius = 0.5f;
                    targetRange = 25f;
                    break;
            }

            RaycastHit[] hits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

            if(hits.Length > 0 && !isAttack)
            {
                StartCoroutine(Attack());
            }

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
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
               Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;
                yield return new WaitForSeconds(2f);
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
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.red;
        }
        yield return new WaitForSeconds(0.1f);

        if(cureHealth>0)
        {
            foreach (MeshRenderer mesh in meshs)
            {
                mesh.material.color = Color.white;
            }
        }
        else
        {
            foreach (MeshRenderer mesh in meshs)
            {
                mesh.material.color = Color.gray;
            }
            gameObject.layer = 12; // øµªÛ¿∫ 14
            isDead = true;
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

            if(enemyType != Type.D)
            {
            Destroy(gameObject, 4);
            }
        }
    }
}
