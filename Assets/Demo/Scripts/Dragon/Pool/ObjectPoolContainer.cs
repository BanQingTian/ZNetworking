
public class ObjectPoolContainer<T>
{
    /// <summary>
    /// 对象
    /// </summary>
    private T _item;
    public T Item
    {
        get
        {
            return _item;
        }
        set
        {
            _item = value;
        }
    }

    /// <summary>
    /// 是否处于使用状态
    /// </summary>
    public bool Used { get; private set; }

    /// <summary>
    /// 设置为使用状态
    /// </summary>
    public void Consume()
    {
        Used = true;
    }
    /// <summary>
    /// 转为未使用状态
    /// </summary>
    public void Release()
    {
        Used = false;
    }
}
