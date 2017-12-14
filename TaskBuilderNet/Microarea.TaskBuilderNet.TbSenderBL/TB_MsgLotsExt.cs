using System;
using System.Collections.Generic;
using System.Linq;
using Microarea.TaskBuilderNet.TbSenderBL.PostaLite;
using Microarea.TaskBuilderNet.TbSenderBL.postalite.api;
using System.Globalization;
using System.Text;

namespace Microarea.TaskBuilderNet.TbSenderBL
{
	public partial class TB_MsgLots
	{
		//-------------------------------------------------------------------------------
		public LotStatus EnumStatus
		{
			get { return (LotStatus)this.Status; }
			set { this.Status = (int)value; }
		}

		//-------------------------------------------------------------------------------
		public Printing EnumPrintType
		{
			get { return (Printing)this.PrintType; }
			set { this.PrintType = (int)value; }
		}

		//-------------------------------------------------------------------------------
		public Delivery EnumDeliveryType
		{
			get { return (Delivery)this.DeliveryType; }
			set { this.DeliveryType = (int)value; }
		}

		//-------------------------------------------------------------------------------
		public bool BoolIncongruous
		{
			get { return this.Incongruous == "1"; }
			set { this.Incongruous = value ? "1" : "0"; }
		}

		//-------------------------------------------------------------------------------
		public bool IsDoubleSided // Fronte-retro
		{
			get
			{
				return
					this.PrintType == (int)Printing.BiancoNeroFronteRetro ||
					this.PrintType == (int)Printing.ColoreFronteRetro;
			}
		}

		//-------------------------------------------------------------------------------
		public ILotInfo GetLotInfo(string company, IPostaLiteSettingsProvider postaLiteSettingsProvider)
		{
			TB_MsgLots lot;
			int totPages;
			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			{
				// aLot potrebbe essere disconnesso e non avere in canna i msg!
				lot = (from p in db.TB_MsgLots
					   where p.LotID == this.LotID
					   select p)
					  .First();
				totPages = MsgHelper.CalculateLotPdfPages(lot, db);
			}

			PostaliteSettings plSettings = postaLiteSettingsProvider.GetSettings(company);

			string country = lot.Country;

			// per evitare -29 Nazione non valida (per Postel è implicita, per PostaLite no)
			if (string.IsNullOrWhiteSpace(country))
			{
				country = plSettings.DefaultCountry;
				if (string.IsNullOrWhiteSpace(country))
					country = "Italia"; // ultima spiaggia
			}

			ILotInfo lotInfo = new LotInfo()
			{
				City = lot.City,
				County = lot.County,
				Description = lot.Description,
				FaxNumber = CleanFaxNumber(lot.Fax),
				State = country,
				TotalPages = totPages,
				TypeDelivery = lot.EnumDeliveryType,
				TypePrinting = lot.EnumPrintType,
				ZipCode = lot.Zip,
				Mittente_NomeCognomeRagioneSociale = plSettings.AddresserCompanyName,
				Mittente_Indirizzo = plSettings.AddresserAddress,
				Mittente_Cap = plSettings.AddresserZipCode,
				Mittente_Localita = plSettings.AddresserCity,
				Mittente_Provincia = plSettings.AddresserCounty,
				Mittente_Email = plSettings.AdviceOfDeliveryEmail
			};

			return lotInfo;
		}

