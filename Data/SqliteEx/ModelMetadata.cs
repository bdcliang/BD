using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BD.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class ModelMetadata
    {        
        /// <summary>
        /// 解析出 属性上加载的Attribute
        /// </summary>
        public Dictionary<string, string> ExtractPropertyAttr { get; set; }
        /// <summary>
        /// 属性集合
        /// </summary>
        private Type ModelType;
        private object[] ModelAttrs;
        private  List<PropertyInfo> PropertyInfos;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public ModelMetadata(Type type)
        {
            ModelType = type;
            PropertyInfos = new List<PropertyInfo>();
            ExtractPropertyAttr = new Dictionary<string, string>();
            ExtactInfos();           
        }

        private void ExtactInfos()
        {
            PropertyInfos.AddRange(ModelType.GetProperties());
            PropertyInfos.ForEach(info=> {
                ModelAttrs = info.GetCustomAttributes(true);
                foreach (var item in ModelAttrs)
                {                    
                    var tmp = (DataAttribute)item;
                    ExtractPropertyAttr.Add(info.Name.ToLower(),tmp.DataFormat);
                }
            });    
        }

        /// <summary>
        /// 获取属性对应的Attr值
        /// </summary>
        /// <param name="propertyName"></param>
        public string GetPropertyAttrValue(string propertyName)
        {
            return ExtractPropertyAttr.ContainsKey(propertyName.ToLower()) ?ExtractPropertyAttr[propertyName.ToLower()]:null;
        }
    }
}
