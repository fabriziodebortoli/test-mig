INSERT [dbo].[MP_Instances] ([InstanceKey], [Description], [Disabled]) VALUES (N'I-M4', N'istanza M4', 0)
INSERT [dbo].[MP_Subscriptions] ([SubscriptionKey], [Description], [ActivationToken], [PreferredLanguage], [ApplicationLanguage], [MinDBSizeToWarn], [InstanceKey]) VALUES (N'M4', N'subscription M4', N'', N'', N'', 2044723, N'I-M4')
SET IDENTITY_INSERT [dbo].[MP_Companies] ON 

INSERT [dbo].[MP_Companies] ([CompanyId], [Name], [Description], [CompanyDBServer], [CompanyDBName], [CompanyDBOwner], [CompanyDBPassword], [Disabled], [DatabaseCulture], [IsUnicode], [PreferredLanguage], [ApplicationLanguage], [Provider], [SubscriptionKey], [UseDMS], [DMSDBServer], [DMSDBName], [DMSDBOwner], [DMSDBPassword]) VALUES (1, N'AziendaM4', N'company di test', N'tst-sql', N'TestM4', N'sa', N'Microarea.', 0, N'', 0, N'', N'', N'SQL', N'M4', 0, N'', N'', N'', N'')
SET IDENTITY_INSERT [dbo].[MP_Companies] OFF
INSERT [dbo].[MP_Accounts] ([AccountName], [Password], [FullName], [Notes], [Email], [LoginFailedCount], [PasswordNeverExpires], [MustChangePassword], [CannotChangePassword], [PasswordExpirationDate], [PasswordDuration], [Disabled], [Locked], [ProvisioningAdmin], [WindowsAuthentication], [PreferredLanguage], [ApplicationLanguage], [ExpirationDate]) VALUES (N'mdelbene@hotmail.com', N'delbene', N'Michela Delbene', N'ufficio amministrativo', N'mdelbene@hotmail.com', 0, 0, 0, 0, CAST(N'1753-01-01 00:00:00.000' AS DateTime), 0, 0, 0, 0, 0, N'', N'', CAST(N'2017-06-23 14:52:56.727' AS DateTime))
