﻿using ClientPortal.Data.Entities;
using ClientPortal.Data.Repositories;
using ClientPortal.Models;

namespace ClientPortal.Services
{
    public class DashboardService
    {
        private readonly IPortalStatsRepository _portalStats;
        private readonly ILogger<DashboardService> _logger;
        readonly IBuildingService _buildingService;

        public DashboardService(IPortalStatsRepository portalStats, ILogger<DashboardService> logger, IBuildingService buildingService)
        {
            _buildingService = buildingService;
            _portalStats = portalStats;
            _logger = logger;
        }

        public DashboardMainResponse GetMainDashboard(int umfaUserId)
        {
            _logger.LogInformation("Getting the stats for main dashboard page...");
            try
            {
                var response = _portalStats.GetDashboardMainAsync(umfaUserId).Result;
                if (response != null && response.Response == "Success") return response;
                else throw new Exception($"Stats not return correctly: {response?.Response}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while getting the stats: {ex.Message}");
                return null;
            }
        }

        public List<DashboardBuilding> GetBuildingList(int umfaUserId)
        {
            _logger.LogInformation("Getting the buildings for buildings dashboard page...");
            try
            {
                List<int> bldgs = new() { 2403, 518 };
                List<DashboardBuilding> ret = new();
                var response = _buildingService.GetUmfaBuildingsAsync(umfaUserId).Result;
                if (response != null && response.Response.Contains("Success"))
                { 
                    foreach (UMFABuilding bld in response.UmfaBuildings)
                    {
                        ret.Add(new() { UmfaBuildingId = bld.BuildingId, BuildingName = bld.Name, PartnerId = bld.PartnerId,
                        PartnerName = bld.Partner, IsSmart = (bldgs.Contains(bld.BuildingId))? true: false });
                    }
                    return ret;
                }
                else throw new Exception($"Stats not return correctly: {response?.Response}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while getting the stats: {ex.Message}");
                return null;
            }
        }
    }
}