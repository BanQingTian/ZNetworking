//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using NRKernal;

//public class test2 : MonoBehaviour {

//    ZClient client;

//    public Transform Cube;
//    public UnityEngine.UI.Text label;

//    private void Awake()
//    {
//        Loom.Initialize();
//    }

//    private void Start()
//    {
//        client = new ZClient(Global.exhibit, gameObject);
//        client.Persist();
//        client.RevMsgEvent += RevRPCMsg;
//        Cube = transform;
//        client.Connect("192.168.69.39", "50010");
//    }
//    public void RevRPCMsg(Zrime.Message msg)
//    {
//        StartCoroutine(moveCor(msg.ContentType == "move right"));
//        label.text = msg.Content;
//    }

//    IEnumerator moveCor(bool left)
//    {
//        Vector3 start = Cube.position;
//        while (Vector3.Distance(start, Cube.position) < 5)
//        {
//            Vector3 dir = left ? Vector3.left : Vector3.right;
//            Cube.position += dir * Time.deltaTime;
//            yield return null;
//        }
//    }

//    bool right = true;
//    // Update is called once per frame
//    void Update () {
//        if (NRInput.GetButtonDown(ControllerButton.TRIGGER))
//        {
//            if (right)
//            {
//                client.SendMsg("move right", "move11111111");
//            }
//            else
//            {
//                client.SendMsg("move left", "move22222222");
//            }
//            right = !right;
//        }
//    }

//    private void OnApplicationQuit()
//    {
//        client?.Leave();
//    }
//}
