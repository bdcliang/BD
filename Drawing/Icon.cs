namespace BD.Drawing
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    public class Icon
    {
        /// <summary>
        /// Convert BitMap to Icon
        /// </summary>
        /// <param name="image"></param>
        /// <param name="nullTonull"></param>
        /// <returns></returns>
        public static System.Drawing.Icon ConvertToIcon(Bitmap image, bool nullTonull = false)
        {
            if (image == null)
            {
                if (nullTonull) { return null; }
                throw new ArgumentNullException("image");
            }
            using (MemoryStream msImg = new MemoryStream()
                     , msIco = new MemoryStream())
            {
                image.Save(msImg, ImageFormat.Png);
                using (var bin = new BinaryWriter(msIco))
                {
                    //写图标头部
                    bin.Write((short)0);      //0-1保留
                    bin.Write((short)1);      //2-3文件类型。1=图标, 2=光标
                    bin.Write((short)1);      //4-5图像数量（图标可以包含多个图像）
                    bin.Write((byte)image.Width); //6图标宽度
                    bin.Write((byte)image.Height); //7图标高度
                    bin.Write((byte)0);      //8颜色数（若像素位深>=8，填0。这是显然的，达到8bpp的颜色数最少是256，byte不够表示）
                    bin.Write((byte)0);      //9保留。必须为0
                    bin.Write((short)0);      //10-11调色板
                    bin.Write((short)32);     //12-13位深
                    bin.Write((int)msImg.Length); //14-17位图数据大小
                    bin.Write(22);         //18-21位图数据起始字节
                    //写图像数据
                    bin.Write(msImg.ToArray());
                    bin.Flush();
                    bin.Seek(0, SeekOrigin.Begin);
                    return new System.Drawing.Icon(msIco);
                }
            }
        }
        /// <summary>
        /// Save Icon to Dir
        /// </summary>
        /// <param name="path"></param>
        /// <param name="image"></param>
        /// <param name="nullTonull"></param>
        public static void SaveIcon(string path, Bitmap image, bool nullTonull = false)
        {
            var tmp = ConvertToIcon(image, nullTonull);                
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                tmp.Save(fs);
                fs.Flush();
            }
        }
    }
}
