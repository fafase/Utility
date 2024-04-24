using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Tools
{
    public class Initialization : MonoBehaviour, IInitializer
    {
        [SerializeField] private List<Object> m_initObjects;
        [SerializeField] private bool m_showDebug = false;
        [SerializeField] private UnityEvent m_onStart;
        [SerializeField] private UnityEventComplete m_onComplete;

        public List<Object> InitObjects => m_initObjects;
        public bool ShowDebug => m_showDebug;
        public bool IsInit { get; set; }
        public void OnStart()
        {
            InitStart?.Invoke();
            m_onStart?.Invoke();
        }
        public void OnComplete(List<InitializationResult> result)
        {
            InitComplete?.Invoke(result);
            m_onComplete?.Invoke(result);
        }

        public event Action InitStart;
        public event Action<List<InitializationResult>> InitComplete;

        private InitializerProces m_init;
        void Awake()
        {
            // Fire and Forget process
            m_init = new InitializerProces();
            m_init.Init(this).Forget();
        }

        void OnDestroy()
        {
            if (IsInit) { return; }
            m_init.Cancel();
        }
    }
    public class InitializerProces
    {
        private CancellationTokenSource m_source;
        private IInitializer m_initializer;
        public async UniTask<List<InitializationResult>> Init(IInitializer initializer)
        {
            m_initializer = initializer;   
            if (m_initializer.IsInit)
            {
                return null;
            }
            List<UniTask<InitializationResult>> tasks = new List<UniTask<InitializationResult>>();
            List<InitializationResult> results = new List<InitializationResult>();
            try
            {
                // Are we already cancelled
                m_source = new CancellationTokenSource();
                m_source.Token.ThrowIfCancellationRequested();
                initializer.OnStart();
                foreach (Object obj in m_initializer.InitObjects) 
                {
                    if(obj is IInit init) 
                    {
                        if (init.ShouldWaitForCompletion)
                        {
                            tasks.Add(init.InitAsync());
                        }
                        else
                        {
                            results.Add(init.Init());
                        }
                    }
                }
                InitializationResult[] rs = await UniTask.WhenAll(tasks).AttachExternalCancellation(m_source.Token);
                results.AddRange(rs);
                if (initializer.ShowDebug)
                {
                    results.ForEach((result) => Debug.Log(result));
                }
            }
            catch (Exception)
            {
                // Propagate with null
                m_initializer.OnComplete(null);
                return null;
            }
            finally 
            {
                m_source?.Dispose();
            }

            m_initializer.OnComplete(results);
            m_initializer.IsInit = true;
            return results;
        }
        public void Cancel()
        {
            if (m_initializer == null || m_initializer.IsInit) 
            {
                return;
            }
            m_source?.Cancel();
        }
        async Task <InitializationResult> asyncMethod() 
        {
            await Task.Delay(500);
            return new InitializationResult(true, "This");
        }
    }
    [System.Serializable]
    public class UnityEventComplete : UnityEvent<List<InitializationResult>>
    {
    }
    public interface IInitializer
    {
        List<Object> InitObjects { get; }
        bool ShowDebug { get; }
        bool IsInit { get; set; }
        void OnStart();
        void OnComplete(List<InitializationResult> list);
    }

    public interface IInit
    {
        UniTask<InitializationResult> InitAsync();
        InitializationResult Init();
        bool ShouldWaitForCompletion { get; }
        bool IsInit { get; }
    }
    public class InitializationResult
    {
        public bool Success { get; } = false;
        public string Message { get; } = null;
        public InitializationResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public override string ToString()
        {
            return $"{Success} - {Message}";
        }

        public string FormatMessage(string objectName, bool result)
        {
            string addendum = result ? "" : "no";
            return $"{objectName} finished Initialization with {addendum}";
        }
    }
}
