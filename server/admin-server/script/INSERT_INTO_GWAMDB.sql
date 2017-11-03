-- Istanza e urls
INSERT [dbo].[MP_Instances] ([InstanceKey], [Description], [Disabled], [PendingDate]) VALUES (N'I-M4', N'istanza cloud M4', 0, DateAdd (day , 10 , getdate()))
INSERT [dbo].[MP_Instances] ([InstanceKey], [Description], [Disabled], [PendingDate]) VALUES (N'I-M4-ENT', N'istanza M4 Enterprise', 0, DateAdd (day , 10 , getdate()))

INSERT [dbo].[MP_ServerURLs] ([InstanceKey], [URLType], [URL]) VALUES (N'I-M4', 0, N'http://test.m4app.com')
INSERT [dbo].[MP_ServerURLs] ([InstanceKey], [URLType], [URL]) VALUES (N'I-M4', 1, N'https://app.m4cloud.netcore.com')

-- Registeredapps
INSERT INTO [dbo].[MP_RegisteredApps] ([AppId],[Name],[Description],[URL],[SecurityValue]) VALUES (N'M-PAI', N'PAI', N'Microarea PAI', N'', N'ju23ff-KOPP-0911-ww2')
INSERT INTO [dbo].[MP_RegisteredApps] ([AppId],[Name],[Description],[URL],[SecurityValue]) VALUES (N'I-M4', N'Istanza M4 Cloud', N'Microarea Mago4', N'', N'ju23ff-KOPP-0911-ila')
INSERT INTO [dbo].[MP_RegisteredApps] ([AppId],[Name],[Description],[URL],[SecurityValue]) VALUES (N'I-M4-ENT', N'Istanza M4 Enterprise', N'Microarea Mago4 Enterprise', N'', N'ju23ff-KOPP-0911-mic')

-- Subscriptions
INSERT [dbo].[MP_Subscriptions] ([SubscriptionKey], [Description], [ActivationToken], [Language], [RegionalSettings], [MinDBSizeToWarn]) VALUES (N'M4', N'subscription M4', N'', N'', N'', 2044723)
INSERT [dbo].[MP_Subscriptions] ([SubscriptionKey], [Description], [ActivationToken], [Language], [RegionalSettings], [MinDBSizeToWarn]) VALUES (N'M4-ENT', N'subscription Enterprise', N'', N'', N'', 2044723)
INSERT [dbo].[MP_Subscriptions] ([SubscriptionKey], [Description], [ActivationToken], [Language], [RegionalSettings], [MinDBSizeToWarn]) VALUES (N'M4-MDC', N'subscription MDC', N'', N'', N'', 2044723)
INSERT [dbo].[MP_Subscriptions] ([SubscriptionKey], [Description], [ActivationToken], [Language], [RegionalSettings], [MinDBSizeToWarn]) VALUES (N'M4-MANUF', N'subscription Manufacturing', N'', N'', N'', 2044723)
INSERT [dbo].[MP_Subscriptions] ([SubscriptionKey], [Description], [ActivationToken], [Language], [RegionalSettings], [MinDBSizeToWarn]) VALUES (N'S-ENT', N'subscription Enteprise', N'', N'', N'', 2044723)

-- SubscriptionInstances
INSERT INTO [dbo].[MP_SubscriptionInstances]([SubscriptionKey],[InstanceKey]) VALUES (N'M4', N'I-M4')
INSERT INTO [dbo].[MP_SubscriptionInstances]([SubscriptionKey],[InstanceKey]) VALUES (N'M4-ENT', N'I-M4')
INSERT INTO [dbo].[MP_SubscriptionInstances]([SubscriptionKey],[InstanceKey]) VALUES (N'M4-MDC', N'I-M4')
INSERT INTO [dbo].[MP_SubscriptionInstances]([SubscriptionKey],[InstanceKey]) VALUES (N'M4-MANUF', N'I-M4')
INSERT INTO [dbo].[MP_SubscriptionInstances]([SubscriptionKey],[InstanceKey]) VALUES (N'S-ENT', N'I-M4-ENT')

