using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
/// <summary>
/// Utilitiy Class for Saving & Loading objects as XML
/// </summary>
public static class Utils
{
    public static T DeserializeFromXml<T>(this string text)
    {
        XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
        TextReader reader = new StringReader(text);

        return (T)xsSubmit.Deserialize(reader);
    }
    public static string SerializeAsXml<T>(this T value)
    {
        if (value == null)
        {
            return string.Empty;
        }
        try
        {
            var xmlserializer = new XmlSerializer(typeof(T));
            var stringWriter = new StringWriter();
            using (var writer = XmlWriter.Create(stringWriter))
            {
                xmlserializer.Serialize(writer, value);
                return stringWriter.ToString();
            }
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred", ex);
        }
    }


    public static void SaveRecordset<T>(T data, string Name)
    {

        string path = Application.dataPath + "/Resources/Maps/" + Name + ".xml";

        XmlSerializer writer = new XmlSerializer(typeof(T));

        FileStream file = File.Create(path);

        writer.Serialize(file, data);

        file.Close();

    }
}