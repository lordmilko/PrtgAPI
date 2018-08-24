using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    /*
     
    Generating a response item
    1. replace \r\n\r\n with \r\n
    2. replace " with \"
    3. replace (<)(.+?)(>)(.+)(</(?:.(?!/))+$) with

    string $2 = "$4",

    4. replace (<)(.+)(/>) with string $2 = null,

    now create a new constructor for your class, copy and paste all the items in as the arguments

    5. do it again, with the replacement text
        new XElement("$2", item.$2),
    6. do 5 again with 4's search value
     
    now create a new response class, the xml is var xml = new XElement("item",
        <your xelements>
    );

    7. replace 3. with $2 = $2;
    8. replace 4 with $2 = $2;

        this goes in the body of your response items constructor

    9. replace 3 and 4 with public string $2 { get; set; }
    */

    [TestClass]
    public abstract class StandardObjectTests<TObject, TItem, TResponse> : BaseObjectTests<TObject, TItem, TResponse> where TResponse : IWebResponse
    {
        protected void Object_CanDeserialize()
        {
            var obj = GetSingleItem();

            Assert.IsTrue(obj != null, "The result of a deserialization attempt was null");

            //todo - loop over our items xml, ensuring that the corresponding sensor property with an xmlelementattribute isnt null
            //this is slightly different from the check we did in sensor_allfields_havevalues?
        }

        protected async Task Object_CanDeserializeAsync()
        {
            var obj = await GetSingleItemAsync();

            Assert.IsTrue(obj != null, "The result of a deserialization attempt was null");
        }

        protected void Object_CanDeserialize_Multiple()
        {
            var objs = GetMultipleItems();

            Assert.AreEqual(GetItems().Length, objs.Count, "Expected number of results");
        }

        protected async Task Object_CanDeserializeAsync_Multiple()
        {
            var objs = await GetMultipleItemsAsync();

            Assert.AreEqual(GetItems().Length, objs.Count, "Expected number of results");
        }

        protected void Object_AllFields_HaveValues(Func<PropertyInfo, bool> customHandler = null)
        {
            var obj = GetSingleItem();

            AssertEx.AllPropertiesAreNotDefault(obj, customHandler);
        }

        protected void Object_AllFields_HaveValues_Multiple()
        {
            var objs = GetMultipleItems();

            foreach (var obj in objs)
            {
                AssertEx.AllPropertiesAreNotDefault(obj);
            }
        }
    }
}
