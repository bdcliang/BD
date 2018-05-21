namespace BD.API
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    internal class Mouse
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void SetCursorPos(int x,int y)
        {
            APIBase.SetCursorPos(x,y);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        public static void SetCursorPos(Point point)
        {
           SetCursorPos(point.X,point.Y);
        }
        /// <summary>
        /// wheel left
        /// </summary>
        public static void WheelLeft()
        {
            APIBase.mouse_event(MouseEventFlag.Wheel, 0, 0, -120, UIntPtr.Zero);
        }

        /// <summary>
        /// mouse click
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void MouseClick(int x, int y)
        {
            SetCursorPos(x, y);
            APIBase.mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
            APIBase.mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }
        /// <summary>
        /// wheel right
        /// </summary>
        public static void WheelRight()
        {
            APIBase.mouse_event(MouseEventFlag.Wheel, 0, 0, 120, UIntPtr.Zero);
        }

        /// <summary>
        /// show or hiddle mouse
        /// </summary>
        public static void ShowMouse()
        {
            APIBase.ShowCursor(1);
        }
        /// <summary>
        /// 
        /// </summary>
        public static void HiddeMouse()
        {
            APIBase.ShowCursor(0);
        }

        /// <summary>
        /// 获取鼠标位置
        /// </summary>
        /// <returns></returns>
        public static Point GetCursorPos()
        {
            Point point = new Point();
            APIBase.GetCursorPos(ref point);
            return point;
        }

        /// <summary>
        /// 鼠标左键按下
        /// </summary>
        public static void MouseLeftDown()
        {
            APIBase.mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
        }
        /// <summary>
        /// 鼠标左键释放
        /// </summary>
        public static void MouseLeftUp()
        {
            APIBase.mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }
    }
}
