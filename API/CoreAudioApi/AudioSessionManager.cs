namespace CoreAudioApi
{
    using CoreAudioApi.Interfaces;
    using System;
    using System.Runtime.InteropServices;

    public class AudioSessionManager
    {
        private IAudioSessionManager2 _AudioSessionManager;
        private SessionCollection _Sessions;

        internal AudioSessionManager(IAudioSessionManager2 realAudioSessionManager)
        {
            IAudioSessionEnumerator enumerator;
            this._AudioSessionManager = realAudioSessionManager;
            Marshal.ThrowExceptionForHR(this._AudioSessionManager.GetSessionEnumerator(out enumerator));
            this._Sessions = new SessionCollection(enumerator);
        }

        public SessionCollection Sessions
        {
            get
            {
                return this._Sessions;
            }
        }
    }
}

