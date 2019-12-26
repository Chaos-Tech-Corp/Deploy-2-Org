using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using deploy2.org.com.Models;

using Microsoft.AspNetCore.Authentication;

namespace deploy2.org.com.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        private static string ghTokenId;
        private static string sfTokenId;
        private static string sfUrl;

        public async Task<IActionResult> IndexAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.Claims.Any(C => C.Type == "urn:salesforce:rest_url"))
                {
                    string accessToken = await HttpContext.GetTokenAsync("access_token");
                    sfTokenId = accessToken;
                    sfUrl = User.Claims.First(C => C.Type == "urn:salesforce:rest_url").Value;
                }
                if (User.Claims.Any(C => C.Type == "urn:github:name"))
                {
                    string accessToken = await HttpContext.GetTokenAsync("access_token");
                    ghTokenId = accessToken;
                }
            }

            return View();
        }

        public JsonResult CreateClass()
        {
            deploy2.org.com.Classes.DeployAssistant g2o = new deploy2.org.com.Classes.DeployAssistant(sfTokenId ?? "", "v47.0", sfUrl ?? "",  ghTokenId, "Chaos-Tech-Corp", "Github-2-Org");

            g2o.UploadComponent();

            return new JsonResult(new { Cool = true });
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
