﻿using ClientPortal.Data.Entities.PortalEntities;
using ClientPortal.Data.Repositories;
using ClientPortal.DtOs;
using ClientPortal.Models.RequestModels;
using ClientPortal.Models.ResponseModels;
using System.Text.Json;

namespace ClientPortal.Services
{
    public interface IAMRMeterService
    {
        Task<AMRMeterResponse> AddMeterAsync(AMRMeterUpdateRequest meter);
        Task<AMRMeterResponse> GetMeterAsync(int id);
        Task<AMRMeterResponseList> GetAllMetersForUser(int userId);
        Task<AMRMeterResponseList> GetAllMetersForUserChart(int userId, int chartId, bool isTenant = false);
        Task<AMRMeterResponse> EditMeterAsync(AMRMeterUpdateRequest meter);
        Task<List<UtilityResponse>> GetMakeModels();

        Task<ScadaRequestDetail?> MoveMeterSchedule(MoveMeterScheduleRequest request);

        Task<bool> RunAmrMeterJob(RunAmrMeterJobRequest request);
    }

    public class AMRMeterService : IAMRMeterService
    {
        private readonly ILogger<AMRMeterService> _logger;
        private readonly IAMRMeterRepository _meterRepo;
        private readonly IScadaRequestService _scadaRequestService;
        private readonly IAmrProfileJobsQueueService _amrProfileJobsQueueService;
        private readonly IAmrReadingsJobsQueueService _amrReadingsJobsQueueService;

        public AMRMeterService(ILogger<AMRMeterService> logger, IAMRMeterRepository meterRepo, IScadaRequestService scadaRequestService, IAmrProfileJobsQueueService amrProfileJobsQueueService, IAmrReadingsJobsQueueService amrReadingsJobsQueueService)
        {
            _logger = logger;
            _meterRepo = meterRepo;
            _scadaRequestService = scadaRequestService;
            _amrProfileJobsQueueService = amrProfileJobsQueueService;
            _amrReadingsJobsQueueService = amrReadingsJobsQueueService;
        }

        public async Task<AMRMeterResponse> EditMeterAsync(AMRMeterUpdateRequest updMeter)
        {
            if (updMeter.Meter == null) throw new ApplicationException($"Empty meter object received to update");
            _logger.LogInformation("Attempt to edit meter: {MeterNo}", updMeter.Meter.MeterNo);
            try
            {
                var result = await _meterRepo.Edit(updMeter.Meter, updMeter.UserId);
                if (result.Id > 0)
                {
                    _logger.LogInformation("Meter updated with id: {Id}", result.Id);
                    return result;
                }
                else throw new Exception("Meter not updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating meter {MeterNo}: {Message}", updMeter.Meter.MeterNo, ex.Message);
                throw new ApplicationException($"Error while updating meter {updMeter.Meter.MeterNo}: {ex.Message}");
            }
        }

        public async Task<AMRMeterResponse> AddMeterAsync(AMRMeterUpdateRequest meter)
        {
            _logger.LogInformation("Attempt to add new meter: {MeterNo}", meter.Meter.MeterNo);
            try
            {
                var result = await _meterRepo.AddMeterAsync(meter);
                if (result.Id > 0)
                {
                    _logger.LogInformation("New meter added with id: {Id}", result.Id);
                    return result;
                }
                else throw new Exception("New meter not added successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while add new meter {MeterNo}: {Message}", meter.Meter.MeterNo, ex.Message);
                throw new ApplicationException(ex.Message);
            }
        }

        public async Task<AMRMeterResponseList> GetAllMetersForUser(int userId)
        {
            _logger.LogInformation("Getting meters for user: {userId}", userId);
            try
            {
                var result = await _meterRepo.GetMetersForUserAsync(userId);
                if (result.Message.StartsWith("Error")) throw new Exception($"Meters for user {userId} not found");
                else return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting meters for user: {userId}: {Message}", userId, ex.Message);
                throw new ApplicationException($"Error while getting meters for user: {userId}: {ex.Message}");
            }
        }

