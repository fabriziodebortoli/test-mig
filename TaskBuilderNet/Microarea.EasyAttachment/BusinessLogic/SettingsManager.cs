using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Core;

namespace Microarea.EasyAttachment.BusinessLogic
{
	///<summary>
	/// Manager dei settings e relativa gestione nel database
	///</summary>
	//================================================================================
	public class SettingsManager : BaseManager
	{
		private DMSModelDataContext dc = null;

		// Settings Standard da tenere in memoria
		private SettingState standardSettingState = new SettingState();

		// Settings dell'utente corrente
		private SettingState usersSettingState = null;
        private int workerId = -1;
		public bool IsAdmin { get; private set; }

		// Properties
		//--------------------------------------------------------------------------------
		public SettingState StandardSettingState { get { return standardSettingState; } }
		public SettingState UsersSettingState
		{
			get
			{
				if (usersSettingState == null)
				{
					usersSettingState = new SettingState();
                    LoadUsersSettings();
				}
				
				return usersSettingState;
			}
			set { usersSettingState = value; }
		}
        
		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public SettingsManager(DMSOrchestrator dmsOrchestrator)
		{
			DMSOrchestrator = dmsOrchestrator;
			ManagerName = "SettingsManager";

			dc = DMSOrchestrator.DataContext;
            workerId = DMSOrchestrator.WorkerId;

			// chiedo a TB se l'utente applicativo con il quale sono connesso e' amministratore
			IsAdmin = CUtility.IsAdmin();
		}

        ///<summary>
        /// Constructor: called in order to check the setting during application startup
        ///</summary>
        //--------------------------------------------------------------------------------
        public SettingsManager(DMSModelDataContext dc, int workerId)
        {
            this.dc = dc;
            this.workerId = workerId;

        }

		///<summary>
		/// Leggo dal database la riga con i valori AllUsers dei Settings
		///</summary>
		//--------------------------------------------------------------------------------
		public SettingState LoadUsersSettings()
		{
			try
			{
				// carico i settings standard
				DMS_Setting usersSettings = GetSetting();

				// se il record non esiste devo inserirlo in tabella al volo
				// possiamo anche decidere di non inserire mai la riga nel db e tenerci solo
				// la struttura in memoria, cosi evitiamo modifiche manuali
                if (usersSettings != null)
				{
					usersSettingState = new SettingState();
                    usersSettingState.WorkerID = -1;
                    usersSettingState.Type = SettingType.AllUsers;
                    usersSettingState = SettingState.Deserialize(usersSettings.Settings.ToString());
                    return usersSettingState;
				}
			}
			catch (Exception e)
			{
				SetMessage(Strings.ErrorLoadingStdSettings, e, "LoadUsersSettings");
			}

			return null;
		}

		///<summary>
		/// Ritorna il record corrispondente nella tabella DMS_Settings
		///</summary>
		//--------------------------------------------------------------------------------
		private DMS_Setting GetSetting()
		{
           
			var setting = (from currSetting in dc.DMS_Settings
						   where currSetting.WorkerID == -1
							&& currSetting.SettingType == (int)SettingType.AllUsers
						   select currSetting);

			return (setting != null && setting.Any()) ? (DMS_Setting)setting.Single() : null;
		}

		///<summary>
		/// Effettua il salvataggio della struttura in memoria dell'utente corrente
		/// inserendo/aggiornando il record nella tabella DMS_Settings
		///</summary>
		//--------------------------------------------------------------------------------
		public bool SaveSettings()
		{
			bool result = false;

			if (usersSettingState == null)
				return result;

			try
			{
				DMS_Setting insertSettings = GetSetting();

				// il setting non esiste: lo aggiungo
				if (insertSettings == null)
				{
					insertSettings = new DMS_Setting();
                    insertSettings.WorkerID = -1;
					insertSettings.SettingType = (int)SettingType.AllUsers;
                    usersSettingState.Type = SettingType.AllUsers;

					using (StringReader sr = new StringReader(usersSettingState.Serialize()))
						insertSettings.Settings = XElement.Load(sr); // NON USARE XElement.Parse(string)!!!!

					dc.DMS_Settings.InsertOnSubmit(insertSettings);
					dc.SubmitChanges();
					result = true;
				}
				else // altrimenti lo aggiorno
				{
                    string lockMsg = string.Empty;
					if (DMSOrchestrator.LockManager.LockRecord(insertSettings, DMSOrchestrator.LockContext, ref lockMsg))
					{
						using (StringReader sr = new StringReader(usersSettingState.Serialize()))
							insertSettings.Settings = XElement.Load(sr); // NON USARE XElement.Parse(string)!!!!

						dc.SubmitChanges();
						DMSOrchestrator.LockManager.UnlockRecord(insertSettings, DMSOrchestrator.LockContext);
						result = true;
					}
					else 
						SetMessage(string.Format(Strings.ErrorSavingSettingsForWorker, usersSettingState.WorkerID), new Exception(lockMsg), "SaveSettings");
				}
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorSavingSettingsForWorker, usersSettingState.WorkerID), e, "SaveSettings");
			}

			return result;
		}

        /////<summary>
        ///// Effettua la cancellazione di tutti i settings di tipo User e con WorkerID diverso da -1
        /////</summary>
        ////--------------------------------------------------------------------------------
        //public bool DeleteAllWorkersSettings()
        //{
        //    bool result = false;

        //    try
        //    {
        //        var settingsToDelete = from setting in dc.DMS_Settings
        //                               where setting.WorkerID != -1 &&
        //                               setting.SettingType == 3/*valore enumerativo user non più usato */
        //                               select setting;

        //        if (settingsToDelete != null)
        //        {
        //            if (settingsToDelete.Any())
        //            {
        //                string lockMsg = string.Empty;
        //                if (DMSOrchestrator.LockManager.LockRecord("DMS_Setting", "DeleteAllWorkers", DMSOrchestrator.LockContext, ref lockMsg))
        //                {
        //                    foreach (var delSetting in settingsToDelete)
        //                        dc.DMS_Settings.DeleteOnSubmit(delSetting);
        //                    dc.SubmitChanges();

        //                    DMSOrchestrator.LockManager.UnlockRecord("DMS_Setting", "DeleteAllWorkers", DMSOrchestrator.LockContext);
        //                    result = true;
        //                }
        //                else
        //                    SetMessage(Strings.ErrorDeletingSettingsForAll, new Exception(lockMsg), "DeleteAllWorkersSettings");
        //            }
        //            else
        //                result = true; // devo ritornare true anche se nella tabella non ci sono record che soddisfano la condizione
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        SetMessage(Strings.ErrorDeletingSettingsForAll, e, "DeleteAllWorkersSettings");
        //    }

        //    return result;
        //}		
	}
}