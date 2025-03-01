USE [MNew]
GO
/****** Object:  StoredProcedure [dbo].[sp_Restaurant_get]    Script Date: 10/03/2021 09:28:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<RcBuilder>
-- Create date: <2021-03-07>
-- sp_Restaurant_Get 1
-- =============================================
ALTER PROCEDURE [dbo].[sp_Restaurant_get]
	@Id INT
AS
BEGIN
	SET NOCOUNT ON;

	-- 1st table: details --
	EXEC sp_Restaurant_Details_get @Id

    -- 2nd table: gallery --
	EXEC sp_Restaurant_Gallery_get @Id
    
    -- 3th table: address --
	DECLARE @AddressId INT = (SELECT AddressId FROM Restaurants WHERE Id = @Id)
	EXEC sp_Address_get @AddressId
    
    -- 4rd table: categories --
	EXEC sp_Restaurant_Categories_get @Id
    
    -- 5th table: working hours --
    EXEC sp_Restaurant_WorkingHours_get @Id

    -- 6th table: menu --
    EXEC sp_Restaurant_Menu_get @Id

    -- 7th table: deals + deals-items map --	
	EXEC sp_Restaurant_Deals_get @Id
    
END
