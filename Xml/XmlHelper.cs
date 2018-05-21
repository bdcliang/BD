using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BD.Xml
{
    /// <summary>
    /// XPath 语法
    /// 1.表示从根节点开始选择  /pets     选择根节点pets
    ///                       /pets/dog 选择pets节点下的dog节点
    /// 2.从整个xml文档中查找，而不考虑当前节点位置  //price 选择文档中所有的price节点
    /// 3.中括号表示选择条件，括号内为条件   //dog[@color=’white’] 所有color为white的dog节点
    /// 4.*表示任何名字的节点或者属性  //dog/* 表示dog节点的所有子节点
    /// 5.@表示选择属性,选择所有dog节点的color属性集合 //dog/@color
    /// 
    /// </summary>
    public class XmlHelper
    {
        private static XmlHelper _default = new XmlHelper();
        /// <summary>
        /// Xml filename=data.xml
        /// </summary>
        public static XmlHelper Default
        {
            get { return _default; }
        }

        public void LoadXml(string xml)
        {
            doc.LoadXml(xml);
        }


        public XmlDocument XmlDocument{get { return doc; }}
        public void Load(string path)
        {
            if (File.Exists(path))
            { _defaultpath = path; doc.Load(path); }
            else
            {
                CreateDefaultXml();
            }
        }
        public void Load(Stream instream)
        {
            doc.Load(instream);
        }
        private XmlDocument doc = new XmlDocument();
        private string _defaultpath = Environment.CurrentDirectory + @"\config.xml";
        private XmlHelper()
        {            
            Load(_defaultpath);
        }

        /// <summary>
        /// ///创建新的xml文档
        /// </summary>
        /// <param name="path"></param>
        public void CreateDefaultXml(string path="")
        {
            if (path == "") path = Environment.CurrentDirectory +@"\config.xml";
            _defaultpath = path;
            string root = "<?xml version=\"1.0\"?><Data></Data>";
            File.WriteAllText(path,root,Encoding.UTF8);
        }

        /// <summary>
        ///保存xml到本地
        /// </summary>
        /// <param name="path"></param>
        public void SaveXml(string path="")
        {
            if (path == "")
                path = _defaultpath;
            doc.Save(path);
        }
        /// <summary>
        /// 保持xml到stream
        /// </summary>
        /// <param name="outstream"></param>
        public void SaveXml(Stream outstream)
        {
            doc.Save(outstream);
        }

        /// <summary>
        /// 更新一个节点的值
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="value"></param>
        public void UpdateSingleNode(string xpath,string value)
        {
            XmlElement xmlElement = (XmlElement)doc.SelectSingleNode(xpath);
            xmlElement.InnerText = value;
            SaveXml();
        }

        /// <summary>
        /// 设置xpath的属性
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetNodeAttr(string xpath,string key,string value)
        {
            XmlElement xmlElement=(XmlElement)doc.SelectSingleNode(xpath);
            xmlElement.SetAttribute(key, value);
            SaveXml();
        }

        public void SetNodeAttr(string xpath, Dictionary<string,string> attrs)
        {
            XmlElement xmlElement = (XmlElement)doc.SelectSingleNode(xpath);
            foreach (KeyValuePair<string,string> item in attrs)
            {
                xmlElement.SetAttribute(item.Key,item.Value);
            }
            SaveXml();
        }
        /// <summary>
        /// 要删除的节点xpath
        /// </summary>
        /// <param name="xpath"></param>
        public void DeleteNode(string xpath)
        {
            XmlElement xmlElement = (XmlElement)doc.SelectSingleNode(xpath);
            XmlElement parent = (XmlElement)doc.SelectSingleNode(xpath+"/..");
            parent.RemoveChild(xmlElement);
            SaveXml();
        }
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="parentXpath"></param>
        /// <param name="nodeName"></param>
        /// <param name="value"></param>
        public void AddNode(string parentXpath,string nodeName,string value)
        {
            XmlElement parent = (XmlElement)doc.SelectSingleNode(parentXpath);
            XmlElement element= doc.CreateElement(nodeName);
            element.InnerText = value;
            parent.AppendChild(element);
            SaveXml();
        }
        public void AddNode(string parentXpath, string nodeName,Dictionary<string,string> attrs)
        {
            XmlElement parent = (XmlElement)doc.SelectSingleNode(parentXpath);
            XmlElement element = doc.CreateElement(nodeName);
            foreach (KeyValuePair<string,string> item in attrs)
            {
                element.SetAttribute(item.Key, item.Value);
            }            
            parent.AppendChild(element);
            SaveXml();
        }

        /// <summary>
        /// 查询指定节点的值
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public string QueryNodeInnerText(string xpath)
        {
            XmlElement xmlElement = (XmlElement)doc.SelectSingleNode(xpath);
            return xmlElement.InnerText;
        }

        /// <summary>
        /// 获取所有的xpath的节点下的childs值对
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public Dictionary<string,string> QueryAllChildsNameInnterText(string xpath)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            XmlElement xmlElement = (XmlElement)doc.SelectSingleNode(xpath);
            var list = xmlElement.ChildNodes;
            foreach (XmlElement item in list)
            {
                dic.Add(item.Name, item.InnerText);
            }
            return dic;
        }

        /// <summary>
        /// 获取xpath所有属性值
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public Dictionary<string,string> QueryNodeAttrs(string xpath)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            XmlElement xmlElement = (XmlElement)doc.SelectSingleNode(xpath);
            var list= xmlElement.Attributes;
            foreach (XmlAttribute item in list)
            {
                dic.Add(item.Name, item.Value);
            }
            return dic;
        }

        /// <summary>
        /// 查询指定xpath的属性key的值
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string QueryNodeAttr(string xpath,string key)
        {
            var dic = QueryNodeAttrs(xpath);
            if (dic.ContainsKey(key))
                return dic[key];
            return "";
        }

        public List<XmlNode> QueryNodeList(string xpath)
        {
            List<XmlNode> list = new List<XmlNode>();
            foreach (XmlNode item in doc.SelectNodes(xpath))
            {
                list.Add(item);
            }
            return list;
        }
    }
}
