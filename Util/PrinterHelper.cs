using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
namespace Borui.WinAPI
{
    /// <summary>
    /// 
    /// </summary>
    public class PrinterHelper
    {
        private PrinterHelper()
        {            
        }
        /// <summary>
        /// 获取安装的打印机列表
        /// </summary>
        /// <returns></returns>
        public static List<string> GetInstalledPrinters()
        {
            List<string> list = new List<string>();
            PrinterSettings prnset = new PrinterSettings();
            
            foreach (var item in PrinterSettings.InstalledPrinters)
            {
                list.Add(item.ToString());
                Console.WriteLine(item.ToString());
            }
            return list;
        }

        /// <summary>
        /// 打印图片
        /// </summary>
        /// <param name="img">图片</param>
        /// <param name="container">图片位置</param>
        /// <param name="pagesetting">页面设置</param>
        /// <param name="printersetting">打印机设置</param>
        public static void PrintImg(Image img,RectangleF container, PageSettings pagesetting=null, PrinterSettings printersetting=null)
        {
            PrintDocument pdoc = new PrintDocument();
            pdoc.DocumentName = "打印文件";
            if (null != pagesetting)
                pdoc.DefaultPageSettings = pagesetting;
            if (null != printersetting)
                pdoc.PrinterSettings = printersetting;
            pdoc.PrintPage += (s, e) => {
                var g = e.Graphics;
                if(container.IsEmpty)
                    container = new RectangleF(e.MarginBounds.Left - (e.PageBounds.Width - g.VisibleClipBounds.Width) / 2,
                        e.MarginBounds.Top - (e.PageBounds.Height - g.VisibleClipBounds.Height) / 2,
                        e.MarginBounds.Width,
                        e.MarginBounds.Height
                        );
                ScaleImageIsotropically(g,img,container);               
            };
            pdoc.Print();
        }

        private static void ScaleImageIsotropically(Graphics grfx,Image img,RectangleF rectf)
        {
            var sizef = new SizeF(img.Width/img.HorizontalResolution,img.Height/img.VerticalResolution);
            float fScale = Math.Min(rectf.Width/sizef.Width,rectf.Height/sizef.Height);
            sizef.Width *= fScale;
            sizef.Height *= fScale;
            grfx.DrawImage(img,rectf.X, rectf.Y, sizef.Width, sizef.Height);
        }
        /// <summary>
        /// 页面设置
        /// </summary>
        /// <param name="width">页面宽度</param>
        /// <param name="height">页面高度</param>
        /// <returns></returns>
        public static PageSettings DefaultPageSetting(int width,int height)
        {
            PageSettings pageset = new PageSettings();
            pageset.Margins = new Margins(0, 0, 0, 0);
            PaperSize psize = new PaperSize("mysize",width,height);
            pageset.PaperSize = psize;
            return pageset;
        }

        /// <summary>
        /// 打印机设置
        /// </summary>
        /// <param name="printer">打印机名称</param>
        /// <returns></returns>
        public static PrinterSettings DefaultPrinterSetting(string printer)
        {
            PrinterSettings prnset = new PrinterSettings();
            prnset.PrinterName = printer??prnset.PrinterName;
            return prnset;
        }

        /// <summary>
        /// 设置默认打印机
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        [DllImport("winspool.drv")]
        public static extern bool SetDefaultPrinter(String Name); //调用win api将指定名称的打印机设置为默认打印机

    }
}