		public static string CleanFaxNumber(string fax)
		{
			StringBuilder sb = new StringBuilder();
			foreach (char c in fax)
			{
				//if (false == char.IsDigit(c)) // commentato: preferisco un errore che spedire al numero sbagliato (in alcune nazioni i numeri sono alfanumerici)
				//    continue;
				if (false == char.IsLetterOrDigit(c)) // PostaLite vuole solo digit, e torna un errore altrimenti, ma preferisco un errore che spedire a un numero errato
					continue;
				if (char.IsWhiteSpace(c))
					continue;
				sb.Append(c);
			}
			return sb.ToString();
		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// Se il lotto è in mano a Zucchetti/PostaLite, non possiamo cambiarlo
		/// </summary>
		/// <returns>a boolean</returns>
		public bool HasAnAlreadyUploadedStatus()
		{
			LotStatus enumStatus = this.EnumStatus;
			return
				enumStatus == LotStatus.Uploading ||
				enumStatus == LotStatus.Uploaded;
		}

		//-------------------------------------------------------------------------------
		public List<TB_MsgQueue> GetMessages(string company)
		{
			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			{
				return (from p in db.TB_MsgQueue
						where p.LotId == this.LotID
						select p)
						.ToList();
			}
		}

		//-------------------------------------------------------------------------------
		public void SaveChanges(string company)
		{
			// aLot è disconnesso
			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			{
				// purtroppo devo fare un retrieve su db con lo stato precedente :-(
				db.TB_MsgLots.Attach(db.TB_MsgLots.Single(p => p.LotID == this.LotID)); // sembrerebbe non fare un c...o, ma fa l'attach nell'object graph
				db.TB_MsgLots.ApplyCurrentValues(this);
				db.SaveChanges();
			}
		}

		//-------------------------------------------------------------------------------
		public static List<TB_MsgLots> GetLots(string company)
		{
			List<TB_MsgLots> list = new List<TB_MsgLots>();
			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			{
				var items = from p in db.TB_MsgLots select p;
				list.AddRange(items);
			}
			return list;
		}

		//-------------------------------------------------------------------------------
		public static List<TB_MsgLots> GetLotsToUpload(string company)
		{
			DateTime now = DateTime.Now;
			//TODOLUCA
			//string allotted = LotStatus.Allotted.ToString();
			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			{
				return 
					(
						from p in db.TB_MsgLots
						where p.Status == (int)LotStatus.Allotted && now > p.SendAfter
						select p
					).ToList();
			}
		}

		//-------------------------------------------------------------------------------
		public static TB_MsgLots GetLot(int lotID, string company)
		{
			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			{
				return db.TB_MsgLots.Single(p => p.LotID == lotID);
			}
		}

		//public static List<TB_MsgLots> GetUploadedAndNotDeliveredLots(string company)
		//{
		//    using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
		//    {
		//        return GetUploadedAndNotDeliveredLots(db);
		//    }
		//}

		public static List<TB_MsgLots> GetUploadedAndNotDeliveredLots(PostaLiteIntegrationEntities db)
		{
			return db.TB_MsgLots
				.Where(p => p.Status == (int)LotStatus.Uploaded &&
					(p.StatusExt == 0 
					//stati che implicano uno stato intermedio:
					|| p.StatusExt == (int)EnumLotStatus.PresoInCarico
					|| p.StatusExt == (int)EnumLotStatus.InElaborazione
					|| p.StatusExt == (int)EnumLotStatus.Sospeso
					|| p.StatusExt == (int)EnumLotStatus.InStampa // o forse è consegnato??? credo lo stato finale sia "Spedito"
					))
				.ToList();
		}

		//private static IQueryable<TB_MsgLots> FilterUploadedAndNotDelivered(IQueryable<TB_MsgLots> lots)
		//{
		//    return lots.Where(p => p.Status == (int)LotStatus.Sent);
		//}

		//-------------------------------------------------------------------------------
		public static TB_MsgLots CreateLot(TB_MsgQueue msg)
		{
			TB_MsgLots lot = new TB_MsgLots(); // TEMP
			lot.Description = string.Empty;
			lot.EnumStatus = LotStatus.Allotted;
			lot.StatusDescriptionExt = string.Empty;
			DateTime now = DateTime.Now;
			lot.TBCreated = now;
			lot.TBModified = now;
			lot.DeliveryType = msg.DeliveryType;
			lot.PrintType = msg.PrintType;
			lot.Fax = msg.Fax;
			lot.Addressee = msg.Addressee;
			lot.Address = msg.Address;
			lot.Zip = msg.Zip;
			lot.City = msg.City;
			lot.County = msg.County;
			lot.Country = msg.Country;
			lot.AddresseeNamespace = msg.AddresseeNamespace;
			lot.AddresseePrimaryKey = msg.AddresseePrimaryKey;
			lot.BoolIncongruous = false;
			return lot;
		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// Invocare solo quando il lotto ha un ID!!!
		/// </summary>
		/// <param name="lot"></param>
		/// <returns></returns>
		public void SetDescription()
		{
			if (this.LotID <= 0)
				throw new ArgumentException();
			this.Description = string.Format(CultureInfo.InvariantCulture, "Mago Envelope {0}", this.LotID);
		}

		//-------------------------------------------------------------------------------
		public void SetExternalState(ILotState lotState)
		{
			if (lotState.Error < 0)
				this.ErrorExt = lotState.Error;
			else
			{
				this.StatusExt = lotState.State;
				this.StatusDescriptionExt = lotState.Description;
			}
			// TODO aggiorna stato interno per casi che lo necessitano (o elimina valori enumerativo non usati)
		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// Numero di pagine PDF che potrebbero ancora stare dentro la busta, senza calcolare eventuali separatori
		/// dovuti a fronte/retro
		/// </summary>
		/// <returns></returns>
		public int AvailablePdfSpace
		{
			get
			{
				int maxPages = this.IsDoubleSided ? MaxPaperPages * 2 : MaxPaperPages;
				int availablePdfPages = maxPages - this.TotalPages;
				if (this.IsDoubleSided && (availablePdfPages % 2 != 0))
					availablePdfPages -= 1; // se fronte-retro servirà una pagina separatore
				if (availablePdfPages <= 0)
					return 0;
				return availablePdfPages;
			}
		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// Numero totale di pagine cartacee (in caso di fronte-retro differisce da pagine pdf TotalPages)
		/// </summary>
		public int TotalPaperPages
		{
			get
			{
				int totalPdfPages = this.TotalPages; // numero totale pagine pdf, copertina compresa
				bool addShims = this.IsDoubleSided; // Fronte-retro
				int actPaperPages = addShims
					? (int)Math.Ceiling(((double)totalPdfPages) / 2)
					: totalPdfPages;
				return actPaperPages;
			}
		}

		//-------------------------------------------------------------------------------
		public const int MaxPaperPages = 50;
	}
}
