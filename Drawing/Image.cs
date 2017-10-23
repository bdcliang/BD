using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace BD.Drawing
{
    public class Image
    {

        public static void SaveThumbnail(Bitmap bit, Size size,string targePath)
        {
            System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters();
            long[] quality = new long[1];
            quality[0] = 75;
            System.Drawing.Imaging.EncoderParameter encoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            System.Drawing.Imaging.ImageCodecInfo[] arrayICI = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            System.Drawing.Imaging.ImageCodecInfo jpegICI = null;
            for (int x = 0; x < arrayICI.Length; x++)
            {
                if (arrayICI[x].FormatDescription.Equals("JPEG"))
                {
                    jpegICI = arrayICI[x];
                    break;
                }
            }            
            var image = Thumbnail(bit,size);
            image.Save(targePath, jpegICI, encoderParams);
        }
        public static void SaveThumbnail(string path, Size size, string targePath)
        {
            using (Bitmap bit = new Bitmap(path))
            {
                SaveThumbnail(bit, size, targePath);
            }
        }

        public static Bitmap Thumbnail(string path, Size size)
        {
            using (Bitmap bit = new Bitmap(path))
            {
              return  Thumbnail(bit, size);
            }
        }
        /// <summary>
        /// Thumbnail image
        /// </summary>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Bitmap Thumbnail(Bitmap source, Size size)
        {
            int HEIGHT = size.Height;//190;
            int WIDTH = size.Width; //190;
            //Configure JPEG Compression Engine   
            //System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters();
            //long[] quality = new long[1];
            //quality[0] = 75;
            //System.Drawing.Imaging.EncoderParameter encoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            //encoderParams.Param[0] = encoderParam;
            //System.Drawing.Imaging.ImageCodecInfo[] arrayICI = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            //System.Drawing.Imaging.ImageCodecInfo jpegICI = null;
            //for (int x = 0; x < arrayICI.Length; x++)
            //{
            //    if (arrayICI[x].FormatDescription.Equals("JPEG"))
            //    {
            //        jpegICI = arrayICI[x];
            //        break;
            //    }
            //}
            int wi, hi;
            wi = WIDTH;
            hi = HEIGHT;
                // maintain the aspect ratio despite the thumbnail size parameters   
            if (source.Width > source.Height)
                {
                    wi = WIDTH;
                    hi = (int)(source.Height * ((decimal)WIDTH / source.Width));
                }
            else
                {
                    hi = HEIGHT;
                    wi = (int)(source.Width * ((decimal)HEIGHT / source.Height));
                }
            using (System.Drawing.Bitmap thumb = new Bitmap(wi, hi))
                {
                    using (Graphics g = Graphics.FromImage(thumb))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.FillRectangle(Brushes.White, 0, 0, wi, hi);
                        g.DrawImage(source, 0, 0, wi, hi);
                    }
                    //string targetPath = Path.Combine(targetFolder, "tmp.jpg");
                    //thumb.Save(targetPath, jpegICI, encoderParams);
                    return (Bitmap)thumb.Clone();
                }
            }
    }
}
