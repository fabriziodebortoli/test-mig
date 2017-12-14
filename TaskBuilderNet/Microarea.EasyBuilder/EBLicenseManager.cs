using System;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder
{
	//=========================================================================
	/// <remarks />
	public static class EBLicenseManager
	{
		static readonly LoginManager loginManager = new LoginManager();
        static bool? canISave;
        static bool? generateCsproj;
        static bool? canPack;
        static bool? canDesign;

        //---------------------------------------------------------------------
        /// <remarks />
        public static bool CanISave
		{
			get
            {
                if (!canISave.HasValue)
                {
                    canISave = loginManager.IsActivated(NameSolverStrings.Extensions, "EasyStudioSave");
                }
                return canISave.Value;
            }
		}

        //---------------------------------------------------------------------
        /// <remarks />
        public static bool GenerateCsproj
        {
            get
            {
                if (!generateCsproj.HasValue)
                {
                    generateCsproj = loginManager.IsActivated(NameSolverStrings.Extensions, "GenerateCsproj");
                }
                return generateCsproj.Value;
            }
        }
        
        //---------------------------------------------------------------------
        /// <remarks />
        public static bool CanPack{
            get
            {
                if (!canPack.HasValue)
                {
                    canPack = loginManager.IsActivated(NameSolverStrings.Extensions, "EasyStudioPack");
                }
                return canPack.Value;
            }
        }

        //---------------------------------------------------------------------
        /// <remarks />
        public static bool CanDesign
        {
            get
            {
                if (!canDesign.HasValue)
                {
                    canDesign = loginManager.IsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioDesigner);
                }
                return canDesign.Value;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Se non vengono sollevate eccezioni allora la licenza viene bruciata.
        /// Lancia una <code>NotEasyBuilderDeveloperLicenseException</code> se l'utente non è un EasyBuilderDEveloper in console
        /// Lancia una <code>Exception</code> se non riesce ad effettuare la chiamata IsCalAvailable verso login manager
        /// Lancia una <code>EasyBuilderCalSoldOutLicenseException</code> se le licenze per EasyBuilder sono terminate.
        /// </summary>
        public static void DemandLicenseForEasyStudio(string authenticationToken)
		{
			//Se l'utente non è EasyBuilderDeveloper allora non può buciare la cal di EasyBuilder
			if (!loginManager.IsEasyBuilderDeveloper(authenticationToken))
				throw new NotEasyStudioDeveloperLicenseException(Resources.UserNotEasyBuilderDeveloper);

			//Provo a bruciare la cal, se disponibile.
			bool isCalAvailable = false;
			try
			{
				isCalAvailable = loginManager.IsCalAvailable(
					authenticationToken,
					NameSolverStrings.Extensions,
					NameSolverStrings.EasyStudioDesigner
					);
			}
			catch (Exception exc)
			{
				throw new Exception(Resources.ErrorContactingLoginManager, exc);
			}

			if (!isCalAvailable)
			{
				throw new EasyStudioCalSoldOutLicenseException(Resources.EasyBuilderCalSoldOut);
			}
		}
	}
}
