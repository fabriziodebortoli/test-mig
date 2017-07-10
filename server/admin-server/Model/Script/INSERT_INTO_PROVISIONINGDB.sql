USE [ProvisioningDB]
GO
-- istanza e url
INSERT [dbo].[MP_Instances] ([InstanceKey], [Description], [Disabled]) VALUES (N'I-M4', N'istanza cloud M4', 0)
INSERT [dbo].[MP_ServerURLs] ([InstanceKey], [URLType], [URL]) VALUES (N'I-M4', 0, N'http://test.m4app.com')
INSERT [dbo].[MP_ServerURLs] ([InstanceKey], [URLType], [URL]) VALUES (N'I-M4', 1, N'https://app.m4cloud.netcore.com')

-- subscriptions
INSERT [dbo].[MP_Subscriptions] ([SubscriptionKey], [Description], [ActivationToken], [PreferredLanguage], [ApplicationLanguage], [MinDBSizeToWarn], [InstanceKey]) VALUES (N'M4', N'subscription M4', N'', N'', N'', 2044723, N'I-M4')
INSERT [dbo].[MP_Subscriptions] ([SubscriptionKey], [Description], [ActivationToken], [PreferredLanguage], [ApplicationLanguage], [MinDBSizeToWarn], [InstanceKey]) VALUES (N'M4-ENT', N'subscription Enterprise', N'', N'', N'', 2044723, N'I-M4')
INSERT [dbo].[MP_Subscriptions] ([SubscriptionKey], [Description], [ActivationToken], [PreferredLanguage], [ApplicationLanguage], [MinDBSizeToWarn], [InstanceKey]) VALUES (N'M4-MDC', N'subscription MDC', N'', N'', N'', 2044723, N'I-M4')
INSERT [dbo].[MP_Subscriptions] ([SubscriptionKey], [Description], [ActivationToken], [PreferredLanguage], [ApplicationLanguage], [MinDBSizeToWarn], [InstanceKey]) VALUES (N'M4-MANUF', N'subscription Manufacturing', N'', N'', N'', 2044723, N'I-M4')
-- accounts
INSERT [dbo].[MP_Accounts] ([AccountName], [Password], [FullName], [Notes], [Email], [LoginFailedCount], [PasswordNeverExpires], [MustChangePassword], [CannotChangePassword], [PasswordExpirationDate], [PasswordDuration], [Disabled], [Locked], [CloudAdmin], [ProvisioningAdmin], [WindowsAuthentication], [PreferredLanguage], [ApplicationLanguage], [Ticks], [ExpirationDate]) VALUES (N'mdelbene@m4.com', N'delbene', N'Michela Delbene', N'ufficio amministrativo', N'mdelbene@m4.com', 0, 0, 0, 0, CAST(N'1753-01-01 00:00:00.000' AS DateTime), 0, 0, 0, 0, 0, 0, N'', N'', 999999999, CAST(N'2027-06-23 14:52:56.727' AS DateTime))
INSERT [dbo].[MP_Accounts] ([AccountName], [Password], [FullName], [Notes], [Email], [LoginFailedCount], [PasswordNeverExpires], [MustChangePassword], [CannotChangePassword], [PasswordExpirationDate], [PasswordDuration], [Disabled], [Locked], [CloudAdmin], [ProvisioningAdmin], [WindowsAuthentication], [PreferredLanguage], [ApplicationLanguage], [Ticks], [ExpirationDate]) VALUES (N'imanzoni@m4.com', N'manzoni', N'Ilaria Manzoni', N'ufficio risorse umane', N'imanzoni@m4.com', 1, 0, 0, 0, CAST(N'2017-06-26 08:50:04.607' AS DateTime), 90, 0, 0, 1, 1, 0, N'', N'', 999999999, CAST(N'2024-06-23 14:52:56.727' AS DateTime))
INSERT [dbo].[MP_Accounts] ([AccountName], [Password], [FullName], [Notes], [Email], [LoginFailedCount], [PasswordNeverExpires], [MustChangePassword], [CannotChangePassword], [PasswordExpirationDate], [PasswordDuration], [Disabled], [Locked], [CloudAdmin], [ProvisioningAdmin], [WindowsAuthentication], [PreferredLanguage], [ApplicationLanguage], [Ticks], [ExpirationDate]) VALUES (N'fricceri@m4.com', N'ricceri', N'Francesco Ricceri', N'', N'fricceri@m4.com', 0, 0, 0, 0, CAST(N'2024-06-23 14:52:56.727' AS DateTime), 90, 0, 0, 0, 1, 0, N'', N'', 999999999, CAST(N'2024-06-23 14:52:56.727' AS DateTime))
INSERT [dbo].[MP_Accounts] ([AccountName], [Password], [FullName], [Notes], [Email], [LoginFailedCount], [PasswordNeverExpires], [MustChangePassword], [CannotChangePassword], [PasswordExpirationDate], [PasswordDuration], [Disabled], [Locked], [CloudAdmin], [ProvisioningAdmin], [WindowsAuthentication], [PreferredLanguage], [ApplicationLanguage], [Ticks], [ExpirationDate]) VALUES (N'abauzone@m4.com', N'bauzone', N'Anna Bauzone', N'', N'abauzone@m4.com', 0, 0, 0, 0, CAST(N'1753-01-01 00:00:00.000' AS DateTime), 0, 0, 0, 0, 1, 0, N'', N'', 0, CAST(N'2027-06-26 15:04:57.713' AS DateTime))
INSERT [dbo].[MP_Accounts] ([AccountName], [Password], [FullName], [Notes], [Email], [LoginFailedCount], [PasswordNeverExpires], [MustChangePassword], [CannotChangePassword], [PasswordExpirationDate], [PasswordDuration], [Disabled], [Locked], [CloudAdmin], [ProvisioningAdmin], [WindowsAuthentication], [PreferredLanguage], [ApplicationLanguage], [Ticks], [ExpirationDate]) VALUES (N'lbruni@m4.com', N'bruni', N'Luca Bruni', N'', N'lbruni@m4.com', 0, 0, 0, 0, CAST(N'1753-01-01 00:00:00.000' AS DateTime), 0, 0, 0, 0, 1, 0, N'', N'', 0, CAST(N'2027-06-26 15:04:57.713' AS DateTime))
-- subscriptionaccounts
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'mdelbene@m4.com', N'M4')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'mdelbene@m4.com', N'M4-MDC')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'mdelbene@m4.com', N'M4-ENT')

INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'imanzoni@m4.com', N'M4')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'imanzoni@m4.com', N'M4-MANUF')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'imanzoni@m4.com', N'M4-ENT')

INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'fricceri@m4.com', N'M4-MDC')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'fricceri@m4.com', N'M4-MANUF')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'fricceri@m4.com', N'M4-ENT')

INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'abauzone@m4.com', N'M4-MANUF')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'abauzone@m4.com', N'M4-ENT')

INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'lbruni@m4.com', N'M4')
INSERT [dbo].[MP_SubscriptionAccounts] ([AccountName], [SubscriptionKey]) VALUES (N'lbruni@m4.com', N'M4-MDC')

-- companies
SET IDENTITY_INSERT [dbo].[MP_Companies] ON 
INSERT [dbo].[MP_Companies] ([CompanyId], [Name], [Description], [CompanyDBServer], [CompanyDBName], [CompanyDBOwner], [CompanyDBPassword], [Disabled], [DatabaseCulture], [IsUnicode], [PreferredLanguage], [ApplicationLanguage], [Provider], [SubscriptionKey], [UseDMS], [DMSDBServer], [DMSDBName], [DMSDBOwner], [DMSDBPassword]) VALUES (1, N'AziendaM4', N'company di test 1', N'tst-sql', N'TestM4', N'sa', N'Microarea.', 0, N'', 0, N'', N'', N'SQL', N'M4', 0, N'', N'', N'', N'')
INSERT [dbo].[MP_Companies] ([CompanyId], [Name], [Description], [CompanyDBServer], [CompanyDBName], [CompanyDBOwner], [CompanyDBPassword], [Disabled], [DatabaseCulture], [IsUnicode], [PreferredLanguage], [ApplicationLanguage], [Provider], [SubscriptionKey], [UseDMS], [DMSDBServer], [DMSDBName], [DMSDBOwner], [DMSDBPassword]) VALUES (2, N'AziendaM4-LIGHT', N'azienda light', N'usr-delbenemic', N'M4-LIGHT', N'sa', N'14', 0, N'', 0, N'', N'', N'SQL', N'M4', 0, N'', N'', N'', N'')
INSERT [dbo].[MP_Companies] ([CompanyId], [Name], [Description], [CompanyDBServer], [CompanyDBName], [CompanyDBOwner], [CompanyDBPassword], [Disabled], [DatabaseCulture], [IsUnicode], [PreferredLanguage], [ApplicationLanguage], [Provider], [SubscriptionKey], [UseDMS], [DMSDBServer], [DMSDBName], [DMSDBOwner], [DMSDBPassword]) VALUES (3, N'AziendaM4-ENT', N'azienda Enterprise', N'usr-delbenemic', N'M4-ENT', N'sa', N'14', 0, N'', 0, N'', N'', N'SQL', N'M4-ENT', 0, N'', N'', N'', N'')
INSERT [dbo].[MP_Companies] ([CompanyId], [Name], [Description], [CompanyDBServer], [CompanyDBName], [CompanyDBOwner], [CompanyDBPassword], [Disabled], [DatabaseCulture], [IsUnicode], [PreferredLanguage], [ApplicationLanguage], [Provider], [SubscriptionKey], [UseDMS], [DMSDBServer], [DMSDBName], [DMSDBOwner], [DMSDBPassword]) VALUES (4, N'AzManufacturing', N'Azienda di produzione', N'', N'', N'', N'', 0, N'', 0, N'', N'', N'', N'M4-MANUF', 0, N'', N'', N'', N'')
INSERT [dbo].[MP_Companies] ([CompanyId], [Name], [Description], [CompanyDBServer], [CompanyDBName], [CompanyDBOwner], [CompanyDBPassword], [Disabled], [DatabaseCulture], [IsUnicode], [PreferredLanguage], [ApplicationLanguage], [Provider], [SubscriptionKey], [UseDMS], [DMSDBServer], [DMSDBName], [DMSDBOwner], [DMSDBPassword]) VALUES (5, N'AzDigital', N'Digital communications', N'', N'', N'', N'', 0, N'', 0, N'', N'', N'', N'M4-MDC', 0, N'', N'', N'', N'')
INSERT [dbo].[MP_Companies] ([CompanyId], [Name], [Description], [CompanyDBServer], [CompanyDBName], [CompanyDBOwner], [CompanyDBPassword], [Disabled], [DatabaseCulture], [IsUnicode], [PreferredLanguage], [ApplicationLanguage], [Provider], [SubscriptionKey], [UseDMS], [DMSDBServer], [DMSDBName], [DMSDBOwner], [DMSDBPassword]) VALUES (6, N'AzDigitalPlus', N'azienda digitale plus', N'', N'', N'', N'', 0, N'', 0, N'', N'', N'', N'M4-MDC', 0, N'', N'', N'', N'')
SET IDENTITY_INSERT [dbo].[MP_Companies] OFF

