using deploy2.org.Classes;
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

        public SalesforceResult UploadComponent(ConfigurationModel model)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "Deploy2Org");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _sfToken);

            var compositeRequest = new SalesforceCompositeRequest()
            {
                allOrNone = true,
                compositeRequest = new List<CompositeSubrequest>()
            };


            var bundleBody = new AuraDefinitionBundle()
            {
                ApiVersion = Double.Parse(model.APIVersion.Replace("v", "")),
                Description = model.ComponentName,
                DeveloperName = model.ComponentName,
                MasterLabel = model.ComponentName
            };
            compositeRequest.compositeRequest.Add(new CompositeSubrequest()
            {
                method = "POST",
                body = bundleBody,
                url = "/services/data/" + _sfVersion + "/tooling/sobjects/AuraDefinitionBundle/",
                referenceId = "aura_bundle"
            });

            //component
            compositeRequest.compositeRequest.Add(new CompositeSubrequest()
            {
                method = "POST",
                body = new AuraDefinition()
                {
                    AuraDefinitionBundleId = "@{aura_bundle.id}",
                    DefType = "COMPONENT",
                    Format = "XML",
                    Source = model.Component.fileContent()
                },
                url = "/services/data/" + _sfVersion + "/tooling/sobjects/AuraDefinition/",
                referenceId = "aura_component"
            });
            //controller
            if (model.Controller != null)
            {
                compositeRequest.compositeRequest.Add(new CompositeSubrequest()
                {
                    method = "POST",
                    body = new AuraDefinition()
                    {
                        AuraDefinitionBundleId = "@{aura_bundle.id}",
                        DefType = "CONTROLLER",
                        Format = "JS",
                        Source = model.Controller.fileContent()
                    },
                    url = "/services/data/" + _sfVersion + "/tooling/sobjects/AuraDefinition/",
                    referenceId = "aura_controller"
                });
            }
            //helper
            if (model.Helper != null)
            {
                compositeRequest.compositeRequest.Add(new CompositeSubrequest()
                {
                    method = "POST",
                    body = new AuraDefinition()
                    {
                        AuraDefinitionBundleId = "@{aura_bundle.id}",
                        DefType = "HELPER",
                        Format = "JS",
                        Source = model.Helper.fileContent()
                    },
                    url = "/services/data/" + _sfVersion + "/tooling/sobjects/AuraDefinition/",
                    referenceId = "aura_helper"
                });
            }
            //style
            if (model.Style != null)
            {
                compositeRequest.compositeRequest.Add(new CompositeSubrequest()
                {
                    method = "POST",
                    body = new AuraDefinition()
                    {
                        AuraDefinitionBundleId = "@{aura_bundle.id}",
                        DefType = "STYLE",
                        Format = "CSS",
                        Source = model.Style.fileContent()
                    },
                    url = "/services/data/" + _sfVersion + "/tooling/sobjects/AuraDefinition/",
                    referenceId = "aura_style"
                });
            }
            //documentation
            if (model.Documentation != null)
            {
                compositeRequest.compositeRequest.Add(new CompositeSubrequest()
                {
                    method = "POST",
                    body = new AuraDefinition()
                    {
                        AuraDefinitionBundleId = "@{aura_bundle.id}",
                        DefType = "DOCUMENTATION",
                        Format = "XML",
                        Source = model.Documentation.fileContent()
                    },
                    url = "/services/data/" + _sfVersion + "/tooling/sobjects/AuraDefinition/",
                    referenceId = "aura_documentation"
                });
            }
            //renderer
            if (model.Renderer != null)
            {
                compositeRequest.compositeRequest.Add(new CompositeSubrequest()
                {
                    method = "POST",
                    body = new AuraDefinition()
                    {
                        AuraDefinitionBundleId = "@{aura_bundle.id}",
                        DefType = "RENDERER",
                        Format = "JS",
                        Source = model.Renderer.fileContent()
                    },
                    url = "/services/data/" + _sfVersion + "/tooling/sobjects/AuraDefinition/",
                    referenceId = "aura_renderer"
                });
            }
            //design
            if (model.Design != null)
            {
                compositeRequest.compositeRequest.Add(new CompositeSubrequest()
                {
                    method = "POST",
                    body = new AuraDefinition()
                    {
                        AuraDefinitionBundleId = "@{aura_bundle.id}",
                        DefType = "DESIGN",
                        Format = "XML",
                        Source = model.Design.fileContent()
                    },
                    url = "/services/data/" + _sfVersion + "/tooling/sobjects/AuraDefinition/",
                    referenceId = "aura_design"
                });
            }
            //design
            if (model.SVG != null)
            {
                compositeRequest.compositeRequest.Add(new CompositeSubrequest()
                {
                    method = "POST",
                    body = new AuraDefinition()
                    {
                        AuraDefinitionBundleId = "@{aura_bundle.id}",
                        DefType = "SVG",
                        Format = "XML",
                        Source = model.SVG.fileContent()
                    },
                    url = "/services/data/" + _sfVersion + "/tooling/sobjects/AuraDefinition/",
                    referenceId = "aura_svg"
                });
            }


            var content = new StringContent(JsonSerializer.Serialize(compositeRequest), Encoding.UTF8, "application/json");

            var response = client.PostAsync(_sfUrl + "tooling/composite", content).Result;
            //need to parse the response to identify errors
            return new SalesforceResult()
            {
                StatusCode = response.StatusCode,
                BodyContent = response.Content.ReadAsStringAsync().Result
            };
        }

        public SalesforceResult CreateApexClass(SalesforceApexClass apexClass)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "Deploy2Org");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _sfToken);

            var content = new StringContent(JsonSerializer.Serialize(apexClass), Encoding.UTF8, "application/json");

            var response = client.PostAsync(_sfUrl + "sobjects/ApexClass", content).Result;
            return new SalesforceResult()
            {
                StatusCode = response.StatusCode,
                BodyContent = response.Content.ReadAsStringAsync().Result
            };

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
