using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRun : MonoBehaviour
{


    ZClient client;

    private void Awake()
    {
        Loom.Initialize();
    }

    private void Start()
    {
        client = new ZClient("szcw", gameObject);
        client.Persist();
    }



    // Update is called once per frame
    void Update()
    {
        transform.rotation = Random.rotation;

        if (Input.GetKeyDown(KeyCode.A))
        {
            client.Connect("localhost", "50010");
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            client.CreateStream();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            client.SendMsg("test","abc");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            client.DisposeStream();
            client.Leave();
        }

    }

    private void OnApplicationQuit()
    {
        client.DisposeStream();
        client.Leave();
    }
}
