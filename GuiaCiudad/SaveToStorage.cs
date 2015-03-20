using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GuiaCiudad
{
    public class SaveToStorage
    {
        public static void WriteToXml<T>(T data, string path)
        {
            // Write to the Isolated Storage
            var xmlWriterSettings = new XmlWriterSettings { Indent = true };
 
            using (var myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream = myIsolatedStorage.OpenFile(path, FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    using (var xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
                    {
                        serializer.Serialize(xmlWriter, data);
                    }
                }
            }
        }

        public static T ReadFromXml<T>(string path)
        {
            T data = default(T);
            try
            {
                using (var myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (myIsolatedStorage.FileExists(path))
                    {
                        using (var stream = myIsolatedStorage.OpenFile(path, FileMode.Open))
                        {
                            var serializer = new XmlSerializer(typeof(T));
                            data = (T)serializer.Deserialize(stream);
                        }
                    }
                }
            }
            catch
            {    //add some code here
            }
            return data;
        }
    }
}
