using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FishFlock
{
    public class CustomTitle : PropertyAttribute
    {
        public string title;
        public CustomTitle(string title)
        {
            this.title = title;
        }
    }

    public class FoldoutArray : PropertyAttribute
    { }

    public class VecHelpers
    {
        public static Vector3 mul(Vector3 l, Vector3 r)
        {
            Vector3 new_v = l;

            new_v.x *= r.x;
            new_v.y *= r.y;
            new_v.z *= r.z;

            return new_v;
        }

        public static Vector3 mul(Vector3 l, float r)
        {
            Vector3 new_v = l;

            new_v.x *= r;
            new_v.y *= r;
            new_v.z *= r;

            return new_v;
        }
    }
}