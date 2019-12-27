using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deploy2.org.Classes
{
    public class SalesforceCompositeRequest
    {
        public Boolean allOrNone { get; set; }
        public List<CompositeSubrequest> compositeRequest { get; set; }
    }

    public class CompositeSubrequest
    {
        public object body { get; set; }
        public string method { get; set; }
        public string referenceId { get; set; }
        public string url { get; set; }
    }
}
