IF SCHEMA_ID('app') IS NULL EXEC('CREATE SCHEMA app')
GO

IF TYPE_ID('[app].Flag') IS NOT NULL DROP TYPE [app].Flag
GO
CREATE TYPE [app].Flag FROM BIT NOT NULL
GO

IF OBJECT_ID ('[app].CountryRegion', 'U') IS NULL
BEGIN
	PRINT 'CREATE table [app].CountryRegion'
	CREATE TABLE [app].CountryRegion(
		Code	VARCHAR(3) NOT NULL,
		[Name]	[app].[Name] NOT NULL,
		CONSTRAINT UNQ_Code UNIQUE (Code)
	) ON [PRIMARY]
END
IF 
OBJECT_ID (N'[app].CountryRegion', N'U') IS NOT NULL AND 
0 = (SELECT COUNT(*) FROM [app].CountryRegion)
BEGIN	
	INSERT [app].CountryRegion
	SELECT 'AD', 'Andorra' UNION
	SELECT 'AE', 'United Arab Emirates' UNION
	SELECT 'AF', 'Afghanistan' UNION
	SELECT 'AG', 'Antigua and Barbuda' UNION
	SELECT 'AI', 'Anguilla' UNION
	SELECT 'AL', 'Albania' UNION
	SELECT 'AM', 'Armenia' UNION
	SELECT 'AN', 'Netherlands Antilles' UNION
	SELECT 'AO', 'Angola' UNION
	SELECT 'AQ', 'Antarctica' UNION
	SELECT 'AR', 'Argentina' UNION
	SELECT 'AS', 'American Samoa' UNION
	SELECT 'AT', 'Austria' UNION
	SELECT 'AU', 'Australia' UNION
	SELECT 'AW', 'Aruba' UNION
	SELECT 'AZ', 'Azerbaijan' UNION
	SELECT 'BA', 'Bosnia and Herzegovina' UNION
	SELECT 'BB', 'Barbados' UNION
	SELECT 'BD', 'Bangladesh' UNION
	SELECT 'BE', 'Belgium' UNION
	SELECT 'BF', 'Burkina Faso' UNION
	SELECT 'BG', 'Bulgaria' UNION
	SELECT 'BH', 'Bahrain' UNION
	SELECT 'BI', 'Burundi' UNION
	SELECT 'BJ', 'Benin' UNION
	SELECT 'BM', 'Bermuda' UNION
	SELECT 'BN', 'Brunei' UNION
	SELECT 'BO', 'Bolivia' UNION
	SELECT 'BR', 'Brazil' UNION
	SELECT 'BS', 'Bahamas, The' UNION
	SELECT 'BT', 'Bhutan' UNION
	SELECT 'BV', 'Bouvet Island' UNION
	SELECT 'BW', 'Botswana' UNION
	SELECT 'BY', 'Belarus' UNION
	SELECT 'BZ', 'Belize' UNION
	SELECT 'CA', 'Canada' UNION
	SELECT 'CC', 'Cocos (Keeling) Islands' UNION
	SELECT 'CD', 'Congo (DRC)' UNION
	SELECT 'CF', 'Central African Republic' UNION
	SELECT 'CG', 'Congo' UNION
	SELECT 'CH', 'Switzerland' UNION
	SELECT 'CI', 'Côte d''Ivoire' UNION
	SELECT 'CK', 'Cook Islands' UNION
	SELECT 'CL', 'Chile' UNION
	SELECT 'CM', 'Cameroon' UNION
	SELECT 'CN', 'China' UNION
	SELECT 'CO', 'Colombia' UNION
	SELECT 'CR', 'Costa Rica' UNION
	SELECT 'CS', 'Serbia and Montenegro' UNION
	SELECT 'CU', 'Cuba' UNION
	SELECT 'CV', 'Cape Verde' UNION
	SELECT 'CX', 'Christmas Island' UNION
	SELECT 'CY', 'Cyprus' UNION
	SELECT 'CZ', 'Czech Republic' UNION
	SELECT 'DE', 'Germany' UNION
	SELECT 'DJ', 'Djibouti' UNION
	SELECT 'DK', 'Denmark' UNION
	SELECT 'DM', 'Dominica' UNION
	SELECT 'DO', 'Dominican Republic' UNION
	SELECT 'DZ', 'Algeria' UNION
	SELECT 'EC', 'Ecuador' UNION
	SELECT 'EE', 'Estonia' UNION
	SELECT 'EG', 'Egypt' UNION
	SELECT 'ER', 'Eritrea' UNION
	SELECT 'ES', 'Spain' UNION
	SELECT 'ET', 'Ethiopia' UNION
	SELECT 'FI', 'Finland' UNION
	SELECT 'FJ', 'Fiji Islands' UNION
	SELECT 'FK', 'Falkland Islands (Islas Malvinas)' UNION
	SELECT 'FM', 'Micronesia' UNION
	SELECT 'FO', 'Faroe Islands' UNION
	SELECT 'FR', 'France' UNION
	SELECT 'GA', 'Gabon' UNION
	SELECT 'GB', 'United Kingdom' UNION
	SELECT 'GD', 'Grenada' UNION
	SELECT 'GE', 'Georgia' UNION
	SELECT 'GF', 'French Guiana' UNION
	SELECT 'GH', 'Ghana' UNION
	SELECT 'GI', 'Gibraltar' UNION
	SELECT 'GL', 'Greenland' UNION
	SELECT 'GM', 'Gambia, The' UNION
	SELECT 'GN', 'Guinea' UNION
	SELECT 'GP', 'Guadeloupe' UNION
	SELECT 'GQ', 'Equatorial Guinea' UNION
	SELECT 'GR', 'Greece' UNION
	SELECT 'GS', 'South Georgia and the South Sandwich Islands' UNION
	SELECT 'GT', 'Guatemala' UNION
	SELECT 'GU', 'Guam' UNION
	SELECT 'GW', 'Guinea-Bissau' UNION
	SELECT 'GY', 'Guyana' UNION
	SELECT 'HK', 'Hong Kong SAR' UNION
	SELECT 'HM', 'Heard Island and McDonald Islands' UNION
	SELECT 'HN', 'Honduras' UNION
	SELECT 'HR', 'Croatia' UNION
	SELECT 'HT', 'Haiti' UNION
	SELECT 'HU', 'Hungary' UNION
	SELECT 'ID', 'Indonesia' UNION
	SELECT 'IE', 'Ireland' UNION
	SELECT 'IL', 'Israel' UNION
	SELECT 'IN', 'India' UNION
	SELECT 'IO', 'British Indian Ocean Territory' UNION
	SELECT 'IQ', 'Iraq' UNION
	SELECT 'IR', 'Iran' UNION
	SELECT 'IS', 'Iceland' UNION
	SELECT 'IT', 'Italy' UNION
	SELECT 'JM', 'Jamaica' UNION
	SELECT 'JO', 'Jordan' UNION
	SELECT 'JP', 'Japan' UNION
	SELECT 'KE', 'Kenya' UNION
	SELECT 'KG', 'Kyrgyzstan' UNION
	SELECT 'KH', 'Cambodia' UNION
	SELECT 'KI', 'Kiribati' UNION
	SELECT 'KM', 'Comoros' UNION
	SELECT 'KN', 'Saint Kitts and Nevis' UNION
	SELECT 'KP', 'North Korea' UNION
	SELECT 'KR', 'Korea' UNION
	SELECT 'KW', 'Kuwait' UNION
	SELECT 'KY', 'Cayman Islands' UNION
	SELECT 'KZ', 'Kazakhstan' UNION
	SELECT 'LA', 'Laos' UNION
	SELECT 'LB', 'Lebanon' UNION
	SELECT 'LC', 'Saint Lucia' UNION
	SELECT 'LI', 'Liechtenstein' UNION
	SELECT 'LK', 'Sri Lanka' UNION
	SELECT 'LR', 'Liberia' UNION
	SELECT 'LS', 'Lesotho' UNION
	SELECT 'LT', 'Lithuania' UNION
	SELECT 'LU', 'Luxembourg' UNION
	SELECT 'LV', 'Latvia' UNION
	SELECT 'LY', 'Libya' UNION
	SELECT 'MA', 'Morocco' UNION
	SELECT 'MC', 'Monaco' UNION
	SELECT 'MD', 'Moldova' UNION
	SELECT 'MG', 'Madagascar' UNION
	SELECT 'MH', 'Marshall Islands' UNION
	SELECT 'MK', 'Macedonia, Former Yugoslav Republic of' UNION
	SELECT 'ML', 'Mali' UNION
	SELECT 'MM', 'Myanmar' UNION
	SELECT 'MN', 'Mongolia' UNION
	SELECT 'MO', 'Macao SAR' UNION
	SELECT 'MP', 'Northern Mariana Islands' UNION
	SELECT 'MQ', 'Martinique' UNION
	SELECT 'MR', 'Mauritania' UNION
	SELECT 'MS', 'Montserrat' UNION
	SELECT 'MT', 'Malta' UNION
	SELECT 'MU', 'Mauritius' UNION
	SELECT 'MV', 'Maldives' UNION
	SELECT 'MW', 'Malawi' UNION
	SELECT 'MX', 'Mexico' UNION
	SELECT 'MY', 'Malaysia' UNION
	SELECT 'MZ', 'Mozambique' UNION
	SELECT 'NA', 'Namibia' UNION
	SELECT 'NC', 'New Caledonia' UNION
	SELECT 'NE', 'Niger' UNION
	SELECT 'NF', 'Norfolk Island' UNION
	SELECT 'NG', 'Nigeria' UNION
	SELECT 'NI', 'Nicaragua' UNION
	SELECT 'NL', 'Netherlands' UNION
	SELECT 'NO', 'Norway' UNION
	SELECT 'NP', 'Nepal' UNION
	SELECT 'NR', 'Nauru' UNION
	SELECT 'NU', 'Niue' UNION
	SELECT 'NZ', 'New Zealand' UNION
	SELECT 'OM', 'Oman' UNION
	SELECT 'PA', 'Panama' UNION
	SELECT 'PE', 'Peru' UNION
	SELECT 'PF', 'French Polynesia' UNION
	SELECT 'PG', 'Papua New Guinea' UNION
	SELECT 'PH', 'Philippines' UNION
	SELECT 'PK', 'Pakistan' UNION
	SELECT 'PL', 'Poland' UNION
	SELECT 'PM', 'Saint Pierre and Miquelon' UNION
	SELECT 'PN', 'Pitcairn Islands' UNION
	SELECT 'PR', 'Puerto Rico' UNION
	SELECT 'PS', 'Palestinian Authority' UNION
	SELECT 'PT', 'Portugal' UNION
	SELECT 'PW', 'Palau' UNION
	SELECT 'PY', 'Paraguay' UNION
	SELECT 'QA', 'Qatar' UNION
	SELECT 'RE', 'Réunion' UNION
	SELECT 'RO', 'Romania' UNION
	SELECT 'RU', 'Russia' UNION
	SELECT 'RW', 'Rwanda' UNION
	SELECT 'SA', 'Saudi Arabia' UNION
	SELECT 'SB', 'Solomon Islands' UNION
	SELECT 'SC', 'Seychelles' UNION
	SELECT 'SD', 'Sudan' UNION
	SELECT 'SE', 'Sweden' UNION
	SELECT 'SG', 'Singapore' UNION
	SELECT 'SH', 'Saint Helena' UNION
	SELECT 'SI', 'Slovenia' UNION
	SELECT 'SJ', 'Svalbard and Jan Mayen' UNION
	SELECT 'SK', 'Slovakia' UNION
	SELECT 'SL', 'Sierra Leone' UNION
	SELECT 'SM', 'San Marino' UNION
	SELECT 'SN', 'Senegal' UNION
	SELECT 'SO', 'Somalia' UNION
	SELECT 'SR', 'Suriname' UNION
	SELECT 'ST', 'São Tomé and Príncipe' UNION
	SELECT 'SV', 'El Salvador' UNION
	SELECT 'SY', 'Syria' UNION
	SELECT 'SZ', 'Swaziland' UNION
	SELECT 'TC', 'Turks and Caicos Islands' UNION
	SELECT 'TD', 'Chad' UNION
	SELECT 'TF', 'French Southern and Antarctic Lands' UNION
	SELECT 'TG', 'Togo' UNION
	SELECT 'TH', 'Thailand' UNION
	SELECT 'TJ', 'Tajikistan' UNION
	SELECT 'TK', 'Tokelau' UNION
	SELECT 'TL', 'Timor-Leste' UNION
	SELECT 'TM', 'Turkmenistan' UNION
	SELECT 'TN', 'Tunisia' UNION
	SELECT 'TO', 'Tonga' UNION
	SELECT 'TR', 'Turkey' UNION
	SELECT 'TT', 'Trinidad and Tobago' UNION
	SELECT 'TV', 'Tuvalu' UNION
	SELECT 'TW', 'Taiwan' UNION
	SELECT 'TZ', 'Tanzania' UNION
	SELECT 'UA', 'Ukraine' UNION
	SELECT 'UG', 'Uganda' UNION
	SELECT 'UM', 'U.S. Minor Outlying Islands' UNION
	SELECT 'US', 'United States' UNION
	SELECT 'UY', 'Uruguay' UNION
	SELECT 'UZ', 'Uzbekistan' UNION
	SELECT 'VA', 'Vatican City' UNION
	SELECT 'VC', 'Saint Vincent and the Grenadine' UNION
	SELECT 'VE', 'Venezuela' UNION
	SELECT 'VG', 'Virgin Islands, British' UNION
	SELECT 'VI', 'Virgin Islands, U.S.' UNION
	SELECT 'VN', 'Vietnam' UNION
	SELECT 'VU', 'Vanuatu' UNION
	SELECT 'WF', 'Wallis and Futuna' UNION
	SELECT 'WS', 'Samoa' UNION
	SELECT 'YE', 'Yemen' UNION
	SELECT 'YT', 'Mayotte' UNION
	SELECT 'ZA', 'South Africa' UNION
	SELECT 'ZM', 'Zambia' UNION
	SELECT 'ZW', 'Zimbabwe'
