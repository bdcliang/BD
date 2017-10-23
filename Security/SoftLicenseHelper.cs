using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace BD.Security
{
    /// <summary>
    /// 使用软加密方式进行LIC文件加密
    ///  SoftLisence enc = new SoftLisence();
    /// enc.CheckValid("changzho"); 验证Key
    /// Console.WriteLine("Last Write" + enc.LLastDate.ToString("yyyy-MM-dd"));
    /// Console.WriteLine("Set Date" + enc.LSetDate.ToString("yyyy-MM-dd"));
    /// enc.ProduceLisence("2016-08-12","changzho"); 写日期和Key
    /// </summary>
    public class SoftLicenseHelper
    {
        private static string KEY_64 = "BRApp888";//注意了，是8个字符，64位
        private static string IV_64 = "BRApp888";
        /// <summary>
        /// 设置密钥
        /// </summary>
        /// <param name="key"></param>
        public static  void SettingKey(string key)
        {
            if(key.Length!=8)
             {
               MessageBox.Show("密钥长度必须为8个字符");
                return;
              }
            KEY_64 = key;
            IV_64 = key;
        }
        private static string str = "苏州博睿多媒体技术有限公司，是一家新兴的技术型企业，致力于展厅展陈技术的业务发展。专注于展厅多媒体智能方向，以整体策划及硬件解决方案为相关服务。几年来，业务遍布江浙沪等区域，得到了客户高度的赞誉和支持。    博睿公司针对城市规划馆，科技体验馆，企业展厅等领域不断进取，开发大量的多媒体互动程序及硬件解决方案，充分体现出博睿在本行业的领先地位。   公司欢迎有志于多媒体展厅行业的人才加入，共创辉煌";

        private static DateTime _LSetDate;
        /// <summary>
        /// 设置到期的时间
        /// </summary>
        public static DateTime LSetDate {
            get {
                DebugCheckValid(KEY_64);
                return _LSetDate;
            }
            set
            {
                _LSetDate = value;
            }
        }

        private static int _LeftDays;
        /// <summary>
        /// 
        /// </summary>
        public static  int LeftDays
        {
            get
            {
                DebugCheckValid(KEY_64);
                return _LeftDays;
            }
            set
            {
                _LeftDays = value;
            }
        }

        private static DateTime _LLastDate;
        /// <summary>
        /// 获取上次写入的时间
        /// </summary>
        public static DateTime LLastDate
        {
            get
            {
                DebugCheckValid(KEY_64);
                return _LLastDate;
            }
            set
            {
                _LLastDate = value;
            }
        }

        /// <summary>
        /// Debug 模式下使用
        /// </summary>
        /// <param name="validKey"></param>
        public static void DebugCheckValid(string validKey = "BRApp888")
        {
            CheckValid(validKey);
        }
        /// <summary>
        /// 检查是否到了设定的期限
        /// </summary>
        /// <param name="validKey">默认为"BIGDSTUD"</param>
        public static void CheckValid(string validKey= "BRApp888")
        {
        SettingKey(validKey);
        string f = string.Format("{0}\\License.LIC", Application.StartupPath);
        if (!System.IO.File.Exists(f))
        {
            MessageBox.Show("配置文件丢失，请联系苏州博睿多媒体技术有限公司！");
            Process.GetCurrentProcess().Kill();
        }
        string app = System.IO.File.ReadAllText(f);
        app = Decode(app);
        app = Decode(app);
        try {
            string[] args = app.Split(':');
            if (args.Length != 4) {
                MessageBox.Show("配置文件错误，请联系苏州博睿多媒体技术有限公司！");
                Process.GetCurrentProcess().Kill();
            }
            string SetDate = args[2];
            string LastTime = args[1];
            DateTime Last = DateTime.Parse(LastTime);
            _LLastDate = Last;
            _LSetDate = DateTime.Parse(SetDate);
            _LeftDays = LSetDate.Subtract(DateTime.Now).Days;
            if (DateTime.Compare(Last, DateTime.Now) >= 0) { MessageBox.Show("请不要随意修改电脑时间，否则软件使用出现问题，概不负责"); Process.GetCurrentProcess().Kill(); }
            if (DateTime.Compare(DateTime.Now,DateTime.Parse(SetDate))>=0) { MessageBox.Show("软件的体验使用期已到，请购买正式稳定版"); Process.GetCurrentProcess().Kill(); }
            ProduceLisence(SetDate,KEY_64);
            } catch {
            MessageBox.Show("配置文件错误，请联系苏州博睿多媒体技术有限公司！");
            Process.GetCurrentProcess().Kill();
        }
        }

        /// <summary>
        /// 更新信息写入到注册文件
        /// </summary>
        /// <param name="SetDate">日期格式2016-08-12</param>
        /// <param name="validKey"></param>
        public static void ProduceLisence(string SetDate,string validKey= "BRApp888")
        {
            ProduceLisence(SetDate, Application.StartupPath,validKey);
        }
        /// <summary>
        /// 加密格式： 混淆文字+当前时间+设定时间+随机码
        /// </summary>
        /// <param name="SetDate">日期格式2016-08-12</param>
        /// <param name="LisencePath"></param>
        /// <param name="validKey"></param>
        public static void ProduceLisence(string SetDate, string LisencePath,string validKey)
        {
        SettingKey(validKey);
        string f = string.Format("{0}\\License.LIC", LisencePath);
        Random r = new Random();
        string msg =r.Next().ToString()+str + ":" + DateTime.Now.ToString("yyyy-MM-dd") + ":" + SetDate + ":" + r.Next().ToString();
        string EncMsg = Encode(msg);
        string SEncMsg = Encode(EncMsg);
        File.WriteAllText(f, SEncMsg);
        }

        private static  string Encode(string data)
        {
            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            int i = cryptoProvider.KeySize;
            MemoryStream ms = new MemoryStream();
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);

            StreamWriter sw = new StreamWriter(cst);
            sw.Write(data);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();
            return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);

        }
        private static  string Decode(string data)
        {
            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);

            byte[] byEnc;
            try
            {
                byEnc = Convert.FromBase64String(data);
            }
            catch
            {
                return null;
            }

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream(byEnc);
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cst);
            return sr.ReadToEnd();
        }
    }
}

