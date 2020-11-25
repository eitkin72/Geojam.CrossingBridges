using AutoMapper;
using Geojam.Api.Db;
using Geojam.Api.Profiles;
using Geojam.Api.Providers.Classes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Geojam.Api.Tests
{
    public class BridgesServiceTest
    {
        private readonly List<Db.Bridge> bridges;
        private readonly List<Db.Hiker> hikers;
        private readonly Mapper mapper;

        public BridgesServiceTest()
        {
            bridges = new List<Db.Bridge>
            {
                new Db.Bridge
                {
                    Id = 1,
                    Order = 2,
                    Span = 50,
                    Hikers = new List<Db.Hiker>()
                },
                new Db.Bridge
                {
                    Id = 2,
                    Order = 3,
                    Span = 150,
                    Hikers = new List<Db.Hiker>()
                },
                new Db.Bridge
                {
                    Id = 3,
                    Order = 1,
                    Span = 100,
                    Hikers = new List<Db.Hiker>()
                }
            };
            hikers = new List<Db.Hiker> {
                new Db.Hiker {
                    BridgeId = 1,
                    Id =  1,
                    Name = "A",
                    CrossingSpeed = 100
                },
                new Db.Hiker {
                    BridgeId = 1,
                    Id =  2,
                    Name = "B",
                    CrossingSpeed = 50
                },                
                new Db.Hiker {
                    BridgeId = 1,
                    Id =  3,
                    Name = "C",
                    CrossingSpeed = 20
                },
                new Db.Hiker {
                    BridgeId = 1,
                    Id =  4,
                    Name = "D",
                    CrossingSpeed = 10
                },
                new Db.Hiker {
                    BridgeId = 2,
                    Id =  5,
                    Name = "E",
                    CrossingSpeed = 2.5
                },
                new Db.Hiker {
                    BridgeId = 2,
                    Id =  6,
                    Name = "F",
                    CrossingSpeed = 25
                },
                new Db.Hiker {
                    BridgeId = 3,
                    Id =  7,
                    Name = "G",
                    CrossingSpeed = 12.5
                },
            };
            var autoMapperProfile = new AutoMapperProfile();
            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile(autoMapperProfile));
            mapper = new Mapper(mapperConfig);
        }

        [Fact]
        public async Task GetBridgesReturnsAllBridges()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<GeojamDbContext>()
                .UseInMemoryDatabase(nameof(GetBridgesReturnsAllBridges))
                .Options;
            var dbContext = new GeojamDbContext(options);
            GenerateDbContext(dbContext);

            var hikerProvider = new HikerProvider(dbContext, null, mapper);
            var bridgesProvider = new BridgesProvider(dbContext, null, mapper, hikerProvider, null);

            // Act
            var actual = await bridgesProvider.GetBridgesAsync();

            // Assert
            Assert.True(actual.IsSuccess);
            Assert.Null(actual.ErrorMessage);
            Assert.True(actual.Bridges.Any());
            Assert.Equal(actual.Bridges.Count(), dbContext.Bridges.Count());
            Assert.IsType<Models.Bridge>(actual.Bridges.First());
        }

        [Fact]
        public async Task GetSortedBridgesReturnsSortedBridgesCollection()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<GeojamDbContext>()
                .UseInMemoryDatabase(nameof(GetSortedBridgesReturnsSortedBridgesCollection))
                .Options;
            var dbContext = new GeojamDbContext(options);
            GenerateDbContext(dbContext);          
            var hikerProvider = new HikerProvider(dbContext, null, mapper);
            var bridgesProvider = new BridgesProvider(dbContext, null, mapper, hikerProvider, null);
            var bridges = await bridgesProvider.GetBridgesAsync();

            // Act
            var actual = await bridgesProvider.GetSortedBridgesAsync(bridges.Bridges.ToArray());

            Assert.True(actual.IsSuccess);
            Assert.Null(actual.ErrorMessage);
            Assert.True(actual.SortedBridges.Any());
            Assert.Equal(actual.SortedBridges.Count(), dbContext.Bridges.Count());
            for (int i = 1; i < actual.SortedBridges.Count; i++)
            {
                Assert.True(actual.SortedBridges.Keys[i - 1] <= actual.SortedBridges.Keys[i]);
            }           
        }


        [Theory]
        [MemberData(nameof(BridgesServiceData.Data), MemberType = typeof(BridgesServiceData))]
        public async Task CrossBridgesReturnsTimeToCrossAllBridgesByAllHikers(int dbContextUniqueIndex, int[] includeBridgeIds, double expected)
        {
            // Arrange
            var dbName = $"{nameof(CrossBridgesReturnsTimeToCrossAllBridgesByAllHikers)}{dbContextUniqueIndex}";
            var options = new DbContextOptionsBuilder<GeojamDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            var dbContext = new GeojamDbContext(options);
            GenerateDbContext(dbContext, includeBridgeIds);
            var hikerProvider = new HikerProvider(dbContext, null, mapper);
            var bridgesProvider = new BridgesProvider(dbContext, null, mapper, hikerProvider, null);
           
            // Act
            var actual = await bridgesProvider.CrossBridgesAsync();

            Assert.True(actual.IsSuccess);
            Assert.Null(actual.ErrorMessage);
            Assert.True(actual.CrossingTime == expected);
        }

        #region private methods


        private void GenerateDbContext(GeojamDbContext dbContext, int[] includeBridgeIds = null)
        {
            if (includeBridgeIds == null || !includeBridgeIds.Any())
                includeBridgeIds = bridges.Select(b => b.Id).ToArray();
            
            foreach (var bridge in bridges.Where(b => includeBridgeIds.Contains(b.Id)))
            {               
                dbContext.Bridges.Add(bridge);
            }
            foreach (var hiker in hikers.Where(h => includeBridgeIds.Contains(h.BridgeId)))
            {              
                dbContext.Hikers.Add(hiker);
            }

            foreach (var bridge in dbContext.Bridges)
            {
                bridge.Hikers = new List<Db.Hiker>();
                bridge.Hikers = dbContext.Hikers.Where(h => h.BridgeId == bridge.Id);
            }
            dbContext.SaveChanges();
        }

        #endregion private methods
    }
}

