using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BD.Security
{
    public class SHA1Helper
    {
        /// <summary>
        /// 使用 SHA1 加密算法来加密
        /// </summary>
        /// <param name="sourceString">原字符串</param>
        /// <returns>加密后字符串</returns>
        public static string SHA1_Encrypt(string sourceString)
        {
            byte[] StrRes = Encoding.UTF8.GetBytes(sourceString);
            HashAlgorithm iSHA = new SHA1CryptoServiceProvider();
            StrRes = iSHA.ComputeHash(StrRes);
            StringBuilder EnText = new StringBuilder();
            foreach (byte iByte in StrRes)
            {
                EnText.AppendFormat("{0:x2}", iByte);
            }
            return EnText.ToString();
        }
    }
}
