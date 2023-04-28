﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPortal.Migrations
{
    public partial class AlterspGetUserAlarmNotificationConfigSP : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_userAMRMeterActiveNotifications",
                table: "userAMRMeterActiveNotifications");

            migrationBuilder.RenameTable(
                name: "userAMRMeterActiveNotifications",
                newName: "UserAMRMeterActiveNotifications");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAMRMeterActiveNotifications",
                table: "UserAMRMeterActiveNotifications",
                column: "Id");
            migrationBuilder.Sql("CREATE OR ALTER proc [dbo].[spGetUserAlarmNotificationConfig]\r\n(\r\n@UserId INT,\r\n@BuildingID int\r\n)\r\n\r\nAS\r\n\r\n--DECLARE\r\n--@UserId INT = 2,\r\n--@BuildingID int = 2983\r\n\r\nDECLARE @TempTable TABLE (\r\n    AMRMeterId INT,\r\n    MeterNo VARCHAR(50),\r\n    Description VARCHAR(250),\r\n    ScadaMeterNo VARCHAR(50),\r\n\t[Night Flow] int,\r\n\t[Burst Pipe] int,\r\n\t[Leak] int,\r\n\t[Daily Usage] int,\r\n\t[Peak] int,\r\n\t[Average] int\r\n);\r\n\r\nDECLARE @AlarmNames NVARCHAR(MAX), @SelectNames nvarchar(max), @PivotQuery NVARCHAR(MAX);\r\n\r\nSELECT @AlarmNames = COALESCE(@AlarmNames + ', ', '') + QUOTENAME(t.AlarmName)\r\nFROM AlarmTypes t (NOLOCK);\r\n\r\nSELECT @SelectNames = COALESCE(@SelectNames + ', ', '') + 'ISNULL(' + QUOTENAME(AlarmName) + ', 0)' + ' as ' + QUOTENAME(AlarmName)\r\nFROM AlarmTypes t (NOLOCK);\r\n\r\nSET @PivotQuery = '\r\nSELECT\r\n    AMRMeterId,\r\n    MeterNo,\r\n    Description,\r\n    MeterSerial AS ScadaMeterNo,\r\n    ' + @SelectNames + '\r\nFROM (\r\n    SELECT\r\n        m.Id as AMRMeterId, m.MeterNo, m.MeterSerial, m.Description,\r\n        --ma.AMRMeterAlarmId, ma.AlarmTypeId,\r\n\t\tt.AlarmName,\r\n        --un.Id, un.NotificationTypeId,\r\n        CASE\r\n            WHEN max(ma.AMRMeterAlarmId) IS NOT NULL AND max(un.Id) IS NOT NULL THEN\r\n                SUM(CAST(un.Email AS INT) + CAST(un.Telegram AS INT) + CAST(un.WhatsApp AS INT))\r\n            ELSE 0\r\n        END AS Configured\r\n    FROM\r\n        AMRMeters m\r\n        LEFT JOIN AMRMeterAlarms ma (NOLOCK) ON (m.Id = ma.AMRMeterId AND ma.Active = 1)\r\n        LEFT JOIN AlarmTypes t (NOLOCK) ON (ma.AlarmTypeId = t.AlarmTypeId)\r\n        LEFT JOIN UserNotifications un (NOLOCK) ON (ma.AlarmTypeId = un.NotificationTypeId AND un.UserId = ' + CONVERT(VARCHAR, @UserId) + ')\r\n\t\twhere m.BuildingId = ' + convert(nvarchar, @BuildingID) + '\r\n    GROUP BY\r\n        m.Id , m.MeterNo, m.MeterSerial, m.Description, t.AlarmName\r\n) p\r\nPIVOT (\r\n    SUM(Configured)\r\n    FOR AlarmName IN (' + @AlarmNames + ')\r\n) AS PivotTable;';\r\n\r\ninsert into @TempTable\r\nEXECUTE sp_executesql @PivotQuery;\r\n\r\n;with cteUserAlarms as (\r\nselect\r\nm.AMRMeterAlarmId, ma.AMRMeterId, ma.AlarmTypeId\r\nfrom\r\nUserAMRMeterActiveNotifications m\r\njoin AMRMeterAlarms ma (NOLOCK) on (m.AMRMeterAlarmId = ma.AMRMeterAlarmId)\r\nwhere m.Enabled = 1 and m.Active = 1\r\n)\r\n\r\nSELECT\r\nt.AMRMeterId, t.MeterNo, t.Description, t.ScadaMeterNo\r\n, case when [Night Flow] = 1 then case when a1.AMRMeterAlarmId is null then 1 else 2 end else 0 end as  [Night Flow]\r\n, case when [Burst Pipe] = 1 then case when a2.AMRMeterAlarmId is null then 1 else 2 end else 0 end as  [Burst Pipe]\r\n, case when [Leak] = 1 then case when a3.AMRMeterAlarmId is null then 1 else 2 end else 0 end as  [Leak]\r\n, case when [Daily Usage] = 1 then case when a4.AMRMeterAlarmId is null then 1 else 2 end else 0 end as  [Daily Usage]\r\n, case when [Peak] = 1 then case when a5.AMRMeterAlarmId is null then 1 else 2 end else 0 end as  [Peak]\r\n, case when [Average] = 1 then case when a6.AMRMeterAlarmId is null then 1 else 2 end else 0 end as  [Average]\r\nFROM\r\n@TempTable t\r\nleft join cteUserAlarms a1 on (t.AMRMeterId = a1.AMRMeterId and a1.AlarmTypeId = 1)\r\nleft join cteUserAlarms a2 on (t.AMRMeterId = a2.AMRMeterId and a2.AlarmTypeId = 2)\r\nleft join cteUserAlarms a3 on (t.AMRMeterId = a3.AMRMeterId and a3.AlarmTypeId = 3)\r\nleft join cteUserAlarms a4 on (t.AMRMeterId = a4.AMRMeterId and a4.AlarmTypeId = 4)\r\nleft join cteUserAlarms a5 on (t.AMRMeterId = a5.AMRMeterId and a5.AlarmTypeId = 5)\r\nleft join cteUserAlarms a6 on (t.AMRMeterId = a6.AMRMeterId and a6.AlarmTypeId = 6)\r\n");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAMRMeterActiveNotifications",
                table: "UserAMRMeterActiveNotifications");

            migrationBuilder.RenameTable(
                name: "UserAMRMeterActiveNotifications",
                newName: "userAMRMeterActiveNotifications");

            migrationBuilder.AddPrimaryKey(
                name: "PK_userAMRMeterActiveNotifications",
                table: "userAMRMeterActiveNotifications",
                column: "Id");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spGetUserAlarmNotificationConfig]");
        }
    }
}
