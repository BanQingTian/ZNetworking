using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class openButterflyCrow : MonoBehaviour {

    public GameObject TL_Butterfly;
    void Start()
    {
        TL_Butterfly.SetActive(false);
        StartCoroutine(ButterCrowOpen());
    }



    IEnumerator ButterCrowOpen()
    {
        yield return new WaitForSeconds(8.0f);
        TL_Butterfly.SetActive(true);
    }

}
