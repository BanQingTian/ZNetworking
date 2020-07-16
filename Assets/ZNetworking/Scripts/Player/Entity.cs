using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zrime;

public class Entity : MonoBehaviour
{
    // 对象数据
    public ZPlayer PlayerInfo;

    public bool isHouseOwner;
    public bool isOwner;

    public virtual void Init(Player info)
    {
        PlayerInfo = new ZPlayer();
        PlayerInfo.Copy(info);
        isHouseOwner = PlayerInfo.IsHouseOwner;
        isOwner = ZClient.Instance.PlayerID == info.PlayerId;
    }

    public void UpdateData(Player info)
    {
        //if (isOwner) return;
        PlayerInfo.Copy(info);
        UpdatePoseData();
    }

    public virtual void UpdatePoseData()
    {
        if (gameObject != null)
        {
            this.transform.position = PlayerInfo.postion;
            this.transform.eulerAngles = PlayerInfo.euler;
        }
    }

}

public class ZPlayer
{
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public bool IsHouseOwner { get; set; }
    public Vector3 postion { get; set; }
    public Vector3 euler { get; set; }
    public string extraContent { get; set; }

    public void Copy(Player player)
    {
        PlayerId = player.PlayerId;
        PlayerName = player.PlayerName;
        IsHouseOwner = player.IsHouseOwner;
        postion = new Vector3(player.Position.X, player.Position.Y, player.Position.Z);
        euler = new Vector3(player.Rotation.EulerX, player.Rotation.EulerY, player.Rotation.EulerZ);
        extraContent = player.ExtraContent;
    }
}


