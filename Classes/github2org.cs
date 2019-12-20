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

namespace github2org.com.Classes
{
    public class github2org
    {

        private string _sfToken;
        private string _sfVersion;
        private string _sfUrl;
        private string _ghToken;
        private string _ghOrg;
        private string _ghRepos;

        public github2org(string sfToken, string sfVersion, string sfUrl, string ghToken, string ghOrg, string ghRepos)
        {
            _sfToken = sfToken;
            _sfVersion = sfVersion;
            _sfUrl = sfUrl.Replace("v{version}", _sfVersion);
            _ghToken = ghToken;
            _ghOrg = ghOrg;
            _ghRepos = ghRepos;
        }

        public void UploadComponent()
        {
            var configFile = RetrieveGithubConfig();

            //first: upload the apex class that will be used later in the component
            if (!string.IsNullOrEmpty(configFile.apex_class))
            {
                //retrieve the class
                var apexFile = RetrieveRepositoryFile(_ghOrg, _ghRepos, configFile.apex_class);
                var fileContent = apexFile.fileContent();
                //get the class name from the code
                var className = Regex.Split(fileContent, " class ", RegexOptions.IgnoreCase)[1].Split('{')[0].Trim();
                var newClass = new SalesforceApexClass()
                {   
                    Body = fileContent,
                    ApiVersion = double.Parse(configFile.api_version.Substring(1)), //remove the v
                    Status ="Active",
                    Name = className
                };
                if (!CreateApexClass(newClass))
                {
                    throw new Exception("Cannot create apex class.");
                }
            }
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
            var githubConfigFile = RetrieveRepositoryFile(_ghOrg, _ghRepos, "github2org.json");

            return JsonSerializer.Deserialize<ConfigurationFile>(githubConfigFile.fileContent());
        }


        private GithubFile RetrieveRepositoryFile(string org, string repo, string file)
        {
            return RetrieveRepositoryFile(org + "/" + repo + "/contents/" + file);
        }

        private GithubFile RetrieveRepositoryFile(string url)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Deploy2Org");
            client.DefaultRequestHeaders.Add("Authorization", "token " + _ghToken);

            var response = client.GetAsync("https://api.github.com/repos/" + url).Result;
            var serializer = new DataContractJsonSerializer(typeof(GithubFile));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonSerializer.Deserialize<GithubFile>(response.Content.ReadAsStringAsync().Result);
            }

            return null;
            
        }
    }
}
