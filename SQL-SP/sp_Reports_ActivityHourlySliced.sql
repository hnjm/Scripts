USE [DelivIT]
GO
/****** Object:  StoredProcedure [dbo].[sp_Reports_ActivityHourlySliced]    Script Date: 08/06/2018 09:06:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<RcBuilder>
-- Create date: <04-07-2017>
-- Description:	<REPORT: activity sliced by the hour>
-- sp_Reports_ActivityHourlySliced '2017-06-01', '2017-06-01'
-- =============================================
ALTER PROCEDURE [dbo].[sp_Reports_ActivityHourlySliced]
	@FromTime smalldatetime = null,
    @ToTime smalldatetime = null	
AS
BEGIN	
	SET NOCOUNT ON;

	-- TIME FILTER -- 
	declare @addTimeFilter bit = 0;
	if(@FromTime is not null AND @ToTime is not null)
		set @addTimeFilter = 1;
	
	if(@addTimeFilter = 1) 
	BEGIN
		-- fix dates -- 
		set @FromTime = cast((convert(varchar, @FromTime, 101) + ' 00:00:00') as smalldatetime)
		set @ToTime = cast((convert(varchar, @ToTime, 101) + ' 23:59:00') as smalldatetime)
	END		

	-- deliveries --
	declare @tblDeliveries table(Id int, EmployeeId varchar(15), ProviderId varchar(15), ProviderType varchar(10), CreatedDate smalldatetime, SDate varchar(20), [Hour] int, SHour varchar(15)) 
	insert into @tblDeliveries
		select	D.Id, D.EmployeeId, 
		case 
			when F.RestaurantId is not null then F.RestaurantId
			when P.ProviderId is not null then P.ProviderId 
		end, 
		case 
			when F.RestaurantId is not null then 'Restaurant'
			when P.ProviderId is not null then 'Packages'
		end, 
		D.CreatedDate,		
		LEFT(CONVERT(varchar, D.CreatedDate, 120), 10), -- 2017-06-01 08:05:20 -> 2017-06-01 --			
		DATEPART(HOUR, D.CreatedDate),
		CAST(DATEPART(HOUR, D.CreatedDate) as varchar(2)) + ':00-' + CAST(DATEPART(HOUR, D.CreatedDate) + 1 as varchar(2)) + ':00'
		from	[dbo].[Deliveries] D 				
				inner join [dbo].[Employees] E on D.EmployeeId = E.Id								
				left join [dbo].[Food] F on F.DeliveryId = D.Id 
				left join [dbo].[Packages] P on P.DeliveryId = D.Id 
		where   D.IsDeleted = 0
		and		(@addTimeFilter = 0 OR D.CreatedDate between @FromTime and @ToTime)
		
	-- 1st resultSet: report rows --
	select * from @tblDeliveries

	-- 2nd resultSet: slices --
	select	COUNT(*),
			SDate, [Hour], SHour
	from	@tblDeliveries
	group by SDate, [Hour], SHour
	order by [Hour]

END
