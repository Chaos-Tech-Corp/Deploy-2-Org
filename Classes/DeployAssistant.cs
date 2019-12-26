using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace deploy2.org.com.Classes
{
    public class DeployAssistant
    {

        private string _sfToken;
        private string _sfVersion;
        private string _sfUrl;
        private string _ghToken;
        private string _ghOrg;
        private string _ghRepo;

        public DeployAssistant(DeployModel model)
        {
            _sfToken = model.sfToken;
            _sfVersion = model.sfVersion;
            if (!string.IsNullOrEmpty(model.sfUrl) && !string.IsNullOrEmpty(_sfVersion))
            {
                _sfUrl = model.sfUrl.Replace("v{version}", _sfVersion);
            }
            _ghToken = model.ghToken;
            _ghOrg = model.ghOrg;
            _ghRepo = model.ghRepo;
        }

        public DeployAssistant(string sfToken, string sfVersion, string sfUrl, string ghToken, string ghOrg, string ghRepos)
        {
            _sfToken = sfToken;
            _sfVersion = sfVersion;
            _sfUrl = sfUrl.Replace("v{version}", _sfVersion);
            _ghToken = ghToken;
            _ghOrg = ghOrg;
            _ghRepo = ghRepos;
        }

        public void UploadComponent()
        {
            var configFile = RetrieveGithubConfig();

            //first: upload the apex class that will be used later in the component
            //if (!string.IsNullOrEmpty(configFile.apex_class))
            //{
            //    //retrieve the class
            //    var apexFile = RetrieveRepositoryFile(_ghOrg, _ghRepo, configFile.apex_class);
            //    var fileContent = apexFile.fileContent();
            //    //get the class name from the code
            //    var className = Regex.Split(fileContent, " class ", RegexOptions.IgnoreCase)[1].Split('{')[0].Trim();
            //    var newClass = new SalesforceApexClass()
            //    {   
            //        Body = fileContent,
            //        ApiVersion = double.Parse(configFile.api_version.Substring(1)), //remove the v
            //        Status ="Active",
            //        Name = className
            //    };
            //    if (!CreateApexClass(newClass))
            //    {
            //        throw new Exception("Cannot create apex class.");
            //    }
            //}
        }

        public bool CreateApexClass(SalesforceApexClass apexClass)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "Deploy2Org");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _sfToken);

            var content = new StringContent(JsonSerializer.Serialize(apexClass), Encoding.UTF8, "application/json");

            var response = client.PostAsync(_sfUrl + "sobjects/ApexClass", content).Result;
            return response.StatusCode == System.Net.HttpStatusCode.Created;

        }

        public ConfigurationFile RetrieveGithubConfig()
        {
            var githubConfigFile = RetrieveRepositoryFile(_ghOrg, _ghRepo, ".deploy2org.json");
            if (githubConfigFile.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new HttpRequestException(githubConfigFile.StatusCode.ToString());
            } else { 
                var cFile = JsonSerializer.Deserialize<ConfigurationFile>(githubConfigFile.fileContent());
                cFile.fileContent = githubConfigFile.fileContent();
                return cFile;
            }
        }

        public ConfigurationModel Validate(ConfigurationFile ConfigFile)
        {
            var model = new ConfigurationModel();

            model.ComponentName = ConfigFile.component_name;
            model.APIVersion = ConfigFile.api_version;

            if (ConfigFile.apex_class != null)
            {
                model.ApexClass = new List<GithubFile>();
                foreach(var className in ConfigFile.apex_class)
                {
                    model.ApexClass.Add(RetrieveRepositoryFile(_ghOrg, _ghRepo, className));
                }
                
            }
            if (ConfigFile.bundle_details != null)
            {
                if (!string.IsNullOrEmpty(ConfigFile.bundle_details.component))
                {
                    model.Component = RetrieveRepositoryFile(_ghOrg, _ghRepo, ConfigFile.bundle_details.component);
                }
                if (!string.IsNullOrEmpty(ConfigFile.bundle_details.controller))
                {
                    model.Controller = RetrieveRepositoryFile(_ghOrg, _ghRepo, ConfigFile.bundle_details.controller);
                }
                if (!string.IsNullOrEmpty(ConfigFile.bundle_details.design))
                {
                    model.Design = RetrieveRepositoryFile(_ghOrg, _ghRepo, ConfigFile.bundle_details.design);
                }
                if (!string.IsNullOrEmpty(ConfigFile.bundle_details.documentation))
                {
                    model.Documentation = RetrieveRepositoryFile(_ghOrg, _ghRepo, ConfigFile.bundle_details.documentation);
                }
                if (!string.IsNullOrEmpty(ConfigFile.bundle_details.helper))
                {
                    model.Helper = RetrieveRepositoryFile(_ghOrg, _ghRepo, ConfigFile.bundle_details.helper);
                }
                if (!string.IsNullOrEmpty(ConfigFile.bundle_details.renderer))
                {
                    model.Renderer = RetrieveRepositoryFile(_ghOrg, _ghRepo, ConfigFile.bundle_details.renderer);
                }
                if (!string.IsNullOrEmpty(ConfigFile.bundle_details.style))
                {
                    model.Style = RetrieveRepositoryFile(_ghOrg, _ghRepo, ConfigFile.bundle_details.style);
                }
                if (!string.IsNullOrEmpty(ConfigFile.bundle_details.svg))
                {
                    model.SVG = RetrieveRepositoryFile(_ghOrg, _ghRepo, ConfigFile.bundle_details.svg);
                }
            }

            return model;
        }

        private GithubFile RetrieveRepositoryFile(string org, string repo, string file)
        {
            return RetrieveRepositoryFile(org + "/" + repo + "/contents/" + file);
        }

        private GithubFile RetrieveRepositoryFile(string url)
        {

            GithubFile ghFile = new GithubFile();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Deploy2Org");
            if (!string.IsNullOrEmpty(_ghToken))
            {
                client.DefaultRequestHeaders.Add("Authorization", "token " + _ghToken);
            }

            var response = client.GetAsync("https://api.github.com/repos/" + url).Result;
            var serializer = new DataContractJsonSerializer(typeof(GithubFile));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ghFile = JsonSerializer.Deserialize<GithubFile>(response.Content.ReadAsStringAsync().Result);
            }
            ghFile.StatusCode = response.StatusCode;
            ghFile.ResponseBody = response.Content.ReadAsStringAsync().Result;

            return ghFile;
            
        }
    }
}
