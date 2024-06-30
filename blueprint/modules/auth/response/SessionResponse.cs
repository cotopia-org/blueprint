namespace blueprint.modules.auth.response
{
    public class SessionResponse
    {
        public string id { get; set; }
        public string sessionName { get; set; }
        public string refreshToken { get; set; }
    }
}
