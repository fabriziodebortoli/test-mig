-- Istanza e urls
INSERT [dbo].[MP_Instances] ([InstanceKey], [Description], [Disabled], [PendingDate], [VerificationCode], [Ticks], [SecurityValue]) VALUES (N'I-M4', N'istanza cloud M4', 0, CAST(N'2027-06-26 15:04:57.713' AS DateTime),'NjM5NDk2MTkwOTc3MTMwMDAwNg==', 999, 'ju23ff-KOPP-0911-ila')
INSERT [dbo].[MP_Instances] ([InstanceKey], [Description], [Disabled], [PendingDate], [VerificationCode], [Ticks], [SecurityValue]) VALUES (N'I-M4-ENT', N'istanza M4 Enterprise', 0, CAST(N'2027-06-26 15:04:57.713' AS DateTime), 'NjM5NDk2MTkwOTc3MTMwMDAwNg==',999, 'ju23ff-KOPP-0911-mic')

INSERT [dbo].[MP_ServerURLs] ([InstanceKey], [URLType], [URL]) VALUES (N'I-M4', 0, N'http://test.m4app.com')
INSERT [dbo].[MP_ServerURLs] ([InstanceKey], [URLType], [URL]) VALUES (N'I-M4', 1, N'https://app.m4cloud.netcore.com')

-- Subscriptions
INSERT [dbo].[MP_Subscriptions] ([SubscriptionKey], [Description], [ActivationToken], [VATNr], [Language], [RegionalSettings], [MinDBSizeToWarn], [Ticks]) VALUES (N'M4', N'subscription M4', N'',N'', N'', N'', 2044723,999)
INSERT [dbo].[MP_Subscriptions] ([SubscriptionKey], [Description], [ActivationToken], [VATNr], [Language], [RegionalSettings], [MinDBSizeToWarn], [Ticks]) VALUES (N'M4-ENT', N'subscription Enterprise', N'',N'', N'', N'', 2044723,999)
INSERT [dbo].[MP_Subscriptions] ([SubscriptionKey], [Description], [ActivationToken], [VATNr], [Language], [RegionalSettings], [MinDBSizeToWarn], [Ticks]) VALUES (N'M4-MDC', N'subscription MDC', N'',N'', N'', N'', 2044723,999)
INSERT [dbo].[MP_Subscriptions] ([SubscriptionKey], [Description], [ActivationToken], [VATNr], [Language], [RegionalSettings], [MinDBSizeToWarn], [Ticks]) VALUES (N'M4-MANUF', N'subscription Manufacturing', N'',N'', N'', N'', 2044723,999)
INSERT [dbo].[MP_Subscriptions] ([SubscriptionKey], [Description], [ActivationToken], [VATNr], [Language], [RegionalSettings], [MinDBSizeToWarn], [Ticks]) VALUES (N'S-ENT', N'subscription Enteprise', N'',N'', N'', N'', 2044723,999)

-- SubscriptionInstances
INSERT INTO [dbo].[MP_SubscriptionInstances]([SubscriptionKey],[InstanceKey], [Ticks]) VALUES (N'M4', N'I-M4',999)
INSERT INTO [dbo].[MP_SubscriptionInstances]([SubscriptionKey],[InstanceKey], [Ticks]) VALUES (N'M4-ENT', N'I-M4',999)
INSERT INTO [dbo].[MP_SubscriptionInstances]([SubscriptionKey],[InstanceKey], [Ticks]) VALUES (N'M4-MDC', N'I-M4',999)
INSERT INTO [dbo].[MP_SubscriptionInstances]([SubscriptionKey],[InstanceKey], [Ticks]) VALUES (N'M4-MANUF', N'I-M4',999)
INSERT INTO [dbo].[MP_SubscriptionInstances]([SubscriptionKey],[InstanceKey], [Ticks]) VALUES (N'S-ENT', N'I-M4-ENT',999)

