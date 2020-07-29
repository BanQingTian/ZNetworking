using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballVFX : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("fireball"))
        {
            DragonManager.Instance.Dragon.BoomEff.gameObject.SetActive(true);
            DragonManager.Instance.Dragon.BoomEff.Play();
            DragonManager.Instance.Dragon.BoomEff.transform.position = other.transform.position;
            other.gameObject.SetActive(false);
        }
        
    }
}
