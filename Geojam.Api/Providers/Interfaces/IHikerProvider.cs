using Geojam.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geojam.Api.Providers.Interfaces
{
    public interface IHikerProvider
    {
        Task<(bool IsSuccess, double? CrossingTime, string ErrorMessage)> CrossBridgeAsync(Models.Hiker hiker, Models.Bridge bridge);
    }
}
