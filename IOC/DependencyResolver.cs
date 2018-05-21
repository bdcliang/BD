using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BD.IOC
{
    public class DependencyResolver
    {

        private static Dictionary<Type, Type> typeDic = new Dictionary<Type, Type>();
        private static Dictionary<string, Type> keyDic = new Dictionary<string, Type>();
        private static Dictionary<Type, object> typeMemDic = new Dictionary<Type, object>();
        private static Dictionary<string, object> keyMemDic = new Dictionary<string, object>();
        /// <summary>
        /// 注册依赖关系
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TType"></typeparam>
        public static void Register<TInterface,TType>() where TType:TInterface
        {
            typeDic.Add(typeof(TInterface),typeof(TType));
        }
        /// <summary>
        /// Key 注册依赖关系
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TType"></typeparam>
        /// <param name="key"></param>
        public static void Register<TInterface, TType>(string key) where TType : TInterface
        {
            keyDic.Add(key,typeof(TType));
        }

        /// <summary>
        /// 默认构造函数获取依赖
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public static TInterface Get<TInterface>()
        {
            object obj;
            if(typeMemDic.TryGetValue(typeof(TInterface),out obj))
            {
                return (TInterface)obj;
            }

            Type type;
            if(typeDic.TryGetValue(typeof(TInterface),out type))
            {
                var tar= (TInterface)Activator.CreateInstance(type);
                typeMemDic.Add(typeof(TInterface),tar);
                return tar;
            }
            return default(TInterface);
        }

        /// <summary>
        /// 带构造参数获取
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TInterface Get<TInterface>(params object[] args)
        {
            Type type;
            if (typeDic.TryGetValue(typeof(TInterface), out type))
            {
                var tar= (TInterface)Activator.CreateInstance(type,args);
                if (typeMemDic.ContainsKey(typeof(TInterface)))
                    typeMemDic.Remove(typeof(TInterface));
                typeMemDic.Add(typeof(TInterface), tar);
                return tar;
            }
            return default(TInterface);
        }
        /// <summary>
        /// 默认构造函数获取依赖
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TInterface Get<TInterface>(string key)
        {
            object obj;
            if (keyMemDic.TryGetValue(key, out obj))
            {
                return (TInterface)obj;
            }
            Type type;
            if (keyDic.TryGetValue(key, out type))
            {
                return (TInterface)Activator.CreateInstance(type);
            }
            return default(TInterface);
        }
        /// <summary>
        /// 带构造参数获取
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="key"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TInterface Get<TInterface>(string key, params object[] args)
        {
            Type type;
            if (keyDic.TryGetValue(key, out type))
            {
                if (keyMemDic.ContainsKey(key))
                    keyMemDic.Remove(key);
                var tar= (TInterface)Activator.CreateInstance(type,args);
                keyMemDic.Add(key,tar);
            }
            return default(TInterface);
        }

    }
}