        public async Task<AMRMeterResponseList> GetAllMetersForUserChart(int userId, int chartId, bool isTenant = false)
        {
            _logger.LogInformation("Getting meters for user: {userId}", userId);
            try
            {
                var result = await _meterRepo.GetMetersForUserChartAsync(userId, chartId, isTenant);
                if (result.Message.StartsWith("Error")) throw new Exception($"Meters for user {userId} and chart {chartId} not found");
                else return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting meters for user: {userId} and chart: {chartId}: {Message}", userId, chartId, ex.Message);
                throw new ApplicationException($"Error while getting meters for user: {userId} and chart {chartId}: {ex.Message}");
            }
        }

        public async Task<AMRMeterResponse> GetMeterAsync(int id)
        {
            _logger.LogInformation("Getting meter with id: {id}", id);
            try
            {
                var meter = await _meterRepo.GetMeterAsync(id);
                
                if (meter is null) throw new Exception($"Meter for id {id} not found");

                var detailProfile = await _scadaRequestService.GetScadaRequestDetailAsyncByJobTypeAndAmrMeterIdAsync(1, id);
                var detailReading = await _scadaRequestService.GetScadaRequestDetailAsyncByJobTypeAndAmrMeterIdAsync(2, id);

                return new AMRMeterResponse(meter, detailProfile, detailReading);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting meter with id: {id}", id);
                throw new ApplicationException(ex.Message);
            }
        }

        public async Task<List<UtilityResponse>> GetMakeModels()
        {
            _logger.LogInformation("Getting all Make and Models");
            try
            {
                var result = await _meterRepo.GetMakeModels();
                if (result != null) return result;
                else throw new ApplicationException("No results return for Make and Models");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while retrieving makes and models: {Message}", ex.Message);
                throw new ApplicationException($"Error while retrieving makes and models: {ex.Message}");
            }
        }

        public async Task<ScadaRequestDetail?> MoveMeterSchedule(MoveMeterScheduleRequest request)
        {
            var detail = await _scadaRequestService.GetScadaRequestDetailAsyncByJobTypeAndAmrMeterIdAsync((int)request.JobType!, (int)request.MeterId!);

            if (detail == null) return null;

            detail.HeaderId = (int)request.NewScheduleId!;

            return await _scadaRequestService.UpdateScadaRequestDetailAsync(detail);
        }

        public async Task<bool> RunAmrMeterJob(RunAmrMeterJobRequest request)
        {
            var detail = await _scadaRequestService.GetScadaRequestDetailAsyncByJobTypeAndAmrMeterIdAsync((int)request.JobType!, (int)request.MeterId!);

            if(detail is null) return false;

            var jobs = new List<AmrJobToRun>();

            var fromDate = detail.LastDataDate ?? new DateTime(DateTime.UtcNow.Year, 1, 1);

            jobs.Add(new AmrJobToRun
            {
                HeaderId = detail.Header.Id,
                DetailId = detail.Id,
                CommsId = detail.AmrMeter.CommsId,
                Key1 = detail.AmrMeter.MeterSerial,
                SqdUrl = detail.AmrScadaUser.SgdUrl,
                ProfileName = detail.AmrScadaUser.ProfileName,
                ScadaUserName = detail.AmrScadaUser.ScadaUserName,
                ScadaPassword = detail.AmrScadaUser.ScadaPassword,
                JobType = detail.Header.JobType,
                FromDate = fromDate,
                ToDate = fromDate.AddDays(7),
            });

            if(request.JobType == 1)
            {
                await _amrProfileJobsQueueService.AddMessageToQueueAsync(JsonSerializer.Serialize(jobs));
            }
            else if (request.JobType == 2)
            {
                await _amrReadingsJobsQueueService.AddMessageToQueueAsync(JsonSerializer.Serialize(jobs));
            }

            return true;
        }
    }
}
