using Geojam.Api.Providers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geojam.Api.Controllers
{
    [ApiController]
    [Route("api/bridges")]
    public class BridgesController : ControllerBase
    {
        private readonly IBridgesProvider bridgesProvider;

        public BridgesController(IBridgesProvider bridgesProvider)
        {
            this.bridgesProvider = bridgesProvider;
        }

        [HttpGet]
        public async Task<ActionResult> GetBridgesAsync()
        {
            var result = await bridgesProvider.GetBridgesAsync();
            if (result.IsSuccess)
            {
                return (Ok(result.Bridges));
            }
            return NotFound();
        }

        [HttpGet(nameof(BridgesController.GetSortedBridgesAsync))]
        public async Task<ActionResult> GetSortedBridgesAsync()
        {
            var bridges = await bridgesProvider.GetBridgesAsync();
            if (bridges.IsSuccess)
            {
                var result = await bridgesProvider.GetSortedBridgesAsync(bridges.Bridges.ToArray());
                return (Ok(result.SortedBridges.Values));
            }
            return NotFound();            

        }

        [HttpGet(nameof(BridgesController.CrossBridgesAsync))]
        public async Task<ActionResult> CrossBridgesAsync()
        {
            var result = await bridgesProvider.CrossBridgesAsync();
            if (result.IsSuccess)
            {
                return (Ok($"Total Crossing Time: {result.CrossingTime} minutes"));
            }
            return NotFound();
        }
    }
}
