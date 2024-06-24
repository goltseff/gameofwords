namespace gameofwords.common.config
{
    public static class Config
    {
        public static void GetConfig( string[] args )
        {
            if(!args.Any( ))
                throw new Exception( $"config file isn't set" );

            var configFile = args[0];
            if(!File.Exists( configFile ))
                throw new ArgumentException( $"no config found at {configFile}" );
            try
            {
                var yaml = System.IO.File.ReadAllText( configFile );
                var deserializer = new YamlDotNet.Serialization.Deserializer( );
                _config = deserializer.Deserialize<ConfigYaml>( yaml );
            }
            catch(Exception)
            {
                var msg = "cannot read config.";
                throw new ArgumentException( msg );
            }
        }

        private static ConfigYaml _config;
        public static string DbConnection => _config.DbConnection;


        private static Service GetService( string serviceName )
        {
            if(_config is null)
            {
                return null;
            }

            if(_config.Services.TryGetValue( serviceName, out var service )) return service;

            return null;
        }

        public static string GetServiceUrl( string serviceName )
        {
            if(GetService( serviceName ) is Service service)
            {
                return service.Url;
            }
            return null;
        }

        public static string GetServiceParam( string serviceName, string paramName )
        {
            if(GetService( serviceName ) is Service service)
            {
                if(service.Parameters is not null &&
                    service.Parameters.TryGetValue( paramName, out var val ))
                {
                    return val;
                }
            }
            return null;
        }

        public static int GetServicePort( string serviceName )
        {
            var url = GetServiceUrl( serviceName );
            var portStr = url.Substring( url.LastIndexOf( ":" )+1 );
            if(portStr.Length > 0)
            {
                int port = int.Parse( portStr );
                return port;
            }
            return 0;
        }


    }
}
