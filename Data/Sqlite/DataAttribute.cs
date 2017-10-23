using System;
using System.Collections.Generic;
using System.Text;

namespace BD.Data
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property,AllowMultiple =false)]
    public class DataAttribute:Attribute
    {
        /// <summary>
        /// 数据段的约束格式
        /// </summary>
        public string DataFormat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public DataAttribute(ColConstrains type)
        {
            this.DataFormat = type.ToString().ToLower();
        }
    }


}
