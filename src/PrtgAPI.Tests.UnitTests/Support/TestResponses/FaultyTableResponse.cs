using System;
using System.Collections.Generic;
using System.Linq;
using PrtgAPI.Utilities;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class FaultyTableResponse : MultiTypeResponse
    {
        private Dictionary<Content, int> throwThreshold;
        private List<string> faultyFunctions;

        private Dictionary<Content, int> hitCount = new Dictionary<Content, int>();

        public FaultyTableResponse()
        {
        }

        public FaultyTableResponse(Dictionary<Content, int> throwThreshold)
        {
            this.throwThreshold = throwThreshold;
        }

        public FaultyTableResponse(string[] functions)
        {
            faultyFunctions = functions.ToList();
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(XmlFunction.TableData):
                    return GetTableResponse(ref address, function);
                default:
                    if (faultyFunctions != null && faultyFunctions.Contains(function))
                        throw new InvalidOperationException($"Requested function '{function}'");

                    return base.GetResponse(ref address, function);
            }
        }

        private IWebResponse GetTableResponse(ref string address, string function)
        {
            var components = UrlUtilities.CrackUrl(address);

            Content content = components["content"].DescriptionToEnum<Content>();

            IncrementCount(content);

            return base.GetResponse(ref address, function);
        }

        private void IncrementCount(Content content)
        {
            if (throwThreshold == null)
                return;

            if (hitCount.ContainsKey(content))
                hitCount[content]++;
            else
                hitCount.Add(content, 1);

            if (throwThreshold.ContainsKey(content) && hitCount[content] >= throwThreshold[content])
                throw new InvalidOperationException($"Requested content '{content}' too many times");
        }
    }
}
