using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BD.Resources
{
    /// <summary>
    /// 把图片放进工程，把图片设置为嵌入资源
    /// </summary>
    public class ResourceManager
    {
        /// <summary>
        /// 根据图片的全名获取图片，默认是执行程序的内部图片
        /// </summary>
        /// <param name="fullname"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static Image GetEmbedimage(string fullname,Assembly assembly=null)
        {
            if(assembly==null)
              assembly= Assembly.GetEntryAssembly();
            var stream = assembly.GetManifestResourceStream(fullname);
            try {
                var bit = Bitmap.FromStream(stream);
                return bit;
            }
            catch {
            }
            finally
            {
                stream.Dispose();
            }
            return null;           
        }

        /// <summary>
        /// 根据图片的全名从指定的资源集合中获取图片
        /// </summary>
        /// <param name="fullname"></param>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        public static Image GetEmbedimage(string fullname,string assemblyPath)
        {
            Image bit = null;
            if(File.Exists(assemblyPath))
            {
                var assembly= Assembly.LoadFile(assemblyPath);
                return GetEmbedimage(fullname,assembly);
            }
            return bit;
        }

        /// <summary>
        /// 获取所有设置为嵌入资源的图片全名
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static List<string> GetAssemblyImagePaths(Assembly assembly=null)
        {
            if(assembly==null)
                assembly= Assembly.GetEntryAssembly();
            var files = assembly.GetManifestResourceNames();           
            string filters = ".jpg.bmp.gif.png.jpeg.ico";
            List<string> list = new List<string>();
            foreach (var key in files)
            {
                string extension = Path.GetExtension(key);
                if (filters.Contains(extension))
                    list.Add(key);
            }
            return list;
        }
        /// <summary>
        /// 根据name模糊查找资源集中的图片
        /// </summary>
        /// <param name="name"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static Image GetAssemblyImage(string name,Assembly assembly=null)
        {
            var list = GetAssemblyImagePaths(assembly);
            string imgpath= list.Find(path => path.Contains(name));
            if (imgpath == null)
                return null;
            return GetEmbedimage(imgpath,assembly);

        }
        /// <summary>
        /// 根据name模糊查找资源集中的图片
        /// </summary>
        /// <param name="name"></param>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        public static Image GetAssemblyImage(string name, string assemblyPath)
        {
            Assembly assembly = null;
            if (File.Exists(assemblyPath))
                assembly = Assembly.LoadFile(assemblyPath);
            else
                return null;
            var list = GetAssemblyImagePaths(assembly);
            string imgpath = list.Find(path => path.Contains(name));
            if (imgpath == null)
                return null;
            return GetEmbedimage(imgpath, assembly);

        }
        /// <summary>
        /// 获取资源集中的所有图片
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static List<Image> GetAssemblyImages(Assembly assembly=null)
        {
            List<Image> imglist = new List<Image>();
            var list = GetAssemblyImagePaths(assembly);
            foreach (var imgpath in list)
            {
                imglist.Add(GetEmbedimage(imgpath, assembly));
            }
            return imglist;
        }
        /// <summary>
        /// 获取资源集中的所有图片
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        public static List<Image> GetAssemblyImages(string assemblyPath)
        {
            List<Image> imglist = new List<Image>();
            Assembly assembly = null;
            if (File.Exists(assemblyPath))
                assembly = Assembly.LoadFile(assemblyPath);
            else
                return imglist;            
            var list = GetAssemblyImagePaths(assembly);
            foreach (var imgpath in list)
            {
                imglist.Add(GetEmbedimage(imgpath, assembly));
            }
            return imglist;
        }
    }
}
