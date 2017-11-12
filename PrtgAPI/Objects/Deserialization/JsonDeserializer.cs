using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PrtgAPI.Objects.Deserialization
{
    [ExcludeFromCodeCoverage]
    internal class JsonDeserializer<T>
    {
        public static T DeserializeType(string json)
        {
            var deserializer = new DataContractJsonSerializer(typeof(T));

            T data = default(T);

            using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                data = (T)deserializer.ReadObject(stream);
            }

            return data;
        }

        public static List<T> DeserializeList(List<string> json)
        {
            return DeserializeList(json, s => s);
        }

        public static List<T> DeserializeList<TList>(List<TList> list, Func<TList, string> retriever, Action<TList, T> postDeserializationAction = null)
        {
            var deserializer = new DataContractJsonSerializer(typeof(T));

            var data = list.Select(j =>
            {
                using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(retriever(j))))
                {
                    var obj = (T)deserializer.ReadObject(stream);
                    postDeserializationAction?.Invoke(j, obj);

                    return obj;
                }
            }).ToList();

            return data;
        }
    }
}
