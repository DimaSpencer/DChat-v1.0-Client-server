using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Models
{
    public enum Purpose
    {
        Login,
        Register
    }
    public class Request
    {
        public Purpose Purpose { get; set; }
        public string Message { get; set; }
    }
}
