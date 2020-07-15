using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Global
{

    public static string languge = "en";

    public static string CurRoom = dragon;

    public const float FrameRate = 30;


    #region Scene

    public const string seaworld = "seaworld";
    public const string exhibit = "exhibit";
    public const string dragon = "dragon";

    public static string GetRoomName(RoomEnum re)
    {
        string name;
        switch (re)
        {
            case RoomEnum.Dragon:
                name = dragon;
                break;
            case RoomEnum.Exhibit:
                name = exhibit;
                break;
            case RoomEnum.Seaworld:
                name = seaworld;
                break;
            default:
                name = seaworld;
                break;
        }
        return name;
    }

    #endregion

}


public enum RoomEnum
{
    Dragon = 0,
    Exhibit = 1,
    Seaworld = 2,
}