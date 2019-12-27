using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deploy2.org.com.Classes
{
    public class ConfigurationFile
    {
        public string component_name { get; set; }
        public string api_version { get; set; }
        public List<string> apex_class { get; set; }
        public List<string> events { get; set; }
        public Bundle_Details bundle_details { get; set; }
        public string fileContent { get; set; }
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

    public class ConfigurationModel
    {
        public string ComponentName { get; set; }
        public string APIVersion { get; set; }
        public List<GithubFile> ApexClass { get; set; }
        public List<GithubFile> Events { get; set; }
        public GithubFile Component { get; set; }
        public GithubFile Controller { get; set; }
        public GithubFile Helper { get; set; }
        public GithubFile Style { get; set; }
        public GithubFile Documentation { get; set; }
        public GithubFile Renderer { get; set; }
        public GithubFile Design { get; set; }
        public GithubFile SVG { get; set; }
    }
}
