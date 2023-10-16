﻿using ClientPortal.Controllers.Authorization;
using ClientPortal.Models.RequestModels;
using ClientPortal.Models.ResponseModels;
using ClientPortal.Services;

namespace ClientPortal.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class TenantDashboardController : Controller
    {
        private readonly ILogger<TenantDashboardController> _logger;
        private readonly IUmfaService _umfaService;

        public TenantDashboardController(ILogger<TenantDashboardController> logger, IUmfaService umfaReportService)
        {
            _logger = logger;
            _umfaService = umfaReportService;
        }

        [HttpGet("tenants")]
        public async Task<ActionResult<List<UmfaTenantDashboardTenant>>> Get([FromQuery] UmfaTenantDashboardTenantListRequest request)
        {
            try
            {
                return await _umfaService.GetTenantDashboardTenantsAsync(request);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error getting tenant dashboard tenants {e.Message}");
                return Problem("Could not return tenants");
            }
        }

        [HttpGet("main")]
        public async Task<ActionResult<UmfaTenantMainDashboardResponse>> Get([FromQuery] UmfaTenantMainDashboardRequest request)
        {
            try
            {
                return await _umfaService.GetTenantMainDashboardAsync(request);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error getting tenant main dashboard {e.Message}");
                return Problem("Could not return tenant main dashboard");
            }
        }

        [HttpGet("main/billing-details")]
        public async Task<ActionResult<List<UmfaTenantMainDashboardBillingDetail>>> GetTenantMainDashboardBillingDetails([FromQuery] UmfaTenantMainDashboardBillingDetailsRequest request)
        {
            try
            {
                return await _umfaService.GetTenantMainDashboardBillingDetailsAsync(request);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not get tenant main dashboard billing details");
                return Problem(e.Message);
            }
        }

        [HttpGet("billing-card-details")]
        public async Task<ActionResult<List<UmfaTenantDashboardBillingCardDetail>>> GetTenantDashboardCardBillingDetails([FromQuery] UmfaTenantDashboardBillingCardDetailsRequest request)
        {
            try
            {
                return await _umfaService.GetTenantDashboardBillingCardDetailsAsync(request);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not get tenant main dashboard billing details");
                return Problem(e.Message);
            }
        }
    }
}
