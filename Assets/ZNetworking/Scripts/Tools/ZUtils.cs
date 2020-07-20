using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ZUtils
{


    public static string GetUUID()
    {
        return Guid.NewGuid().ToString();
    }

    public static long GetTimeStamp(bool bflag = true)
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        long ret;
        if (bflag)
            ret = Convert.ToInt64(ts.TotalSeconds);
        else
            ret = Convert.ToInt64(ts.TotalMilliseconds);
        return ret;
    }


    // 获取SD卡路径
    public static string GetSdcardPath()
    {
        string path = null;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        path = System.IO.Directory.GetParent(Application.dataPath).ToString() + "/";
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            path = "file://" + System.IO.Directory.GetCurrentDirectory()+"/";
#elif UNITY_ANDROID
            path ="file:///storage/emulated/0/";
#endif
        return path;
    }


    // 判断物体是否在视野内
    public static bool CheckInSight(Vector3 self, Vector3 target, float angle)
    {
        float forwardDir = Vector3.Dot(self, target);
        if (forwardDir < 0)
        {
            return false;
        }
        if (angle > Vector3.Angle(self, target))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 将pose转换成矩阵
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public static Matrix4x4 GetTMatrix(Vector3 position, Quaternion rotation)
    {
        return Matrix4x4.TRS(position, rotation, Vector3.one);
    }

    /// <summary>
    /// 对应点pos矩阵转换
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static Vector3 GetPositionFromTMatrix(Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03;
        position.y = matrix.m13;
        position.z = matrix.m23;

        return position;
    }

    /// <summary>
    /// 对应点rotation矩阵转换
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static Quaternion GetRotationFromTMatrix(Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }
}
