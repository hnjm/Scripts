USE [ExciteRollTime]
GO
/****** Object:  StoredProcedure [dbo].[sp_Report_NewUsers_ByBranch_Count]    Script Date: 21/07/2021 10:27:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<RcBuilder>
-- Create date: <2021-07-07>
-- sp_Report_NewUsers_ByBranch_Count '2021-04-28', '2021-04-28', N'רולנט'
-- sp_Report_NewUsers_ByBranch_Count @CompanyName=N'רולנט'
-- =============================================
ALTER PROCEDURE [dbo].[sp_Report_NewUsers_ByBranch_Count]	
	@OpeningDateFrom DATE = NULL,  
	@OpeningDateTo DATE = NULL,
	@CompanyName NVARCHAR(50) = ''
AS
BEGIN	
	SET NOCOUNT ON;

	-- TIME FILTER -- 
	DECLARE @addTimeFilter bit = 0;
	if(@OpeningDateFrom IS NOT NULL AND @OpeningDateTo IS NOT NULL)
		set @addTimeFilter = 1;
	
	if(@addTimeFilter = 1) 
	BEGIN
		-- fix dates -- 
		set @OpeningDateFrom = CAST((CONVERT(VARCHAR, @OpeningDateFrom, 101) + ' 00:00:00') AS DATE)
		set @OpeningDateTo = CAST((CONVERT(VARCHAR, @OpeningDateTo, 101) + ' 23:59:00') AS DATE)
	END	
	
	SELECT	COUNT(DISTINCT C.ClientUniqueId) AS CNT_Users, 
			SUM(IIF(IsPromotionApproved = 1, 1, 0)) AS CNT_Approved,  
			SUM(IIF(IsPromotionApproved = 0, 1, 0)) AS CNT_NotApproved,  
			C.OpeningBranchNo, 
			C.OpeningBranchName, 
			S.CompanyName,
			CONCAT(ROUND((CAST(SUM(IIF(IsPromotionApproved = 1, 1, 0)) AS FLOAT) / COUNT(DISTINCT C.ClientUniqueId) * 100), 0), '%') AS 'P_Approved'
	FROM	[dbo].[Clients] C WITH(NOLOCK)			
			INNER JOIN 
			[dbo].[Stores] S WITH(NOLOCK)
			ON(S.BranchNo = C.OpeningBranchNo)
	WHERE	(@addTimeFilter = 0 OR [OpeningDate] between @OpeningDateFrom and @OpeningDateTo)
	AND		(@CompanyName = '' OR S.CompanyName = @CompanyName)
	GROUP BY OpeningBranchNo, OpeningBranchName, CompanyName
	ORDER BY CompanyName, OpeningBranchNo
END
