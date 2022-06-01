using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TinyURL.App
{
    public class Config
    {
        public string BASE_URL;
    }

    public class URLConf
    {
        public Config Config;
        public URLConf()
        {
            Config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(File.ReadAllText("App/Config.json"));
        }
    }
}
