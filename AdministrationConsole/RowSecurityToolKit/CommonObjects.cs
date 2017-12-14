using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.RowSecurityToolKit
{
	// Costanti
	//================================================================================
	public class ConstStrings
	{
		public const string TableName		= "TableName";
		public const string Name			= "Name";
		public const string Selected		= "Selected";
		public const string CatalogTableInfo= "CatalogTableInfo";
		public const string ColumnName		= "ColumnName";
		public const string Company			= "Company";
		public const string CompanyId		= "CompanyId";
	}

	///<summary>
	/// Classe derivata per avere una struttura globale di entita' e relative tabelle referenziate
	///</summary>
	//================================================================================
	public class RSEntityInfo : RSEntity
	{
		// lista delle tabelle che referenziano l'entita' corrente 
		public List<RSTableInfo> RsTablesInfo = new List<RSTableInfo>();

		//--------------------------------------------------------------------------------
		public RSEntityInfo(RSEntity rse)
			: base(rse.Name, rse.MasterTableNamespace, rse.DocumentNamespace, rse.Description, rse.Priority, rse.ParentModuleInfo)
		{
			// assegno le colonne
			this.RsColumns = rse.RsColumns;
		}
	}

	///<summary>
	/// Classe derivata per avere la struttura delle tabelle referenziate da un'entita'
	///</summary>
	//================================================================================
	public class RSTableInfo : RSTable
	{
		// lista delle colonne della tabella che referenziano l'entita' corrente che si sta analizzando
		public List<RSColumns> RsColumns = new List<RSColumns>();

		//--------------------------------------------------------------------------------
		public RSTableInfo(RSTable rst)
			: base(rst.NameSpace)
		{
			// assegno la lista di TUTTE le entita' referenziate dalla tabella (quindi potrebbero essere piu' d'una)
			// ma se devo controllare le colonne, fa fede la lista RsColumns
			this.RsEntityBaseList = rst.RsEntityBaseList;
		}
	}

	//============================================================================
	public class RSRelatedTable
	{
		private string tblNamespace;
		public string TableNamespace { get { return tblNamespace; } }

		public List<List<string>> ColumnsList = new List<List<string>>();

		//---------------------------------------------------------------------------
		public RSRelatedTable(string tblNamespace)
		{
			this.tblNamespace = tblNamespace;
		}
	}

	#region Comparer
	// per ordinare alfabeticamente le colonne nella combobox
	//============================================================================
	public class CatalogColumnComparer : IComparer
	{
		//---------------------------------------------------------------------------
		int IComparer.Compare(Object cc1, Object cc2)
		{
			return (new CaseInsensitiveComparer(System.Globalization.CultureInfo.InvariantCulture)).Compare
				(
				((CatalogColumn)cc1).Name,
				((CatalogColumn)cc2).Name
				);
		}
	}

	// per ordinare alfabeticamente le tabelle nella combobox (con i generic)
	//=========================================================================
	public class CatalogEntryGenericComparer : IComparer<CatalogTableEntry>
	{
		//---------------------------------------------------------------------------
		public int Compare(CatalogTableEntry cte1, CatalogTableEntry cte2)
		{
			return string.Compare(cte1.TableName, cte2.TableName, StringComparison.InvariantCultureIgnoreCase);
		}
	}

	// per ordinare alfabeticamente le colonne nella combobox (con i generic)
	//=========================================================================
	public class CatalogColumnGenericComparer : IComparer<CatalogColumn>
	{
		//---------------------------------------------------------------------------
		public int Compare(CatalogColumn cc1, CatalogColumn cc2)
		{
			return string.Compare(cc1.Name, cc2.Name, StringComparison.InvariantCultureIgnoreCase);
		}
	}
	# endregion

	# region Classe per avere una cella nel DataGridView con il pulsante di Check
	//================================================================================
	public class DataGridViewCheckImageButtonCell : DataGridViewImageButtonCell
	{
		//--------------------------------------------------------------------------------
		public override void LoadImages()
		{
			this.ToolTipText = string.Empty;
			_buttonImageNormal = Strings.Check_unselected;
			_buttonImageHot = Strings.Check_selected;
		}
	}

	//================================================================================
	public class DataGridViewCheckImageButtonColumn : DataGridViewButtonColumn
	{
		//--------------------------------------------------------------------------------
		public DataGridViewCheckImageButtonColumn()
		{
			this.CellTemplate = new DataGridViewCheckImageButtonCell();
			this.Width = 22;
			this.Resizable = DataGridViewTriState.False;
			this.ToolTipText = string.Empty;
		}
	}
	# endregion

	# region DataGridViewImageButtonCell abstract class
	// An abstract class that implements the functionality of an image button
	// except for a single abstract method to load the Normal, Hot and Disabled 
	// images that represent the icon that is displayed on the button. The loading
	// of these images is done in each derived concrete class.
	//================================================================================
	public abstract class DataGridViewImageButtonCell : DataGridViewButtonCell
	{
		private bool _enabled;                // Is the button enabled
		private PushButtonState _buttonState; // What is the button state
		protected Image _buttonImageHot;      // The hot image
		protected Image _buttonImageNormal;   // The normal image
		protected Image _buttonImageDisabled; // The disabled image
		private int _buttonImageOffset;       // The amount of offset or border around the image

		//--------------------------------------------------------------------------------
		protected DataGridViewImageButtonCell()
		{
			// In my project, buttons are enabled by default
			_enabled = true;
			_buttonState = PushButtonState.Normal;

			// Changing this value affects the appearance of the image on the button.
			_buttonImageOffset = 2;

			// Call the routine to load the images specific to a column.
			LoadImages();
		}

		// Button Enabled Property
		//--------------------------------------------------------------------------------
		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				_enabled = value;
				_buttonState = value ? PushButtonState.Normal : PushButtonState.Disabled;
			}
		}

		// PushButton State Property
		//--------------------------------------------------------------------------------
		public PushButtonState ButtonState { get { return _buttonState; } set { _buttonState = value; } }

		// Image Property
		// Returns the correct image based on the control's state.
		//--------------------------------------------------------------------------------
		public Image ButtonImage
		{
			get
			{
				switch (_buttonState)
				{
					case PushButtonState.Disabled:
						return _buttonImageDisabled;

					case PushButtonState.Hot:
						return _buttonImageHot;

					case PushButtonState.Normal:
						return _buttonImageNormal;

					case PushButtonState.Pressed:
						return _buttonImageNormal;

					case PushButtonState.Default:
						return _buttonImageNormal;

					default:
						return _buttonImageNormal;
				}
			}
		}

		//--------------------------------------------------------------------------------
		protected override void Paint(Graphics graphics,
					Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
					DataGridViewElementStates elementState, object value,
					object formattedValue, string errorText,
					DataGridViewCellStyle cellStyle,
					DataGridViewAdvancedBorderStyle advancedBorderStyle,
					DataGridViewPaintParts paintParts)
		{
			//base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
			SolidBrush cellBackground = null;

			// Draw the cell background, if specified.
			if ((paintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
			{
				cellBackground = new SolidBrush(cellStyle.BackColor);
				graphics.FillRectangle(cellBackground, cellBounds);
				cellBackground.Dispose();
			}

			// Draw the cell borders, if specified.
			if ((paintParts & DataGridViewPaintParts.Border) == DataGridViewPaintParts.Border)
				PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);

			// Calculate the area in which to draw the button.
			// Adjusting the following algorithm and values affects
			// how the image will appear on the button.
			Rectangle buttonArea = cellBounds;

			Rectangle buttonAdjustment = BorderWidths(advancedBorderStyle);

			buttonArea.X += buttonAdjustment.X;
			buttonArea.Y += buttonAdjustment.Y;
			buttonArea.Height -= buttonAdjustment.Height;
			buttonArea.Width -= buttonAdjustment.Width;

			Rectangle imageArea = new Rectangle(buttonArea.X + _buttonImageOffset, buttonArea.Y + _buttonImageOffset, 16, 16);

			ButtonRenderer.DrawButton(graphics, buttonArea, ButtonImage, imageArea, false, ButtonState);
			cellBackground.Dispose();
		}

		// An abstract method that must be created in each derived class.
		// The images in the derived class will be loaded here.
		//--------------------------------------------------------------------------------
		public abstract void LoadImages();
	}
	# endregion

	# region CRSFunctions (metodi statici per Encrypt / Decrypt files)
	///<summary>
	/// Sfrutta la classe Crypto in Generic
	/// Si occupa di criptare e decriptare file con l'algoritmo TripleDES
	///</summary>
	//================================================================================
	public static class CRSFunctions
	{
		///<summary>
		/// Metodo che si occupa di eseguire l'encrypt di un file
		/// Viene caricato in memoria il contenuto del file da criptare
		/// Viene criptato e salvato nella stessa directory creando un file con lo stesso nome
		/// e l'estensione .crs
		/// Nel caso in cui il file .crs esista gia' viene sovrascritto
		///</summary>
		//--------------------------------------------------------------------------------
		public static bool Encrypt(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				return false;

			byte[] content = File.ReadAllBytes(filePath);
			byte[] encryptedContent = Crypto.EncryptToByteArray(content);
			string newFile = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + NameSolverStrings.CrsExtension);

			if (File.Exists(newFile))
			{
				File.SetAttributes(newFile, FileAttributes.Normal);
				File.Delete(newFile);
			}

			using (FileStream s = new FileStream(newFile, FileMode.Create))
				s.Write(encryptedContent, 0, encryptedContent.Length);

			File.SetAttributes(newFile, FileAttributes.ReadOnly);

			return true;
		}

		///<summary>
		/// Metodo che si occupa di eseguire il decrypt di un file .crs
		/// Viene caricato in memoria il contenuto del file da criptare
		/// Viene criptato e salvato nella stessa directory creando un file con lo stesso nome
		/// e l'estensione .xml
		/// Nel caso in cui il file .xml esista gia' viene sovrascritto
		///</summary>
		//--------------------------------------------------------------------------------
		public static bool DecryptFile(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				return false;

			if (Path.GetExtension(filePath).ToUpperInvariant() != NameSolverStrings.CrsExtension.ToUpperInvariant())
				return false;

			byte[] content = File.ReadAllBytes(filePath);
			byte[] decryptedContent = Crypto.DecryptToByteArray(content);
			string newFile = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + NameSolverStrings.XmlExtension);

			if (File.Exists(newFile))
			{
				File.SetAttributes(newFile, FileAttributes.Normal);
				File.Delete(newFile);
			}

			using (FileStream s = new FileStream(newFile, FileMode.Create))
				s.Write(decryptedContent, 0, decryptedContent.Length);

			File.SetAttributes(newFile, FileAttributes.ReadOnly);

			return true;
		}
	}
	# endregion
}
