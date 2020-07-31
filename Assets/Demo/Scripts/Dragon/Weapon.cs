using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    public GameObject Model;
    public Bullet BubbleBullet;
    public Transform ShootPoint;
    public AudioSource ShootAudio;
    public void ShootBubble(string playerId)
    {
        ShootAudio.Play();

        Bullet b = PoolManager.Instance.Get(BubbleBullet.gameObject, ShootPoint.position, ShootPoint.rotation, null).GetComponent<Bullet>();

        float randomScale = Random.Range(0.12f, 0.3f);
        b.transform.localScale = Vector3.one * randomScale;
        b.Rig.isKinematic = false;
        b.belongId = playerId;
        b.Rig.AddForce(ShootPoint.forward * 300);
    }
}
