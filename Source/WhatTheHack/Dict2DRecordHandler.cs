using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using HugsLib.Settings;

namespace WhatTheHack;

public class Dict2DRecordHandler : SettingHandleConvertible
{
    private readonly XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
    private readonly XmlSerializer serializer = new XmlSerializer(typeof(Record));
    public Dictionary<string, Dictionary<string, Record>> inner = new Dictionary<string, Dictionary<string, Record>>();

    public Dict2DRecordHandler()
    {
        ns.Add("", "");
    }

    public Dictionary<string, Dictionary<string, Record>> InnerList
    {
        get => inner;
        set => inner = value;
    }

    public override void FromString(string settingValue)
    {
        inner = new Dictionary<string, Dictionary<string, Record>>();
        if (settingValue.Equals(string.Empty))
        {
            return;
        }

        var xmlDoc = new XmlDocument
        {
            InnerXml = settingValue
        };

        foreach (XmlNode dictNode in xmlDoc.FirstChild.ChildNodes)
        {
            var nestedDict = new Dictionary<string, Record>();
            var name = dictNode.Name;
            foreach (XmlNode recordNode in dictNode.ChildNodes)
            {
                var rdr = new StringReader(recordNode.InnerXml);
                nestedDict.Add(recordNode.Name, (Record)serializer.Deserialize(rdr));
            }

            inner.Add(name, nestedDict);
        }
    }

    public override string ToString()
    {
        var xmlDoc = new XmlDocument();
        var root = xmlDoc.AppendChild(xmlDoc.CreateElement("Dict2DRecordHandler"));

        foreach (var kv in inner)
        {
            var child = xmlDoc.CreateElement(kv.Key);
            foreach (var kvInner in kv.Value)
            {
                var childInner = xmlDoc.CreateElement(kvInner.Key);
                childInner.InnerXml = SerializeToString(kvInner.Value);
                child.AppendChild(childInner);
            }

            root.AppendChild(child);
        }

        return xmlDoc.OuterXml;
    }

    private string SerializeToString<T>(T value)
    {
        var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
        var xmlSerializer = new XmlSerializer(value.GetType());
        var settings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = true
        };

        using var stream = new StringWriter();
        using var writer = XmlWriter.Create(stream, settings);
        xmlSerializer.Serialize(writer, value, emptyNamespaces);
        return stream.ToString();
    }
}