using System.ComponentModel;
using Microarea.Common.GenericForms;
using TaskBuilderNetCore.Common.CustomAttributes;
using TaskBuilderNetCore.Interfaces;
using Microarea.Common.WebServicesWrapper;

namespace TaskBuilderNetCore.EasyStudio.Services
{
	//====================================================================
	[Name("licSvc"), Description("This service manages easystudio licence info and serialization.")]
	public class LicenceService : Service
	{
        LoginManager loginManager;
            
        //---------------------------------------------------------------
        /// <summary>
        /// EasyStudio is running in a Developer Edition
        /// </summary>
        public bool IsDeveloperEdition { get => loginManager.IsActivated(NameSolverStrings.TBS, NameSolverStrings.DevelopmentEdition); }

        //---------------------------------------------------------------------
        /// <summary>
        /// EasyStudio can save changes
        /// </summary>
        public bool CanSaveChanges { get  => loginManager.IsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioSave); }

        //---------------------------------------------------------------------
        /// <summary>
        /// EasyStudio can generate csproj
        /// </summary>
        public bool CanGenerateCsproj { get => loginManager.IsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioGenerateCsProj); }

        //---------------------------------------------------------------------
        /// <summary>
        /// EasyStudio can generate csproj
        /// </summary>
        public bool CanPack { get => loginManager.IsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioPack); }

        //---------------------------------------------------------------------
        /// <summary>
        /// EasyStudio can generate csproj
        /// </summary>
        public bool CanDesign { get => loginManager.IsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioDesigner); }

        //---------------------------------------------------------------------
        public LicenceService()
        {
            loginManager = LoginFacilities.loginManager;
        }
    }
}
