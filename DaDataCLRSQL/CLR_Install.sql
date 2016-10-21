
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP FUNCTION [dbo].[Klient_getDaDataInfo]
GO

DROP FUNCTION [dbo].[DaDataGetClientInfo]
GO

DROP FUNCTION [dbo].[DaDataGetBankInfo]
GO


DROP FUNCTION [dbo].[DaDataGetAddressInfo]
GO

DROP FUNCTION [dbo].[DaDataGetEmailInfo]
GO

DROP FUNCTION [dbo].[DaDataGetNameInfo]
GO

DROP FUNCTION [dbo].[DaDataCheckToken]
GO

DROP FUNCTION [dbo].[DaDataCheckURL]
GO

DROP FUNCTION dbo.DaDataToken
GO


DROP ASSEMBLY [DaDataCLRSQL]
GO

CREATE ASSEMBLY [DaDataCLRSQL]
AUTHORIZATION [dbo]
FROM 'D:\CLR\DaData\DaDataCLRSQL.dll'
WITH PERMISSION_SET = UNSAFE
GO

CREATE FUNCTION dbo.DaDataCheckToken ()
RETURNS NVARCHAR(200)
AS
	EXTERNAL NAME [DaDataCLRSQL].[SQLCalls.CheckDD].CheckToken

GO

CREATE FUNCTION dbo.DaDataCheckURL ()
RETURNS NVARCHAR(200)
AS
	EXTERNAL NAME [DaDataCLRSQL].[SQLCalls.CheckDD].CheckURL

GO

CREATE FUNCTION dbo.DaDataToken ()
RETURNS NVARCHAR(50)
AS
BEGIN
	RETURN 'PUT_YOUR_TOKEN_HERE'
END

GO

CREATE FUNCTION [dbo].[DaDataGetClientInfo] (
	@ClientQuery	NVARCHAR(500),
	@token			NVARCHAR(50))
RETURNS TABLE (
		orgType NVARCHAR(15),
		orgStatus NVARCHAR(15),
		name NVARCHAR(150),
		nameFull NVARCHAR(300),
		mnemo NVARCHAR(100),
		managName NVARCHAR(100),
		managPost NVARCHAR(100),
		regDate SMALLDATETIME,
		ligDate SMALLDATETIME,
		addr NVARCHAR(200),
		opf NVARCHAR(100),
		inn NVARCHAR(20),
		kpp NVARCHAR(20),
		ogrn NVARCHAR(20),
		okpo NVARCHAR(20),
		region NVARCHAR(100),
		area NVARCHAR(100),
		city NVARCHAR(100),
		settle NVARCHAR(100),
		capitalMarker NVARCHAR(3),
		regkladrid NVARCHAR(50)
	) WITH EXECUTE AS CALLER
AS
	EXTERNAL NAME [DaDataCLRSQL].[SQLCalls.ClientApi].GetClientInfo
GO

CREATE FUNCTION [dbo].[DaDataGetBankInfo] (
	@BankQuery	NVARCHAR(500),
	@token		NVARCHAR(50))
RETURNS TABLE (
		name NVARCHAR(150),
		namePay NVARCHAR(150),
		nameFull NVARCHAR(300),
		opf NVARCHAR(100),
		bic NVARCHAR(30),
		swift NVARCHAR(30),
		okpo NVARCHAR(30),
		korr NVARCHAR(30),
		phone NVARCHAR(30),
		addr NVARCHAR(300),
		region NVARCHAR(100),
		city NVARCHAR(100),
		regkladrid NVARCHAR(100),
		regDate SMALLDATETIME,
		ligDate SMALLDATETIME,
		status NVARCHAR(30)
	) WITH EXECUTE AS CALLER
AS
	EXTERNAL NAME [DaDataCLRSQL].[SQLCalls.BankApi].GetBankInfo
GO


CREATE FUNCTION [dbo].[DaDataGetAddressInfo] (
	@AddrrQuery	NVARCHAR(500),
	@token		NVARCHAR(50))
