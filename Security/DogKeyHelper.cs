namespace BD.Security
{
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

/// <summary>
/// YuTian Encrypt Dog Helper class
/// </summary>
internal sealed class DogKeyHelper
    {
        private static string keyPath = "";
        private static DogKeyPWD ytsoftkey = new DogKeyPWD();
        /// <summary>
        /// 是否存在着加密锁
        /// </summary>
        /// <returns></returns>
        public static bool FindPort()
        {
            //这个用于判断系统中是否存在着加密锁。不需要是指定的加密锁,
            if (ytsoftkey.FindPort(0, ref keyPath) != 0)
            {
                MessageBox.Show("请插入加密狗后，再进行操作。", "未找到加密狗", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 返回加密狗的ID号
        /// </summary>
        /// <returns></returns>
        public static string GetKey()
        {
            if (!FindPort())
            {
                return "";
            }
            /*'用于返回加密狗的ID号，加密狗的ID号由两个长整型组成。
            '提示1：锁ID是每一把都是唯一的，每一把是唯一的，是指每一把锁的ID都不相同
            '提示2: ID唯一是指两个ID转化为16进制字符串后并连接起来后是唯一的*/
            int id_1 = 0, id_2 = 0;
            if (ytsoftkey.GetID(ref id_1, ref id_2, keyPath) != 0)
            {
                MessageBox.Show("返回加密狗ID错误！", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return "";
            }
            return id_1.ToString("X") + id_2.ToString("X");
        }
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns></returns>
        public static string ReadString()
        {
            string sKey = "FFFFFFFF";
            return ReadString(sKey);
        }
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="sKey">读密码</param>
        /// <returns></returns>
        public static string ReadString(string sKey)
        {
            if (!FindPort())
            {
                return "";
            }
            int ret;
            short nlen;
            byte[] buf = new byte[1];
            string outstring = "";
            //先从地址0读到以前写入的字符串的长度
            ret = ytsoftkey.YReadEx(buf, 0, 1, sKey, sKey, keyPath);
            nlen = buf[0];
            if (ret != 0)
            {
                return "";//读取字符串长度错误
            }
            //再读取相应长度的字符串
            ret = ytsoftkey.YReadString(ref outstring, 1, nlen, sKey, sKey, keyPath);
            if (ret != 0)
            {
                return "";
            }
            else
            {
                return outstring;
            }
        }
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="inString">数据</param>
        /// <returns></returns>
        public static bool WriteString(string inString)
        {
            string sKey = "FFFFFFFF";
            return WriteString(inString, sKey);
        }
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="inString">数据</param>
        /// <param name="sKey">写密码</param>
        /// <returns></returns>
        public static bool WriteString(string inString, string sKey)
        {
            if (!FindPort())
            {
                return false;
            }
            int ret;
            int nlen;
            byte[] buf = new byte[1];
            nlen = DogKeyPWD.lstrlenA(inString);
            buf[0] = System.Convert.ToByte(nlen);
            //写入字符串到地址1
            ret = ytsoftkey.YWriteString(inString, 1, sKey, sKey, keyPath);
            if (ret != 0)
            {
                return false;
            }
            //写入字符串的长度到地址0
            ret = ytsoftkey.YWriteEx(buf, 0, 1, sKey, sKey, keyPath);
            if (ret != 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string StrENC(string msg, string key)
        {
            return ytsoftkey.StrEnc(msg, key);
        }
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string StrENC(string msg)
        {
            return StrENC(msg, "#BoruiMultiMedia20170117$$$$#");
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string StrDEC(string msg, string key)
        {
            return ytsoftkey.StrDec(msg, key);
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string StrDEC(string msg)
        {
            return StrDEC(msg, "#BoruiMultiMedia20170117$$$$#");
        }

        public static string StrMD5(string msg)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] palindata = Encoding.Default.GetBytes(msg);//将要加密的字符串转换为字节数组
            byte[] encryptdata = md5.ComputeHash(palindata);//将字符串加密后也转换为字符数组
            return Convert.ToBase64String(encryptdata);//将加密后的字节数组转换为加密字符串
        }
    }
}