-- Accounts
INSERT [dbo].[MP_Accounts] ([AccountName], [Password], [FullName], [Notes], [Email], [LoginFailedCount], [PasswordNeverExpires], [MustChangePassword], [CannotChangePassword], [PasswordExpirationDate], 
[PasswordDuration], [Disabled], [Locked], [WindowsAuthentication], [Language], [RegionalSettings], [Ticks], [ExpirationDate]) 
VALUES (N'mdelbene@m4.com', N'delbene', N'Michela Delbene', N'ufficio amministrativo', N'mdelbene@m4.com', 0, 0, 0, 0, CAST(N'1753-01-01 00:00:00.000' AS DateTime), 5, 0, 0,  
0, N'', N'', 999999999, CAST(N'2027-06-23 14:52:56.727' AS DateTime))
INSERT [dbo].[MP_Accounts] ([AccountName], [Password], [FullName], [Notes], [Email], [LoginFailedCount], [PasswordNeverExpires], [MustChangePassword], [CannotChangePassword], [PasswordExpirationDate], 
[PasswordDuration], [Disabled], [Locked], [WindowsAuthentication], [Language], [RegionalSettings], [Ticks], [ExpirationDate]) 
VALUES (N'imanzoni@m4.com', N'manzoni', N'Ilaria Manzoni', N'ufficio risorse umane', N'imanzoni@m4.com', 1, 0, 0, 0, CAST(N'2017-06-26 08:50:04.607' AS DateTime), 90, 0, 0,
0, N'', N'', 999999999, CAST(N'2024-06-23 14:52:56.727' AS DateTime))
INSERT [dbo].[MP_Accounts] ([AccountName], [Password], [FullName], [Notes], [Email], [LoginFailedCount], [PasswordNeverExpires], [MustChangePassword], [CannotChangePassword], [PasswordExpirationDate], 
[PasswordDuration], [Disabled], [Locked], [WindowsAuthentication], [Language], [RegionalSettings], [Ticks], [ExpirationDate])
VALUES (N'fricceri@m4.com', N'ricceri', N'Francesco Ricceri', N'', N'fricceri@m4.com', 0, 0, 0, 0, CAST(N'2024-06-23 14:52:56.727' AS DateTime), 90, 0, 0, 
0, N'', N'', 999999999, CAST(N'2024-06-23 14:52:56.727' AS DateTime))
INSERT [dbo].[MP_Accounts] ([AccountName], [Password], [FullName], [Notes], [Email], [LoginFailedCount], [PasswordNeverExpires], [MustChangePassword], [CannotChangePassword], [PasswordExpirationDate], 
[PasswordDuration], [Disabled], [Locked], [WindowsAuthentication], [Language], [RegionalSettings], [Ticks], [ExpirationDate]) 
VALUES (N'abauzone@m4.com', N'bauzone', N'Anna Bauzone', N'', N'abauzone@m4.com', 0, 0, 0, 0, CAST(N'1753-01-01 00:00:00.000' AS DateTime), 40, 0, 0, 
0, N'', N'', 0, CAST(N'2027-06-26 15:04:57.713' AS DateTime))
INSERT [dbo].[MP_Accounts] ([AccountName], [Password], [FullName], [Notes], [Email], [LoginFailedCount], [PasswordNeverExpires], [MustChangePassword], [CannotChangePassword], [PasswordExpirationDate], 
[PasswordDuration], [Disabled], [Locked], [WindowsAuthentication], [Language], [RegionalSettings], [Ticks], [ExpirationDate]) 
VALUES (N'lbruni@m4.com', N'bruni', N'Luca Bruni', N'', N'lbruni@m4.com', 0, 0, 0, 0, CAST(N'1753-01-01 00:00:00.000' AS DateTime), 30, 0, 0, 
0, N'', N'', 0, CAST(N'2027-06-26 15:04:57.713' AS DateTime))

-- SubscriptionAccounts
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'mdelbene@m4.com', N'M4')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'mdelbene@m4.com', N'M4-MDC')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'mdelbene@m4.com', N'M4-ENT')

INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'imanzoni@m4.com', N'M4')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'imanzoni@m4.com', N'M4-MANUF')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'imanzoni@m4.com', N'M4-ENT')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'imanzoni@m4.com', N'S-ENT')

INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'fricceri@m4.com', N'M4-MDC')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'fricceri@m4.com', N'M4-MANUF')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'fricceri@m4.com', N'M4-ENT')

INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'abauzone@m4.com', N'M4-MANUF')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'abauzone@m4.com', N'M4-ENT')

INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'lbruni@m4.com', N'M4')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'lbruni@m4.com', N'M4-MDC')

