namespace BD.Math
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    /// <summary>
    /// 获取随机数据
    /// </summary>
    public class Random
    {
        private static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static int GetRandom()
        {
            long tick = DateTime.Now.Ticks;
            System.Random ran = new System.Random(GetRandomSeed());
            return ran.Next();
        }

        /// <summary>
        /// 获取一个带上下限的随机数
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRandom(int min, int max)
        {
            long tick = DateTime.Now.Ticks;
            System.Random ran = new System.Random(GetRandomSeed());
            return ran.Next(min,max);
        }
        /// <summary>
        /// 获取n个的的随机数
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static IList<int> GetGroupRandom(int num)
        {
            IList<int> list = new List<int>();
            for (int i = 0; i < num; i++)
            {
                list.Add(GetRandom());
            }
            return list;
        }
        /// <summary>
        /// 获取nge的带上下限的随机数
        /// </summary>
        /// <param name="num"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static IList<int> GetGroupRandom(int num,int min,int max)
        {
            IList<int> list = new List<int>();
            for (int i = 0; i < num; i++)
            {
                list.Add(GetRandom(min,max));
            }
            return list;
        }
        /// <summary>
        /// 获取num个不重复的随机数
        /// </summary>
        /// <param name="num"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static IList<int> GetUniqueRandom(int num, int min, int max)
        {
            IList<int> list = new List<int>();
            for (int i = 0; ; i++)
            {
                var tmp = GetRandom(min, max);
                if (!list.Contains(tmp))
                list.Add(tmp);
                if (list.Count >= num) return list;
            }
        }
        /// <summary>
        /// 获取num个不重复的随机数
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static IList<int> GetUniqueRandom(int num)
        {
            IList<int> list = new List<int>();
            for (int i = 0; ; i++)
            {
                var tmp = GetRandom();
                if (!list.Contains(tmp))
                    list.Add(tmp);
                if (list.Count >= num) return list;
            }
        }
    }
}
