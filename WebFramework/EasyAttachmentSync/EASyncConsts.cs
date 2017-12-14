
namespace Microarea.WebServices.EasyAttachmentSync
{
	//================================================================================
	public class EASyncConsts
	{
		public const string SSPI = "SSPI";
		public const string DMS_Settings = "DMS_Settings";
		public const string DMS_ArchivedDocContent = "DMS_ArchivedDocContent";
		public const string DMS_ArchivedDocTextContent = "DMS_ArchivedDocTextContent";

		public const string FileNameTag = "FileNameTag";
		public const string DescriptionTag = "DescriptionTag";

		public const string CreateFTSCatalog = @"if not exists (SELECT name FROM sys.fulltext_catalogs WHERE name = '{0}_FTS')
													CREATE FULLTEXT CATALOG [{0}_FTS] WITH ACCENT_SENSITIVITY = OFF";

		public const string CreateFTIndexOnBinary = @"if not exists (select * from sysobjects AS obj INNER JOIN sys.fulltext_indexes
												ON sys.fulltext_indexes.object_id = obj.id WHERE obj.name = 'DMS_ArchivedDocContent')
												BEGIN
												CREATE FULLTEXT INDEX ON [dbo].[DMS_ArchivedDocContent] (BinaryContent TYPE COLUMN ExtensionType {0})
												KEY INDEX PK_DMS_ArchivedDocContent
												ON [{1}_FTS]
												END";

		public const string CreateFTIndexOnText = @"if not exists (select * from sysobjects AS obj INNER JOIN sys.fulltext_indexes
												ON sys.fulltext_indexes.object_id = obj.id WHERE obj.name = 'DMS_ArchivedDocTextContent')
												BEGIN
												CREATE FULLTEXT INDEX ON [dbo].[DMS_ArchivedDocTextContent] (TextContent {0})
												KEY INDEX PK_DMS_ArchivedDocTextContent
												ON [{1}_FTS]
												END";

		public const string DropUDF = @"if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[{0}]') 
										and OBJECTPROPERTY(id, N'IsTableFunction') = 1)
										DROP FUNCTION [dbo].[{0}]";

		public const string EnableFullTextIndex = "ALTER FULLTEXT INDEX ON [{0}] ENABLE";
		public const string DisableFullTextIndex = "ALTER FULLTEXT INDEX ON [{0}] DISABLE";

		public const string LoadOSResources = "sp_fulltext_service 'load_os_resources', 1";
	}
}