RETURNS TABLE (
		addr NVARCHAR(200),
		postalCode NVARCHAR(10),
		country NVARCHAR(50),
		region NVARCHAR(100),
		area NVARCHAR(100),
		city NVARCHAR(100),
		settle NVARCHAR(100),
		street NVARCHAR(100),
		house NVARCHAR(10),
		house_type NVARCHAR(50),
		block NVARCHAR(10),
		block_type NVARCHAR(50),
		flat NVARCHAR(10),
		flat_type NVARCHAR(50),
		timezone NVARCHAR(5),
		capitalMarker NVARCHAR(5),
		regkladrid NVARCHAR(50),
		fias_level NVARCHAR(5),
		kladrid NVARCHAR(50)
	) WITH EXECUTE AS CALLER
AS
	EXTERNAL NAME [DaDataCLRSQL].[SQLCalls.AdressApi].GetAdressInfo
GO

CREATE FUNCTION [dbo].[DaDataGetEmailInfo] (
	@EmailQuery	NVARCHAR(500),
	@token		NVARCHAR(50))
RETURNS TABLE (
		email NVARCHAR(50),
		local NVARCHAR(30),
		domain NVARCHAR(20)
	) WITH EXECUTE AS CALLER
AS
	EXTERNAL NAME [DaDataCLRSQL].[SQLCalls.EmailApi].GetEmailInfo
GO

CREATE FUNCTION [dbo].[DaDataGetNameInfo] (
	@NameQuery	NVARCHAR(500),
	@token		NVARCHAR(50))
RETURNS TABLE (
		FullName NVARCHAR(300),
		Name NVARCHAR(50),
		Middlename NVARCHAR(50),
		Surname NVARCHAR(50),
		Gender NVARCHAR(10)
	) WITH EXECUTE AS CALLER
AS
	EXTERNAL NAME [DaDataCLRSQL].[SQLCalls.NamesApi].GetNameInfo
GO


CREATE FUNCTION [dbo].[Klient_getDaDataInfo] (
	@Query	NVARCHAR(500),
	@KlType	AS TINYINT	= NULL)
RETURNS TABLE
AS
	RETURN (
	SELECT ddgci.name, ISNULL(ddgci.mnemo, ddgci.name) mnemo, ddgci.nameFull, ISNULL(ddgci.ligDate, ddgci.regDate) StateDate, ddgci.addr, ddgci.inn, ddgci.kpp, ddgci.okpo, ddgci.ogrn, ddgci.region, ddgci.city, tOrgStatus.stDescr OrgStatus
		FROM dbo.DaDataGetClientInfo(@Query, dbo.DaDataToken()) ddgci
			LEFT JOIN (VALUES ('ACTIVE', 'Действующая'),
			('LIQUIDATING', 'ЛИКВИДИРУЕТСЯ!!!'),
			('LIQUIDATED', 'ЛИКВИДИРОВАНА!!!')) AS tOrgStatus (OrgStatus, stDescr) ON ddgci.OrgStatus = tOrgStatus.OrgStatus
			LEFT JOIN (VALUES ('LEGAL', 1),
			('INDIVIDUAL', 2)) AS tOrgType (orgType, klType) ON ddgci.orgType = tOrgType.orgType
		WHERE tOrgType.klType = @KlType OR @KlType IS NULL
	)


GO

SELECT dbo.DaDataCheckToken()
SELECT dbo.DaDataCheckURL()

SELECT *
	FROM dbo.DaDataGetClientInfo('7719044994', dbo.DaDataToken()) t
	ORDER BY city

SELECT *
	FROM dbo.DaDataGetClientInfo('иванов владислав краснодар', dbo.DaDataToken()) t
	ORDER BY city

SELECT *
	FROM dbo.[DaDataGetBankInfo]('042809679', dbo.DaDataToken()) t

SELECT *
	FROM dbo.[DaDataGetAddressInfo]('электрозаводская 21 с. 27', dbo.DaDataToken()) t

SELECT *
	FROM dbo.[DaDataGetEmailInfo]('dhc@', dbo.DaDataToken()) t

SELECT *
	FROM dbo.[DaDataGetNameInfo]('Иванова Илона Петровна', dbo.DaDataToken()) t