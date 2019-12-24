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

    }
}
