using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    float hAxis;
    float vAxis;
    public float speed = 15.0f;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;
    public GameObject ThrowGrenade;
    public int hasGrenades;
    public Camera followcamera;

    public int ammo;
    public int coin;
    public int health;
    public int score;

    public int maxammo;
    public int maxcoin;
    public int maxhealth;
    public int maxhasGrenades;

    bool wDown;
    bool jDown;
    bool fDown;
    bool gDown;
    bool rDown;

    bool isJump;
    bool isDodge;
    bool iDown;
    bool isSwap;
    bool isFireReady= true;
    bool isreload;
    bool isBorder;
    bool isDamage;
    bool isShop;

    bool sDown1;
    bool sDown2;
    bool sDown3;
    Rigidbody rb;

    Vector3 moveVec;
    Vector3 dodgeVec;
    Animator anim;
    MeshRenderer[] meshs;

    GameObject nearObject;
    public Weapon equipWeapon;
    int equipWeaponIndex=-1;
    float fireDelay;


    void Awake()
    {
        //스크립트가 달려있는 오브젝트의 자식 오브젝트에 animator가 달려있으므로 GetComponentInChildren통해 자식오브젝트에 접근하고
        //<>안에 접근할 컴포넌트 이름을 넣어준다.
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();

        //PlayerPrefs.SetInt("MaxScore", 112500);
        Debug.Log(PlayerPrefs.GetInt("MaxScore"));

    }
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Dodge();
        Interation();
        Swap();
        Grenade();
        Attack();
        Reload();
    }

    void GetInput()
    {
        hAxis = Input.GetAxis("Horizontal");
        vAxis = Input.GetAxis("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interation");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
        rDown = Input.GetButtonDown("Reload");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        rb.velocity = Vector3.zero;
        if (isDodge)
        {
            moveVec = dodgeVec;
        }
        if (isSwap || !isFireReady || isreload)
        {
            moveVec = Vector3.zero;
        }
        if(!isBorder)
        {
        //wDown(Walk)상태일때 속력값에 0.3곱해 이동을 느리게 한다. false시 기본속도 유지를 위해 *1f를 뒤에 써준다
        transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;
        }

        anim.SetBool("IsRun", moveVec != Vector3.zero); //moveVec가 Vector3.zero이면 움직임이 없다는뜻, 즉 가만히 있는게 아니라면 IsRun은 True로 해준다
        anim.SetBool("IsWalk", wDown);//input amanager에 등록되있는 Walk의 단축키 left shift가 눌렸을 경우, IsWalk를 True로 만들어준다는 뜻

    }

    void Turn()
    {
        //키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);
        //마우스에 의한 회전
        if (fDown) { 
        Ray ray = followcamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayhit;
            if (Physics.Raycast(ray, out rayhit, 100))
            {
                Vector3 nextVec = rayhit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap)
        {
            rb.AddForce(Vector3.up * 15, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;

        }
    }


    void Attack()
    {
        if (equipWeapon == null)
            return;
        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;
        

        if (fDown && isFireReady && !isDodge && !isSwap && !isShop)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee? "doSwing": "doShot");
            fireDelay = 0;
        }
    }

    void Grenade()
    {
        if(hasGrenades ==0)
        {
            return;
        }
        if (gDown && !isreload && !isSwap)
        {
            Ray ray = followcamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayhit;
            if (Physics.Raycast(ray, out rayhit, 100))
            {
                Vector3 nextVec = rayhit.point - transform.position;
                nextVec.y = 10;

                GameObject instantGrenade = Instantiate(ThrowGrenade, transform.position , transform.rotation);
                Rigidbody Grenaderb = instantGrenade.GetComponent<Rigidbody>();
                Grenaderb.AddForce(nextVec, ForceMode.Impulse);
                Grenaderb.AddTorque(Vector3.back*10, ForceMode.Impulse);

                hasGrenades --;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }

    void Reload()
    {
        if (equipWeapon == null)
            return;

        if (equipWeapon.type == Weapon.Type.Melee)
            return;
        if (ammo == 0)
            return;
        if(rDown && !isJump && !isDodge && !isSwap && isFireReady && !isShop)
        {
            anim.SetTrigger("doReload");
            isreload = true;

            Invoke("ReloadOut", 2f);
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo: equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isreload = false;
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap && !isShop)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            //시간차 만드는 함수 Invoke
            Invoke("DodgeOut", 0.4f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }
    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;

        int weaponIndex = -1;
            if (sDown1)weaponIndex = 0;
            if (sDown2)weaponIndex = 1;
            if (sDown3)weaponIndex = 2;
        if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge && !isShop)
        {
            if(equipWeapon !=null)
            equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");
            isSwap = true;

            Invoke("SwapOut", 0.4f);

        }
    }

    void SwapOut()
    {
        isSwap = false;
    }
    void Interation()
    {
        if (iDown && nearObject != null && !isJump && !isDodge)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;
                Debug.Log("eeeeeee");

                Destroy(nearObject);
            }
            else if(nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
            }
        }
    }

    void FreezeRotation()
    {
        rb.angularVelocity = Vector3.zero;
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, moveVec,5, LayerMask.GetMask("Wall"));
    }

    private void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }



    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.tag=="Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxammo)
                    {
                        ammo = maxammo;
                    }
                    break;
                case Item.Type.Coin: 
                    coin+= item.value;
                    if(coin>maxcoin)
                    {
                        coin = maxcoin;
                    }
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if(health>maxhealth)
                    {
                        health = maxhealth;
                    }
                    break;
                case Item.Type.Grenade:
                    if (hasGrenades == maxhasGrenades)
                        return;
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    break;
            }
            Destroy(other.gameObject);
        }
        else if(other.tag == "EnemyBullet")
        {
            if(!isDamage)
            {
            Bullet enemyBullet = other.GetComponent<Bullet>();
            health -= enemyBullet.damage;
                bool isBossAtk = other.name == "BossMeleeArea";

            StartCoroutine(OnDamage(isBossAtk));
            }
               
               Destroy(other.gameObject);
        }
    }

    IEnumerator OnDamage(bool isBossAtk)
    {
        isDamage = true;
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.red;
        }
        if(isBossAtk)
        {
            rb.AddForce(transform.forward * -25f, ForceMode.Impulse);
        }
        yield return new WaitForSeconds(1f);
        isDamage = false;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }
        if (isBossAtk)
        {
            rb.velocity = Vector3.zero;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon" || other.tag =="Shop")
        {
            nearObject = other.gameObject;

           // Debug.Log(nearObject.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = null;
        }
        else if(other.tag =="Shop")
        {
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Exit();
            nearObject = null;
            isShop = false;
        }
    }

}

