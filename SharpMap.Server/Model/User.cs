using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SharpMapServer.Model
{
    public class User
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}