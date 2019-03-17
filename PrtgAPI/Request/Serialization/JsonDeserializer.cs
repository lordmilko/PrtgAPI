using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PrtgAPI.Request.Serialization
{
    [ExcludeFromCodeCoverage]
    internal class JsonDeserializer<T>
    {
        internal static T DeserializeType(PrtgResponse response)
        {
            switch(response.Type)
            {
                case PrtgResponseType.Stream:
                    return DeserializeType(response.GetStreamUnsafe());
                case PrtgResponseType.String:
                    return DeserializeType(response.StringValue);
                default:
                    throw new NotImplementedException($"Don't know how to deserialize JSON from response of type {response.Type}.");
            }
        }

        private static T DeserializeType(string json)
        {
            var deserializer = new DataContractJsonSerializer(typeof(T));

            T data;

            using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                data = (T)deserializer.ReadObject(stream);
            }

            return data;
        }

        private static T DeserializeType(Stream stream)
        {
            var deserializer = new DataContractJsonSerializer(typeof(T));

            T data = (T)deserializer.ReadObject(stream);

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
