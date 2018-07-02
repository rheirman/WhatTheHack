using HugsLib.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Verse;

namespace WhatTheHack
{
    public class Dict2DRecordHandler : SettingHandleConvertible
    {
        public Dictionary<String, Dictionary<String, Record>> inner = new Dictionary<String, Dictionary<String, Record>>();
        public Dictionary<String, Dictionary<String, Record>> InnerList { get { return inner; } set { inner = value; } }
        private XmlSerializer serializer = new XmlSerializer(typeof(Record));
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

        public Dict2DRecordHandler()
        {
            ns.Add("", "");
        }
        public override void FromString(string settingValue)
        {
            inner = new Dictionary<String, Dictionary<String, Record>>();
            if (!settingValue.Equals(string.Empty))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.InnerXml = settingValue;

                foreach (XmlNode dictNode in xmlDoc.FirstChild.ChildNodes)
                {
                    Dictionary<String, Record> nestedDict = new Dictionary<string, Record>();
                    String name = dictNode.Name;
                    foreach(XmlNode recordNode in dictNode.ChildNodes)
                    {
                        StringReader rdr = new StringReader(recordNode.InnerXml);
                        nestedDict.Add(recordNode.Name, (Record)serializer.Deserialize(rdr));
                    }
                    inner.Add(name, nestedDict);
                }
            }
        }

        public override string ToString()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode root = xmlDoc.AppendChild(xmlDoc.CreateElement("Dict2DRecordHandler"));

            foreach (KeyValuePair< String, Dictionary < String, Record >> kv in inner){
                XmlElement child = xmlDoc.CreateElement(kv.Key);
                foreach (KeyValuePair<String, Record> kvInner in kv.Value)
                {
                    XmlElement childInner = xmlDoc.CreateElement(kvInner.Key);
                    StringWriter writer = new StringWriter();
                    serializer.Serialize(writer, kvInner.Value, ns);
                    childInner.InnerXml = writer.ToString();
                    child.AppendChild(childInner);
                }
                root.AppendChild(child);
            }
            Log.Message(xmlDoc.OuterXml);
            return xmlDoc.OuterXml;
        }
    }

}
