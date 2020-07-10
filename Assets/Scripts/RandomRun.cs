using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRun : MonoBehaviour
{


    ZClient client;

    public Transform Cube;
    public UnityEngine.UI.Text label;

    private void Awake()
    {
        Loom.Initialize();
    }

    private void Start()
    {
        client = new ZClient(Global.exhibit, gameObject);
        client.Persist();
        client.RevMsgEvent += RevRPCMsg;
        Cube = transform;

        client.Connect("192.168.69.39", "50010");
    }

    public void RevRPCMsg(Zrime.Message msg)
    {
        StartCoroutine(moveCor(msg.ContentType == "move right"));
        label.text = msg.Content;
    }

    IEnumerator moveCor(bool left)
    {
        Vector3 start = Cube.position;
        while (Vector3.Distance(start, Cube.position) < 5)
        {
            Vector3 dir = left ? Vector3.left : Vector3.right;
            Cube.position += dir * Time.deltaTime;
            yield return null;
        }
    }

    public void SendMsg1()
    {
        client.SendMsg("move right", "move11111111");
    }
    public void SendMsg2()
    {
        client.SendMsg("move left", "move22222222");
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Random.rotation;

        if (Input.GetKeyDown(KeyCode.A))
        {
            client.Connect("192.168.69.39", "50010");
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            client.CreateStream();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            client.SendMsg("test", "adfasdfasdfasdfasdfasdfasdfasdfasdf");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            client.Leave();
        }

    }

    private void OnApplicationQuit()
    {
        client.Leave();
    }
}
