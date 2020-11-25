using AutoMapper;
using Geojam.Api.Db;
using Geojam.Api.Models;
using Geojam.Api.Providers.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
    public class BridgesProvider : IBridgesProvider
    {
        private readonly GeojamDbContext dbContext;
        private readonly ILogger<BridgesProvider> logger;
        private readonly IMapper mapper;
        private readonly IHikerProvider hikerProvider;        

        public BridgesProvider(GeojamDbContext dbContext, ILogger<BridgesProvider> logger, IMapper mapper, IHikerProvider hikerProvider, IWebHostEnvironment env)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.mapper = mapper;
            this.hikerProvider = hikerProvider;
            if (env != null)
            {                
                var path = $@"{env.ContentRootPath}\..\Data.yaml";
                SeedData(path);
            }
        }

        private void SeedData(string file)
        {
            if (!dbContext.Bridges.Any())
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(new YamlDotNet.Serialization.NamingConventions.PascalCaseNamingConvention())
                    .Build();

                var yamlText = File.OpenText(file);
                var yamlObjects = deserializer.Deserialize<IEnumerable<Db.Bridge>>(yamlText);
                dbContext.Bridges.AddRange(yamlObjects);
                foreach (var bridge in dbContext.Bridges)
                {
                    foreach (var hiker in bridge.Hikers)
                    {
                        hiker.Bridge = bridge;
                        hiker.BridgeId = bridge.Id;
                        dbContext.Hikers.Add(hiker);
                    }
                }
                dbContext.SaveChanges();
            }
        }
                
        public async Task<(bool IsSuccess, IEnumerable<Models.Bridge> Bridges, string ErrorMessage)> GetBridgesAsync()
        {
            try
            {               
                var bridges = await dbContext.Bridges.ToListAsync();
                var hikers = await dbContext.Hikers.ToListAsync();                
                if (bridges != null && bridges.Any())
                {
                    foreach (var bridge in bridges)
                    {
                        bridge.Hikers = (hikers != null && hikers.Any()) 
                            ? hikers.Where(h => h.BridgeId == bridge.Id) 
                            : Enumerable.Empty<Db.Hiker>();
                    }
                    var result = mapper.Map<IEnumerable<Db.Bridge>, IEnumerable<Models.Bridge>>(bridges);

                    return (true, result, null);
                }
                return (false, null, "Not Found");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
                return (false, null, ex.Message);
            }
        }

        public async Task<(bool IsSuccess, SortedList<int, Models.Bridge> SortedBridges, string ErrorMessage)> GetSortedBridgesAsync(Models.Bridge[] bridges)
        {
            try
            {
                SortedList<int, Models.Bridge> result = new SortedList<int, Models.Bridge>();
                foreach (var bridge in bridges)
                {                    
                        result.Add(bridge.Order, bridge);                    
                }
                return (true, await Task.Run(() => result), null);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
                return (false, null, ex.Message);
            }
        }

        public async Task<(bool IsSuccess, double? CrossingTime, SortedList<double, Models.Hiker> SortedHikers, string ErrorMessage)> CrossBridgeAsync(SortedList<double, Models.Hiker> hikers, Models.Bridge bridge)
        {
            try
            {                
                if (hikers == null) hikers = new SortedList<double, Models.Hiker>();
                foreach (var hiker in bridge.Hikers)
                {
                    hikers.Add(hiker.CrossingSpeed, hiker);
                }

                if (!hikers.Any())           // no hikers at this bridge - nobody needs to cross; return zero minutes
                    return (true, 0, hikers, null);
                else if (hikers.Count == 1)  // only one hiker at this bridge - return this hiker's crossing time
                {
                    var oneHikerResult = await hikerProvider.CrossBridgeAsync(hikers.First().Value, bridge);
                    return (true, oneHikerResult.CrossingTime, hikers, null);
                }
                else if (hikers.Count == 2)  // only two hiker at this bridge - return the second hiker's crossing time
                {
                    var twoHikersResult = await hikerProvider.CrossBridgeAsync(hikers.Last().Value, bridge);
                    return (true, twoHikersResult.CrossingTime, hikers, null);
                }
                else
                {
                    var fastestHiker = hikers.Last().Value;
                    int numberOfReturnTrips = hikers.Count - 2;
                    var taskResult = await hikerProvider.CrossBridgeAsync(fastestHiker, bridge);
                    var fastestHikerCrossTime = (taskResult.IsSuccess) ? taskResult.CrossingTime.Value : 0;
                    double crossingTime = fastestHikerCrossTime * numberOfReturnTrips;
                    for (int i = 0; i <= hikers.Count - 2; i++)
                    {
                        taskResult = await hikerProvider.CrossBridgeAsync(hikers[hikers.Keys[i]], bridge);
                        crossingTime = (taskResult.IsSuccess) ? crossingTime + taskResult.CrossingTime.Value : crossingTime;
                    }
                    return (true, crossingTime, hikers, null);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
                return (false, null, null, ex.Message);
            }
        }

        public async Task<(bool IsSuccess, double? CrossingTime, string ErrorMessage)> CrossBridgesAsync()
        {
            try
            {
                var bridges = await GetBridgesAsync();
                if (!bridges.IsSuccess) throw new Exception(bridges.ErrorMessage);
                var sortedBridges = await GetSortedBridgesAsync(bridges.Bridges.ToArray());
                if (!sortedBridges.IsSuccess) throw new Exception(sortedBridges.ErrorMessage);

                double crossingTime = 0;
                SortedList<double, Models.Hiker> sortedHikers = null;
                foreach (var bridge in sortedBridges.SortedBridges)
                {
                    var taskResult = await CrossBridgeAsync(sortedHikers, bridge.Value);
                    if (!taskResult.IsSuccess) throw new Exception(taskResult.ErrorMessage);
                    crossingTime += taskResult.CrossingTime.Value;
                    sortedHikers = taskResult.SortedHikers;
                }
                return (true, crossingTime, null);              
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
                return (false, null, ex.Message);                
            }
        }
    }
}