-- companyaccounts
INSERT [dbo].[MP_CompanyAccounts] ([AccountName], [CompanyId], [Admin]) VALUES (N'mdelbene@m4.com', 1, 0)
INSERT [dbo].[MP_CompanyAccounts] ([AccountName], [CompanyId], [Admin]) VALUES (N'imanzoni@m4.com', 1, 0)

INSERT [dbo].[MP_CompanyAccounts] ([AccountName], [CompanyId], [Admin]) VALUES (N'lbruni@m4.com', 2, 0)
INSERT [dbo].[MP_CompanyAccounts] ([AccountName], [CompanyId], [Admin]) VALUES (N'mdelbene@m4.com', 2, 0)

INSERT [dbo].[MP_CompanyAccounts] ([AccountName], [CompanyId], [Admin]) VALUES (N'mdelbene@m4.com', 3, 0)
INSERT [dbo].[MP_CompanyAccounts] ([AccountName], [CompanyId], [Admin]) VALUES (N'abauzone@m4.com', 3, 0)
INSERT [dbo].[MP_CompanyAccounts] ([AccountName], [CompanyId], [Admin]) VALUES (N'imanzoni@m4.com', 3, 0)

INSERT [dbo].[MP_CompanyAccounts] ([AccountName], [CompanyId], [Admin]) VALUES (N'abauzone@m4.com', 4, 0)
INSERT [dbo].[MP_CompanyAccounts] ([AccountName], [CompanyId], [Admin]) VALUES (N'imanzoni@m4.com', 4, 0)
INSERT [dbo].[MP_CompanyAccounts] ([AccountName], [CompanyId], [Admin]) VALUES (N'fricceri@m4.com', 4, 0)

INSERT [dbo].[MP_CompanyAccounts] ([AccountName], [CompanyId], [Admin]) VALUES (N'mdelbene@m4.com', 5, 0)
INSERT [dbo].[MP_CompanyAccounts] ([AccountName], [CompanyId], [Admin]) VALUES (N'lbruni@m4.com', 5, 0)
INSERT [dbo].[MP_CompanyAccounts] ([AccountName], [CompanyId], [Admin]) VALUES (N'fricceri@m4.com', 5, 0)

INSERT [dbo].[MP_CompanyAccounts] ([AccountName], [CompanyId], [Admin]) VALUES (N'lbruni@m4.com', 6, 0)
INSERT [dbo].[MP_CompanyAccounts] ([AccountName], [CompanyId], [Admin]) VALUES (N'fricceri@m4.com', 6, 0)
