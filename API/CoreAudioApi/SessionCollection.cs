namespace CoreAudioApi
{
    using CoreAudioApi.Interfaces;
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class SessionCollection
    {
        private IAudioSessionEnumerator _AudioSessionEnumerator;

        internal SessionCollection(IAudioSessionEnumerator realEnumerator)
        {
            this._AudioSessionEnumerator = realEnumerator;
        }

        public int Count
        {
            get
            {
                int num;
                Marshal.ThrowExceptionForHR(this._AudioSessionEnumerator.GetCount(out num));
                return num;
            }
        }

        public AudioSessionControl this[int index]
        {
            get
            {
                IAudioSessionControl2 control;
                Marshal.ThrowExceptionForHR(this._AudioSessionEnumerator.GetSession(index, out control));
                return new AudioSessionControl(control);
            }
        }
    }
}

