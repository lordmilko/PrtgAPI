using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    /*
     
    Generating a response item
    1. replace \r\n\r\n with \r\n
    2. replace " with \"
    3. replace (<)(.+?)(>)(.+)(</(?:.(?!/))+$) with

    string $2 = "$4",

    4. replace (<)(.+)(/>) with string $2 = null,

    5. do it again, with the replacement text new XElement("$2", item.$2),
    6. do 5 again with 4's search value
     

    7. replace 3. with $2 = $2;
    8. replace 4 with $2 = $2
    */

    [TestClass]
    public abstract class ObjectTests<TObject, TItem, TResponse> : BaseObjectTests<TObject, TItem, TResponse> where TResponse : IWebResponse
    {
        protected void Object_CanDeserialize()
        {
            var obj = GetSingleItem();

            Assert.IsTrue(obj != null, "The result of a deserialization attempt was null");

            //todo - loop over our items xml, ensuring that the corresponding sensor property with an xmlelementattribute isnt null
            //this is slightly different from the check we did in sensor_allfields_havevalues?
        }

        protected void Object_CanDeserialize_Multiple()
        {
            var objs = GetMultipleItems();

            Assert.IsTrue(objs.Count == GetItems().Length, $"The deserialization result contained {objs.Count} elements, however {GetItems().Length} were requested.");
        }

        protected void Object_AllFields_HaveValues(Func<PropertyInfo, bool> customHandler = null)
        {
            var obj = GetSingleItem();

            Assert2.AllPropertiesAreNotDefault(obj, customHandler);
        }

        protected void Object_AllFields_HaveValues_Multiple()
        {
            var objs = GetMultipleItems();

            foreach (var obj in objs)
            {
                Assert2.AllPropertiesAreNotDefault(obj);
            }
        }
    }
}
