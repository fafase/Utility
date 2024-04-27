using System.Collections.Generic;

namespace Tools
{
    public sealed class UpdateManager : Singleton<UpdateManager>, IUpdateManager
    {
        private List <IUpdateBehaviour> m_behaviours = new List<IUpdateBehaviour>();
        public int UpdateCount => m_behaviours.Count;
        public void RegisterUpdate(IUpdateBehaviour behaviour) 
        {
            if(behaviour == null) 
            {
                return;
            }
            if (m_behaviours.Contains(behaviour)) 
            {
                return;
            }
            m_behaviours.Add(behaviour);
        }

        public void UnregisterUpdate(IUpdateBehaviour behaviour)
        {
            if(behaviour == null) 
            {
                return;
            }
            m_behaviours.Remove(behaviour);
        }

        private void Update()
        {
            for (int i = m_behaviours.Count -1 ; i >=0; --i) 
            {
                IUpdateBehaviour current = m_behaviours[i];
                if(current == null) 
                {
                    m_behaviours.Remove(current);
                    continue;
                }
                if (current.IsActive) 
                {
                    current.OnUpdate();
                }          
            }
        }
    }
    public interface IUpdateManager 
    {
        void RegisterUpdate(IUpdateBehaviour behaviour);
        void UnregisterUpdate(IUpdateBehaviour behaviour);
    }

    public interface IUpdateBehaviour 
    {
        bool IsActive { get; }
        void OnUpdate();
    }
}
