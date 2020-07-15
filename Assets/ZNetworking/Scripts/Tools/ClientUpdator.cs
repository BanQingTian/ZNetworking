using System;
using UnityEngine;

public class ClientUpdator : MonoBehaviour
{
    public event Action onUpdate;
    private static ClientUpdator m_Instance;
    public static ClientUpdator Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = GameObject.FindObjectOfType<ClientUpdator>();
                if (m_Instance == null)
                {
                    GameObject go = new GameObject("ClientUpdator");
                    m_Instance = go.AddComponent<ClientUpdator>();
                    DontDestroyOnLoad(m_Instance);
                }
            }
            return m_Instance;
        }
    }

    void Update()
    {
        onUpdate?.Invoke();
    }
}
