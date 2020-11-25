using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geojam.Api.Profiles
{
    public class AutoMapperProfile : AutoMapper.Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Db.Hiker, Models.Hiker>();
            CreateMap<Db.Bridge, Models.Bridge>();
        }
    }
}
