using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deploy2.org.Classes
{
    public class AuraDefinitionBundle
    {
        public double ApiVersion { get; set; }
        public string Description { get; set; }
        public string DeveloperName { get; set; }
        public string MasterLabel { get; set; }
        public AuraDefinitionBundle Metadata { get; set; }
    }
}
