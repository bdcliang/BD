using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreAudioApi;
namespace BD.API
{
    public class Audio
    {

        static MMDevice defaultDevice = null;
        static Audio()
        {            
            MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
            defaultDevice = devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
        }
        /// <summary>
        /// get the status of system volume
        /// </summary>
        /// <returns></returns>
        public static bool IsMuted()
        {
            return defaultDevice.AudioEndpointVolume.Mute;
        }
        //
        /// <summary>
        /// set system mute 
        /// </summary>
        public static void SetMute()
        {
            defaultDevice.AudioEndpointVolume.Mute = true;
        }
        // 
        /// <summary>
        /// set system unmute
        /// </summary>
        public static void SetUnMute()
        {
            defaultDevice.AudioEndpointVolume.Mute = false;
        }
        /// <summary>
        /// Volume down step
        /// 
        /// </summary>
        public static void VolumeStepDown()
        {
            defaultDevice.AudioEndpointVolume.VolumeStepDown();
        }
        /// <summary>
        /// volume up step
        /// </summary>
        public static void VolumeStepUp()
        {
            defaultDevice.AudioEndpointVolume.VolumeStepUp();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static AudioEndPointVolumeVolumeRange VolumeRange()
        {
            return defaultDevice.AudioEndpointVolume.VolumeRange;
        }
    }
}
