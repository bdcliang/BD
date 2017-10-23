using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace BD.Net
{
    public class DownLoadHelper
    {
        /// <summary>
        /// 获取网络图片
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Image GetWebImage(string uri)
        {
            return Image.FromStream(System.Net.WebRequest.Create(uri).GetResponse().GetResponseStream());
        }
    }
}
