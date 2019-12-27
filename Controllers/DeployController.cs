using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using deploy2.org.com.Models;

using Microsoft.AspNetCore.Authentication;
using deploy2.org.com.Extensions;
using deploy2.org.com.Classes;
using System.Net.Http;

namespace deploy2.org.com.Controllers
{
    public class DeployController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public DeployController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {

            string referer = Request.Headers["Referer"].ToString();
            referer = "https://github.com/Chaos-Tech-Corp/Modal-Confirmation";
            var segments = new Uri(referer).LocalPath.Split('/');
            var ghOrg = segments[1];
            var ghRepo = segments[2];

            //var existing_details = HttpContext.Session.Get<DeployModel>("DEPLOY_MODEL");

            var details = new DeployModel();
            details.ghOrg = ghOrg;
            details.ghRepo = ghRepo;
            details.ghReferer = referer;

            HttpContext.Session.Set<DeployModel>("DEPLOY_MODEL", details);

            //authenticate github first
            return Redirect("/signin-gh");
        }

        public IActionResult Preview()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("/Authentication-Error");
            }

            var existing_details = HttpContext.Session.Get<DeployModel>("DEPLOY_MODEL");

            if (existing_details == null)
            {
                return Redirect("/Session-Error");
            }

            if (User.Claims.Any(C => C.Type == "urn:github:name"))
            {
                string accessToken = HttpContext.GetTokenAsync("access_token").Result;
                existing_details.ghToken = accessToken;
            }

            //retrieve the Github Configuration and detials
            var deploy = new DeployAssistant(existing_details);

            var configFile = deploy.RetrieveGithubConfig();

            existing_details.configurationFile = configFile;

            HttpContext.Session.Set<DeployModel>("DEPLOY_MODEL", existing_details);

            return View(configFile);
        }

        public PartialViewResult Validate()
        {
            var existing_details = HttpContext.Session.Get<DeployModel>("DEPLOY_MODEL");

            //retrieve the Github Configuration and detials
            var deploy = new DeployAssistant(existing_details);

            var componentModel = deploy.Validate(existing_details.configurationFile);

            existing_details.configurationModel = componentModel;

            HttpContext.Session.Set<DeployModel>("DEPLOY_MODEL", existing_details);

            return PartialView("_Validate", existing_details);
        }

        [HttpGet]
        public IActionResult TryMe([FromQuery] string template)
        {
            ViewBag.template = template;
            return View();
        }

        [HttpPost]
        public IActionResult TryMe([FromForm] string template, string random)
        {
            if (string.IsNullOrEmpty(random))
            {
                //no javascript or javascript error
            }

            ViewBag.template = template;
            if (string.IsNullOrEmpty(template) || template.Trim().Length <= 0)
            {
                ViewBag.Error = "Please enter a valid GitHub repository URL.";
                return View("TryMe");
            }
            if (!Uri.IsWellFormedUriString(template, UriKind.Absolute))
            {
                ViewBag.Error = "Please enter a valid GitHub repository URL.";
                return View("TryMe");
            }
            template = template.Trim();
            if (!template.ToLower().StartsWith("https://github.com/"))
            {
                ViewBag.Error = "Please enter a valid GitHub repository URL.";
                return View("TryMe");
            }
  
            //at the time of fetching the configuration file from the repository there will be more validations

            var segments = new Uri(template).LocalPath.Split('/');
            var ghOrg = segments[1];
            var ghRepo = segments[2];

            var details = new DeployModel();
            details.ghOrg = ghOrg;
            details.ghRepo = ghRepo;
            details.ghReferer = template;

            //retrieve the Github Configuration and detials
            var deploy = new DeployAssistant(details);
            try
            {
                if (User.Claims.Any(C => C.Type == "urn:github:name"))
                {
                    string accessToken = HttpContext.GetTokenAsync("access_token").Result;
                    details.ghToken = accessToken;
                }

                var config = deploy.RetrieveGithubConfig();

                details.configurationFile = config;

                HttpContext.Session.Set<DeployModel>("DEPLOY_MODEL", details);

                return View("TryMe", config);

            } catch (HttpRequestException ex)
            {
                if (ex.Message == System.Net.HttpStatusCode.Unauthorized.ToString() || ex.Message == System.Net.HttpStatusCode.Forbidden.ToString())
                {
                    //authenticate 
                    return Redirect("/signin-gh?redirect=/deploy/tryme?template=" + template);
                } else
                {
                    ViewBag.Error = "Couldn't retrieve the file, server returned a " + ex.Message + " error.";
                    return View("TryMe");
                }
            } catch (Exception ex)
            {
                ViewBag.Error = "Couldn't process the configuration file.";
                return View("TryMe");
            }
        }


        [HttpPost]
        public JsonResult Validate(string random)
        {

            var existing_details = HttpContext.Session.Get<DeployModel>("DEPLOY_MODEL");

            //retrieve the Github Configuration and detials
            var deploy = new DeployAssistant(existing_details);

            var componentModel = deploy.Validate(existing_details.configurationFile);

            //differentiate the validated model with the configuration file
            ConfigurationFile diff = new ConfigurationFile();
            if (existing_details.configurationFile.apex_class != null)
            {
                diff.apex_class = new List<string>();
                foreach (var aClass in existing_details.configurationFile.apex_class)
                {
                    if (componentModel.ApexClass != null && componentModel.ApexClass.Any(C => aClass.ToLower().EndsWith(C.path.ToLower()) ))
                    {
                        diff.apex_class.Add("ok");
                    } else
                    {
                        diff.apex_class.Add("error");
                    }

                }
            }

            if (existing_details.configurationFile.bundle_details != null) {
                diff.bundle_details = new Bundle_Details();
                diff.bundle_details.component = (existing_details.configurationFile.bundle_details.component != null && componentModel.Component != null && existing_details.configurationFile.bundle_details.component.ToLower().EndsWith(componentModel.Component.path.ToLower())) ? "ok" : "error";
                diff.bundle_details.controller = (existing_details.configurationFile.bundle_details.controller != null && componentModel.Controller != null && existing_details.configurationFile.bundle_details.controller.ToLower().EndsWith(componentModel.Controller.path.ToLower())) ? "ok" : "error";
                diff.bundle_details.design = (existing_details.configurationFile.bundle_details.design != null && componentModel.Design != null && existing_details.configurationFile.bundle_details.design.ToLower().EndsWith(componentModel.Design.path.ToLower())) ? "ok" : "error";
                diff.bundle_details.documentation = (existing_details.configurationFile.bundle_details.documentation != null && componentModel.Documentation != null && existing_details.configurationFile.bundle_details.documentation.ToLower().EndsWith(componentModel.Documentation.path.ToLower())) ? "ok" : "error";
                diff.bundle_details.helper = (existing_details.configurationFile.bundle_details.helper != null && componentModel.Helper != null && existing_details.configurationFile.bundle_details.helper.ToLower().EndsWith(componentModel.Helper.path.ToLower())) ? "ok" : "error";
                diff.bundle_details.renderer = (existing_details.configurationFile.bundle_details.renderer != null && componentModel.Renderer != null && existing_details.configurationFile.bundle_details.renderer.ToLower().EndsWith(componentModel.Renderer.path.ToLower())) ? "ok" : "error";
                diff.bundle_details.style = (existing_details.configurationFile.bundle_details.style != null && componentModel.Style != null && existing_details.configurationFile.bundle_details.style.ToLower().EndsWith(componentModel.Style.path.ToLower())) ? "ok" : "error";
                diff.bundle_details.svg = (existing_details.configurationFile.bundle_details.svg != null && componentModel.SVG != null && existing_details.configurationFile.bundle_details.svg.ToLower().EndsWith(componentModel.SVG.path.ToLower())) ? "ok" : "error";
            }

            return new JsonResult(diff);
        }

        [HttpPost]
        public IActionResult Deploy(string environment)
        {
            environment = environment ?? "test";

            if (User.Claims.Any(C => C.Type == "urn:salesforce:rest_url"))
            {
                var details = HttpContext.Session.Get<DeployModel>("DEPLOY_MODEL");

                details.sfUrl = User.Claims.First(C => C.Type == "urn:salesforce:rest_url").Value;
                details.sfToken = HttpContext.GetTokenAsync("access_token").Result;

                HttpContext.Session.Set<DeployModel>("DEPLOY_MODEL", details);
            } else
            {
                //authenticate 
                return Redirect("/signin-sf?environment=" + environment + "&redirect=/deploy/deploy");
            }

            return View();
        }
    }
}
