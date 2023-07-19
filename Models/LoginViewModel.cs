public class LoginViewModel
{
    public string Username { get; set; }
    public string Password { get; set; }
    public bool? CalledFromAdmin { get; set; }
    public string? Error { get; set; }
}

