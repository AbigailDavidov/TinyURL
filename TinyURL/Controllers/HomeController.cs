using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Caching;
using TinyURL.Models;
using TinyURL.App;
using MongoDB.Driver;
using MongoDB.Bson;

namespace TinyURL.Controllers
{

    public class URLResponse
    {
		public string url { get; set; }
		public string status { get; set; }
		public string token { get; set; }
	}

    public class HomeController : Controller
    {
        public MemoryCache cache = MemoryCache.Default;
        private MongoClient client = new MongoClient();

        // index Route
        [HttpGet, Route("/")]
        public IActionResult Index()
        {
            return View();
        }

        // URL shorten route
		[HttpPost, Route("/")]
		public IActionResult PostURL([FromBody] string url) 
        {
            var collection = client.GetDatabase("TinyUrl").GetCollection<BsonDocument>("urls");
            var builder_filter = Builders<BsonDocument>.Filter;
            var filterByURL = builder_filter.Eq("URL", url);
            var cache = new MemoryCache("urlCache");
            try
            {
                if (!url.Contains("http"))
                {
                    url = "http://" + url;
                }
                Shortener shortURL = new Shortener(url, cache);
                return Json(shortURL.Token);
            }
            catch (Exception ex)
            {
                if (ex.Message == "URL already exists")
                {
                    var token = collection.Find(filterByURL).First()["Token"].AsString;
                    Response.StatusCode = 400;
                    return Json(new URLResponse() { url = url, status = "URL already exists", token = token });
                }
                throw new Exception(ex.Message);
            }
            return StatusCode(500);
        }
		
        // Redirect route
		[HttpGet, Route("/{token}")]
        public IActionResult URLRedirect([FromRoute] string token) 
        {
            var collection = client.GetDatabase("TinyUrl").GetCollection<BsonDocument>("urls");
            var builder_filter = Builders<BsonDocument>.Filter;
            var filterByToken = builder_filter.Eq("Token", token);
            var url = collection.Find(filterByToken).First()["URL"];
            return Redirect(url.AsString);
        }


    }
}
