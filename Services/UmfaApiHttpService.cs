﻿using ClientPortal.Data.Entities.UMFAEntities;
using ClientPortal.Models.RequestModels;
using ClientPortal.Models.ResponseModels;
using ClientPortal.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Web;

namespace ClientPortal.Services
{

    public interface IUmfaService
    {
        public Task<UMFAShopsSpResponse> GetShopsAsync(UmfaShopsRequest request);

        public Task<List<UMFAShop>> GetTenantShopsAsync(UmfaTenantShopsSpRequest request);
        public Task<List<UMFATenant>> GetTenantsAsync(UmfaTenantsSpRequest request);

        public Task<TenantSlipCardInfo> GetTenantSlipCardInfoAsync(TenantSlipCardInfoSpRequest request);
        public Task<TenantSlipCriteriaResponse> GetTenantSlipCriteriaAsync(TenantSlipCriteriaSpRequest request);
        public Task<TenantSlipReportSpResponse> GetTenantSlipReportsAsync(TenantSlipReportSpRequest request);
        public Task<TenantSlipDataResponse> GetTenantSlipDataAsync(TenantSlipDataRequest request);
        public Task<TenantSlipDataForArchiveSpResponse> GetTenantSlipDataForArchiveAsync(TenantSlipDataForArchiveSpRequest request);

        public Task AddMappedMeterAsync(MappedMeterSpRequest request);

        #region Reports
        public Task<UtilityRecoveryReportResponse> GetUtilityRecoveryReportAsync(UtilityRecoveryReportRequest request);
        public Task<ShopUsageVarianceReportResponse> GetShopUsageVarianceReportAsync(ShopUsageVarianceRequest request);
        public Task<ShopCostVarianceReportResponse> GetShopCostVarianceReportAsync(ShopUsageVarianceRequest request);
        public Task<ConsumptionSummaryResponse> GetConsumptionSummaryReportAsync(ConsumptionSummarySpRequest request);
        public Task<ConsumptionSummaryReconResponse> GetConsumptionSummaryReconReportAsync(ConsumptionSummaryReconRequest request);
        public Task<BuildingRecoveryReport> GetBuildingRecoveryReportAsync(BuildingRecoveryReportSpRequest request);
        public Task UpdateReportArhivesFileFormatsAsync(UpdateArchiveFileFormatSpRequest request);
        #endregion

        public Task<List<ShopDashboardShop>> GetDashboardShopDataAsync(int buildingId);
        public Task<ShopDashboardResponse> GetShopDashboardMainAsync(int buildingId, int shopId, int history);

        public Task<FileFormatDataSpResponse> GetFileFormatData(FileFormatDataSpRequest request);

        public Task<List<UMFAPeriod>> GetBuildingPeriods(int buildingId);

        public Task<List<UmfaShopDashboardBillingDetail>> GetShopDashboardBillingDetailsAsync(int buildingId, int shopId, int history);

        public Task<List<UmfaShopDashboardOccupation>> GetShopDashboardOccupationsAsync(int buildingId, int shopId);

        public Task<List<UmfaShopDashboardAssignedMeter>> GetShopDashboardAssignedMetersAsync(int buildingId, int shopId, int history);
    }


