using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SharpMapServer.Model
{
    public class WmsLayer
    {
        [Key]
        public string Name { get; set; }
        public string Description { get; set; }
        public string DataSource { get; set; }
        public string Provider { get; set; }
    }
}