-- SubscriptionDatabases
INSERT INTO [dbo].[MP_SubscriptionDatabases] ([InstanceKey],[SubscriptionKey],[Name],[Description],[DBServer],[DBName],[DBOwner],[DBPassword],[Disabled],[DatabaseCulture],[IsUnicode],[Provider],[UseDMS],[DMSDBServer],[DMSDBName],[DMSDBOwner],[DMSDBPassword],[Test], [UnderMaintenance]) 
VALUES (N'I-M4', N'M4', N'AziendaM4', N'company di test 1', N'tst-sql', N'TestM4', N'sa', N'Microarea.', 0, N'', 0, N'SQL', 0, N'', N'', N'', N'', 0, 0)
INSERT INTO [dbo].[MP_SubscriptionDatabases] ([InstanceKey],[SubscriptionKey],[Name],[Description],[DBServer],[DBName],[DBOwner],[DBPassword],[Disabled],[DatabaseCulture],[IsUnicode],[Provider],[UseDMS],[DMSDBServer],[DMSDBName],[DMSDBOwner],[DMSDBPassword],[Test], [UnderMaintenance]) 
VALUES (N'I-M4', N'M4', N'AziendaM4-LIGHT', N'azienda light', N'usr-delbenemic', N'M4-LIGHT', N'sa', N'14', 0, N'', 0, N'SQL', 0, N'', N'', N'', N'', 0, 0)
INSERT INTO [dbo].[MP_SubscriptionDatabases] ([InstanceKey],[SubscriptionKey],[Name],[Description],[DBServer],[DBName],[DBOwner],[DBPassword],[Disabled],[DatabaseCulture],[IsUnicode],[Provider],[UseDMS],[DMSDBServer],[DMSDBName],[DMSDBOwner],[DMSDBPassword],[Test], [UnderMaintenance]) 
VALUES (N'I-M4', N'M4-ENT', N'AziendaM4-ENT', N'azienda Enterprise', N'usr-delbenemic', N'M4-ENT', N'sa', N'14', 0, N'', 0, N'SQL',  0, N'', N'', N'', N'', 0, 0)
INSERT INTO [dbo].[MP_SubscriptionDatabases] ([InstanceKey],[SubscriptionKey],[Name],[Description],[DBServer],[DBName],[DBOwner],[DBPassword],[Disabled],[DatabaseCulture],[IsUnicode],[Provider],[UseDMS],[DMSDBServer],[DMSDBName],[DMSDBOwner],[DMSDBPassword],[Test], [UnderMaintenance]) 
VALUES (N'I-M4', N'M4-MANUF', N'AzManufacturing', N'Azienda di produzione', N'', N'', N'', N'', 0, N'', 0, N'', 0, N'', N'', N'', N'', 0, 0)
INSERT INTO [dbo].[MP_SubscriptionDatabases] ([InstanceKey],[SubscriptionKey],[Name],[Description],[DBServer],[DBName],[DBOwner],[DBPassword],[Disabled],[DatabaseCulture],[IsUnicode],[Provider],[UseDMS],[DMSDBServer],[DMSDBName],[DMSDBOwner],[DMSDBPassword],[Test], [UnderMaintenance])  
VALUES (N'I-M4', N'M4-MDC', N'AzDigital', N'Digital communications', N'', N'', N'', N'', 0, N'', 0, N'',  0, N'', N'', N'', N'', 0, 0)
INSERT INTO [dbo].[MP_SubscriptionDatabases] ([InstanceKey],[SubscriptionKey],[Name],[Description],[DBServer],[DBName],[DBOwner],[DBPassword],[Disabled],[DatabaseCulture],[IsUnicode],[Provider],[UseDMS],[DMSDBServer],[DMSDBName],[DMSDBOwner],[DMSDBPassword],[Test], [UnderMaintenance]) 
VALUES (N'I-M4', N'M4-MDC', N'AzDigitalPlus', N'azienda digitale plus', N'', N'', N'', N'', 0, N'', 0, N'', 0, N'', N'', N'', N'', 0, 0)

-- Roles
INSERT [dbo].[MP_Roles] ([RoleName], [ParentRoleName], [Description], [Disabled]) VALUES (N'Admin', N'', N'Amministratore generico', 0)

-- AccountRoles
INSERT [dbo].[MP_AccountRoles] ([RoleName], [AccountName], [EntityKey], [Level]) VALUES (N'Admin', N'abauzone@m4.com', N'M4-MANUF', N'SUBSCRIPTION')
INSERT [dbo].[MP_AccountRoles] ([RoleName], [AccountName], [EntityKey], [Level]) VALUES (N'Admin', N'imanzoni@m4.com', N'M4-MANUF', N'SUBSCRIPTION')
INSERT [dbo].[MP_AccountRoles] ([RoleName], [AccountName], [EntityKey], [Level]) VALUES (N'Admin', N'imanzoni@m4.com', N'M4', N'SUBSCRIPTION')
INSERT [dbo].[MP_AccountRoles] ([RoleName], [AccountName], [EntityKey], [Level]) VALUES (N'Admin', N'imanzoni@m4.com', N'S-ENT', N'SUBSCRIPTION')
INSERT [dbo].[MP_AccountRoles] ([RoleName], [AccountName], [EntityKey], [Level]) VALUES (N'Admin', N'mdelbene@m4.com', N'I-M4', N'INSTANCE')
INSERT [dbo].[MP_AccountRoles] ([RoleName], [AccountName], [EntityKey], [Level]) VALUES (N'Admin', N'fricceri@m4.com', N'*', N'INSTANCE')
INSERT [dbo].[MP_AccountRoles] ([RoleName], [AccountName], [EntityKey], [Level]) VALUES (N'Admin', N'imanzoni@m4.com', N'I-M4-ENT', N'INSTANCE')
