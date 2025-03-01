USE [MemorialDB]
GO
/****** Object:  StoredProcedure [dbo].[sp_Accounts_Get]    Script Date: 28/10/2022 13:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<RcBuilder>
-- sp_Accounts_Get '000000001,000000002'
-- =============================================
ALTER PROCEDURE [dbo].[sp_Accounts_Get]
	@sIds VARCHAR(MAX),
	-- paging --
	@PageNum int = 1,
	@PageSize tinyint = 20 
AS
BEGIN	
	SET NOCOUNT ON;    

	-- PAGING CALCULATION --
	declare @IndexFrom int = ((@PageNum - 1) * @PageSize) + 1
	declare @IndexTo int = @PageNum * @PageSize
	--------

	DECLARE @tIdsAll TABLE(RowId INT, Id VARCHAR(15))
	;WITH CTE_Ids (RowId, Id) AS (		
		SELECT	ROW_NUMBER() OVER(ORDER BY (SELECT NULL)),
				RTRIM(LTRIM([Value])) 
		FROM	STRING_SPLIT(@sIds, ',')		
	)	
	INSERT INTO @tIdsAll		
		SELECT	RowId, A.Id 
		FROM	[dbo].[Accounts] A WITH(NOLOCK) 
				INNER JOIN CTE_Ids C
				ON(A.Id = C.Id)
		WHERE	IsDeleted = 0  -- remove deleted users

	DECLARE @tIds TABLE(Id VARCHAR(15))		
	INSERT INTO @tIds
		SELECT Id FROM @tIdsAll WHERE RowId BETWEEN @IndexFrom AND @IndexTo
	
	-- 1st table: accounts --
	SELECT	*
	FROM	[dbo].[Accounts] WITH(NOLOCK)	
	WHERE	Id IN (SELECT Id FROM @tIds)	

	-- 2nd table: settings --			
	SELECT	*
	FROM	Settings WITH(NOLOCK) 					
	WHERE	AccountId IN (SELECT Id FROM @tIds)

	-- 3rd table: albums --
	SELECT	* 
	FROM	[dbo].[Albums] WITH(NOLOCK)			
	WHERE	AccountId IN (SELECT Id FROM @tIds)	
	AND		IsDeleted = 0

	-- 4th table: paging --
	DECLARE @RowCount INT = (SELECT COUNT(*) FROM @tIdsAll)
	SELECT	@RowCount As 'RowCount', 
			@PageNum As 'PageNum', 
			@PageSize As 'PageSize', 
			@IndexFrom As 'IndexFrom', 
			@IndexTo As 'IndexTo' 
END