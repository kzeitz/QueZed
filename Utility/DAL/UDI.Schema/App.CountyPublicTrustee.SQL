IF OBJECT_ID ('[app].CountyPublicTrustee', 'U') IS NULL
BEGIN
	PRINT 'CREATE table [app].CountyPublicTrustee'
	CREATE TABLE [app].CountyPublicTrustee(
		CountyID		INT NOT NULL,
		Title			[app].[Name] NULL,
		FirstName		[app].[Name] NOT NULL,
		MiddleName		[app].[Name] NULL,
		LastName		[app].[Name] NOT NULL,
		Suffix			VARCHAR(10) NULL,
		eMailAddress	[app].eMailAddress NULL,
		PhoneNumber		[app].PhoneNumber NULL,
		CONSTRAINT		[PK_CountyPublicTrustee] PRIMARY KEY CLUSTERED(CountyID),
		CONSTRAINT		[FK_CountyPublicTrustee_County] FOREIGN KEY(CountyID) REFERENCES [app].County(ID)
	) ON [PRIMARY]
END
PRINT 'CREATE constraint FK_CountyPublicTrustee_County'
IF 
OBJECT_ID (N'[app].CountyPublicTrustee', N'U') IS NOT NULL AND 
0 = (SELECT COUNT(*) FROM [app].CountyPublicTrustee)
BEGIN	
	INSERT [app].CountyPublicTrustee
	SELECT 1,  'Development', 'County', 'Public', 'Trustee', 'Dev', 'info@gov-soft.com', NULL UNION
	SELECT 2,  NULL, 'Carol', NULL, 'Snyder', NULL, 'csnyder@co.adams.co.us', NULL UNION
	SELECT 3,  NULL, 'Lois', NULL, 'Widhalm', NULL, 'lwidhalm@alamosacounty.org', NULL UNION
	SELECT 4,  NULL, 'Ana', 'Maria', 'Peters-Ruddick', NULL, 'apeters-ruddick@co.arapahoe.co.us', NULL UNION
	SELECT 5,  NULL, 'Betty', NULL, 'Diller', NULL, 'bdiller@archuletacounty.org', NULL UNION
	SELECT 6,  NULL, 'Susan', NULL, 'Cochell', NULL, NULL, NULL UNION
	SELECT 7,  NULL, 'Alta', 'Mae', 'Brown', NULL, NULL, NULL UNION
	SELECT 8,  NULL, 'Richard', NULL, 'Gebhardt', NULL, 'rggpt@indra.com', NULL UNION
	SELECT 9,  NULL, 'Kim', NULL, 'Pfeifer', NULL, 'kpfeifer@broomfield.org', '303-464-5810' UNION
	SELECT 10, NULL, 'Diana', NULL, 'Wood', NULL, 'dwood@chaffeecounty.org', NULL UNION
	SELECT 11, NULL, 'Sharla', NULL, 'Anderson', NULL, 'cctreas@rebeltec.net', NULL UNION
	SELECT 12, NULL, 'Irene', NULL, 'Kincade', NULL, 'ikincade@co.clear-creek.co.us', '303-679-2330' UNION
	SELECT 13, NULL, 'Mack', NULL, 'Crowther', NULL, 'mack@co.conejos.co.us', '719-376-6783' UNION
	SELECT 14, NULL, 'Lorraine', NULL, 'Medina', NULL, 'lorraine.medina@costillacounty.net', NULL UNION
	SELECT 15, NULL, 'Lynne', NULL, 'Bauer', NULL, 'treasurer@crowleycounty.net', NULL UNION
	SELECT 16, NULL, 'Virginia', NULL, 'Trujillo', NULL, 'custertreas@centurytel.net', NULL UNION
	SELECT 17, NULL, 'Jim', NULL, 'Ventrello', NULL, 'jventrello@deltacounty.com', NULL UNION
	SELECT 18, NULL, 'Stephanie', NULL, 'O''Malley', NULL, 'malley@denvergov.org', NULL UNION
	SELECT 19, NULL, 'Janie', NULL, 'Stiasny', NULL, 'dctreas@centurytel.net', NULL UNION
	SELECT 20, NULL, 'Dianne', NULL, 'Bailey', NULL, 'dbailey@douglas.co.us', NULL UNION
	SELECT 21, NULL, 'Karen', NULL, 'Sheaffer', NULL, 'karen.sheaffer@eaglecounty.us', NULL UNION
	SELECT 22, NULL, 'Billie', NULL, 'Mills', NULL, 'billie.mills@elbertcounty-co.gov', NULL UNION
	SELECT 23, NULL, 'Thomas', 'S', 'Mowle', NULL, 'thomasmowle@elpasopublictrustee.com', '719-520-6787' UNION
	SELECT 24, NULL, 'Pat', NULL, 'McFarland', NULL, 'pat.mcfarland@fremontco.com', NULL UNION
	SELECT 25, NULL, 'Georgia', NULL, 'Chamberlain', NULL, 'gchamberlain@garfield-county.com', NULL UNION
	SELECT 26, NULL, 'Alynn', NULL, 'Huffman', NULL, 'gcpt@co.gilpin.co.us', NULL UNION
	SELECT 27, NULL, 'Tina', NULL, 'Whitmer', NULL, 'treas@co.grand.co.us', NULL UNION
	SELECT 28, NULL, 'Melody', NULL, 'Marks', NULL, 'mmarks@gunnisoncounty.org', NULL UNION
	SELECT 29, NULL, 'Kerry', NULL, 'Carl', NULL, 'hinsdaletreasurer@centurytel.net', NULL UNION
	SELECT 30, NULL, 'Debbie', NULL, 'Reynolds', NULL, 'cotreas@huerfano.us', NULL UNION
	SELECT 31, NULL, 'Letitia', NULL, 'Willcox', NULL, 'jctreasurerandpt@hotmail.com', NULL UNION
	SELECT 32, NULL, 'Margaret', NULL, 'Chapman', NULL, 'mchapman@jeffco.us', NULL UNION
	SELECT 33, NULL, 'Steve', NULL, 'Baxter', NULL, 'kct@kiowacountycolo.com', NULL UNION
	SELECT 34, NULL, 'Sandra', NULL, 'Berry', NULL, 'treasurer@kitcarsoncounty.org', NULL UNION
	SELECT 35, NULL, 'Ed', NULL, 'Murray', NULL, 'murrayej@co.laplata.co.us', NULL UNION
	SELECT 36, NULL, 'Tina', NULL, 'Tekansik', NULL, 'ttekansik@co.lake.co.us', NULL UNION
	SELECT 37, NULL, 'Deborah', NULL, 'Morgan', NULL, 'morgand@larimer.org', NULL UNION
	SELECT 38, NULL, 'Donna', NULL, 'Leonetti', NULL, 'lacpublictrustee@yahoo.com', NULL UNION
	SELECT 39, NULL, 'Jim', NULL, 'Covington', NULL, 'lctreas@lincolncountyco.us', NULL UNION
	SELECT 40, NULL, 'Patty', NULL, 'Bartlett', NULL, 'bartlettp@logancountyco.gov', NULL UNION
	SELECT 41, NULL, 'Paul', NULL, 'Brown', NULL, 'mesacopt@harrissuites.com', '970-244-1690' UNION
	SELECT 42, NULL, 'Patti', NULL, 'Payne', NULL, 'patti_payne@hotmail.com', NULL UNION
	SELECT 43, NULL, 'Robert', NULL, 'Razzano', NULL, 'rrazzano@moffatcounty.net', '970-824-9128' UNION
	SELECT 44, NULL, 'Sherry', NULL, 'Dyess', NULL, 'sdyess@co.montezuma.co.us', NULL UNION
	SELECT 45, NULL, 'Rosemary', NULL, 'Murphy', NULL, 'rmurphy@co.montrose.co.us', '970-252-4572' UNION
	SELECT 46, NULL, 'Bob', NULL, 'Sagel', NULL, 'rsagel@co.morgan.co.us', '970-542-3519' UNION
	SELECT 47, NULL, 'Dennis', NULL, 'Smith', NULL, 'dsmith@oterogov.org', NULL UNION
	SELECT 48, NULL, 'Jeannine', NULL, 'Casolari', NULL, 'jcasolari@ouraycountyco.gov', NULL UNION
	SELECT 49, NULL, 'Michelle', NULL, 'Miller', NULL, 'mmiller@parkco.us', NULL UNION
	SELECT 50, NULL, 'Linda', NULL, 'Statz', NULL, 'phillipstres@pctelcom.coop', NULL UNION
	SELECT 51, NULL, 'Tom', NULL, 'Oken', NULL, 'treasurer@co.pitkin.co.us', NULL UNION
	SELECT 52, NULL, 'Lorraine', NULL, 'Woolley', NULL, 'lwoolley@prowerscounty.net', NULL UNION
	SELECT 53, NULL, 'Nick', NULL, 'Gradisar', NULL, 'nick@pueblopt.com', NULL UNION
	SELECT 54, NULL, 'Karen', NULL, 'Arnold', NULL, 'karnold@co.rio-blanco.co.us', NULL UNION
	SELECT 55, NULL, 'Peggy', NULL, 'Kern', NULL, 'rgtreas@riograndecounty.org', NULL UNION
	SELECT 56, NULL, 'Jeanne', NULL, 'Whiddon', NULL, 'jwhiddon@co.routt.co.us', NULL UNION
	SELECT 57, NULL, 'Connie', NULL, 'Trujillo', NULL, 'ctrujillo@saguachecounty-co.gov', NULL UNION
	SELECT 58, NULL, 'Beverly', NULL, 'Rich', NULL, 'bevrich@frontier.net', NULL UNION
	SELECT 59, NULL, 'Janice', NULL, 'Stout', NULL, 'jans@sanmiguelcounty.org', NULL UNION
	SELECT 60, NULL, 'Wanda', NULL, 'Lowery', NULL, 'sedgcotreas@pctelcom.coop', NULL UNION
	SELECT 61, NULL, 'Bill', NULL, 'Wallace', NULL, 'billw@co.summit.co.us', '970-453-3441' UNION
	SELECT 62, NULL, 'Connie', NULL, 'Joiner', NULL, 'joinerc@co.teller.co.us', NULL UNION
	SELECT 63, NULL, 'Debi', NULL, 'Cooper', NULL, 'dcooper@co.washington.co.us', NULL UNION
	SELECT 64, NULL, 'Susie', NULL, 'Velasquez', NULL, NULL, NULL UNION
	SELECT 65, NULL, 'DeeAnn', NULL, 'Stults', NULL, 'treasurer@co.yuma.co.us', NULL