-- Accounts
INSERT [dbo].[MP_Accounts] ([AccountName], [Password], [FullName], [Notes], [VATNr], [Email], [LoginFailedCount], [PasswordNeverExpires], [MustChangePassword], [CannotChangePassword], [PasswordExpirationDate], 
[PasswordDuration], [Disabled], [Locked], [WindowsAuthentication], [Language], [RegionalSettings], [Ticks], [ExpirationDate]) 
VALUES (N'mdelbene@m4.com', N'delbene', N'Michela Delbene', N'ufficio amministrativo', N'', N'mdelbene@m4.com', 0, 0, 0, 0, CAST(N'2027-06-26 15:04:57.713' AS DateTime), 5, 0, 0,  
0, N'', N'', 999, CAST(N'2027-06-26 15:04:57.713' AS DateTime))
INSERT [dbo].[MP_Accounts] ([AccountName], [Password], [FullName], [Notes], [VATNr], [Email], [LoginFailedCount], [PasswordNeverExpires], [MustChangePassword], [CannotChangePassword], [PasswordExpirationDate], 
[PasswordDuration], [Disabled], [Locked], [WindowsAuthentication], [Language], [RegionalSettings], [Ticks], [ExpirationDate]) 
VALUES (N'imanzoni@m4.com', N'manzoni', N'Ilaria Manzoni', N'ufficio risorse umane', N'', N'imanzoni@m4.com', 1, 0, 0, 0, CAST(N'2027-06-26 15:04:57.713' AS DateTime), 90, 0, 0,
0, N'', N'', 999, CAST(N'2027-06-26 15:04:57.713' AS DateTime))
INSERT [dbo].[MP_Accounts] ([AccountName], [Password], [FullName], [Notes], [VATNr], [Email], [LoginFailedCount], [PasswordNeverExpires], [MustChangePassword], [CannotChangePassword], [PasswordExpirationDate], 
[PasswordDuration], [Disabled], [Locked], [WindowsAuthentication], [Language], [RegionalSettings], [Ticks], [ExpirationDate])
VALUES (N'fricceri@m4.com', N'ricceri', N'Francesco Ricceri', N'', N'', N'fricceri@m4.com', 0, 0, 0, 0,CAST(N'2027-06-26 15:04:57.713' AS DateTime), 90, 0, 0, 
0, N'', N'', 999, CAST(N'2027-06-26 15:04:57.713' AS DateTime))
INSERT [dbo].[MP_Accounts] ([AccountName], [Password], [FullName], [Notes], [VATNr], [Email], [LoginFailedCount], [PasswordNeverExpires], [MustChangePassword], [CannotChangePassword], [PasswordExpirationDate], 
[PasswordDuration], [Disabled], [Locked], [WindowsAuthentication], [Language], [RegionalSettings], [Ticks], [ExpirationDate]) 
VALUES (N'abauzone@m4.com', N'bauzone', N'Anna Bauzone', N'', N'', N'abauzone@m4.com', 0, 0, 0, 0, CAST(N'2027-06-26 15:04:57.713' AS DateTime), 40, 0, 0, 
0, N'', N'', 999, CAST(N'2027-06-26 15:04:57.713' AS DateTime))
INSERT [dbo].[MP_Accounts] ([AccountName], [Password], [FullName], [Notes], [VATNr], [Email], [LoginFailedCount], [PasswordNeverExpires], [MustChangePassword], [CannotChangePassword], [PasswordExpirationDate], 
[PasswordDuration], [Disabled], [Locked], [WindowsAuthentication], [Language], [RegionalSettings], [Ticks], [ExpirationDate]) 
VALUES (N'lbruni@m4.com', N'bruni', N'Luca Bruni', N'', N'', N'lbruni@m4.com', 0, 0, 0, 0, CAST(N'2027-06-26 15:04:57.713' AS DateTime), 30, 0, 0, 
0, N'', N'', 999, CAST(N'2027-06-26 15:04:57.713' AS DateTime))

-- SubscriptionAccounts
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey], [Ticks]) VALUES (N'mdelbene@m4.com', N'M4',999)
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey], [Ticks]) VALUES (N'mdelbene@m4.com', N'M4-MDC',999)
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey], [Ticks]) VALUES (N'mdelbene@m4.com', N'M4-ENT',999)

INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey], [Ticks]) VALUES (N'imanzoni@m4.com', N'M4',999)
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey], [Ticks]) VALUES (N'imanzoni@m4.com', N'M4-MANUF',999)
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey], [Ticks]) VALUES (N'imanzoni@m4.com', N'M4-ENT',999)
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey], [Ticks]) VALUES (N'imanzoni@m4.com', N'S-ENT',999)

INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey], [Ticks]) VALUES (N'fricceri@m4.com', N'M4-MDC',999)
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey], [Ticks]) VALUES (N'fricceri@m4.com', N'M4-MANUF',999)
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey], [Ticks]) VALUES (N'fricceri@m4.com', N'M4-ENT',999)

INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey], [Ticks]) VALUES (N'abauzone@m4.com', N'M4-MANUF',999)
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey], [Ticks]) VALUES (N'abauzone@m4.com', N'M4-ENT',999)

INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey], [Ticks]) VALUES (N'lbruni@m4.com', N'M4',999)
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey], [Ticks]) VALUES (N'lbruni@m4.com', N'M4-MDC',999)

-- Roles
INSERT [dbo].[MP_Roles] ([RoleName], [ParentRoleName], [Description], [Disabled]) VALUES (N'Admin', N'', N'Amministratore generico', 0)

-- AccountRoles
INSERT [dbo].[MP_AccountRoles] ([RoleName], [AccountName], [EntityKey], [Level], [Ticks]) VALUES (N'Admin', N'abauzone@m4.com', N'M4-MANUF', N'SUBSCRIPTION',999)
INSERT [dbo].[MP_AccountRoles] ([RoleName], [AccountName], [EntityKey], [Level], [Ticks]) VALUES (N'Admin', N'imanzoni@m4.com', N'M4-MANUF', N'SUBSCRIPTION',999)
INSERT [dbo].[MP_AccountRoles] ([RoleName], [AccountName], [EntityKey], [Level], [Ticks]) VALUES (N'Admin', N'imanzoni@m4.com', N'M4', N'SUBSCRIPTION',999)
INSERT [dbo].[MP_AccountRoles] ([RoleName], [AccountName], [EntityKey], [Level], [Ticks]) VALUES (N'Admin', N'imanzoni@m4.com', N'S-ENT', N'SUBSCRIPTION',999)
INSERT [dbo].[MP_AccountRoles] ([RoleName], [AccountName], [EntityKey], [Level], [Ticks]) VALUES (N'Admin', N'mdelbene@m4.com', N'I-M4', N'INSTANCE',999)
INSERT [dbo].[MP_AccountRoles] ([RoleName], [AccountName], [EntityKey], [Level], [Ticks]) VALUES (N'Admin', N'fricceri@m4.com', N'*', N'INSTANCE',999)
INSERT [dbo].[MP_AccountRoles] ([RoleName], [AccountName], [EntityKey], [Level], [Ticks]) VALUES (N'Admin', N'imanzoni@m4.com', N'I-M4-ENT', N'INSTANCE',999)

---- SubscriptionDatabases - solo provisioning DB
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

