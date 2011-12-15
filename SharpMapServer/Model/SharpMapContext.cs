using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace SharpMapServer.Model
{
    public class SharpMapContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<WmsLayer> Layers { get; set; }
        public DbSet<WmsCapabilities> Capabilities { get; set; }
    }
}