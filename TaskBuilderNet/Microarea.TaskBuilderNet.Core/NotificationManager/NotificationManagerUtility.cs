using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceModel.Channels;
using System.ServiceModel;
using Microarea.TaskBuilderNet.Core.NotificationService;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using System.Security;
using System.ComponentModel;
using System.Reflection;
using System.Drawing;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.TaskBuilderNet.Core.NotificationManager
{
	public enum NotificationMessageType { Error = 0, Ok = 1, Info = 2, MileStone = 3 };
	//public enum NotificationType { BrainBusiness = 0 };

	/// <summary>
	/// classe che incapsula i metodi per creare un istanza del NotificationService client
	/// </summary>
	public static class NotificationServiceUtility
	{
		/// <summary>
		/// restituisce il client a partire dall'url
		/// </summary>
		/// <param name="siteUrl"></param>
		/// <returns></returns>
		public static NotificationServiceClient GetNotificationServiceClient(string siteUrl)
		{
			Uri serviceUri = new Uri(siteUrl);
			EndpointAddress endpointAddress = new EndpointAddress(serviceUri);

			//Create the binding here
			System.ServiceModel.Channels.Binding binding = BindingFactory.CreateInstance();

			NotificationServiceClient client = new NotificationServiceClient(binding, endpointAddress);
			return client;
		}
	}

	public static class BindingFactory
	{
		public static System.ServiceModel.Channels.Binding CreateInstance()
		{
			BasicHttpBinding binding = new BasicHttpBinding();
			binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
			binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
			binding.UseDefaultWebProxy = true;
			return binding;
		}
	}

	public static class NotificationManagerUtility
	{
		/// <summary>
		/// Restituisce i tre principali colori del tema
		/// </summary>
		/// <returns></returns>
		public static MainColorsTheme GetMainColorsTheme()
		{
			MainColorsTheme colors = null;
			try
			{
				colors = new MainColorsTheme();
				ITheme theme = DefaultTheme.GetTheme();

				colors.Background	= theme.GetThemeElementColor("WorkflowBkgColor");
				colors.Primary		= theme.GetThemeElementColor("WorkflowPrimaryColor");
				colors.Secondary	= theme.GetThemeElementColor("WorkflowSecondaryBkgColor");
				colors.Hover		= theme.GetThemeElementColor("WorkflowHoverColor");
				colors.Text			= theme.GetThemeElementColor("WorkflowTextColor");
			}
			catch
			{
				colors = new MainColorsTheme 
				{ 
					Background= Color.White,
					Primary = Color.Gray, 
					Secondary= Color.LightSlateGray,
					Hover = Color.LightGray, 
					Text = Color.Black 	
				};
			}
			return colors;
		}

		/// <summary>
		/// metodo utilizzato per il controllo dei campi obbligatori
		/// </summary>
		/// <param name="form"></param>
		/// <param name="controls"></param>
		/// <param name="errorProvider"></param>
		/// <returns></returns>
		public static bool AllControlsValidated(Form form, System.Windows.Forms.Control.ControlCollection controls, ErrorProvider errorProvider)
		{
			form.ValidateChildren();
			foreach(Control control in controls)
				if(errorProvider.GetError(control) != "")
					return false;
			return true;
		}

		/// <summary>
		/// Setto lo stile flat sul control passato e ricorsivamente ai suoi figli
		/// </summary>
		/// <param name="parent"></param>
		public static void SetFlatStyleFlat(Control parent)
		{
			foreach(Control control in parent.Controls)
			{
				ButtonBase button = control as ButtonBase;
				GroupBox group = control as GroupBox;
				Label label = control as Label;

				if(button != null)
					button.FlatStyle = FlatStyle.Flat;
				else if(group != null)
					group.FlatStyle = FlatStyle.Flat;
				else if(label != null)
					label.FlatStyle = FlatStyle.Flat;

				SetFlatStyleFlat(control);
			}
		}

		/// <summary>
		/// recuperato online da stackoverflow per fare prima
		/// </summary>
		/// <param name="Container"></param>
		public static void LockControlValues(System.Windows.Forms.Control Container)
		{
			try
			{
				foreach(Control ctrl in Container.Controls)
				{
					if(ctrl.GetType() == typeof(TextBox))
						((TextBox)ctrl).ReadOnly = true;
					if(ctrl.GetType() == typeof(ComboBox))
						((ComboBox)ctrl).Enabled = false;
					if(ctrl.GetType() == typeof(CheckBox))
						((CheckBox)ctrl).Enabled = false;
					if(ctrl.GetType() == typeof(DateTimePicker))
						((DateTimePicker)ctrl).Enabled = false;
					if(ctrl.GetType() == typeof(ListBox))
						((ListBox)ctrl).Enabled = false;
					if(ctrl.Controls.Count > 0)
						LockControlValues(ctrl);
				}
			}
			catch(Exception)
			{
				//MessageBox.Show(ex.ToString());
			}
		}

		/// <summary>
		/// Verifica l'esistenza e nel caso crea l'event Source per il NotificationManager 
		/// </summary>
		/// <returns></returns>
		internal static bool CheckNotificationServiceEventLog()
		{
			try
			{
				// NB:				
				// The Source can be any random string, but the name must be distinct from other
				// sources on the computer. It is common for the source to be the name of the 
				// application or another identifying string. An attempt to create a duplicated 
				// Source value throws an exception. However, a single event log can be associated
				// with multiple sources.
				if(EventLog.SourceExists(NameSolverStrings.NotificationService))
				{
					string existingLogName = EventLog.LogNameFromSourceName(NameSolverStrings.NotificationService, ".");
					if(String.Compare(Diagnostic.EventLogName, existingLogName, StringComparison.InvariantCultureIgnoreCase) == 0)
						return true;

					EventLog.DeleteEventSource(NameSolverStrings.NotificationService);
				}
				EventLog.CreateEventSource(NameSolverStrings.NotificationService, Diagnostic.EventLogName);

				if(!EventLog.Exists(Diagnostic.EventLogName))
					return false;

				return true;
			}
			catch(SecurityException se)
			{
				Debug.Fail(NotificationManagerStrings.EventSourceCreationError + se.Message);
				return false;
			}
			catch(Exception exception)
			{
				Debug.Fail(NotificationManagerStrings.EventSourceCreationError + exception.Message);
				return false;
			}
		}

		

		/// <summary>
		/// Metodo per generare il nome univoco per identificare il worker 
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="workerId"></param>
		/// <returns></returns>
		public static string BuildBrainBusinessUserName(int companyId, int workerId)
		{
			return "Company:" + companyId.ToString() + "/Worker:" + workerId.ToString();
		}

		/// <summary>
		/// Case InSensitive.
		/// Splitta la stringa su cui viene chiamato il metodo in un array e verifica che tutti gli elementi dell'array,
		/// anche in "disordine", siano presenti in almeno uno delle stringhe passate come parametro al metodo
		/// </summary>
		/// <param name="s"></param>
		/// <param name="collection"></param>
		/// <returns></returns>
		public static bool In(this string s, params string[] collection)
		{
			var sArray = s.ToUpper().Split(null);

			string collectionConcatenated = string.Empty;

			foreach(var element in collection)
				collectionConcatenated += element.ToUpper() + " ";

			foreach(var element in sArray)
				if(!collectionConcatenated.Contains(element))
					return false;

			return true;
		}

		/// <summary>
		/// uguale alla Left in ...Generic
		/// Tronca la stringa su cui chiamo l'extension method alla lunghezza passata come parametro
		/// </summary>
		/// <param name="s"></param>
		/// <param name="maxLength"></param>
		/// <returns></returns>
		//public static string Truncate(this string s, int maxLength)
		//{
		//	if(maxLength <= 0) return string.Empty;
		//	return s != null && s.Length > maxLength ? s.Substring(0, maxLength) : s;
		//}

		/// <summary>
		/// Mostra un messaggio d'errore tramite il diagnostic viewer
		/// </summary>
		/// <param name="description"></param>
		/// <param name="errorTrace"></param>
		/// <param name="title"></param>
		/// <returns></returns>
		public static bool SetErrorMessage(string description, string errorTrace, string title)
		{
			return DiagnosticViewer.ShowErrorTrace(description, errorTrace, title) == DialogResult.OK;
		}

		/// <summary>
		/// Mostra un messaggio d'info tramite il diagnostic viewer
		/// </summary>
		/// <param name="description"></param>
		/// <param name="errorTrace"></param>
		/// <param name="title"></param>
		/// <returns></returns>
		public static bool SetInfoMessage(string message, string title)
		{
			return DiagnosticViewer.ShowInformation(message, title)==DialogResult.OK;
		}

		/// <summary>
		/// Per NumberOfUnprocessedFormsNotifyEventHandler: Controlla se c'è qualcuno registrato a ricevere gli eventi, se sì lo spara
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void Raise(this NewFormNotifyEventHandler handler, object sender, NewFormNotifyEventArgs e)
		{
			if(handler != null)
				handler(sender, e);
		}

		/// <summary>
		/// Per NumberOfUnprocessedFormsNotifyEventHandler: Controlla se c'è qualcuno registrato a ricevere gli eventi, se sì lo spara
		/// su un nuovo thread
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void RaiseOnDifferentThread(this NewFormNotifyEventHandler handler, object sender, NewFormNotifyEventArgs e)
		{
			if(handler != null)
				Task.Factory.StartNewOnDifferentThread(() => handler.Raise(sender, e));
		}

		//-----------------Display name localizable method to lookup resources---------------------------
		/// <summary>
		/// Metodo utilizzato per recuperare una il valore di una stringa localizzata
		/// </summary>
		/// <param name="resourceManagerProvider"></param>
		/// <param name="resourceKey"></param>
		/// <returns></returns>
		internal static string LookupResource(Type resourceManagerProvider, string resourceKey)
		{
			foreach(PropertyInfo staticProperty in resourceManagerProvider.GetProperties(BindingFlags.Static | BindingFlags.NonPublic))
			{
				if(staticProperty.PropertyType == typeof(System.Resources.ResourceManager))
				{
					System.Resources.ResourceManager resourceManager = (System.Resources.ResourceManager)staticProperty.GetValue(null, null);
					return resourceManager.GetString(resourceKey);
				}
			}

			return resourceKey; // Fallback with the key name
		}
	}

	//--------------------------------Display name localizable class-------------------------------------
	/// <summary>
	/// classe utilizzata per utilizzare il displayNameAttribute anche sfruttando la localizzazione
	/// usage:
	/// [DisplayNameLocalized(typeof(Strings), "Status")]
	///	public int Status { get; set; }
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event)]
	public class DisplayNameLocalizedAttribute : DisplayNameAttribute
	{
		public DisplayNameLocalizedAttribute(Type resourceManagerProvider, string resourceKey)
			: base(NotificationManagerUtility.LookupResource(resourceManagerProvider, resourceKey))
		{
		}
	}

	/// <summary>
	/// Gli argument per gli aggiornamenti all'interfaccia grafica del numero delle form ancora da processare
	/// ha un campo int number
	/// </summary>
	public class NewFormNotifyEventArgs : System.EventArgs
	{
		public int Number { get; set; }
	}

	/// <summary>
	/// Delegate per sparare le notifiche riguardanti il numero delle form ancora da processare
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void NewFormNotifyEventHandler(object sender, NewFormNotifyEventArgs e);

	/// <summary>
	/// questa classe principalmente incapsula la classe di brain business in modo da isolare
	/// la dipendenza al ws di bb solo all'interno di questa dll
	/// </summary>
	public class MyBBFormInstanceBase
	{
		public int FormInstanceId { get; set; }
		public string Title { get; set; }
		public bool Processed { get; set; }
		public DateTime DateSubmitted { get; set; } //ricevuta dal client
		public DateTime DateProcessed { get; set; } //rispedita al server
		public string UserName { get; set; }
	}

	/// <summary>
	/// Questa classe è stata pensata per aggiungere i due campi alla classe base
	/// in futuro si potrebbe eliminare la classe base e i metodi che vi fanno riferimento
	/// e mantenere solo la extended
	/// </summary>
	public class MyBBFormInstanceExtended : MyBBFormInstanceBase
	{
		public int AttachmentId { get; set; }
		public string Requester { get; set; }
		public string Description { get; set; }
	}

	/// <summary>
	/// Classe principale utilizzata per costruire una form a partire dalla descrizione ricevutaa da Brain Business
	/// </summary>
	public class NotificationField
	{
		public string WfId { get; private set; }
		public string Label { get; private set; }
		public string FieldName { get; private set; }
		public string Description { get; private set; }  //non necessario?
		public bool IsMandatory { get; private set; }
		public bool IsReadOnly { get; private set; }

		public NotificationField(string wfId, string label, string fieldName, string description, string isMandatory, string isReadOnly)
		{
			WfId = wfId;
			Label = label;
			FieldName = fieldName;
			Description = description;
			IsMandatory = (isMandatory == "true" ? true : false);
			IsReadOnly = (isReadOnly == "true" ? true : false);
		}
	}

	/// <summary>
	/// Rappresentazione delle label di Brain Business
	/// </summary>
	public class NotificationLabel : NotificationField
	{
		public NotificationLabel(string wfId, string label, string fieldName, string description)
			: base(wfId, label, fieldName, description, "false", "true") { }
	}

	/// <summary>
	/// Rappresentazione delle caselle di testo di Brain Business
	/// </summary>
	public class NotificationText : NotificationField
	{
		public string Value { get; set; }
		public int Lines { get; set; }

		public NotificationText(string wfId, string label, string fieldName, string description, string isMandatory, string isReadOnly, string value, string lines)
			: base(wfId, label, fieldName, description, isMandatory, isReadOnly)
		{
			Value = (value == null ? "" : value);
			Lines = Int32.Parse(lines);
		}
	}

	/// <summary>
	/// Rappresentazione di un menu DropDown di Brain Business, è stato implementato generico
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class NotificationDropDown<T> : NotificationField
	{
		public List<NotificationDropDownItem<T>> ItemList { get; set; }
		public T DefaultValue { get; set; }
		public string DefaultDescription { set { } get { return this.ItemList.Where(x => x.Value.Equals(this.DefaultValue)).Select(x => x.Description).FirstOrDefault(); } }


		public NotificationDropDown(string wfId, string label, string fieldName, string description, string isMandatory, string isReadOnly, T value, List<NotificationDropDownItem<T>> itemList)
			: base(wfId, label, fieldName, description, isMandatory, isReadOnly)
		{
			DefaultValue = value;
			ItemList = itemList;
		}

		public void UpdateDefaultValue(string description)
		{
			try
			{
				DefaultValue = (from item in ItemList where item.Description.Equals(description) select item.Value).First();
			}
			catch(Exception)
			{
				//in pratica vorrei che se non trova elementi nella lista, non venga assegnato nessun nuovo valore
				//al DefaultValue, mentre con il metodo FirstOrDefault(), non so cosa ci metta, per questo catcho l'exception
				//e non faccio niente
			}
		}
	}


	/// <summary>
	/// Rappresentazione di una casella di editor di data di Brain Business
	/// </summary>
	public class NotificationDateEdit : NotificationField
	{
		public DateTime DateTime { get; set; }

		public NotificationDateEdit(string wfId, string label, string fieldName, string description, string isMandatory, string isReadOnly, string dateTime)
			: base(wfId, label, fieldName, description, isMandatory, isReadOnly)
		{
			try
			{
				DateTime = DateTime.Parse(dateTime);
			}
			catch(Exception)
			{
				DateTime = DateTime.Now;
			}
		}
	}

	/// <summary>
	/// Rappresentazione dei tipi contenuti dentro un menu Drop Down di Brain Business
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class NotificationDropDownItem<T>
	{
		public string Description { get; set; }
		public T Value { get; set; }
	}

	//private class ItemList<T> : List<Item<T>> {
	//    //string DefaultDescription { set { } get { return this.Where(x => x.Value.Equals(this.DefaultValue)).Select(x => x.Description).FirstOrDefault(); } }
	//    //T DefaultValue { get; set; }

	//}

	public class MainColorsTheme 
	{
		public Color Background		{ get; set; }
		public Color Primary		{ get; set; }
		public Color Secondary		{ get; set; }
		public Color Hover			{ get; set; }
		public Color Text			{ get; set; }
		
		public bool IsBackgroundDark()
		{
			return !((1 - (0.299 * Primary.R + 0.587 * Primary.G + 0.114 * Primary.B) / 255) < 0.5);
		}
	}
}