    public class UmfaApiHttpService : IUmfaService
    {
        private readonly HttpClient _httpClient;
        public UmfaApiHttpService(IOptions<UmfaApiSettings> options) 
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(options.Value.BaseUrl)
            };
        }

        private async Task<string> GetAsync(string endpoint, object? queryParams = null)
        {
            string queryString = queryParams != null ? ToQueryString(queryParams) : string.Empty;
            
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint + queryString);
            
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> PostAsync(string endpoint, object data, string mediaType = "application/json")
        {
            string jsonData = JsonSerializer.Serialize(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, mediaType);
            
            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);
            
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> PutAsync(string endpoint, object data, string mediaType = "application/json")
        {
            string jsonData = JsonSerializer.Serialize(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, mediaType);

            HttpResponseMessage response = await _httpClient.PutAsync(endpoint, content);
            
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> DeleteAsync(string endpoint, object data = null, string mediaType = "application/json")
        {
            if (data != null)
            {
                string jsonData = JsonSerializer.Serialize(data);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, mediaType);
                
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, endpoint)
                {
                    Content = content
                };
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync(endpoint);
                
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadAsStringAsync();
            }
        }

        private string ToQueryString(object queryParams)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            var properties = queryParams.GetType().GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(queryParams, null);
                if (value != null)
                {
                    queryString[property.Name] = value.ToString();
                }
            }

            return "?" + queryString.ToString();
        }

        public async Task<TenantSlipDataForArchiveSpResponse> GetTenantSlipDataForArchiveAsync(TenantSlipDataForArchiveSpRequest request)
        {
            var response = await GetAsync("tenantSlips/archiveData", request);

            return JsonSerializer.Deserialize<TenantSlipDataForArchiveSpResponse>(response);
        }

        public async Task<TenantSlipCriteriaResponse> GetTenantSlipCriteriaAsync(TenantSlipCriteriaSpRequest request)
        {
            var response = await GetAsync("tenantSlips/criteria", request);
            return  JsonSerializer.Deserialize<TenantSlipCriteriaResponse>(response);
        }

        public async Task<TenantSlipCardInfo> GetTenantSlipCardInfoAsync(TenantSlipCardInfoSpRequest request)
        {
            var response = await GetAsync("tenantSlips/cardInfo", request);
            return JsonSerializer.Deserialize<TenantSlipCardInfo>(response);
        }

        public async Task<List<UMFATenant>> GetTenantsAsync(UmfaTenantsSpRequest request)
        {
            var response = await GetAsync("tenants", request);
            return JsonSerializer.Deserialize<List<UMFATenant>>(response);
        }

        public async Task<List<UMFAShop>> GetTenantShopsAsync(UmfaTenantShopsSpRequest request)
        {
            var response = await GetAsync("tenants/shops", request);
            return JsonSerializer.Deserialize<List<UMFAShop>>(response);
        }

        public async Task<UMFAShopsSpResponse> GetShopsAsync(UmfaShopsRequest request)
        {
            var response = await GetAsync("shops", request);
            return JsonSerializer.Deserialize<UMFAShopsSpResponse>(response);
        }

        public async Task<TenantSlipReportSpResponse> GetTenantSlipReportsAsync(TenantSlipReportSpRequest request)
        {
            var response = await GetAsync("tenantSlips/reports", request);
            return JsonSerializer.Deserialize<TenantSlipReportSpResponse>(response);
        }

        public async Task<TenantSlipDataResponse> GetTenantSlipDataAsync(TenantSlipDataRequest request)
        {
            var response = await PutAsync("tenantSlips/data", request);
            return JsonSerializer.Deserialize<TenantSlipDataResponse>(response);
        }

        #region
        public async Task<UtilityRecoveryReportResponse> GetUtilityRecoveryReportAsync(UtilityRecoveryReportRequest request)
        {
            var response = await GetAsync("reports/utilityrecoveryreport", request);
            return JsonSerializer.Deserialize<UtilityRecoveryReportResponse>(response);
        }

        public async Task<ShopUsageVarianceReportResponse> GetShopUsageVarianceReportAsync(ShopUsageVarianceRequest request)
        {
            var response = await GetAsync("reports/shopUsagevariancereport", request);
            return JsonSerializer.Deserialize<ShopUsageVarianceReportResponse>(response);
        }

        public async Task<ShopCostVarianceReportResponse> GetShopCostVarianceReportAsync(ShopUsageVarianceRequest request)
        {
            var response = await GetAsync("reports/shopcostvariancereport", request);
            return JsonSerializer.Deserialize<ShopCostVarianceReportResponse>(response);
        }

        public async Task<ConsumptionSummaryResponse> GetConsumptionSummaryReportAsync(ConsumptionSummarySpRequest request)
        {
            var response = await PutAsync("reports/consumptionsummaryreport", request);
            return JsonSerializer.Deserialize<ConsumptionSummaryResponse>(response);
        }

        public async Task<ConsumptionSummaryReconResponse> GetConsumptionSummaryReconReportAsync(ConsumptionSummaryReconRequest request)
        {
            var response = await GetAsync("reports/consumptionsummaryreconreport", request);
            return JsonSerializer.Deserialize<ConsumptionSummaryReconResponse>(response);
        }

        public async Task UpdateReportArhivesFileFormatsAsync(UpdateArchiveFileFormatSpRequest request)
        {
            await PutAsync("reports/updatereportarhivesfileformats", request);
        }
        #endregion

        public async Task AddMappedMeterAsync(MappedMeterSpRequest request)
        {
            await PostAsync("mappedmeters", request);
        }

        public async Task<List<ShopDashboardShop>> GetDashboardShopDataAsync(int buildingId)
        {
            var response = await GetAsync($"dashboard/buildings/{buildingId}/shops");
            return JsonSerializer.Deserialize<List<ShopDashboardShop>>(response);
        }

        public async Task<BuildingRecoveryReport> GetBuildingRecoveryReportAsync(BuildingRecoveryReportSpRequest request)
        {
            var response = await GetAsync("reports/buildingrecoveryreport", request);
            return JsonSerializer.Deserialize<BuildingRecoveryReport>(response);
        }

        public async Task<FileFormatDataSpResponse> GetFileFormatData(FileFormatDataSpRequest request)
        {
            var response = await GetAsync("archives/fileformatdata", request);
            return JsonSerializer.Deserialize<FileFormatDataSpResponse>(response);
        }

        public async Task<List<UMFAPeriod>> GetBuildingPeriods(int buildingId)
        {
            var response = await GetAsync($"buildings/{buildingId}/periods");
            return JsonSerializer.Deserialize<List<UMFAPeriod>>(response);
        }

        public async Task<ShopDashboardResponse> GetShopDashboardMainAsync(int buildingId, int shopId, int history)
        {
            var response = await GetAsync($"dashboard/buildings/{buildingId}/shops/{shopId}", new UmfaShopDashboardRequest { History = history});

            var umfaDashboard = JsonSerializer.Deserialize<UmfaShopDashboardResponse>(response);

            return new ShopDashboardResponse(umfaDashboard);
        }

        public async Task<List<UmfaShopDashboardBillingDetail>> GetShopDashboardBillingDetailsAsync(int buildingId, int shopId, int history)
        {
            var response = await GetAsync($"dashboard/buildings/{buildingId}/shops/{shopId}/billing-details", new UmfaShopDashboardBillingDetailsRequest { History = history });

            return JsonSerializer.Deserialize<List<UmfaShopDashboardBillingDetail>>(response);

        }

        public async Task<List<UmfaShopDashboardOccupation>> GetShopDashboardOccupationsAsync(int buildingId, int shopId)
        {
            var response = await GetAsync($"dashboard/buildings/{buildingId}/shops/{shopId}/occupations");

            return JsonSerializer.Deserialize<List<UmfaShopDashboardOccupation>>(response);
        }

        public async Task<List<UmfaShopDashboardAssignedMeter>> GetShopDashboardAssignedMetersAsync(int buildingId, int shopId, int history)
        {
            var response = await GetAsync($"dashboard/buildings/{buildingId}/shops/{shopId}/assigned-meters", new UmfaShopDashboardAssignedMetersRequest { History = history });

            return JsonSerializer.Deserialize<List<UmfaShopDashboardAssignedMeter>>(response);
        }
    }
}
