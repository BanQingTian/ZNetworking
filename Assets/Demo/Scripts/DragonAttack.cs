using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonAttack : MonoBehaviour
{

    public GameObject AttackPoint;
    public GameObject Fireball;
    public Rigidbody Rig;
    public Transform DragonParent;

    public void ShootFireball()
    {
        Rig.isKinematic = true;
        Rig.isKinematic = false;
        Fireball.transform.position = AttackPoint.transform.position;
        Fireball.transform.rotation = AttackPoint.transform.rotation;
        Fireball.SetActive(true);
        Rig.AddForce(AttackPoint.transform.forward * 300);
    }

    public void RotateHead()
    {
        float random = Random.Range(50f, 130f);
        //random = random > 0 ? 40 : -40;

        StopCoroutine(rotateHeadCor(DragonParent, random));
        StartCoroutine(rotateHeadCor(DragonParent, random));

        Debug.Log(random);
    }
    private IEnumerator rotateHeadCor(Transform startTrans, float end)
    {
        float dir = Time.deltaTime * 10;
        dir = end > startTrans.eulerAngles.y ? dir : -dir;
        while (Mathf.Abs(startTrans.eulerAngles.y - end) > dir * 4)
        {
            Vector3 v3 = DragonParent.transform.eulerAngles;
            float y = v3.y + dir;
            startTrans.transform.eulerAngles = new Vector3(v3.x, y, v3.z);
            if (y > 130 || y < 50)
            {
                startTrans.transform.eulerAngles = new Vector3(v3.x, Mathf.Clamp(y, 50, 130), v3.z);
            }
            yield return null;
        }
        startTrans.eulerAngles = new Vector3(DragonParent.transform.eulerAngles.x, end, DragonParent.transform.eulerAngles.z);
    }

}