END
ELSE PRINT 'SKIPPED create table [app].CountyRegion'
GO

IF OBJECT_ID ('[app].StateProvince', 'U') IS NULL
BEGIN
	PRINT 'CREATE table [app].StateProvince'
	CREATE TABLE [app].StateProvince(
		ID						INT IDENTITY(1,1) NOT NULL,
		Code					VARCHAR(3) NOT NULL,
		CountryRegionCode		VARCHAR(3) NOT NULL,
		IsOnlyStateProvinceFlag	[app].Flag CONSTRAINT [DF_StateProvince_IsOnlyStateProvinceFlag] DEFAULT (0),
		[Name]					[app].[Name] NOT NULL,
		CONSTRAINT [PK_StateProvince] PRIMARY KEY CLUSTERED(ID),
		CONSTRAINT [FK_StateProvince_CountryRegion] FOREIGN KEY (CountryRegionCode) REFERENCES [app].CountryRegion(Code)
	) ON [PRIMARY]
END
IF 
OBJECT_ID (N'[app].StateProvince', N'U') IS NOT NULL AND 
0 = (SELECT COUNT(*) FROM [app].StateProvince)
BEGIN	
	INSERT [app].StateProvince
	SELECT 'AB', 'CA', 0, 'Alberta' UNION 
	SELECT 'AK', 'US', 0, 'Alaska' UNION 
	SELECT 'AL', 'US', 0, 'Alabama' UNION 
	SELECT 'AR', 'US', 0, 'Arkansas' UNION 
	SELECT 'AS', 'AS', 1, 'American Samoa' UNION 
	SELECT 'AZ', 'US', 0, 'Arizona' UNION 
	SELECT 'BC', 'CA', 0, 'British Columbia' UNION 
	SELECT 'BY', 'DE', 0, 'Bayern' UNION 
	SELECT 'CA', 'US', 0, 'California' UNION 
	SELECT 'CO', 'US', 0, 'Colorado' UNION 
	SELECT 'CT', 'US', 0, 'Connecticut' UNION 
	SELECT 'DC', 'US', 0, 'District of Columbia' UNION 
	SELECT 'DE', 'US', 0, 'Delaware' UNION 
	SELECT 'ENG','GB', 1, 'England' UNION 
	SELECT 'FL', 'US', 0, 'Florida' UNION 
	SELECT 'FM', 'FM', 1, 'Micronesia' UNION 
	SELECT 'GA', 'US', 0, 'Georgia' UNION 
	SELECT 'GU', 'US', 0, 'Guam' UNION 
	SELECT 'HE', 'DE', 0, 'Hessen' UNION 
	SELECT 'HH', 'DE', 0, 'Hamburg' UNION 
	SELECT 'HI', 'US', 0, 'Hawaii' UNION 
	SELECT 'IA', 'US', 0, 'Iowa' UNION 
	SELECT 'ID', 'US', 0, 'Idaho' UNION 
	SELECT 'IL', 'US', 0, 'Illinois' UNION 
	SELECT 'IN', 'US', 0, 'Indiana' UNION 
	SELECT 'KS', 'US', 0, 'Kansas' UNION 
	SELECT 'KY', 'US', 0, 'Kentucky' UNION 
	SELECT 'LA', 'US', 0, 'Louisiana' UNION 
	SELECT 'LB', 'CA', 0, 'Labrador' UNION 
	SELECT 'MA', 'US', 0, 'Massachusetts' UNION 
	SELECT 'MB', 'CA', 0, 'Manitoba' UNION 
	SELECT 'MD', 'US', 0, 'Maryland' UNION 
	SELECT 'ME', 'US', 0, 'Maine' UNION 
	SELECT 'MH', 'MH', 1, 'Marshall Islands' UNION 
	SELECT 'MI', 'US', 0, 'Michigan' UNION 
	SELECT 'MN', 'US', 0, 'Minnesota' UNION 
	SELECT 'MO', 'US', 0, 'Missouri' UNION 
	SELECT 'MP', 'MP', 1, 'Northern Mariana Islands' UNION 
	SELECT 'MS', 'US', 0, 'Mississippi' UNION 
	SELECT 'MT', 'US', 0, 'Montana' UNION 
	SELECT 'NB', 'CA', 0, 'Brunswick' UNION 
	SELECT 'NC', 'US', 0, 'North Carolina' UNION 
	SELECT 'ND', 'US', 0, 'North Dakota' UNION 
	SELECT 'NE', 'US', 0, 'Nebraska' UNION 
	SELECT 'NF', 'CA', 0, 'Newfoundland' UNION 
	SELECT 'NH', 'US', 0, 'New Hampshire' UNION 
	SELECT 'NJ', 'US', 0, 'New Jersey' UNION 
	SELECT 'NM', 'US', 0, 'New Mexico' UNION 
	SELECT 'NS', 'CA', 0, 'Nova Scotia' UNION 
	SELECT 'NSW','AU', 0, 'New South Wales' UNION 
	SELECT 'NT', 'CA', 0, 'Northwest Territories' UNION 
	SELECT 'NV', 'US', 0, 'Nevada' UNION 
	SELECT 'NW', 'DE', 0, 'Nordrhein-Westfalen' UNION 
	SELECT 'NY', 'US', 0, 'New York' UNION 
	SELECT 'OH', 'US', 0, 'Ohio' UNION 
	SELECT 'OK', 'US', 0, 'Oklahoma' UNION 
	SELECT 'ON', 'CA', 0, 'Ontario' UNION 
	SELECT 'OR', 'US', 0, 'Oregon' UNION 
	SELECT 'PA', 'US', 0, 'Pennsylvania' UNION 
	SELECT 'PE', 'CA', 0, 'Prince Edward Island' UNION 
	SELECT 'PR', 'US', 0, 'Puerto Rico' UNION 
	SELECT 'PW', 'PW', 1, 'Palau' UNION 
	SELECT 'QC', 'CA', 0, 'Quebec' UNION 
	SELECT 'QLD','AU', 0, 'Queensland' UNION 
	SELECT 'RI', 'US', 0, 'Rhode Island' UNION 
	SELECT 'SA', 'AU', 0, 'South Australia' UNION 
	SELECT 'SC', 'US', 0, 'South Carolina' UNION 
	SELECT 'SD', 'US', 0, 'South Dakota' UNION 
	SELECT 'SK', 'CA', 0, 'Saskatchewan' UNION 
	SELECT 'SL', 'DE', 0, 'Saarland' UNION 
	SELECT 'TAS','AU', 0, 'Tasmania' UNION 
	SELECT 'TN', 'US', 0, 'Tennessee' UNION 
	SELECT 'TX', 'US', 0, 'Texas' UNION 
	SELECT 'UT', 'US', 0, 'Utah' UNION 
	SELECT 'VA', 'US', 0, 'Virginia' UNION 
	SELECT 'VI', 'VI', 1, 'Virgin Islands' UNION 
	SELECT 'VIC','AU', 0, 'Victoria' UNION 
	SELECT 'VT', 'US', 0, 'Vermont' UNION 
	SELECT 'WA', 'US', 0, 'Washington' UNION 
	SELECT 'WI', 'US', 0, 'Wisconsin' UNION 
	SELECT 'WV', 'US', 0, 'West Virginia' UNION 
	SELECT 'WY', 'US', 0, 'Wyoming' UNION 
	SELECT 'YT', 'CA', 0, 'Yukon Territory' UNION 
	SELECT 'FR', 'FR', 1, 'France' UNION 
	SELECT 'BB', 'DE', 0, 'Brandenburg' UNION 
	SELECT 'SN', 'DE', 0, 'Saxony' UNION 
	SELECT '01', 'FR', 0, 'Ain' UNION 
	SELECT '02', 'FR', 0, 'Aisne' UNION 
	SELECT '03', 'FR', 0, 'Allier' UNION 
	SELECT '04', 'FR', 0, 'Alpes-de-Haute Provence' UNION 
	SELECT '05', 'FR', 0, 'Alpes (Haute)' UNION 
	SELECT '06', 'FR', 0, 'Alpes-Maritimes' UNION 
	SELECT '07', 'FR', 0, 'Ardèche' UNION 
	SELECT '08', 'FR', 0, 'Ardennes' UNION 
	SELECT '09', 'FR', 0, 'Ariège' UNION 
	SELECT '10', 'FR', 0, 'Aube' UNION 
	SELECT '11', 'FR', 0, 'Aude' UNION 
	SELECT '12', 'FR', 0, 'Aveyron' UNION 
	SELECT '13', 'FR', 0, 'Bouches du Rhône' UNION 
	SELECT '14', 'FR', 0, 'Calvados' UNION 
	SELECT '15', 'FR', 0, 'Cantal' UNION 
	SELECT '16', 'FR', 0, 'Charente' UNION 
	SELECT '17', 'FR', 0, 'Charente-Maritime' UNION 
	SELECT '18', 'FR', 0, 'Cher' UNION 
	SELECT '19', 'FR', 0, 'Corrèze' UNION 
	SELECT '20', 'FR', 0, 'Corse' UNION 
	SELECT '21', 'FR', 0, 'Côte d''Or' UNION 
	SELECT '22', 'FR', 0, 'Côtes-d''Armor' UNION 
	SELECT '23', 'FR', 0, 'Creuse' UNION 
	SELECT '24', 'FR', 0, 'Dordogne' UNION 
	SELECT '25', 'FR', 0, 'Toubs' UNION 
	SELECT '26', 'FR', 0, 'Drôme' UNION 
	SELECT '27', 'FR', 0, 'Eure' UNION 
	SELECT '28', 'FR', 0, 'Eure et Loir' UNION 
	SELECT '29', 'FR', 0, 'Finistère' UNION 
	SELECT '30', 'FR', 0, 'Gard' UNION 
	SELECT '31', 'FR', 0, 'Garonne (Haute)' UNION 
	SELECT '32', 'FR', 0, 'Gers' UNION 
	SELECT '33', 'FR', 0, 'Gironde' UNION 
	SELECT '34', 'FR', 0, 'Hérault' UNION 
	SELECT '35', 'FR', 0, 'Ille et Vilaine' UNION 
	SELECT '36', 'FR', 0, 'Indre' UNION 
	SELECT '37', 'FR', 0, 'Indre et Loire' UNION 
	SELECT '38', 'FR', 0, 'Isère' UNION 
	SELECT '39', 'FR', 0, 'Jura' UNION 
	SELECT '40', 'FR', 0, 'Landes' UNION 
	SELECT '41', 'FR', 0, 'Loir et Cher' UNION 
	SELECT '42', 'FR', 0, 'Loire' UNION 
	SELECT '43', 'FR', 0, 'Loire (Haute)' UNION 
	SELECT '44', 'FR', 0, 'Loire Atlantique' UNION 
	SELECT '45', 'FR', 0, 'Loiret' UNION 
	SELECT '46', 'FR', 0, 'Lot' UNION 
	SELECT '47', 'FR', 0, 'Lot et Garonne' UNION 
	SELECT '48', 'FR', 0, 'Lozère' UNION 
	SELECT '49', 'FR', 0, 'Maine et Loire' UNION 
	SELECT '50', 'FR', 0, 'Manche' UNION 
	SELECT '51', 'FR', 0, 'Marne' UNION 
	SELECT '52', 'FR', 0, 'Marne (Haute)' UNION 
	SELECT '53', 'FR', 0, 'Mayenne' UNION 
	SELECT '54', 'FR', 0, 'Meurthe et Moselle' UNION 
	SELECT '55', 'FR', 0, 'Meuse' UNION 
	SELECT '56', 'FR', 0, 'Morbihan' UNION 
	SELECT '57', 'FR', 0, 'Moselle' UNION 
	SELECT '58', 'FR', 0, 'Nièvre' UNION 
	SELECT '59', 'FR', 0, 'Nord' UNION 
	SELECT '60', 'FR', 0, 'Oise' UNION 
	SELECT '61', 'FR', 0, 'Orne' UNION 
	SELECT '62', 'FR', 0, 'Pas de Calais' UNION 
	SELECT '63', 'FR', 0, 'Puy de Dôme' UNION 
	SELECT '64', 'FR', 0, 'Pyrénées Atlantiques' UNION 
	SELECT '65', 'FR', 0, 'Pyrénées (Hautes)' UNION 
	SELECT '66', 'FR', 0, 'Pyrénées Orientales' UNION 
	SELECT '67', 'FR', 0, 'Rhin (Bas)' UNION 
	SELECT '68', 'FR', 0, 'Rhin (Haut)' UNION 
	SELECT '69', 'FR', 0, 'Rhône' UNION 
	SELECT '70', 'FR', 0, 'Saône (Haute)' UNION 
	SELECT '71', 'FR', 0, 'Saône et Loire' UNION 
	SELECT '72', 'FR', 0, 'Sarthe' UNION 
	SELECT '73', 'FR', 0, 'Savoie' UNION 
	SELECT '74', 'FR', 0, 'Savoie Haute' UNION 
	SELECT '75', 'FR', 0, 'Seine (Paris)' UNION 
	SELECT '76', 'FR', 0, 'Seine Maritime' UNION 
	SELECT '77', 'FR', 0, 'Seine et Marne' UNION 
	SELECT '78', 'FR', 0, 'Yveline' UNION 
	SELECT '79', 'FR', 0, 'Sèvres (Deux)' UNION 
	SELECT '80', 'FR', 0, 'Somme' UNION 
	SELECT '81', 'FR', 0, 'Tarne' UNION 
	SELECT '82', 'FR', 0, 'Tarne et Garonne' UNION 
	SELECT '83', 'FR', 0, 'Var' UNION 
	SELECT '84', 'FR', 0, 'Vaucluse' UNION 
	SELECT '85', 'FR', 0, 'La Vendée' UNION 
	SELECT '86', 'FR', 0, 'Vienne' UNION 
	SELECT '87', 'FR', 0, 'Vienne (Haute)' UNION 
	SELECT '88', 'FR', 0, 'Vosges' UNION 
	SELECT '89', 'FR', 0, 'Yonne' UNION 
	SELECT '90', 'FR', 0, 'Belford (Territoire de)' UNION 
	SELECT '91', 'FR', 0, 'Essonne' UNION 
	SELECT '92', 'FR', 0, 'Hauts de Seine' UNION 
	SELECT '93', 'FR', 0, 'Seine Saint Denis' UNION 
	SELECT '94', 'FR', 0, 'Val de Marne' UNION 
	SELECT '95', 'FR', 0, 'Val d''Oise'
