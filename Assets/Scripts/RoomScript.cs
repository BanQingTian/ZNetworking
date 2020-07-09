using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Grpc.Core;
using World;
using System.Collections.Generic;

public class RoomScript : MonoBehaviour
{
    [SerializeField]
    string roomID;

    [SerializeField]
    Transform cameraTransform;

    [SerializeField]
    GameObject dummyPrefab;

    string userID;

    Channel channel;
    World.Room.RoomClient client;

    Dictionary<string, GameObject> userGameObjects = new Dictionary<string, GameObject>();

    void OnEnable()
    {
        channel = new Channel("localhost:50051", ChannelCredentials.Insecure);
        client = new World.Room.RoomClient(channel);

        JoinResponse response = client.Join(new JoinRequest
        {
            RoomId = this.roomID
        });

        this.userID = response.UserId;
    }

    void OnDisable()
    {
        client.Leave(new LeaveRequest
        {
            UserId = this.userID,
            RoomId = this.roomID,
        });

        channel.ShutdownAsync().Wait();
    }

    // Update is called once per frame
    void Update()
    {
        var context = SynchronizationContext.Current;
        Vector3 position = cameraTransform.position;
        Vector3 rotation = cameraTransform.eulerAngles;

        Task.Run(() =>
        {
            var response = client.Sync(new SyncRequest
            {
                RoomId = this.roomID,
                User = new User
                {
                    UserId = this.userID,
                    Transform = new Transform_
                    {
                        X = position.x,
                        Y = position.y,
                        Z = position.z,
                    },
                    Rotation = new Rotation_
                    {
                        EulerX = rotation.x,
                        EulerY = rotation.y,
                        EulerZ = rotation.z,
                    }
                },
            });

            var enumrator = response.Users.GetEnumerator();

            context.Post(__ =>
            {
                SyncRoom(enumrator);
            }, null);
        });
    }

    private void SyncRoom(IEnumerator<User> enumrator)
    {
        var exceptUserIDList = new List<string>(userGameObjects.Keys);

        while (enumrator.MoveNext())
        {
            var user = enumrator.Current;

            if (this.userID != user.UserId)
            {
                GameObject userGameObject;
                if (userGameObjects.ContainsKey(user.UserId))
                {
                    userGameObject = userGameObjects[user.UserId];
                }
                else
                {
                    userGameObject = Instantiate(dummyPrefab, Vector3.zero, Quaternion.identity, this.transform.root);
                    userGameObjects[user.UserId] = userGameObject;
                }

                userGameObject.transform.position = new Vector3(
                  x: user.Transform.X, y: user.Transform.Y, z: user.Transform.Z
                );
                userGameObject.transform.eulerAngles = new Vector3(
                  x: user.Rotation.EulerX, y: user.Rotation.EulerY, z: user.Rotation.EulerZ
                );

                exceptUserIDList.Remove(user.UserId);
            }
        }

        foreach (var userId in exceptUserIDList)
        {
            Destroy(userGameObjects[userId]);
            userGameObjects.Remove(userId);
        }
    }
}
