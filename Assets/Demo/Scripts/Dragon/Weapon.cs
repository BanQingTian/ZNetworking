using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    public GameObject Model;
    public Bullet BubbleBullet;
    public Transform ShootPoint;

    public void ShootBubble(string playerId)
    {
        Bullet b = PoolManager.Instance.Get(BubbleBullet.gameObject, ShootPoint.position, ShootPoint.rotation, null).GetComponent<Bullet>();
        b.Rig.isKinematic = false;
        b.belongId = playerId;
        b.Rig.AddForce(ShootPoint.forward * 300);
    }
}
