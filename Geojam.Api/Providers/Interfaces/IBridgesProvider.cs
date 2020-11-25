using Geojam.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geojam.Api.Providers.Interfaces
{
    public interface IBridgesProvider
    {
        Task<(bool IsSuccess, IEnumerable<Bridge> Bridges, string ErrorMessage)> GetBridgesAsync();

        Task<(bool IsSuccess, SortedList<int, Models.Bridge> SortedBridges, string ErrorMessage)> GetSortedBridgesAsync(Models.Bridge[] bridge);

        Task<(bool IsSuccess, double? CrossingTime, SortedList<double, Models.Hiker> SortedHikers, string ErrorMessage)> CrossBridgeAsync(SortedList<double, Models.Hiker> hikers, Models.Bridge bridge);

        Task<(bool IsSuccess, double? CrossingTime, string ErrorMessage)> CrossBridgesAsync();
    }
}