END
ELSE PRINT 'SKIPPED create table [app].CountyPublicTrustee'
GO

IF OBJECT_ID ('[app].CountyPublicTrusteeOffice', 'U') IS NULL
BEGIN
	PRINT 'CREATE table [app].CountyPublicTrusteeOffice'
	CREATE TABLE [app].CountyPublicTrusteeOffice(
		CountyID		INT NOT NULL,
		Line1			[app].[LongName] NOT NULL,
		Line2			[app].[LongName] NULL,
		City			[app].[Name] NOT NULL,
		StateProvinceID	INT NOT NULL,
		PostalCode		[app].[ShortName] NOT NULL,
		WebSite		    [app].URL NULL,
		eMailAddress	[app].eMailAddress NULL,
		PhoneNumber		[app].PhoneNumber NULL,
		FaxNumber		[app].PhoneNumber NULL,
		CONSTRAINT		[PK_CountyPublicTrusteeOffice] PRIMARY KEY CLUSTERED([CountyID]),
		CONSTRAINT		[FK_CountyPublicTrusteeOffice_County] FOREIGN KEY(CountyID) REFERENCES [app].County(ID)
	) ON [PRIMARY]
END
IF 
OBJECT_ID (N'[app].CountyPublicTrusteeOffice', N'U') IS NOT NULL AND 
0 = (SELECT COUNT(*) FROM [app].CountyPublicTrusteeOffice)
BEGIN
	DECLARE @coloradoID INT
	SELECT @coloradoID = ID FROM [app].StateProvince WHERE [Name] = 'Colorado'
	INSERT [app].CountyPublicTrusteeOffice
	SELECT 1, '440 Indiana St', '#150', 'Golden', @coloradoID, '80401', 'www.govsoft.com', 'support@gov-soft.com', '7208752301', '7208752311' UNION
	SELECT 2, '1000 Judicial Center Dr', '#200', 'Brighton', @coloradoID, '80601', 'www.co.adams.co.us', 'eforeclosures@co.adams.co.us', '3038355700', '3038355711' UNION
	SELECT 3, 'PO Box 659', NULL, 'Alamosa', @coloradoID, '81101', 'www.alamosacounty.org', NULL, '7195893626', '7195896118' UNION
	SELECT 4, '2329 W Main St', '#100', 'Littleton', @coloradoID, '80120', 'www.co.arapahoe.co.us', 'publictrustee@co.arapahoe.co.us', '3037300071', '3037300076' UNION
	SELECT 5, 'PO Box 790', NULL, 'Pagosa Springs',@coloradoID, '81147', 'www.archuletacounty.org', 'publictrustee@archuletacounty.org', '9702648325', '9702648329' UNION
	SELECT 6, '741 Main St', NULL, 'Springfield', @coloradoID, '81073', 'www.bacacounty.net', 'bacatrea@bacacounty.net', '7195234262', '7195234302' UNION
	SELECT 7, 'PO Box 31', NULL, 'Las Animas', @coloradoID, '81054', NULL, 'bent.treasurer@bentcounty.net', '7194562211', '7194560375' UNION
	SELECT 8, '1790 38th St', '#100', 'Boulder', @coloradoID, '80301', 'www.bouldercountypt.org', NULL, '3034433636', '3034490676' UNION
	SELECT 9, '1 DesCombes Dr', NULL, 'Broomfield', @coloradoID, '80020', 'www.broomfield.org', 'eforeclosures@broomfield.org', '3034645819', '3034386252' UNION
	SELECT 10, 'PO Box 249', NULL, 'Salida', @coloradoID, '81201', 'www.chaffeecounty.org', 'eforeclosure@chaffeecounty.org', '7195396808', '7195399344' UNION
	SELECT 11, 'PO Box 157', NULL, 'Cheyenne Wells', @coloradoID, '80810', NULL, NULL, '7197675657', '7197675540' UNION
	SELECT 12, 'PO Box 2000', NULL, 'Georgetown', @coloradoID, '80444', 'www.co.clearcreek.co.us', NULL, '3036792353', '3036792441' UNION
	SELECT 13, 'PO Box 97', NULL, 'Conejos', @coloradoID, '81129', NULL, NULL, '7193765919', '7193762442' UNION
	SELECT 14, 'PO Box 348', NULL, 'San Luis', @coloradoID, '81152', NULL, NULL, '7196723342', '7196724276' UNION
	SELECT 15, '631 Main St', NULL, 'Ordway', @coloradoID, '81063', 'www.crowleycounty.net', NULL, '7192674444', '7192674608' UNION
	SELECT 16, 'PO Box 209', NULL, 'Westcliffe', @coloradoID, '81252', NULL, NULL, '7197832341', '7197832885' UNION
	SELECT 17, '501 Palmer St', '#202', 'Delta', @coloradoID, '81416', 'www.deltacounty.com', 'bpierce@deltacounty.com', '9708742135', '9708742141' UNION
	SELECT 18, '201 West Colfax Ave', 'Department 101', 'Denver', @coloradoID, '80202', 'www.denvergov.org/Public_Trustee', 'cures.redemptions@denvergov.org', '7208658400', '7208658789' UNION
	SELECT 19, 'PO Box 421', NULL, 'Dove Creek', @coloradoID, '81324', NULL, NULL, '9706772386', '9706772946' UNION
	SELECT 20, 'PO Box 999', NULL, 'Castle Rock', @coloradoID, '80104', 'www.co.douglas.co.us', 'pubtrust@douglas.co.us', '3036607417', '3038143034' UNION
	SELECT 21, 'PO Box 479', NULL, 'Eagle', @coloradoID, '81631', 'www.eaglecounty.us', 'publict@eaglecounty.us', '9703288870', '9703288871' UNION
	SELECT 22, '105 E Vermijo Ave', '#101', 'Colorado Springs', @coloradoID, '80903', 'www.elpasopublictrustee.com', 'eforeclosures@elpasopublictrustee.com', '7195206780', '7195206781' UNION
	SELECT 23, 'PO Box 67', NULL, 'Kiowa', @coloradoID, '80117', 'www.elbertcountyco.gov', 'eforeclosures@elbertcounty-co.gov', '3036213120', '3036213164' UNION
	SELECT 24, '615 Macon Ave', '#104', 'Canon City', @coloradoID, '81212', 'www.fremontco.com', 'debbie.chess@fremontco.com', '7192767380', '7192767385' UNION
	SELECT 25, 'PO Box 1069', NULL, 'Glenwood Springs', @coloradoID, '81602', 'www.garfieldcounty.com', 'publictrustee@garfield-county.com', '9709456382', '9703845010' UNION
	SELECT 26, 'PO Box 368', NULL, 'Central City', @coloradoID, '80427', 'www.co.gilpin.co.us', NULL, '3035825222', '3035823086' UNION
	SELECT 27, 'PO Box 288', NULL, 'Hot Sulphur Springs', @coloradoID, '80451', 'www.co.grand.co.us', NULL, '9707253347', '9707253689' UNION
	SELECT 28, 'PO Box 479', NULL, 'Gunnison', @coloradoID, '81230', 'www.gunnisoncounty.org', 'eforeclosure@gunnisoncounty.org', '9706412231', '9706417955' UNION
	SELECT 29, 'PO Box 336', NULL, 'Lake City', @coloradoID, '81235', NULL, NULL, '9709442223', '9709442202' UNION
	SELECT 30, '401 Main St', '#206', 'Walsenburg', @coloradoID, '81089', NULL, NULL, '7197381280', '7197383995' UNION
	SELECT 31, 'PO Box 458', NULL, 'Walden', @coloradoID, '80480', NULL, NULL, '9707234220', '9707233214' UNION
	SELECT 32, '100 Jefferson County Pkwy', NULL, 'Golden', @coloradoID, '80419', 'www.jeffco.us', NULL, '3032718580', '3032718588' UNION
	SELECT 33, 'PO Box 247', NULL, 'Eads', @coloradoID, '81036', 'www.kiowacountycolo.com', NULL, '7194385831', '7194385327' UNION
	SELECT 34, 'PO Box 396', NULL, 'Burlington', @coloradoID, '80807', 'www.kitcarsoncounty.org', NULL, '7193468434', '7193465238' UNION
	SELECT 35, 'PO Box 99', NULL, 'Durango', @coloradoID, '81302', 'www.co.laplata.co.us', 'eforeclosures@co.laplata.co.us', '9703826352', '9703759405' UNION
	SELECT 36, 'PO Box 276', NULL, 'Leadville', @coloradoID, '80461', 'www.lakecountyco.com', NULL, '7194864117', '7194863725' UNION
	SELECT 37, '315 W Oak St', '#400', 'Fort Collins', @coloradoID, '80521', 'www.co.larimer.co.us', NULL, '9704987453', '9704987455' UNION
	SELECT 38, 'PO Box 13', NULL, 'Trinidad', @coloradoID, '81082', NULL, NULL, '7198462981', '7198452591' UNION
	SELECT 39, 'PO Box 7', NULL, 'Hugo', @coloradoID, '80821', 'www.lincolncountyco.us', NULL, '7197432633', '7197432838' UNION
	SELECT 40, 'PO Box 1151', NULL, 'Sterling', @coloradoID, '80751', 'www.logancountyco.gov', NULL, '9705222462', '9705214179' UNION
	SELECT 41, 'PO Box 20000', NULL, 'Grand Junction', @coloradoID, '81502-5043', 'www.mesacounty.us', 'mesacoptdep@harrissuites.com', '9702569570x136', '9702569570' UNION
	SELECT 42, 'PO Box 70', NULL, 'Creede', @coloradoID, '81130', NULL, NULL, '7196582325', '7196582931' UNION
	SELECT 43, 'PO Box 6', NULL, 'Craig', @coloradoID, '81626', 'www.co.moffat.co.us', NULL, '9708249111', '9708249187' UNION
	SELECT 44, '109 W Main St', '#109', 'Cortez', @coloradoID, '81321', 'www.co.montezuma.co.us', 'ptrustee@co.montezuma.co.us', '9705657550', '9705656320' UNION
	SELECT 45, 'PO Box 609', NULL, 'Montrose', @coloradoID, '81402', 'www.co.montrose.co.us', 'eforeclosures@co.montrose.co.us', '9702493565', '9702524566' UNION
	SELECT 46, 'PO Box 593', NULL, 'Fort Morgan', @coloradoID, '80701', 'www.mctre.org', 'eforeclosure@co.morgan.co.us', '9705423518', '9705423520' UNION
	SELECT 47, 'PO Box 501', NULL, 'La Junta', @coloradoID, '81050', NULL, 'eforeclosures@oterogov.org', '7193833033', '7193830109' UNION
	SELECT 48, 'PO Box 149', NULL, 'Ouray', @coloradoID, '81427', 'www.ouraycountyco.gov', NULL, '9703254487', '9703250452' UNION
	SELECT 49, 'PO Box 638', NULL, 'Fairplay', @coloradoID, '80440', 'www.parkco.us/treasurer.htm', 'ptrustee@parkco.us', '7198364238', '7198364243' UNION
	SELECT 50, 'PO Box 267', NULL, 'Holyoke', @coloradoID, '80734', NULL, NULL, '9708542822', '9708542822' UNION
	SELECT 51, '506 E Main St', '#201', 'Aspen', @coloradoID, '81611', 'www.aspenpitkin.com', NULL, '9709205170', '9709205175' UNION
	SELECT 52, '301 S Main St', '#200', 'Lamar', @coloradoID, '81052', 'www.prowerscounty.net', 'eforeclosures@prowerscounty.net', '7193368081', '7193367232' UNION
	SELECT 53, '1848 Vinewood Ln', NULL, 'Pueblo', @coloradoID, '81005', 'www.pueblopt.com', NULL, '7195450821', '7195450825' UNION
	SELECT 54, 'PO Box G', NULL, 'Meeker', @coloradoID, '81641', 'www.co.rioblanco.co.us', NULL, '9708789660', '9708785796' UNION
	SELECT 55, 'PO Box 129', NULL, 'Del Norte', @coloradoID, '81132', NULL, NULL, '7196572747', '7196572514' UNION
	SELECT 56, 'PO Box 770907', NULL, 'Steamboat Springs', @coloradoID, '80477', 'www.co.routt.co.us', 'foreclosures@co.routt.co.us', '9708705420', '9708705426' UNION
	SELECT 57, 'PO Box 177', NULL, 'Saguache', @coloradoID, '81149', 'www.saguachecounty.net', NULL, '7196552656', '7196552617' UNION
	SELECT 58, 'PO Box 368', NULL, 'Silverton', @coloradoID, '81433', NULL, NULL, '9703875488', '9703875144' UNION
	SELECT 59, 'PO Box 488', NULL, 'Telluride', @coloradoID, '81435', 'www.sanmiguelcounty.org', 'eforeclosures@sanmiguelcounty.org', '9707284451', '9707284397' UNION
	SELECT 60, '315 Cedar St', '#210', 'Julesburg', @coloradoID, '80737', NULL, NULL, '9704743473', '9704743507' UNION
	SELECT 61, 'PO Box 289', NULL, 'Breckenridge', @coloradoID, '80424', 'www.summit.co.us', 'treasurerdepartment@co.summit.co.us', '9704533440', '9704533536' UNION
	SELECT 62, 'PO Box 367', NULL, 'Cripple Creek', @coloradoID, '80813', 'www.co.teller.co.us', NULL, '7196892985', '7196890157' UNION
	SELECT 63, 'PO Box 218', NULL, 'Akron', @coloradoID, '80720', 'www.co.washington.co.us', NULL, '9703456601', '9703456607' UNION
	SELECT 64, '809 9th St', '#1', 'Greeley', @coloradoID, '80631', 'www.wcpto.com', 'eforeclosures@wcpto.com', '9703524365', '9703525091' UNION
	SELECT 65, '310 Ash St', '#C', 'Wray', @coloradoID, '80758', 'www.yumacounty.net', NULL, '9703324965', '9703320614'
END
ELSE PRINT 'SKIPPED create table [app].CountyPublicTrusteeOffice'
GO