END
ELSE PRINT 'SKIPPED create table [app].StateProvince'
GO

IF OBJECT_ID ('[app].County', 'U') IS NULL
BEGIN
	PRINT 'CREATE table [app].County'
	CREATE TABLE [app].County(
		ID				INT IDENTITY(1, 1) NOT FOR REPLICATION NOT NULL,
		[Name]			[app].[Name] NOT NULL,
		Classification	INT NOT NULL,
		CONSTRAINT [PK_County] PRIMARY KEY CLUSTERED (ID),
	) ON [PRIMARY]
END
IF 
OBJECT_ID (N'[app].County', N'U') IS NOT NULL AND 
0 = (SELECT COUNT(*) FROM [app].County)
BEGIN	
	INSERT [app].County VALUES('Development', 0)
	INSERT [app].County
	SELECT 'Adams', 2 UNION
	SELECT 'Alamosa', 3 UNION
	SELECT 'Arapahoe', 2 UNION
	SELECT 'Archuleta', 3 UNION
	SELECT 'Baca', 3 UNION
	SELECT 'Bent', 3 UNION
	SELECT 'Boulder', 2 UNION
	SELECT 'Broomfield', 3 UNION
	SELECT 'Chaffee', 3 UNION
	SELECT 'Cheyenne', 3 UNION
	SELECT 'Clear Creek', 3 UNION
	SELECT 'Conejos', 3 UNION
	SELECT 'Costilla', 3 UNION
	SELECT 'Crowley', 3 UNION
	SELECT 'Custer', 3 UNION
	SELECT 'Delta', 3 UNION
	SELECT 'Denver', 1 UNION
	SELECT 'Dolores', 3 UNION
	SELECT 'Douglas', 2 UNION
	SELECT 'Eagle', 3 UNION
	SELECT 'El Paso', 2 UNION
	SELECT 'Elbert', 3 UNION
	SELECT 'Fremont', 3 UNION
	SELECT 'Garfield', 3 UNION
	SELECT 'Gilpin', 3 UNION
	SELECT 'Grand', 3 UNION
	SELECT 'Gunnison', 3 UNION
	SELECT 'Hinsdale', 3 UNION
	SELECT 'Huerfano', 3 UNION
	SELECT 'Jackson', 3 UNION
	SELECT 'Jefferson', 2 UNION
	SELECT 'Kiowa', 3 UNION
	SELECT 'Kit Carson', 3 UNION
	SELECT 'La Plata', 3 UNION
	SELECT 'Lake', 3 UNION
	SELECT 'Larimer', 2 UNION
	SELECT 'Las Animas', 3 UNION
	SELECT 'Lincoln', 3 UNION
	SELECT 'Logan', 3 UNION
	SELECT 'Mesa', 2 UNION
	SELECT 'Mineral', 3 UNION
	SELECT 'Moffat', 3 UNION
	SELECT 'Montezuma', 3 UNION
	SELECT 'Montrose', 3 UNION
	SELECT 'Morgan', 3 UNION
	SELECT 'Otero', 3 UNION
	SELECT 'Ouray', 3 UNION
	SELECT 'Park', 3 UNION
	SELECT 'Phillips', 3 UNION
	SELECT 'Pitkin', 3 UNION
	SELECT 'Prowers', 3 UNION
	SELECT 'Pueblo', 2 UNION
	SELECT 'Rio Blanco', 3 UNION
	SELECT 'Rio Grande', 3 UNION
	SELECT 'Routt', 3 UNION
	SELECT 'Saguache', 3 UNION
	SELECT 'San Juan', 3 UNION
	SELECT 'San Miguel', 3 UNION
	SELECT 'Sedgwick', 3 UNION
	SELECT 'Summit', 3 UNION
	SELECT 'Teller', 3 UNION
	SELECT 'Washington', 3 UNION
	SELECT 'Weld', 2 UNION
	SELECT 'Yuma', 3
END
ELSE PRINT 'SKIPPED create table [app].County'
GO