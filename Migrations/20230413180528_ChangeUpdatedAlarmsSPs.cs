﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientPortal.Migrations
{
    public partial class ChangeUpdatedAlarmsSPs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE OR ALTER proc [dbo].[spAlarmConfigLeakDetection]\r\n(\r\n@MeterSerialNo nvarchar(250),\r\n@ProfileStartDTM smalldatetime,\r\n@ProfileEndDTM smalldatetime,\r\n@StartTime time,\r\n@EndTime time\r\n)\r\n\r\nAS\r\n\r\n--declare\r\n--@MeterSerialNo nvarchar(250) = '57770290',\r\n--@ProfileStartDTM smalldatetime = '2023-02-06 00:00',\r\n--@ProfileEndDTM smalldatetime = '2023-02-15 23:59',\r\n--@StartTime time = '22:00',\r\n--@EndTime time = '08:00'\r\n\r\ndeclare\r\n@TimeDiff int\r\n\r\ndeclare @tProfile table (Id int identity(1,1), ProfileId int, SerialNo varchar(250), ReadingDate smalldatetime, P1 decimal(18,4))\r\n\r\ninsert into @tProfile\r\nselect\r\np.Id, p.SerialNumber, p.ReadingDate, p.P1\r\nfrom\r\nScadaProfileData p (NOLOCK)\r\nwhere\r\np.SerialNumber = @MeterSerialNo\r\nand p.IsActive = 1\r\nand p.ReadingDate between @ProfileStartDTM and @ProfileEndDTM\r\norder by p.ReadingDate\r\n\r\nSELECT @TimeDiff =  DATEDIFF(MI, CONCAT('2023-04-01 ', @StartTime), CONCAT(case when @EndTime < @StartTime then '2023-04-02 ' else '2023-04-01 ' end, @EndTime)                    )\r\n\r\ndeclare @tData table (Id int identity(1,1), ReadingDate smalldatetime, ActFlow decimal(18,4), Calculated bit)\r\n\r\ninsert into @tData\r\nselect\r\nconvert(varchar(16), d.ReadingDate, 120) as ReadingDate,\r\nActFlow = d.P1,\r\nCalculated = case when d.Status = 0 then 0 else 1 end\r\nfrom\r\nScadaProfileData d (NOLOCK)\r\nwhere\r\nd.SerialNumber = @MeterSerialNo\r\nand d.ReadingDate >= @ProfileStartDTM and d.ReadingDate <= @ProfileEndDTM\r\nand d.isActive = 1\r\norder by\r\nd.ReadingDate\r\n\r\nselect ReadingDate, ActFlow as ActFlow, Calculated\r\n,case when @EndTime < @StartTime and (\r\n\t( cast(ReadingDate as time(0)) >= @StartTime and cast(ReadingDate as time(0)) < cast('1900-01-01 23:59' as time(0)) ) or\r\n\t( cast(ReadingDate as time(0)) >= cast('1900-01-01 00:00' as time(0)) and cast(ReadingDate as time(0)) < @EndTime )) then '#006600'\r\nwhen @EndTime >= @StartTime and (\r\n\t( cast(ReadingDate as time(0)) between @StartTime and @EndTime )) then '#006600'\r\nelse '#00cc00' end as Color\r\nfrom @tData\r\n\r\nselect\r\nb.SerialNo as MeterSerial, MIN(b.ReadingStart) as PeriodStartDTM, MAX(b.ReadingEnd) as PeriodEndDTM\r\n, AVG(b.ProfileAverage) as IntervalAvg, AVG(b.NFTotal) as NFAvg, MAX(b.NFPeak) as NFPeak, MIN(b.NFMin) as NFMin\r\nfrom\r\n(select\r\na.Id,\r\np.SerialNo,\r\na.StartDTM as ReadingStart,\r\na.EndDTM as ReadingEnd,\r\nAVG(p.P1) as ProfileAverage,\r\nSUM(p.P1) as NFTotal,\r\nMAX(p.P1) as NFPeak,\r\nMIN(p.P1) as NFMin\r\nfrom\r\n@tProfile p\r\njoin\r\n(select\r\nId, ProfileId, SerialNo, ReadingDate as StartDTM, DATEADD(MI, @TimeDiff, ReadingDate) as EndDTM\r\nfrom @tProfile\r\nwhere\r\nDATEPART(HH, ReadingDate) = DATEPART(HH, @StartTime)\r\nand DATEPART(MI, ReadingDate) = DATEPART(MI, @StartTime)\r\n) a on (p.SerialNo = a.SerialNo and p.ReadingDate between a.StartDTM and a.EndDTM)\r\ngroup by\r\na.Id, a.StartDTM, a.EndDTM, p.SerialNo\r\n) b\r\ngroup by b.SerialNo");
            migrationBuilder.Sql("CREATE OR ALTER proc [dbo].[spAlarmConfigAvgUsage]\r\n(\r\n@MeterSerialNo nvarchar(250),\r\n@ProfileStartDTM smalldatetime,\r\n@ProfileEndDTM smalldatetime,\r\n@AvgStartTime time,\r\n@AvgEndTime time\r\n)\r\n\r\nAS\r\n\r\n--declare\r\n--@MeterSerialNo nvarchar(250) = '57770290',\r\n--@ProfileStartDTM smalldatetime = '2023-02-01 00:00',\r\n--@ProfileEndDTM smalldatetime = '2023-02-28 23:59',\r\n--@AvgStartTime time = '01:00',\r\n--@AvgEndTime time = '05:00'\r\n\r\ndeclare\r\n@TimeDiff int\r\n\r\ndeclare @tProfile table (Id int identity(1,1), ProfileId int, SerialNo varchar(250), ReadingDate smalldatetime, P1 decimal(18,4))\r\n\r\ninsert into @tProfile\r\nselect\r\np.Id, p.SerialNumber, p.ReadingDate, p.P1\r\nfrom\r\nScadaProfileData p (NOLOCK)\r\nwhere\r\np.SerialNumber = @MeterSerialNo\r\nand p.IsActive = 1\r\nand p.ReadingDate between @ProfileStartDTM and @ProfileEndDTM\r\norder by p.ReadingDate\r\n\r\nSELECT @TimeDiff =  DATEDIFF(MI, CONCAT('2023-04-01 ', @AvgStartTime), CONCAT(case when @AvgEndTime < @AvgStartTime then '2023-04-02 ' else '2023-04-01 ' end, @AvgEndTime)                    )\r\n\r\ndeclare @tData table (Id int identity(1,1), ReadingDate smalldatetime, ActFlow decimal(18,4), Calculated bit)\r\n\r\ninsert into @tData\r\nselect\r\nconvert(varchar(16), d.ReadingDate, 120) as ReadingDate,\r\nActFlow = d.P1,\r\nCalculated = case when d.Status = 0 then 0 else 1 end\r\nfrom\r\nScadaProfileData d (NOLOCK)\r\nwhere\r\nd.SerialNumber = @MeterSerialNo\r\nand d.ReadingDate >= @ProfileStartDTM and d.ReadingDate <= @ProfileEndDTM\r\nand d.isActive = 1\r\norder by\r\nd.ReadingDate\r\n\r\nselect ReadingDate, ActFlow as ActFlow, Calculated\r\n,case when @AvgEndTime < @AvgStartTime and (\r\n\t( cast(ReadingDate as time(0)) >= @AvgStartTime and cast(ReadingDate as time(0)) < cast('1900-01-01 23:59' as time(0)) ) or\r\n\t( cast(ReadingDate as time(0)) >= cast('1900-01-01 00:00' as time(0)) and cast(ReadingDate as time(0)) < @AvgEndTime )) then '#FF6600'\r\nwhen @AvgEndTime >= @AvgStartTime and (\r\n\t( cast(ReadingDate as time(0)) between @AvgStartTime and @AvgEndTime )) then '#FF6600'\r\nelse '#00cc00' end as Color\r\nfrom @tData\r\n\r\nselect\r\nb.SerialNo as MeterSerial, MIN(b.ReadingStart) as PeriodStartDTM, MAX(b.ReadingEnd) as PeriodEndDTM\r\n, AVG(b.ProfileAverage) as IntervalAvg, AVG(b.AvgTotal) as PeriodAvg, MAX(b.AvgPeak) as AvgPeak, MIN(b.AvgMin) as AvgMin\r\nfrom\r\n(\r\nselect\r\na.Id,\r\np.SerialNo,\r\na.StartDTM as ReadingStart,\r\na.EndDTM as ReadingEnd,\r\nAVG(p.P1) as ProfileAverage,\r\nSUM(p.P1/2) as AvgTotal,\r\nMAX(p.P1) as AvgPeak,\r\nMIN(p.P1) as AvgMin\r\nfrom\r\n@tProfile p\r\njoin\r\n(\r\nselect\r\nId, ProfileId, SerialNo, ReadingDate as StartDTM, DATEADD(MI, @TimeDiff, ReadingDate) as EndDTM\r\nfrom @tProfile\r\nwhere\r\nDATEPART(HH, ReadingDate) = DATEPART(HH, @AvgStartTime)\r\nand DATEPART(MI, ReadingDate) = DATEPART(MI, @AvgStartTime)\r\n) a on (p.SerialNo = a.SerialNo and p.ReadingDate between a.StartDTM and a.EndDTM)\r\ngroup by\r\na.Id, a.StartDTM, a.EndDTM, p.SerialNo\r\n) b\r\ngroup by b.SerialNo");
            migrationBuilder.Sql("CREATE OR ALTER proc [dbo].[spAlarmConfigNightFlow]\r\n(\r\n@MeterSerialNo nvarchar(250),\r\n@ProfileStartDTM smalldatetime,\r\n@ProfileEndDTM smalldatetime,\r\n@NFStartTime time,\r\n@NFEndTime time\r\n)\r\n\r\nAS\r\n\r\n--declare\r\n--@MeterSerialNo nvarchar(250) = '57770290',\r\n--@ProfileStartDTM smalldatetime = '2023-02-01 00:00',\r\n--@ProfileEndDTM smalldatetime = '2023-02-28 23:59',\r\n--@NFStartTime time = '02:00',\r\n--@NFEndTime time = '05:00'\r\n\r\ndeclare\r\n@TimeDiff int\r\n\r\ndeclare @tProfile table (Id int identity(1,1), ProfileId int, SerialNo varchar(250), ReadingDate smalldatetime, P1 decimal(18,4))\r\n\r\ninsert into @tProfile\r\nselect\r\np.Id, p.SerialNumber, p.ReadingDate, p.P1\r\nfrom\r\nScadaProfileData p (NOLOCK)\r\nwhere\r\np.SerialNumber = @MeterSerialNo\r\nand p.IsActive = 1\r\nand p.ReadingDate between @ProfileStartDTM and @ProfileEndDTM\r\norder by p.ReadingDate\r\n\r\nSELECT @TimeDiff =  DATEDIFF(MI, CONCAT('2023-04-01 ', @NFStartTime), CONCAT(case when @NFEndTime < @NFStartTime then '2023-04-02 ' else '2023-04-01 ' end, @NFEndTime)                    )\r\n\r\ndeclare @tData table (Id int identity(1,1), ReadingDate smalldatetime, ActFlow decimal(18,4), Calculated bit)\r\n\r\ninsert into @tData\r\nselect\r\nconvert(varchar(16), d.ReadingDate, 120) as ReadingDate,\r\nActFlow = d.P1,\r\nCalculated = case when d.Status = 0 then 0 else 1 end\r\nfrom\r\nScadaProfileData d (NOLOCK)\r\nwhere\r\nd.SerialNumber = @MeterSerialNo\r\nand d.ReadingDate >= @ProfileStartDTM and d.ReadingDate <= @ProfileEndDTM\r\nand d.isActive = 1\r\norder by\r\nd.ReadingDate\r\n\r\nselect ReadingDate, ActFlow as ActFlow, Calculated\r\n,case when @NFEndTime < @NFStartTime and (\r\n\t( cast(ReadingDate as time(0)) >= @NFStartTime and cast(ReadingDate as time(0)) < cast('1900-01-01 23:59' as time(0)) ) or\r\n\t( cast(ReadingDate as time(0)) >= cast('1900-01-01 00:00' as time(0)) and cast(ReadingDate as time(0)) < @NFEndTime )) then '#4949d0'\r\nwhen @NFEndTime >= @NFStartTime and (\r\n\t( cast(ReadingDate as time(0)) between @NFStartTime and @NFEndTime )) then '#4949d0'\r\nelse '#00cc00' end as Color\r\nfrom @tData\r\n\r\nselect\r\nb.SerialNo as MeterSerial, MIN(b.ReadingStart) as PeriodStartDTM, MAX(b.ReadingEnd) as PeriodEndDTM\r\n, AVG(b.ProfileAverage) as IntervalAvg, AVG(b.NFTotal) as NFAvg, MAX(b.NFPeak) as NFPeak, MIN(b.NFMin) as NFMin\r\nfrom\r\n(select\r\na.Id,\r\np.SerialNo,\r\na.StartDTM as ReadingStart,\r\na.EndDTM as ReadingEnd,\r\nAVG(p.P1) as ProfileAverage,\r\nSUM(p.P1) as NFTotal,\r\nMAX(p.P1) as NFPeak,\r\nMIN(p.P1) as NFMin\r\nfrom\r\n@tProfile p\r\njoin\r\n(select\r\nId, ProfileId, SerialNo, ReadingDate as StartDTM, DATEADD(MI, @TimeDiff, ReadingDate) as EndDTM\r\nfrom @tProfile\r\nwhere\r\nDATEPART(HH, ReadingDate) = DATEPART(HH, @NFStartTime)\r\nand DATEPART(MI, ReadingDate) = DATEPART(MI, @NFStartTime)\r\n) a on (p.SerialNo = a.SerialNo and p.ReadingDate between a.StartDTM and a.EndDTM)\r\ngroup by\r\na.Id, a.StartDTM, a.EndDTM, p.SerialNo\r\n) b\r\ngroup by b.SerialNo");
            migrationBuilder.Sql("CREATE OR ALTER proc [dbo].[spAlarmConfigPeakUsage]\r\n(\r\n@MeterSerialNo nvarchar(250),\r\n@ProfileStartDTM smalldatetime,\r\n@ProfileEndDTM smalldatetime,\r\n@PeakStartTime time,\r\n@PeakEndTime time,\r\n@NoOfPeaks int = 5\r\n)\r\n\r\nAS\r\n\r\n--declare\r\n--@MeterSerialNo nvarchar(250) = '57770290',\r\n--@ProfileStartDTM smalldatetime = '2023-02-01 00:00',\r\n--@ProfileEndDTM smalldatetime = '2023-02-28 23:59',\r\n--@PeakStartTime time = '22:00',\r\n--@PeakEndTime time = '05:00',\r\n--@NoOfPeaks int = 5\r\n\r\ndeclare\r\n@TimeDiff int\r\n\r\ndeclare @tProfile table (Id int identity(1,1), ProfileId int, SerialNo varchar(250), ReadingDate smalldatetime, P1 decimal(18,4))\r\n\r\ninsert into @tProfile\r\nselect\r\np.Id, p.SerialNumber, p.ReadingDate, p.P1\r\nfrom\r\nScadaProfileData p (NOLOCK)\r\nwhere\r\np.SerialNumber = @MeterSerialNo\r\nand p.IsActive = 1\r\nand p.ReadingDate between @ProfileStartDTM and @ProfileEndDTM\r\norder by p.ReadingDate\r\n\r\nSELECT @TimeDiff =  DATEDIFF(MI, CONCAT('2023-04-01 ', @PeakStartTime), CONCAT(case when @PeakEndTime < @PeakStartTime then '2023-04-02 ' else '2023-04-01 ' end, @PeakEndTime)                    )\r\n\r\ndeclare @tData table (Id int identity(1,1), ReadingDate smalldatetime, ActFlow decimal(18,4), Calculated bit)\r\n\r\ninsert into @tData\r\nselect\r\nconvert(varchar(16), d.ReadingDate, 120) as ReadingDate,\r\nActFlow = d.P1,\r\nCalculated = case when d.Status = 0 then 0 else 1 end\r\nfrom\r\nScadaProfileData d (NOLOCK)\r\nwhere\r\nd.SerialNumber = @MeterSerialNo\r\nand d.ReadingDate >= @ProfileStartDTM and d.ReadingDate <= @ProfileEndDTM\r\nand d.isActive = 1\r\norder by\r\nd.ReadingDate\r\n\r\nselect ReadingDate, ActFlow as ActFlow, Calculated\r\n,case when @PeakEndTime < @PeakStartTime and (\r\n\t( cast(ReadingDate as time(0)) >= @PeakStartTime and cast(ReadingDate as time(0)) < cast('1900-01-01 23:59' as time(0)) ) or\r\n\t( cast(ReadingDate as time(0)) >= cast('1900-01-01 00:00' as time(0)) and cast(ReadingDate as time(0)) < @PeakEndTime )) then '#6800A2'\r\nwhen @PeakEndTime >= @PeakStartTime and (\r\n\t( cast(ReadingDate as time(0)) between @PeakStartTime and @PeakEndTime )) then '#6800A2'\r\nelse '#00cc00' end as Color\r\nfrom @tData\r\n\r\nselect top (@NoOfPeaks)\r\nb.SerialNo as MeterSerial\r\n, (select min(ReadingDate) from @tProfile where ReadingDate between b.ReadingStart and b.ReadingEnd and P1 = Peak) as OccDTM\r\n, b.Peak as Peak\r\nfrom\r\n(\r\nselect\r\na.Id,\r\np.SerialNo,\r\na.StartDTM as ReadingStart,\r\na.EndDTM as ReadingEnd,\r\nMAX(p.P1) as Peak\r\nfrom\r\n@tProfile p\r\njoin\r\n(select\r\nId, ProfileId, SerialNo, ReadingDate as StartDTM, DATEADD(MI, @TimeDiff, ReadingDate) as EndDTM\r\nfrom @tProfile\r\nwhere\r\nDATEPART(HH, ReadingDate) = DATEPART(HH, @PeakStartTime)\r\nand DATEPART(MI, ReadingDate) = DATEPART(MI, @PeakStartTime)\r\n) a on (p.SerialNo = a.SerialNo and p.ReadingDate between a.StartDTM and a.EndDTM)\r\ngroup by\r\na.Id, a.StartDTM, a.EndDTM, p.SerialNo\r\n) b");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spAlarmConfigLeakDetection]");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spAlarmConfigAvgUsage]");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spAlarmConfigNightFlow]");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[spAlarmConfigPeakUsage]");
        }
    }
}
