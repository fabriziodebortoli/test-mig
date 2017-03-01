public class DSInitMessage
{
  
    public string selection_type { get; set; }
    public string like_value { get; set; }
    public string disabled { get; set; }
    public string good_type { get; set; }
}

public class LoginInfoMessage
{
    public string userName { get; set; }
    public string companyName { get; set; }
    public bool admin { get; set; }
    public string connectionString { get; set; }
    public string providerName { get; set; }
    public bool useUnicode { get; set; }
    public string preferredLanguage { get; set; }
    public string applicationLanguage { get; set; }
}