using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deploy2.org.Classes
{
    [Serializable]
    public class SalesforceResult
    {
        public System.Net.HttpStatusCode StatusCode { get; set; }
        public string BodyContent { get; set; }
    }
}
