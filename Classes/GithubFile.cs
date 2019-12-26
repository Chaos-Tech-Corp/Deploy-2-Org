using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace deploy2.org.com.Classes
{
    public class GithubFile
    {
        public string ResponseBody { get; set; }
        public System.Net.HttpStatusCode StatusCode { get; set; }

        public string name { get; set; }
        public string path { get; set; }
        public string sha { get; set; }
        public int size { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string git_url { get; set; }
        public string download_url { get; set; }
        public string type { get; set; }
        public string content { get; set; }
        public string encoding { get; set; }
        public GithubLinks _links { get; set; }
        public string fileContent()
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(content)).Trim();
        }
    }
}