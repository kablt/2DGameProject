using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class Item : MonoBehaviour
{
    //enum을 통해 저장할 타입을 열거한것
    public enum Type {Ammo, Coin , Grenade , Heart , Weapon };
    public Type type;
    public int value;

    Rigidbody rb;
    SphereCollider sphereCollider;

    void Awake()
    {
       rb= GetComponent<Rigidbody>();
        //같은컴포넌트가 2개이상 일때 겟컴포넌트로 불러올시 가장위에 있는것을 불러온다.
        sphereCollider= GetComponent<SphereCollider>();
    }
    private void Update()
    {
        //아이템 회전을 구현, Vector3.up은 해당 아이템의 y축을 기준으로 잡은것
        transform.Rotate(Vector3.up * 30 * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag=="Floor")
        {
            rb.isKinematic = true;
            sphereCollider.enabled = false;
        }
    }
}
