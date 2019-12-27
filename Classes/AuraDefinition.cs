using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deploy2.org.Classes
{
    public class AuraDefinition
    {
        public string DefType { get; set; }
        public string Format { get; set; }//css,js,xml
        public string AuraDefinitionBundleId { get; set; }
        public string Source { get; set; }
    }
}
