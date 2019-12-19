using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SharpMapServer.Model
{
    public class WmsCapabilities
    {
        public int ID { get; set; }
        public string Abstract { get; set; }
        public string AccessConstraints { get; set; }
        public string Fees { get; set; }
        public uint LayerLimit { get; set; }
        public uint MaxWidth { get; set; }
        public uint MaxHeight { get; set; }
        public string OnlineResource { get; set; }
        public string Title { get; set; }
        public string Keywords { get; set; }
    }
}