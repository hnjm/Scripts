USE [ExciteRollTime]
GO
/****** Object:  StoredProcedure [dbo].[sp_Report_AgeGroup_Get]    Script Date: 14/12/2021 10:11:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<RcBuilder>
-- Create date: <2021-07-07>
-- [@DateFrom, @DateTo, @CompanyName, @IsPromotionApproved]
-- sp_Report_Gender_Get
-- sp_Report_Gender_Get @CompanyName=N'הפיווטש'
-- sp_Report_Gender_Get '2021-01-01', '2021-06-01'
-- sp_Report_Gender_Get @IsPromotionApproved=1
-- =============================================
ALTER PROCEDURE [dbo].[sp_Report_Gender_Get]	
	@DateFrom DATE = NULL,  
	@DateTo DATE = NULL,
	@CompanyName NVARCHAR(50) = '',
	@IsPromotionApproved BIT = NULL
AS
BEGIN	
	SET NOCOUNT ON;

	-- TIME FILTER -- 
	DECLARE @addTimeFilter bit = 0;
	if(@DateFrom IS NOT NULL AND @DateTo IS NOT NULL)
		set @addTimeFilter = 1;
	
	if(@addTimeFilter = 1) 
	BEGIN
		-- fix dates -- 
		set @DateFrom = CAST((CONVERT(VARCHAR, @DateFrom, 101) + ' 00:00:00') AS DATE)
		set @DateTo = CAST((CONVERT(VARCHAR, @DateTo, 101) + ' 23:59:00') AS DATE)
	END	
	
	PRINT(CONCAT(@DateFrom, ' - ', @DateTo))

	;WITH CTE(AgeGroup, Gender, Branch, Trs_AVG) AS ( 
		SELECT	dbo.fn_GetAgeGroup(dbo.fn_GetAgeFromBirthDate(C.BirthDate)),
				dbo.fn_GetGenderName(C.Gender),
				S.CompanyName,												
				ROUND(SUM(Amount) / COUNT(TransactionNo), 2)
		FROM	[dbo].[SalesLines] L WITH(NOLOCK)						
				INNER JOIN 
				[dbo].[Stores] S WITH(NOLOCK)
				ON(S.BranchNo = L.SaleBranchNo)
				INNER JOIN 
				[dbo].[Clients] C WITH(NOLOCK)
				ON(C.ClientUniqueId = L.ClientUniqueId)
		WHERE	L.CategoryNo NOT IN('99999', '9005', '0')
		AND		(@addTimeFilter = 0 OR [TransactionDate] between @DateFrom and @DateTo)		
		AND		(@CompanyName = '' OR S.CompanyName = @CompanyName)				
		AND		(@IsPromotionApproved IS NULL OR C.IsPromotionApproved = @IsPromotionApproved)
		--AND      C.ClientUniqueId IN ('000_0509522272_רולתיקגרנדקניוןחיפה')
		GROUP BY dbo.fn_GetAgeGroup(dbo.fn_GetAgeFromBirthDate(C.BirthDate)), dbo.fn_GetGenderName(C.Gender), CompanyName
	)
	SELECT * FROM CTE 	
	--SELECT * FROM SalesLines WHERE ClientUniqueId = '000_0509522272_רולתיקגרנדקניוןחיפה' AND (@addTimeFilter = 0 OR [TransactionDate] between @DateFrom and @DateTo)		
END