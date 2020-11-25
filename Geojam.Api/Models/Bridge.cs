using System;
using System.Collections.Generic;

namespace Geojam.Api.Models
{
    public class Bridge
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public int Span { get; set; }
        public IEnumerable<Hiker> Hikers { get; set; }
    }
}
