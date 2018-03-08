using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace GH.Core.Helpers
{
    public static class ConfigHelper
    {
        public static T GetConfig<T>()
        {
            var type = typeof(T);
            var configPathAttribute = typeof(ConfigPathAttribute);
            if (Attribute.IsDefined(type, configPathAttribute))
            {
                var configPath = ((ConfigPathAttribute)Attribute.GetCustomAttribute(type, configPathAttribute)).Path;

                configPath = CommonFunctions.MapPath(configPath);
                if (!File.Exists(configPath))
                {
                    return default(T);
                }
                else
                {
                    var serializer = new XmlSerializer(type);
                    var reader = XmlReader.Create(configPath);
                    try
                    {
                        var configs = (T)serializer.Deserialize(reader);
                        return configs;
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            else
            {
                return default(T);
            }
        }

        public static void UpdateConfig<T>(T config)
        {
            var type = typeof(T);
            var configPathAttribute = typeof(ConfigPathAttribute);
            if (Attribute.IsDefined(type, configPathAttribute))
            {
                var configPath = ((ConfigPathAttribute)Attribute.GetCustomAttribute(type, configPathAttribute)).Path;

                configPath = CommonFunctions.MapPath(configPath);

                var directoryPath = Path.GetDirectoryName(configPath);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var serializer = new XmlSerializer(type);
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                var writer = XmlWriter.Create(configPath, settings);
                try
                {
                    serializer.Serialize(writer, config);
                }
                finally
                {
                    writer.Close();
                }
            }
        }
    }

    public class ConfigPathAttribute : Attribute
    {
        public ConfigPathAttribute(string path)
        {
            Path = path;
        }
        public string Path { get; set; }
    }
}