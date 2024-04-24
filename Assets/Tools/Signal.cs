using System;
using System.Collections.Generic;

namespace Tools
{
    public static class Signal
    {
        public delegate void SignalCallback<T>(T data);
        public delegate void SignalCallbackNoParam();

        private static IDictionary<Type, Delegate> s_handlers = new Dictionary<Type, Delegate>();

        public static int GetCount<T>() where T:SignalData
        {
            var del = GetDelegate<T>().Item2;
            return del != null ? del.GetInvocationList().Length : 0;
        }
        
        public static void Connect<T>(SignalCallback<T> callback) where T : SignalData => InternalConnect<T>(callback);
        
        public static void Connect<T>(SignalCallbackNoParam callback) where T : SignalData => InternalConnect<T>(callback);
        
        public static void Disconnect<T>(SignalCallbackNoParam callback) where T : SignalData => InternalDisconnect<T>(callback);
        
        public static void Disconnect<T>(SignalCallback<T> callback) where T : SignalData => InternalDisconnect<T>(callback);

        public static void Send<T>(T data) where T : SignalData 
        {
            var (t, del) = GetDelegate<T>();
            del?.DynamicInvoke(data);
        }

        public static void Send<T>() where T : SignalData 
        {
            var (t, del) = GetDelegate<T>();
            del?.DynamicInvoke();
        }

        public static void ClearAll() => s_handlers.Clear();

        public static void ClearList<T>() where T: SignalData => s_handlers.Remove(typeof(T));

        private static void InternalConnect<T>(Delegate callback) where T : SignalData
        {
            var (t, del) = GetDelegate<T>();
            s_handlers[t] = Delegate.Combine(del, callback);
        }

        private static void InternalDisconnect<T>(Delegate callback) where T : SignalData
        {
            var (t, del) = GetDelegate<T>();
            s_handlers[t] = Delegate.Remove(del, callback);
        }

        private static (Type,Delegate) GetDelegate<T>() where T : SignalData
        {          
            Type t = typeof(T);
            if (!s_handlers.ContainsKey(t)) 
            {
                s_handlers[t] = null;
            }
            return (t,s_handlers[t]);   
        }
    }

    public class SignalData { }
}
