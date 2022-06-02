using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Caching;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TinyURL.App
{
	public class TinyUrl
	{
//		public string _id { get; set; }
		public string URL { get; set; }
		public string ShortenedURL { get; set; }
		public string Token { get; set; }
		public DateTime Created { get; set; } = DateTime.Now;
	}

	public class Shortener
	{
		public string Token { get; set; }
		private TinyUrl biturl;

		// The method with which we generate the token
		private Shortener GenerateToken()
		{
			string urlsafe = string.Empty;
			Enumerable.Range(48, 75)
			  .Where(i => i < 58 || i > 64 && i < 91 || i > 96)
			  .OrderBy(o => new Random().Next())
			  .ToList()
			  .ForEach(i => urlsafe += Convert.ToChar(i)); // Store each char into urlsafe
			Token = urlsafe.Substring(new Random().Next(0, urlsafe.Length), new Random().Next(2, 6));
			return this;
		}

		public Shortener(string url, MemoryCache cache)
		{		
			var client = new MongoClient();
			var collection = client.GetDatabase("TinyUrl").GetCollection<BsonDocument>("urls");
			var builder_filter = Builders<BsonDocument>.Filter;
			var filterByURL = builder_filter.Eq("URL", url);
			if (cache.Contains(url) || collection.Find(filterByURL).Count() > 0)
				throw new Exception("URL already exists");
			// If the token exists in our DB we want generate a new one
			var token = GenerateToken().Token;			
			while (collection.Find(builder_filter.Eq("Token", token)).Count() > 0) token = GenerateToken().Token;
			// Store the values in the TinyURL model
			biturl = new TinyUrl()
			{
				Token = token,
				URL = url,
				ShortenedURL = new URLConf().Config.BASE_URL + Token
			};
			BsonDocument doc = new BsonDocument().Add("Token", biturl.Token);
			doc.Add("URL", biturl.URL);
			doc.Add("ShortenedURL", biturl.ShortenedURL);
			doc.Add("Created", biturl.Created);
			// Save the TinyURL model to the DB
			collection.InsertOne(doc);
			
		}
	}
}
