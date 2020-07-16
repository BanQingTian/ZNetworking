using UnityEngine;

public class FlockChildFactory : MonoBehaviour
{
    public static FlockChildFactory Instance;

    void Awake()
    {
        Instance = this;
    }

    public FlockChild[] mChildren;

    public FlockChild GetRandomChild()
    {
        int index = Random.Range(0, mChildren.Length);
        return mChildren[index];
    }
}