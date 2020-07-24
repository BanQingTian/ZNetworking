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
    /// 跟随旋转 update
    /// </summary>
    /// <param name="watcher"></param>
    /// <param name="target"></param>
    /// <param name="speed"></param>
    public static void Look(Transform watcher, Transform target, float speed = 7)
    {
        Vector3 directionToFace = target.position - watcher.position;//sphere 就是要注视的obj
        //Debug.DrawRay(watcher.position, target.position, Color.green);//在scene中画出debug线
        //watcher.rotation = Quaternion.LookRotation(directionToFace);//LookAt

        //如果想要缓慢lookat，而不是在sphere移动的时候瞬间lookat（头转太快脖子容易闪到，也不太合理），可以使用slerp函数，写法如下：（需要把第三句代码注释掉）
        Quaternion targetRotation = Quaternion.LookRotation(directionToFace);//先获取LookAt角度
        watcher.rotation = Quaternion.Slerp(watcher.rotation, targetRotation, Time.deltaTime * speed);//在1秒内注视LookAt完毕


    }

    // 获取两个向量的夹角  Vector3.Angle 只能返回 [0, 180] 的值
    // 如真实情况下向量 a 到 b 的夹角（80 度）则 b 到 a 的夹角是（-80）
    // 通过 Dot、Cross 结合获取到 a 到 b， b 到 a 的不同夹角
    public static void GetAngle(Vector3 a, Vector3 b)
    {
        Vector3 c = Vector3.Cross(a, b);
        float angle = Vector3.Angle(a, b);

        //Debug.Log(Mathf.Acos(Vector3.Dot(a.normalized, b.normalized))*Mathf.Rad2Deg);
        //Debug.Log(Mathf.Acos(Vector3.Dot(b.normalized, a.normalized)) * Mathf.Rad2Deg);

        //Debug.Log(Vector3.Cross(a, b));
        //Debug.Log(Vector3.Cross(a.normalized, b.normalized));
        //Debug.Log(Vector3.Cross(b.normalized, a.normalized));

        // b 到 a 的夹角
        float sign = Mathf.Sign(Vector3.Dot(c.normalized, Vector3.Cross(a.normalized, b.normalized)));
        float signed_angle = angle * sign;

        Debug.Log("b -> a :" + signed_angle);

        // a 到 b 的夹角
        sign = Mathf.Sign(Vector3.Dot(c.normalized, Vector3.Cross(b.normalized, a.normalized)));
        signed_angle = angle * sign;

        Debug.Log("a -> b :" + signed_angle);
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
