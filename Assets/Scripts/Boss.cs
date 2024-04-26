using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    public GameObject missile;
    public Transform MissilePortA;
    public Transform MissilePortB;
    public bool islook;

    Vector3 lookvec;
    Vector3 tauntVec;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponentInChildren<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        StartCoroutine(Think());

        nav.isStopped = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            StopAllCoroutines();    
            return;
        }
        if (islook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookvec = new Vector3(h, 0, v) * 5f;
            transform.LookAt(target.position + lookvec);
        }
        else
        {
            nav.SetDestination(tauntVec);
        }
    }

    IEnumerator Think()
    {
        yield return new WaitForSeconds(0.1f);
        int ranAction = Random.Range(0, 5);
        switch(ranAction)
        {
            case 0:
            case 1:
                StartCoroutine(MissileShot());
                break;
                //미사일 패턴
            case 2:
            case 3:
                StartCoroutine(RockShot());
                break;
                //돌굴러가는패턴
            case 4:
                //점프공격
                StartCoroutine(Taunt());
                break;

        }

    }

    IEnumerator MissileShot()
    {
        anim.SetTrigger("doShot");
        yield return new WaitForSeconds(0.2f);
        GameObject insatantMissileA = Instantiate(missile, MissilePortA.position, MissilePortA.rotation);
        BossMissile bossMissileA = insatantMissileA.GetComponent<BossMissile>();
        bossMissileA.target = target;

        yield return new WaitForSeconds(0.3f);
        GameObject insatantMissileB = Instantiate(missile, MissilePortB.position, MissilePortB.rotation);
        BossMissile bossMissileB = insatantMissileB.GetComponent<BossMissile>();
        bossMissileB.target = target;

        yield return new WaitForSeconds(2f);

        StartCoroutine(Think());
    }

    IEnumerator RockShot()
    {
        islook = false;
        anim.SetTrigger("doBigShot");
        Instantiate(bullet, transform.position, transform.rotation);
        yield return new WaitForSeconds(3f);
        islook = true;
        StartCoroutine(Think());
    }

    IEnumerator Taunt()
    {
        tauntVec = target.position + lookvec;

        islook = false;
        nav.isStopped = false;
        boxCollider.enabled = false;
        anim.SetTrigger("doTaunt");

        yield return new WaitForSeconds(1.5f);
        mealarea.enabled = true;
        yield return new WaitForSeconds(0.5f);
        mealarea.enabled = false;
        yield return new WaitForSeconds(1f);
        islook = true;
        nav.isStopped = true;
        boxCollider.enabled = true;
        StartCoroutine(Think());
    }
}
