using MessagePack;
using System;

namespace Define
{
    [MessagePackObject]
    public class UserLogin  // Bỏ "partial"
    {
        [Key(0)]
        public string Username { get; set; }

        [Key(1)]
        public string Password { get; set; }

        public UserLogin() { }

        public UserLogin(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    [MessagePackObject]
    public class UserLoginResponse  // Bỏ "partial"
    {
        [Key(0)]
        public bool Success { get; set; }

        [Key(1)]
        public string Message { get; set; }

        [Key(2)]
        public string Token { get; set; }

        public UserLoginResponse() { }
    }
}
