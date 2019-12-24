using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deploy2.org.com.Classes
{
    public class DeployModel
    {
        public string sfToken { get; set; }
        public string sfVersion { get; set; }
        public string sfUrl { get; set; }


        public string ghReferer { get; set; }
        public string ghToken { get; set; }
        public string ghOrg { get; set; }
        public string ghRepo { get; set; }

        public ConfigurationFile configurationFile { get; set; }

        public ConfigurationModel configurationModel { get; set; }

    }
}
