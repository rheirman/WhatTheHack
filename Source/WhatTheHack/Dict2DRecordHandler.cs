using HugsLib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace WhatTheHack
{
    public class Dict2DRecordHandler : SettingHandleConvertible
    {
        public Dictionary<String, Dictionary<String, Record>> inner = new Dictionary<String, Dictionary<String, Record>>();
        public Dictionary<String, Dictionary<String, Record>> InnerList { get { return inner; } set { inner = value; } }

        public override void FromString(string settingValue)
        {
            inner = new Dictionary<String, Dictionary<String, Record>>();
            if (!settingValue.Equals(string.Empty))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(settingValue);
                foreach (XmlNode dictNode in xmlDoc.ChildNodes)
                {
                    Dictionary<String, Record> nestedDict = new Dictionary<string, Record>();
                    String name = dictNode.Name;
                    foreach(XmlNode recordNode in dictNode.ChildNodes)
                    {
                        bool isSelected = Convert.ToBoolean(dictNode.Attributes.GetNamedItem("isSelected").Value);
                        bool isException = Convert.ToBoolean(dictNode.Attributes.GetNamedItem("isException").Value);
                        string label = dictNode.Attributes.GetNamedItem("label").Value;
                        nestedDict.Add(recordNode.Name, new Record(isSelected, isException, label));
                    }
                    inner.Add(name, nestedDict);
                }
            }
        }

        public override string ToString()
        {
            return "";
            /*
            XmlDocument xmlDoc = new XmlDocument();

            foreach (KeyValuePair<string, Dictionary<String, Record>> item in inner)
            {
                
                xmlDoc.AppendChild()
            }

            /*
            List<String> strings = new List<string>();
            foreach (KeyValuePair<string, Record> item in inner)
            {
                strings.Add(item.Key +","+item.Value.ToString());
            }

            return inner != null ? String.Join("|", strings.ToArray()) : "";
            */
        }
    }

}
