using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public int damageHp = 1;
    public string belongId;
    public Rigidbody Rig;

    private void OnEnable()
    {
        Invoke("ReleaseObj", 2);
    }
    private void ReleaseObj()
    {
        Rig.isKinematic = true;
        PoolManager.Instance.Release(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dragon") && !DragonManager.Instance.Dragon.dead)
        {
            DragonAttack dragon = other.GetComponentInParent<DragonAttack>();
            if(dragon != null)
            {
                dragon.DamageDragon(1);
            }
        }
        
        gameObject.SetActive(false);
    }
    
}
