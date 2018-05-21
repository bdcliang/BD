namespace CoreAudioApi
{
    using CoreAudioApi.Interfaces;
    using System;
    using System.Runtime.InteropServices;

    internal class AudioEndpointVolumeCallback : IAudioEndpointVolumeCallback
    {
        private AudioEndpointVolume _Parent;

        internal AudioEndpointVolumeCallback(AudioEndpointVolume parent)
        {
            this._Parent = parent;
        }

        [PreserveSig]
        public int OnNotify(IntPtr NotifyData)
        {
            AUDIO_VOLUME_NOTIFICATION_DATA audio_volume_notification_data = (AUDIO_VOLUME_NOTIFICATION_DATA) Marshal.PtrToStructure(NotifyData, typeof(AUDIO_VOLUME_NOTIFICATION_DATA));
            IntPtr ptr = Marshal.OffsetOf(typeof(AUDIO_VOLUME_NOTIFICATION_DATA), "ChannelVolume");
            IntPtr ptr2 = (IntPtr) (((long) NotifyData) + ((long) ptr));
            float[] channelVolume = new float[audio_volume_notification_data.nChannels];
            for (int i = 0; i < audio_volume_notification_data.nChannels; i++)
            {
                channelVolume[i] = (float) Marshal.PtrToStructure(ptr2, typeof(float));
            }
            AudioVolumeNotificationData notificationData = new AudioVolumeNotificationData(audio_volume_notification_data.guidEventContext, audio_volume_notification_data.bMuted, audio_volume_notification_data.fMasterVolume, channelVolume);
            this._Parent.FireNotification(notificationData);
            return 0;
        }
    }
}

