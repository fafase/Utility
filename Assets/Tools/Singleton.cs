using UnityEngine;

namespace Tools
{
    public abstract class Singleton<T> : MonoBehaviour  where T : Singleton<T>
    {
        private static T s_instance;

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(this);
                return;
            }
            s_instance = this as T;      
            DontDestroyOnLoad(gameObject);
        }

        public static T Instance 
        {
            get 
            { 
                if (s_instance == null) 
                {
                    s_instance = FindObjectOfType<T>();
                    if(s_instance == null) 
                    {
                        GameObject obj = new GameObject(typeof(T).Name);
                        s_instance = obj.AddComponent<T>();
                    }
                }
                return s_instance;
            }
        }
    }
}