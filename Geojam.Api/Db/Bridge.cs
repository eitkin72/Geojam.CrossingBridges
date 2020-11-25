using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geojam.Api.Db
{
    public class Bridge
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public int Span { get; set; }
        public IEnumerable<Hiker> Hikers { get; set; }
    }
}
