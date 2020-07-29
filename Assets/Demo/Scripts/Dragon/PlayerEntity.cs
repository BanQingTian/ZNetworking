using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zrime;
using NRKernal;

/// <summary>
/// GRPC网络组件实例
/// </summary>
public class PlayerEntity : Entity
{
    public Weapon Weapon;

    public override void Init(Player info)
    {
        base.Init(info);
    }

    public override void UpdatePoseData()
    {
        base.UpdatePoseData();

        if (Weapon != null)
        {
            if (DragonManager.Instance.PlayingFight)
            {
                if (PlayerInfo.PlayerName.Equals("arcore"))
                {
                    Weapon.gameObject.SetActive(false);
                }
                else
                {
                    Weapon.gameObject.SetActive(true);
                }
            }
            else
            {
                Weapon.gameObject.SetActive(false);
            }
            

            // 同步手柄位置
            Weapon.transform.position = new Vector3(PlayerInfo.secondPostion.x, PlayerInfo.secondPostion.y, PlayerInfo.secondPostion.z);
            Weapon.transform.eulerAngles = new Vector3(PlayerInfo.secondEuler.x, PlayerInfo.secondEuler.y, PlayerInfo.secondEuler.z);
        }
    }

}
