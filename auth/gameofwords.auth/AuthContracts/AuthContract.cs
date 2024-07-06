namespace gameofwords.auth.AuthContracts
{
    public class VkData
    {
        public VKResponse[] Response { get; set; }
    }
    public class VkAuth
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public int user_id { get; set; }
    }
    public class GoogleAuth
    {
        public string access_token { get; set; }
        public string id_token { get; set; }
    }
    public class VKResponse
    {
        public long Id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
    }
    public class GoogleData
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}
