using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zrime;


/// <summary>
/// GRPC网络组件实例
/// </summary>
public class PlayerEntity : Entity
{
    public Transform Weapon;


    public override void Init(Player info)
    {
        base.Init(info);
    }

    public override void UpdatePoseData()
    {
        base.UpdatePoseData();

        if (Weapon != null)
        {
            Weapon.position = new Vector3(PlayerInfo.secondPostion.x, PlayerInfo.secondPostion.y, PlayerInfo.secondPostion.z);
            Weapon.eulerAngles = new Vector3(PlayerInfo.secondEuler.x, PlayerInfo.secondEuler.y, PlayerInfo.secondEuler.z);
        }
    }

}
