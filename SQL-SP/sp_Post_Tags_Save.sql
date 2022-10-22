USE [MemorialDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<RcBuilder>
-- sp_Post_Tags_Save 1, 'Friends, Family, Colleagues'
-- =============================================
ALTER PROCEDURE [dbo].[sp_Post_Tags_Save]	
	@Id INT,    
    @Tags NVARCHAR(MAX) -- splitted by ','   
AS
BEGIN	
	SET NOCOUNT ON;    
	
	BEGIN TRANSACTION
	BEGIN TRY
	
		DELETE FROM PostsTags WHERE PostId = @Id
		INSERT INTO PostsTags 
			SELECT @Id, RTRIM(LTRIM([Value])) FROM STRING_SPLIT(@Tags, ',')
		
		COMMIT TRANSACTION
	END TRY
	BEGIN CATCH		 
		PRINT(ERROR_MESSAGE())	     		
		ROLLBACK TRANSACTION 
	END CATCH 
END