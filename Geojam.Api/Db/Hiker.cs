using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geojam.Api.Db
{
    public class Hiker
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double CrossingSpeed { get; set; }

        public Bridge Bridge { get; set; }
        public int BridgeId { get; set; }
    }
}
