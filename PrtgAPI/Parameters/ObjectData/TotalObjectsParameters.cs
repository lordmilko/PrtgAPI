using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class TotalObjectsParameters : Parameters
    {
        public TotalObjectsParameters(Content content)
        {
            Content = content;
            this[Parameter.Count] = 0;

            if(content == Content.ProbeNode)
                this[Parameter.FilterXyz] = new SearchFilter(Property.ParentId, "0");
        }

        public Content Content
        {
            get { return (Content)this[Parameter.Content]; }
            set { this[Parameter.Content] = value; }
        }
    }
}
