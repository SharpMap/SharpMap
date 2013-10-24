using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace SharpMapServer.Model
{
    public class SharpMapContext
    {
        public List<User> Users { get; set; }
        public List<WmsLayer> Layers { get; set; }
        public WmsCapabilities Capabilities { get; set; }
    }
}