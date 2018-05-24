using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BD.API
{
    public class Screen
    {
        [DllImport("gdi32.dll")]
        public static extern int GetDeviceGammaRamp(IntPtr hDC, ref RAMP lpRamp);

        [DllImport("gdi32.dll")]
        public static extern int SetDeviceGammaRamp(IntPtr hDC, ref RAMP lpRamp);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct RAMP
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public UInt16[] Red;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public UInt16[] Green;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public UInt16[] Blue;
        }
        static RAMP ramp;
        static Screen()
        {
            ramp= new RAMP();
        }
        /// <summary>
        /// gamma 必须在3和44之间
        /// </summary>
        /// <param name="gamma"></param>
        void SetGamma(int gamma)
        {
            if (gamma < 3 || gamma > 44) return;
            ramp.Red = new ushort[256];
            ramp.Green = new ushort[256];
            ramp.Blue = new ushort[256];

            for (int i = 1; i< 256; i++)
            {                
                ramp.Red[i] = ramp.Green[i] = ramp.Blue[i] = (ushort) (System.Math.Min(65535, System.Math.Max(0, System.Math.Pow((i + 1) / 256.0, gamma* 0.1) * 65535 + 0.5)));
            }
            SetDeviceGammaRamp(GetDC(IntPtr.Zero), ref ramp);
        }
    }
}
