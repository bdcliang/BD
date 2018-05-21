namespace BD.Messaging
{
    using System;

    public class WeakAction<T> : WeakAction, IExecuteWithObject
    {
        private readonly Action<T> _action;

        public WeakAction(object target, Action<T> action) : base(target, null)
        {
            this._action = action;
        }

        public void Execute()
        {
            if ((this._action != null) && base.IsAlive)
            {
                this._action(default(T));
            }
        }

        public void Execute(T parameter)
        {
            if ((this._action != null) && base.IsAlive)
            {
                this._action(parameter);
            }
        }

        public void ExecuteWithObject(object parameter)
        {
            T local = (T) parameter;
            this.Execute(local);
        }

        public Action<T> Action
        {
            get
            {
                return this._action;
            }
        }
    }
}

