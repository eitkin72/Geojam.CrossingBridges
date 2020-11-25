using AutoMapper;
using Geojam.Api.Db;
using Geojam.Api.Models;
using Geojam.Api.Providers.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Geojam.Api.Providers.Classes
{ 
    public class HikerProvider : IHikerProvider
    {
        private readonly GeojamDbContext dbContext;
        private readonly ILogger<BridgesProvider> logger;
        private readonly IMapper mapper;

        public HikerProvider(GeojamDbContext dbContext, ILogger<BridgesProvider> logger, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.mapper = mapper;

        }

        public async Task<(bool IsSuccess, double? CrossingTime, string ErrorMessage)> CrossBridgeAsync(Models.Hiker hiker, Models.Bridge bridge)
        {
            try
            {
                var timeToCross = await Task.Run(() => bridge.Span / hiker.CrossingSpeed);
                return (true, timeToCross, null);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
                return (false, null, ex.Message);
            }
        }       
    }
}
