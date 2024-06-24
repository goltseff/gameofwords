using YamlDotNet.Serialization;

namespace gameofwords.common.config
{
    public class ConfigYaml
    {
        [YamlMember( Alias = "db-connection", ApplyNamingConventions = false )]
        public string DbConnection { get; set; }

        [YamlMember( Alias = "services", ApplyNamingConventions = false )]
        public Dictionary<string, Service> Services { get; set; }
    }

    public class Service
    {
        [YamlMember( Alias = "url", ApplyNamingConventions = false )]
        public string Url { get; set; }
        [YamlMember( Alias = "parameters", ApplyNamingConventions = false )]
        public Dictionary<string, string> Parameters { get; set; }
    }
}
