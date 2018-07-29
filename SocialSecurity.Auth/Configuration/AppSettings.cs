namespace SocialSecurity.Auth.Configuration
{
    /// <summary>
    /// AppSettings
    /// </summary>
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }
    }

    public class ConnectionStrings
    {
        public string AuthContext { get; set; }
    }
}