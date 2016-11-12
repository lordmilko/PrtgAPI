using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Helpers
{
    static class AttributeHelpers
    {
        public static string[] GetPSVisibleMembers(this Type type)
        {
            var properties = type.GetProperties();

            var list = new List<string>();

            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes(typeof(Attributes.PSVisibleAttribute), false);

                if (attributes.Length > 0)
                {
                    if (((Attributes.PSVisibleAttribute) attributes.First()).Visible)
                    {
                        list.Add(property.Name);
                    }
                }
                else
                {
                    throw new Exceptions.Internal.MissingAttributeException(type, property.Name);
                }
            }

            return list.ToArray();
        }
    }
}
