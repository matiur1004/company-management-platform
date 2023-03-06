﻿using AutoMapper;
using ClientPortal.Data.Entities.UMFAEntities;
using ClientPortal.Models.ResponseModels;
using Dapper;
using System.Data;

namespace ClientPortal.Data.Repositories
{
    public interface IPortalStatsRepository
    {
        Task<PortalStatsResponse> GetStatsAsync();
        Task<DashboardMainResponse> GetDashboardMainAsync(int umfaUserId);
        Task<DashboardMainResponse> GetDashboardBuildingAsync(int buildingId);
    }
    public class PortalStatsRepository : IPortalStatsRepository
    {
        private readonly ILogger<PortalStatsRepository> _logger;
        private readonly UmfaDBContext _context;
        private readonly IMapper _mapper;

        public PortalStatsRepository(ILogger<PortalStatsRepository> logger, 
            UmfaDBContext context, 
            IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
        }

        public async Task<DashboardMainResponse> GetDashboardMainAsync(int umfaUserId)
        {
            var stats = new DashboardMainResponse();
            try
            {
                //var result = await _context.GetStats.FromSqlRaw<PortalStats>("exec upPortal_stats").ToListAsync();
                //stats = _mapper.Map<PortalStatsResponse>(result.FirstOrDefault());
                stats.Response = "Success";
                stats.BuildingStats = new() { NumberOfBuildings = 194, TotalGLA = 381729.2M, TotalNumberOfMeters = 15605 };
                stats.ShopStats = new() {  NumberOfShops = 5578, OccupiedPercentage = 0.778M, TotalArea = 2262098.26M};
                stats.TenantStats = new() { NumberOfTenants = 6208, OccupiedPercentage = 0.652M, RecoverablePercentage = 0.612M };
                stats.SmartStats = new() { TotalSmart = 7283, SolarCount = 42, GeneratorCount = 563, ConsumerElectricityCount = 6270, 
                    ConsumerWaterCount = 191, BulkCount = 176, CouncilChkCount = 41 };
                stats.GraphStats = new() { 
                    new() { PeriodName = "August 2022", TotalSales = 52835320.84M, TotalElectricityUsage = 12897791.42M, TotalWaterUsage = 104573.03M },
                    new() { PeriodName = "September 2022", TotalSales = 51580188.45M, TotalElectricityUsage = 12805219.77M, TotalWaterUsage = 104543.01M },
                    new() { PeriodName = "October 2022", TotalSales = 46190196.02M, TotalElectricityUsage = 11979008.47M, TotalWaterUsage = 109944.85M },
                    new() { PeriodName = "November 2022", TotalSales = 48446496.20M, TotalElectricityUsage = 13179913.95M, TotalWaterUsage = 116619.28M },
                    new() { PeriodName = "December 2022", TotalSales = 46739366.05M, TotalElectricityUsage = 13030128.39M, TotalWaterUsage = 104230.20M },
                    new() { PeriodName = "January 2023", TotalSales = 47336223.17M, TotalElectricityUsage = 12792658.64M, TotalWaterUsage = 111528.69M }
                };
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while retrieving stats from database: {ex.Message}");
                stats.Response = $"Error while retrieving stats from database: {ex.Message}";
                return stats;
            }
        }

        public async Task<DashboardMainResponse> GetDashboardBuildingAsync(int buildingId)
        {
            var stats = new DashboardMainResponse();
            try
            {
                var CommandText = $"exec upPortal_BuildingDashboard {buildingId}";
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                var results = await connection.QueryAsync<BuildingDashboard>(CommandText);

                if (results == null || results.Count() == 0)
                    return stats;

                var BuildingDB = results.ToList();

                var first = BuildingDB.FirstOrDefault();
                stats.Response = "Success";
                stats.BuildingStats = new() { NumberOfBuildings = 0, TotalGLA = first.TotalGLA, TotalNumberOfMeters = first.TotalNumberOfMeters };
                stats.ShopStats = new() { NumberOfShops = first.NumberOfShops, OccupiedPercentage = first.ShopOccPerc, TotalArea = first.TotalArea };
                stats.TenantStats = new() { NumberOfTenants = first.NumberOfTenants, OccupiedPercentage = first.TenOccPerc, RecoverablePercentage = first.RecoverablePercentage };
                stats.SmartStats = new()
                {
                    TotalSmart = first.TotalSmart,
                    SolarCount = first.SolarCount,
                    GeneratorCount = first.GeneratorCount,
                    ConsumerElectricityCount = first.ConsumerElectricityCount,
                    ConsumerWaterCount = first.ConsumerWaterCount,
                    BulkCount = first.BulkCount,
                    CouncilChkCount = first.CouncilChkCount
                };

                stats.GraphStats = new();
                foreach (var buildingStat in BuildingDB ) 
                {
                    stats.GraphStats.Add(new() { PeriodName = buildingStat.PeriodName, TotalSales = buildingStat.TotalSales, 
                        TotalElectricityUsage = buildingStat.TotalElectricityUsage, TotalWaterUsage = buildingStat.TotalWaterUsage });
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while retrieving stats from database: {ex.Message}");
                stats.Response = $"Error while retrieving stats from database: {ex.Message}";
                return stats;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<PortalStatsResponse> GetStatsAsync()
        {
            //_logger.LogInformation($"Retrieving stats from database...");
            var stats = new PortalStatsResponse() { Partners = -1, Buildings = 0, Clients = 0, Shops = 0, Tenants = 0, Response = "State: Initiated" };
            try
            {
                var result = await _context.GetStats.FromSqlRaw<PortalStats>("exec upPortal_stats").ToListAsync();
                stats = _mapper.Map<PortalStatsResponse>(result.FirstOrDefault());
                return stats;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error while retrieving stats from database: {ex.Message}");
                stats.Response = $"Error while retrieving stats from database: {ex.Message}";
                return stats;
            }
        } 
    }
}
