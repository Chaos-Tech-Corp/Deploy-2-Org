using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace github2org.com.Classes
{
    public class ConfigurationFile
    {
        public string component_name { get; set; }
        public string api_version { get; set; }
        public string apex_class { get; set; }
        public Bundle_Details bundle_details { get; set; }
    }

    public class Bundle_Details
    {
        public string component { get; set; }
        public string controller { get; set; }
        public string helper { get; set; }
        public string style { get; set; }
        public string documentation { get; set; }
        public string renderer { get; set; }
        public string design { get; set; }
        public string svg { get; set; }

    }
}
