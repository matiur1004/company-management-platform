﻿using ClientPortal.Controllers.Authorization;
using ClientPortal.Data;
using ClientPortal.Data.Entities.PortalEntities;
using ClientPortal.Services;
using ServiceStack;
using System.Dynamic;

namespace ClientPortal.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class TriggeredAlarmNotificationsController : ControllerBase
    {
        private readonly PortalDBContext _context;
        private readonly IUserService _userService;

        public TriggeredAlarmNotificationsController(PortalDBContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // GET: TriggeredAlarmNotifications
        [HttpGet("getTriggeredAlarmNotifications")]
        public async Task<ActionResult<IEnumerable<TriggeredAlarmNotification>>> GetTriggeredAlarmNotifications()
        {
            if (_context.TriggeredAlarmNotifications == null)
            {
                return NotFound();
            }
            return await _context.TriggeredAlarmNotifications.ToListAsync();
        }

        // GET: TriggeredAlarmNotifications/5
        [HttpGet("getTriggeredAlarmNotification/{id}")]
        public async Task<ActionResult<TriggeredAlarmNotification>> GetTriggeredAlarmNotification(int id)
        {
            if (_context.TriggeredAlarmNotifications == null)
            {
                return NotFound();
            }
            var triggeredAlarmNotification = await _context.TriggeredAlarmNotifications.FindAsync(id);

            if (triggeredAlarmNotification == null)
            {
                return NotFound();
            }

            return triggeredAlarmNotification;
        }

        // GET: ActiveTriggeredAlarmNotificationsForUser
        [HttpGet("getActiveTriggeredAlarmNotificationsForUser/{userId}")]
        public async Task<ActionResult<IEnumerable<TriggeredAlarmNotificationResult>>> GetActiveTriggeredAlarmNotificationsForUser(int userId)
        {
            if (_context.TriggeredAlarmNotifications == null)
            {
                return NotFound();
            }
            var results = await _context.TriggeredAlarmNotifications
                .Where(a => (a.Active && a.UserId == userId))
                .ToListAsync();

            var userResults = new List<TriggeredAlarmNotificationResult>();

            foreach (var result in results)
            {
                var userResult = new TriggeredAlarmNotificationResult();

                //Get User
                var user = _userService.GetUserById(result.UserId);
                //Get Status
                var status = GetStatus(result.Status);
                userResult.FirstName = user.FirstName;
                userResult.LastName = user.LastName;
                userResult.NotificationEmailAddress = user.NotificationEmailAddress;
                userResult.Status = status;
                userResult.SendDate = result.SendDateTime;
                userResult.SendStatusMessage = result.SendStatusMessage;
                userResults.Add(userResult);
            }
            return userResults;
        }

        // GET: AllTriggeredAlarmNotificationsForUser
        [HttpGet("getAllTriggeredAlarmNotificationsForUser/{userId}")]
        public async Task<ActionResult<IEnumerable<TriggeredAlarmNotification>>> GetAllTriggeredAlarmNotificationsForUser(int userId)
        {
            if (_context.TriggeredAlarmNotifications == null)
            {
                return NotFound();
            }
            return await _context.TriggeredAlarmNotifications
                .Where(a => (a.UserId == userId))
                .ToListAsync();
        }

        // POST: CreateOrUpdateTriggeredAlarmNotification
        [HttpPost("createOrUpdateTriggeredAlarmNotification")]
        public async Task<ActionResult<TriggeredAlarmNotification>> CreateOrUpdateTriggeredAlarmNotification(TriggeredAlarmNotification triggeredAlarmNotification)
        {
            if (_context.TriggeredAlarmNotifications == null)
            {
                return Problem("Entity set 'PortalDBContext.TriggeredAlarmNotifications'  is null.");
            }

            if (triggeredAlarmNotification.TriggeredAlarmNotificationId > 0)
            {
                // UPDATE
                _context.Entry(triggeredAlarmNotification).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TriggeredAlarmNotificationExists(triggeredAlarmNotification.TriggeredAlarmNotificationId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return NoContent();
            }
            else
            {
                // ADD
                _context.TriggeredAlarmNotifications.Add(triggeredAlarmNotification);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetTriggeredAlarmNotification", new { id = triggeredAlarmNotification.TriggeredAlarmNotificationId }, triggeredAlarmNotification);
            }
        }

        // DELETE: TriggeredAlarmNotifications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTriggeredAlarmNotification(int id)
        {
            if (_context.TriggeredAlarmNotifications == null)
            {
                return NotFound();
            }
            var triggeredAlarmNotification = await _context.TriggeredAlarmNotifications.FindAsync(id);
            if (triggeredAlarmNotification == null)
            {
                return NotFound();
            }

            _context.TriggeredAlarmNotifications.Remove(triggeredAlarmNotification);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("getNotificationsToSend")]
        public ActionResult<IEnumerable<NotificationsSPResult>> GetNotificationsToSend()
        {
            var notifications = new List<NotificationsSPResult>();

            List<dynamic> resultList = new List<dynamic>();
            
            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "spGetNotificationsToSend";
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    _context.Database.OpenConnection();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dynamic result = new ExpandoObject();
                            var dictionary = result as IDictionary<string, object>;

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                dictionary.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader[i]);
                            }
                            resultList.Add(result);
                        }
                    }
                }

                foreach (var result in resultList)
                {
                    var notification = new NotificationsSPResult();

                    //Get User
                    var user = _userService.GetUserById(result.UserId);
                    //var aMRMeterTriggeredAlarmId = result.AMRMeterTriggeredAlarmId;
                    //var amrMeterTriggeredAlarm = _context.AMRMeterTriggeredAlarms
                    //    .Where(a => a.AMRMeterTriggeredAlarmId == aMRMeterTriggeredAlarmId)
                    //    .FirstOrDefaultAsync();
                    notification.User = user;
                    notification.AlarmDescription = result.AlarmDescription;
                    notification.AlarmName = result.NotificationEmailAddress;
                    notification.AMRMeterAlarmId = result.AMRMeterAlarmId;
                    notification.AMRMeterId = result.AMRMeterId;
                    notification.AMRMeterTriggeredAlarmId = result.AMRMeterTriggeredAlarmId;
                    notification.BuildingId = result.BuildingId;
                    notification.BuildingName = result.BuildingName;
                    notification.Description = result.Description;
                    notification.MeterNo = result.MeterNo;
                    notification.MeterSerial = result.MeterSerial;
                    notification.Name = result.Name;
                    notification.NotificationSendTypeId = result.NotificationSendTypeId;
                    notification.OccStartDTM = result.OccStartDTM;
                    notification.UmfaId = result.UmfaId;
                    notification.UserId = result.UserId;

                    notifications.Add(notification);
                }
            }
            catch (Exception)
            {
                return Problem($"Failed to get NotificationsToSend");
            }
            return Ok(notifications);
        }

        private bool TriggeredAlarmNotificationExists(int id)
        {
            return (_context.TriggeredAlarmNotifications?.Any(e => e.TriggeredAlarmNotificationId == id)).GetValueOrDefault();
        }

        public class TriggeredAlarmNotificationResult
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string NotificationEmailAddress { get; set; }
            public string Status { get; set; }
            public DateTime? SendDate { get; set; }
            public string? SendStatusMessage { get; set; }

        }

        public class NotificationsSPResult
        {
            public int AMRMeterTriggeredAlarmId { get; set; }
            public int AMRMeterAlarmId { get; set; }
            public int UserId { get; set; }
            public User User { get; set; }
            public int BuildingId { get; set; }
            public int UmfaId { get; set; }
            public string BuildingName { get; set; }
            public int AMRMeterId { get; set; }
            public string MeterNo { get; set; }
            public string MeterSerial { get; set; }
            public string Description { get; set; }
            public string AlarmName { get; set; }
            public string AlarmDescription { get; set; }
            public DateTime OccStartDTM { get; set; }
            public int NotificationSendTypeId { get; set; }
            public string Name { get; set; }
        }

        private static string GetStatus(int id)
        {
            var status = "";
            switch (id)
            {
                case 1:
                    status = "Not Sent";
                    break;
                case 2:
                    status = "Sent Successfully";
                    break;
                case 3:
                    status = "Error";
                    break;
            }
            return status;
        }
    }
}