namespace BD.Util
{
    using System;
    using System.Runtime.InteropServices;
    using System.Drawing;
    /// <summary>
    /// 
    /// </summary>
    public class ScreenCaptureHelper
    {
        [DllImport("gdi32.dll")]
        private static extern void BitBlt(
            IntPtr hdcDest, //目标设备句柄
            int nXDest,     //目标对象左上角X坐标
            int nYDest,     //目标对象左上角Y坐标
            int nWidth,     //图像宽度
            int nHeight,    //图像高度
            IntPtr hdcSrc,  //原设备句柄
            int nXSrc,      //原设备X坐标
            int nYSrc,      //原设备Y坐标
            System.Int32 dwRop);//13369376
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDC(
            string lpszDrive,
            string lpszDevice,
            string lpszOutput,
            IntPtr lpszData);
        /// <summary>
        /// 指定区域截图API—BitBlt
        /// </summary>
        /// <param name="ScreenX">屏幕左上角X坐标</param>
        /// <param name="ScreenY">屏幕左上角Y坐标</param>
        /// <param name="ImageWidth">区域宽度</param>
        /// <param name="ImageHeight">区域高度</param>
        /// <returns>Bitmap</returns>
        public static System.Drawing.Bitmap PrintScreenImage(int ScreenX, int ScreenY, int ImageWidth, int ImageHeight)
        {
            IntPtr dc1 = CreateDC("DISPLAY", null, null, (IntPtr)null);
            Graphics g1 = Graphics.FromHdc(dc1);
            Bitmap ScreenImage = new Bitmap(ImageWidth, ImageHeight, g1);
            Graphics g2 = Graphics.FromImage(ScreenImage);
            IntPtr dc3 = g1.GetHdc();
            IntPtr dc2 = g2.GetHdc();

            BitBlt(dc2, 0, 0, ImageWidth, ImageHeight, dc3, ScreenX, ScreenY, 13369376);
            g1.ReleaseHdc(dc3);
            g2.ReleaseHdc(dc2);
            g1.Dispose();
            g2.Dispose();
            return ScreenImage;

        }
        /// <summary>
        /// 区域截屏CopyFromScreen
        /// </summary>
        /// <param name="ScreenX">屏幕左上角X坐标</param>
        /// <param name="ScreenY">屏幕左上角Y坐标</param>
        /// <param name="ImageWidth">区域宽度</param>
        /// <param name="ImageHeight">区域高度</param>
        /// <param name="CP">CopyPixelOperation.*</param>
        /// <returns>Bitmap</returns>
        public static System.Drawing.Bitmap CopyScreenImage(int ScreenX, int ScreenY, int ImageWidth, int ImageHeight, CopyPixelOperation CP)
        {

            Bitmap ScreenImage = new Bitmap(ImageWidth, ImageHeight);
            Graphics g2 = Graphics.FromImage(ScreenImage);
            g2.CopyFromScreen(new Point(ScreenX, ScreenY), new Point(0, 0), new Size(ImageWidth, ImageHeight), CP);
            g2.Dispose();
            return ScreenImage;
        }

    }
}
