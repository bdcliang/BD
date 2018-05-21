namespace BD.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    public class Messenger : IMessenger
    {
        private static Messenger _defaultInstance;
        private Dictionary<Type, List<WeakActionAndToken>> _recipientsOfSubclassesAction;
        private Dictionary<Type, List<WeakActionAndToken>> _recipientsStrictAction;
        private readonly object _registerLock = new object();
        private static readonly object CreationLock = new object();

        private void Cleanup()
        {
            CleanupList(this._recipientsOfSubclassesAction);
            CleanupList(this._recipientsStrictAction);
        }

        private static void CleanupList(IDictionary<Type, List<WeakActionAndToken>> lists)
        {
            if (lists != null)
            {
                lock (lists)
                {
                    List<Type> list = new List<Type>();
                    foreach (KeyValuePair<Type, List<WeakActionAndToken>> pair in lists)
                    {
                        List<WeakActionAndToken> list2 = new List<WeakActionAndToken>();
                        foreach (WeakActionAndToken token in pair.Value)
                        {
                            if ((token.Action == null) || !token.Action.IsAlive)
                            {
                                list2.Add(token);
                            }
                        }
                        foreach (WeakActionAndToken token2 in list2)
                        {
                            pair.Value.Remove(token2);
                        }
                        if (pair.Value.Count == 0)
                        {
                            list.Add(pair.Key);
                        }
                    }
                    foreach (Type type in list)
                    {
                        lists.Remove(type);
                    }
                }
            }
        }

        private static bool Implements(Type instanceType, Type interfaceType)
        {
            if ((interfaceType != null) && (instanceType != null))
            {
                foreach (Type type in instanceType.GetInterfaces())
                {
                    if (type == interfaceType)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void OverrideDefault(Messenger newMessenger)
        {
            _defaultInstance = newMessenger;
        }

        public virtual void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            this.Register<TMessage>(recipient, null, false, action);
        }

        public virtual void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            this.Register<TMessage>(recipient, null, receiveDerivedMessagesToo, action);
        }

        public virtual void Register<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            this.Register<TMessage>(recipient, token, false, action);
        }

        public virtual void Register<TMessage>(object recipient, object token, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            lock (this._registerLock)
            {
                Dictionary<Type, List<WeakActionAndToken>> dictionary;
                Type key = typeof(TMessage);
                if (receiveDerivedMessagesToo)
                {
                    if (this._recipientsOfSubclassesAction == null)
                    {
                        this._recipientsOfSubclassesAction = new Dictionary<Type, List<WeakActionAndToken>>();
                    }
                    dictionary = this._recipientsOfSubclassesAction;
                }
                else
                {
                    if (this._recipientsStrictAction == null)
                    {
                        this._recipientsStrictAction = new Dictionary<Type, List<WeakActionAndToken>>();
                    }
                    dictionary = this._recipientsStrictAction;
                }
                lock (dictionary)
                {
                    List<WeakActionAndToken> list;
                    if (!dictionary.ContainsKey(key))
                    {
                        list = new List<WeakActionAndToken>();
                        dictionary.Add(key, list);
                    }
                    else
                    {
                        list = dictionary[key];
                    }
                    WeakAction<TMessage> action2 = new WeakAction<TMessage>(recipient, action);
                    WeakActionAndToken item = new WeakActionAndToken {
                        Action = action2,
                        Token = token
                    };
                    list.Add(item);
                }
            }
            this.Cleanup();
        }

        public static void Reset()
        {
            _defaultInstance = null;
        }

        public virtual void Send<TMessage>(TMessage message)
        {
            this.SendToTargetOrType<TMessage>(message, null, null);
        }

        public virtual void Send<TMessage, TTarget>(TMessage message)
        {
            this.SendToTargetOrType<TMessage>(message, typeof(TTarget), null);
        }

        public virtual void Send<TMessage>(TMessage message, object token)
        {
            this.SendToTargetOrType<TMessage>(message, null, token);
        }

        private static void SendToList<TMessage>(TMessage message, IEnumerable<WeakActionAndToken> list, Type messageTargetType, object token)
        {
            if (list != null)
            {
                foreach (WeakActionAndToken token2 in list.Take<WeakActionAndToken>(list.Count<WeakActionAndToken>()).ToList<WeakActionAndToken>())
                {
                    IExecuteWithObject action = token2.Action as IExecuteWithObject;
                    if (((((action != null) && token2.Action.IsAlive) && (token2.Action.Target != null)) && (((messageTargetType == null) || (token2.Action.Target.GetType() == messageTargetType)) || Implements(token2.Action.Target.GetType(), messageTargetType))) && (((token2.Token == null) && (token == null)) || ((token2.Token != null) && token2.Token.Equals(token))))
                    {
                        action.ExecuteWithObject(message);
                    }
                }
            }
        }

        private void SendToTargetOrType<TMessage>(TMessage message, Type messageTargetType, object token)
        {
            Type instanceType = message.GetType();
            if (this._recipientsOfSubclassesAction != null)
            {
                foreach (Type type2 in this._recipientsOfSubclassesAction.Keys.Take<Type>(this._recipientsOfSubclassesAction.Count<KeyValuePair<Type, List<WeakActionAndToken>>>()).ToList<Type>())
                {
                    List<WeakActionAndToken> list = null;
                    if (((instanceType == type2) || instanceType.IsSubclassOf(type2)) || Implements(instanceType, type2))
                    {
                        lock (this._recipientsOfSubclassesAction)
                        {
                            list = this._recipientsOfSubclassesAction[type2].Take<WeakActionAndToken>(this._recipientsOfSubclassesAction[type2].Count<WeakActionAndToken>()).ToList<WeakActionAndToken>();
                        }
                    }
                    SendToList<TMessage>(message, list, messageTargetType, token);
                }
            }
            if ((this._recipientsStrictAction != null) && this._recipientsStrictAction.ContainsKey(instanceType))
            {
                List<WeakActionAndToken> list3 = null;
                lock (this._recipientsStrictAction)
                {
                    list3 = this._recipientsStrictAction[instanceType].Take<WeakActionAndToken>(this._recipientsStrictAction[instanceType].Count<WeakActionAndToken>()).ToList<WeakActionAndToken>();
                }
                SendToList<TMessage>(message, list3, messageTargetType, token);
            }
            this.Cleanup();
        }

        public virtual void Unregister(object recipient)
        {
            UnregisterFromLists(recipient, this._recipientsOfSubclassesAction);
            UnregisterFromLists(recipient, this._recipientsStrictAction);
        }

        public virtual void Unregister<TMessage>(object recipient)
        {
            this.Unregister<TMessage>(recipient, null, null);
        }

        public virtual void Unregister<TMessage>(object recipient, Action<TMessage> action)
        {
            this.Unregister<TMessage>(recipient, null, action);
        }

        public virtual void Unregister<TMessage>(object recipient, object token)
        {
            this.Unregister<TMessage>(recipient, token, null);
        }

        public virtual void Unregister<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            UnregisterFromLists<TMessage>(recipient, token, action, this._recipientsStrictAction);
            UnregisterFromLists<TMessage>(recipient, token, action, this._recipientsOfSubclassesAction);
            this.Cleanup();
        }

        private static void UnregisterFromLists(object recipient, Dictionary<Type, List<WeakActionAndToken>> lists)
        {
            if (((recipient != null) && (lists != null)) && (lists.Count != 0))
            {
                lock (lists)
                {
                    foreach (Type type in lists.Keys)
                    {
                        foreach (WeakActionAndToken token in lists[type])
                        {
                            WeakAction action = token.Action;
                            if ((action != null) && (recipient == action.Target))
                            {
                                action.MarkForDeletion();
                            }
                        }
                    }
                }
            }
        }

        private static void UnregisterFromLists<TMessage>(object recipient, object token, Action<TMessage> action, Dictionary<Type, List<WeakActionAndToken>> lists)
        {
            Type key = typeof(TMessage);
            if (((recipient != null) && (lists != null)) && ((lists.Count != 0) && lists.ContainsKey(key)))
            {
                lock (lists)
                {
                    foreach (WeakActionAndToken token2 in lists[key])
                    {
                        WeakAction<TMessage> action2 = token2.Action as WeakAction<TMessage>;
                        if ((((action2 != null) && (recipient == action2.Target)) && ((action == null) || (action == action2.Action))) && ((token == null) || token.Equals(token2.Token)))
                        {
                            token2.Action.MarkForDeletion();
                        }
                    }
                }
            }
        }

        public static Messenger Default
        {
            get
            {
                if (_defaultInstance == null)
                {
                    lock (CreationLock)
                    {
                        if (_defaultInstance == null)
                        {
                            _defaultInstance = new Messenger();
                        }
                    }
                }
                return _defaultInstance;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WeakActionAndToken
        {
            public WeakAction Action;
            public object Token;
        }
    }
}

