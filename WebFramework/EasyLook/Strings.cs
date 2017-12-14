using System;
using System.Collections;
using System.Drawing;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.Web.EasyLook
{
	//=========================================================================
	public class Strings
	{
		public const string WebFramework			= "WebFramework";
		public const string EasyLookFull			= "EasyLookFull";
		public const string EasyLookBrandKey		= "EasyLook";
	}

	//=========================================================================
	public  class PathImageStrings
	{
		public static string ReportImagePath			= Helper.GetImageUrl("RunReport.GIF");
		public static string AllUsersReportImagePath	= Helper.GetImageUrl("RunAllUsersReport.GIF");
		public static string CurrentUserReportImagePath	= Helper.GetImageUrl("RunCurrentUserReport.GIF");

		public static string DocumentImagePath			= Helper.GetImageUrl("RunDocument.GIF");
		public static string BatchImagePath				= Helper.GetImageUrl("RunBatch.GIF");
		public static string FunctionImagePath			= Helper.GetImageUrl("RunFunction.GIF");
	}

	//=========================================================================
	public  class SessionKey
	{
		public static string Parser					= "Parser";
		public static string ReportTitle			= "ReportTitle";
		public static string ReloadHistory			= "ReloadHistory";
		public static string ReportsToDeleting		= "ReportsToDeleting";
		public static string Reports				= "Reports";
		public static string ReportType				= "ReportType";
		public static string ImageState				= "ImageState";
		public static string Visible				= "Visible";
		public static string Anonimous				= "Anonymous";
		public static string ResultNodeCollection	= "ResultNodeCollection";
		public static string ShowDescription		= "ShowDescription";
		public static string ShowDate				= "ShowDate";
		public static string DocumentsSelected		= "DocumentsSelected";
		public static string ActiveThreads			= "ActiveThreads";
	}

	//=========================================================================
	public class DefaultSettings
	{
		public static Color currentUserCommandForeColor	= Color.CornflowerBlue;
		public static Color allUsersCommandForeColor	= Color.RoyalBlue;
		public static Color standardCommandForeColor	= Color.Blue;	
	}

	//=========================================================================
		/// <summary>
		/// Classe che mi permette di confrontare due date
		/// </summary>
		public class RunnedReportComparer : IComparer
		{
			/// <summary>
			/// Funzione che confronta due date e ritorna la più grande
			/// </summary>
			/// <param name="x"></param>
			/// <param name="y"></param>
			/// <returns></returns>
			public int Compare(object x, object y)
			{
				DateTime d1 = ((RunnedReport)x).TimeStamp;
				DateTime d2 = ((RunnedReport)y).TimeStamp;
	
				int res = DateTime.Compare(d1, d2);
				switch (res)
				{
					case -1:
						return 1;
					case 0:
						return 0;
					case 1:
						return -1;
				}
	
				return 0;
			}
		}

	//=========================================================================
	public class CommonFunctions
	{
		//---------------------------------------------------------------------
		public static string GetBrandedTitle()
		{
			string brandedName = InstallationData.BrandLoader.GetBrandedStringBySourceString(Strings.EasyLookBrandKey);
            IBrandInfo brandInfo = InstallationData.BrandLoader.GetMainBrandInfo();
			string title = string.IsNullOrEmpty(brandInfo.ProductTitle) ? brandedName : brandInfo.ProductTitle + " - " + brandedName;

			return title;
		}
	}
}
