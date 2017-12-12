using Microarea.TaskBuilderNet.Core.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{
    //================================================================================
    public class WndObjDescriptionContainerRoot : WndObjDescriptionContainer
	{
		private bool childrenChanged = false; //indica se e' cambiato il numero delle finestre figlie

		
		public string FocusId = null;
		internal TBWebFormControl.ActionCode ActionCode;

		//--------------------------------------------------------------------------------------
		public bool ChildrenChanged
		{
			get { return childrenChanged; }
			set { childrenChanged = value; }
		}

		//--------------------------------------------------------------------------------------
		public void LoadBinary (byte[] bytes)
		{
			using (BinaryParser parser = new BinaryParser(new MemoryStream(bytes)))
			{
				ActionCode = (TBWebFormControl.ActionCode)parser.ParseInt();
				FromStream(parser);
				FocusId = parser.ParseString();
				Debug.Assert(parser.EOF);
			}
		}

		///<summary>
		/// Metodo che aggiorna l'albero delle descrizioni, a partire dalla lista dei delta.
		/// Nella lista dei delta possono esserci descrizioni:
		/// WndDescriptionState.ADDED: descrizioni di nuove finestre, vanno aggiunte alla descrizione padre
		/// WndDescriptionState.REMOVED: descrizione di finestre rimosse, vanno rimosse dalla descrizione padre
		/// WndDescriptionState.UPDATED: descrizione di finestre cambiate rispetto al round trip precedente, vanno aggiornate
		/// </summary>
		
		//--------------------------------------------------------------------------------------
		internal void UpdateFromDelta(WndObjDescriptionContainer deltaStructure)
		{
			//nella lista dei delta vi sono prima i controlli padre rispetto ai controlli figli,
			//in modo che al momento della creazione/rimozione di una descrizione, il suo padre sia gia' presente
			for (int i = 0; i < deltaStructure.Count; i++)
			{
				//prendo la descrizione che mi arriva nella lista dei delta
				WndObjDescription deltaDesc = deltaStructure[i] as WndObjDescription;
				if (deltaDesc == null)
					continue;

				//cerco la corrispondente descrizione nell'albero delle descrizioni completo
				//(nel caso di descrizione di finestra rimossa non e' presente)
				WndObjDescription desc = Find(deltaDesc.Id);
				switch (deltaDesc.State)
				{
					case WndDescriptionState.REMOVED:
					{
						bool bRemoved = false;
						//cerco la descrizione della finestra padre, rimuovo questa descrizione dalle sue descrizioni figlie
						//e imposto il booleano che indica che i figli sono cambiati 

						if (desc != null)
						{
							WndObjDescription parentDesc = Find(deltaDesc.ParentId);
							if (parentDesc != null)
							{
								bRemoved = parentDesc.Childs.Remove(desc);
								Debug.Assert(bRemoved);
								parentDesc.ChildrenChanged = true;
							}
							//se non ho trovato la descrizione padre, e' una finestre figlia di "root", cioe' figlia diretta del TbWebFormControl
							else
							{
								bRemoved = Remove(desc);
								Debug.Assert(bRemoved);
								ChildrenChanged = true;
							}
						}
						break;
					}
					case WndDescriptionState.UNCHANGED:
					{
						Debug.Assert(false);//non devono arrivare delta di una desc se non e' cambiato niente
						break;
					}
					case WndDescriptionState.UPDATED:
					{
						//aggiorno la descrizione precedente con la nuova
						if (desc != null)
							desc.Assign(deltaDesc);
						break;
					}
					case WndDescriptionState.ADDED:
					{
						//cerco la descrizione della finestra padre, aggiongo questa descrizione dalle sue descrizioni figlie
						//e imposto il booleano che indica che i figli sono cambiati 
						WndObjDescription parentDesc = Find(deltaDesc.ParentId);
						if (parentDesc != null)
						{
							parentDesc.Childs.Add(deltaDesc);
							parentDesc.ChildrenChanged = true;
						}
						//se non ho trovato la descrizione padre, e' una finestre figlia di "root", cioe' figlia diretta del TbWebFormControl
						else
						{
							ChildrenChanged = true; 
							Add(deltaDesc.Clone());
						}
						break;
					}
				}
			}
		}
	}

	//================================================================================
	public class WndObjDescriptionContainer : List<GenericDescription>
	{
		//--------------------------------------------------------------------------------------
		internal void FromStream (BinaryParser parser)
		{
			int count = parser.ParseInt();
			for (int i = 0; i < count; i++)
			{
				try
				{
					GenericDescription desc = GenericDescription.GetWndObjDescription(parser);
					desc.FromStream(parser);
					Add(desc);

				}
				catch
				{
					throw;
				}
			}
		}

		///<summary>
		///Restituisce true se la descrizione che ha id uguale all'id passato come parametro
		///e' presente nelle descrizioni
		/// </summary>
		//--------------------------------------------------------------------------------------
		protected bool Contains(string id)
		{
			foreach (WndObjDescription desc in this)
			{
				if (id == desc.Id)
					return true;
				bool found = desc.Childs.Contains(id);
				if (found)
				 return true;
			}

			return false;
		}

		///<summary>
		///Restituisce la descrizione che ha id uguale all'id passato come parametro se
		///e' presente nelle descrizioni
		/// </summary>
		//--------------------------------------------------------------------------------------
		protected WndObjDescription Find(string id)
		{
			foreach (WndObjDescription desc in this)
			{
				if (id == desc.Id)
					return desc;
				WndObjDescription oldDesc = desc.Childs.Find(id);
				if (oldDesc != null)
					return oldDesc;
			}
			return null;
		}
	}

	//================================================================================
	public class ImageBuffer
	{
		public string Id;
		public byte[] Buffer;
		
		public bool IsEmpty { get { return Buffer.Length == 0; } }
		
		public Bitmap CreateBitmap ()
		{
			MemoryStream ms = new MemoryStream(Buffer);
			return new Bitmap(ms);
		}
		//--------------------------------------------------------------------------------
		internal void FromStream (BinaryParser parser)
		{
			Id = parser.ParseString();
			int size = parser.ParseInt();
			Buffer = parser.ParseBytes(size);
		}
		//--------------------------------------------------------------------------------
		public override bool Equals (object obj)
		{
			ImageBuffer target = (ImageBuffer)obj;
			return Id == target.Id && Buffer.Equals(target.Buffer);
		}
		//--------------------------------------------------------------------------------
		public override int GetHashCode ()
		{
			return Id.GetHashCode() + Buffer.GetHashCode();
		}

		//--------------------------------------------------------------------------------------
		internal virtual void Assign(ImageBuffer clone)
		{
			this.Id = clone.Id;
			Buffer = clone.Buffer;
		}
	}

	//================================================================================
	public abstract class GenericDescription
	{
		//--------------------------------------------------------------------------------------
		public static GenericDescription GetWndObjDescription(BinaryParser parser)
		{
			string runtimeclass = parser.ParseAnsiString();
			if (runtimeclass.StartsWith("C"))
				runtimeclass = runtimeclass.Substring(1);
			return (GenericDescription)Activator.CreateInstance(Type.GetType("Microarea.TaskBuilderNet.UI.TBWebFormControl." + runtimeclass, true));
		}

		//--------------------------------------------------------------------------------
		internal abstract void FromStream (BinaryParser parser);
	}
    
	//================================================================================
	public class AcceleratorDescription : GenericDescription
	{
		List<Accelerator> accelerators;
		public string WindowId;
		//--------------------------------------------------------------------------------------
		internal List<Accelerator> Accelerators
		{
			get { return accelerators; }
		}

		//--------------------------------------------------------------------------------------
		public AcceleratorDescription ()
		{

		}

		//--------------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			int count = parser.ParseInt();
			accelerators = new List<Accelerator>(count);
			for (int i = 0; i < count; i++)
			{
				Accelerator acc = new Accelerator(parser.ParseInt16(), parser.ParseInt16(), parser.ParseInt32());
				accelerators.Add(acc);
			}
		}
	}

	//==============================================================================
	public class FontDescription : GenericDescription
	{
		public string FontFaceName;
		public int FontHeight;
		public bool Bold;
		public bool Italic;

		//--------------------------------------------------------------------------------
		internal override void FromStream(BinaryParser parser)
		{
			FontFaceName = parser.ParseString();
			FontHeight = parser.ParseInt();
			Bold = parser.ParseBool();
			Italic = parser.ParseBool();
		}
	}

	//TENERE ALLINEATO QUESTO ENUMERATIVO CON QUELLO IN TaskBuilder\Framework\TbGeneric\WndObjDescription.h
	public enum WndDescriptionState { REMOVED, UNCHANGED, UPDATED, ADDED };
	public enum TextAlignment { NONE = -1, LEFT, CENTER, RIGHT };
    public enum SplitterMode { S_VERTICAL, S_HORIZONTAL };
    public enum CommandCategory { CTG_UNDEFINED, CTG_EDIT, CTG_NAVIGATION, CTG_SEARCH, CTG_PRINT, CTG_TOOLS, CTG_ADVANCED };

    //================================================================================
    public class WndObjDescription : GenericDescription
	{
		//************ATTENZIONE************
		//TENERE ALLINEATO QUESTO ENUMERATIVO CON QUELLO IN TaskBuilder\Framework\TbGeneric\WndObjDescription.h
        public enum WndObjType
        {
			Undefined = 0, View = 1, Label = 2, Button = 3, PdfButton = 4, BeButton = 5, BeButtonRight = 6,
			SaveFileButton = 7, Image = 8, Group = 9, Radio = 10, Check = 11, Combo = 12, Edit = 13, Toolbar = 14,
			ToolbarButton = 15, Tabber = 16, Tab = 17, BodyEdit = 18, Radar = 19, HotLink = 20, Table = 21,
			ColTitle = 22, Cell = 23, List = 24, CheckList = 25, Tree = 26, TreeNode = 27, Menu = 28, MenuItem = 29,
			ListCtrl = 30, ListCtrlItem = 31, ListCtrlDetails = 32, Spin = 33, /*Report = 34,*/ StatusBar = 35, SbPane = 36,
			/*Title = 37 non più usata, il titolo è nella proprietà text del'oggetto, il rettangolo del titolo non ci interessa più*/
			MainMenu = 38, AuxRadarToolbar = 39, Frame = 40, RadarFrame = 41, PrintDialog = 42, Dialog = 43,
			PropertyDialog = 44, GenericWndObj = 45, RadarHeader = 46, FileDialog = 47, BETreeCell = 48, BtnImageAndText = 49,
			MultiChart = 50, TreeAdv = 51, TreeNodeAdv = 52, MailAddressEdit = 53, WebLinkEdit = 54, AddressEdit = 55,
			/*FieldReport = 56, TableReport = 57,*/
			EasyBuilderToolbar = 58, FloatingToolbar = 59, MSCombo = 60, UploadFileButton = 61, Thread = 62,
			ProgressBar = 63, CaptionBar = 64, RadioGroup = 65, Panel = 66, TabbedToolbar = 67, NamespaceEdit = 68,
			Constants = 69,
			//tile description type
			Tile = 71,
			//tile group type
			TileGroup = 72,
			//smaller element contained in a tile 
			TilePart = 73,
			//static section of tile part (usually contains labels)
			TilePartStatic = 74,
			//content section of tile part (usually contains input control)
			TilePartContent = 75,
			TileManager = 76,
			TilePanel = 77,
			LayoutContainer = 78,
			HeaderStrip = 79,
			PropertyGrid = 80,
			PropertyGridItem = 81,
			TreeBodyEdit = 82,
			StatusTile = 83,
			HotFilter = 84,
			StatusTilePanel = 85,
			Splitter = 86,
			DockingPane = 87
		}
		private bool childrenChanged = false;
		public string ParentId;
		public string Id;
		public string Cmd;
		public string Tooltip;
		public bool HasTabIndex;
        public WndObjType Type;
        public string Text;
		public WndDescriptionState State;
        public bool HasFocus;
		public int X;
		public int Y;
		public int Width;
		public int Height;
		public bool Enabled = true;
		public AcceleratorDescription Accelerator = null;
		public FontDescription Font = null;
		public WndObjDescriptionContainer Childs = new WndObjDescriptionContainer();


		///<summary>
		/// Ritorna true se sono state aggiunte o rimosse descrizioni di finestre figlie
		/// </summary>
		//--------------------------------------------------------------------------------
		public bool ChildrenChanged
		{
			get { return childrenChanged; }
			set { childrenChanged = value; }
		}

		//--------------------------------------------------------------------------------
		public bool HasAccelerator { get { return Accelerator != null; } }

		//--------------------------------------------------------------------------------
		public bool HasCustomFont { get { return Font != null; } }

		//--------------------------------------------------------------------------------------
		public override string ToString ()
		{
			return string.Format("Id: '{0}'; Type: '{1}'; Text: '{2}'", Id, Type, Text);
		}

		//--------------------------------------------------------------------------------------
		public void LoadBinary (byte[] bytes)
		{
			using (BinaryParser parser = new BinaryParser(new MemoryStream(bytes)))
			{
				string runtimeclass = parser.ParseAnsiString();
				if (runtimeclass.StartsWith("C"))
					runtimeclass = runtimeclass.Substring(1);
				Debug.Assert(runtimeclass == GetType().Name);
				FromStream(parser);
				Debug.Assert(parser.EOF);
			}
		}
		//--------------------------------------------------------------------------------------
		public WndObjDescription Find (string id)
		{
			if (this.Id == id)
				return this;

			WndObjDescription found = null;
			foreach (WndObjDescription child in Childs)
				if ((found = child.Find(id)) != null)
					return found;
			return null;
		}

        //--------------------------------------------------------------------------------------
        public TBWebControl CreateControl()
        {
            switch (this.Type)
            {
                case WndObjDescription.WndObjType.View: return new TBView();
                case WndObjDescription.WndObjType.Label: return new TBLabel();
                case WndObjDescription.WndObjType.Button: return new TBButton();
                case WndObjDescription.WndObjType.PdfButton: return new TBPdfButton();
                case WndObjDescription.WndObjType.BeButton:
                case WndObjDescription.WndObjType.BeButtonRight: return new TBBodyEditButton();
                case WndObjDescription.WndObjType.SaveFileButton: return new TBSaveFileButton();
				case WndObjDescription.WndObjType.UploadFileButton: return new TBUploadFileButton();
				case WndObjDescription.WndObjType.BtnImageAndText: return new TBTextImageButton();
                case WndObjDescription.WndObjType.Image: return new TBImage();
                case WndObjDescription.WndObjType.Group: return new TBGroupBox();
                case WndObjDescription.WndObjType.Radio: return new TBRadioButton();
                case WndObjDescription.WndObjType.Check: return new TBCheckBox();
                case WndObjDescription.WndObjType.Combo: return new TBDropDownList();
				case WndObjDescription.WndObjType.MSCombo: return new TBMSDropDownList();
                case WndObjDescription.WndObjType.Edit: return new TBTextBox();
                case WndObjDescription.WndObjType.Toolbar:
                case WndObjDescription.WndObjType.EasyBuilderToolbar: return new TBToolbar();
				case WndObjDescription.WndObjType.FloatingToolbar: return new TBFloatingToolbar();
                case WndObjDescription.WndObjType.ToolbarButton:
                    {
                        if (((ToolbarBtnDescription)this).IsTabbedToolbarButton)
                            return new TBTabbedToolbarButton();
                        else
                            return new TBToolbarButton();
                    }
                case WndObjDescription.WndObjType.Tabber: return new TBTabber();
                case WndObjDescription.WndObjType.BodyEdit: return new TBGridContainer();
                case WndObjDescription.WndObjType.Radar: return new TBRadarGrid();
                case WndObjDescription.WndObjType.HotLink: return new TBHotLink();
                case WndObjDescription.WndObjType.Table: return new TBGridTable();
                case WndObjDescription.WndObjType.ColTitle: return new TBGridColTitle();
                case WndObjDescription.WndObjType.Cell: return new TBGridCell();
				case WndObjDescription.WndObjType.BETreeCell: return new TBGridTreeCell();
                case WndObjDescription.WndObjType.List: return new TBListBox();
                case WndObjDescription.WndObjType.CheckList: return new TBChecklListBox();
                case WndObjDescription.WndObjType.Tree: return new TBTreeView();
                case WndObjDescription.WndObjType.TreeNode: return new TBTreeNode();
				case WndObjDescription.WndObjType.TreeAdv: return new TBTreeView();
				case WndObjDescription.WndObjType.TreeNodeAdv: return new TBTreeNodeAdv();
                case WndObjDescription.WndObjType.Menu: return new TBMenu();
                case WndObjDescription.WndObjType.ListCtrl: return new TBListControl();
                case WndObjDescription.WndObjType.ListCtrlDetails: return new TBListControlDetails();
                case WndObjDescription.WndObjType.Spin: return new TBSpinControl();
                case WndObjDescription.WndObjType.StatusBar: return new TBPanel();
                case WndObjDescription.WndObjType.SbPane: return new TbStatusBarPane();
				case WndObjDescription.WndObjType.MultiChart: return new TBMultiChart();
				case WndObjDescription.WndObjType.MailAddressEdit : return new TBMailAddressTextBox();
				case WndObjDescription.WndObjType.WebLinkEdit: return new TBWebLinkTextBox();
				case WndObjDescription.WndObjType.AddressEdit: return new TBAddressTextBox();
                case WndObjDescription.WndObjType.ProgressBar: return new TBProgressBar();
                case WndObjDescription.WndObjType.TabbedToolbar: return new TBTabbedToolbar();
                case WndObjDescription.WndObjType.Panel: return new TBDummyPanel();

                //menu di frame, titolo di frame e toolbar di editazione del radar non generano un webcontrol,
                //ma vengono solo usate per calcoli di tipo spaziale nel disegno della form
                case WndObjDescription.WndObjType.AuxRadarToolbar:
                case WndObjDescription.WndObjType.RadarHeader:
                case WndObjDescription.WndObjType.MainMenu: return null;

                // le description di primo livello (principali) non generano Panels
                case WndObjDescription.WndObjType.Frame         : return null;
                case WndObjDescription.WndObjType.RadarFrame    : return null;
                case WndObjDescription.WndObjType.Dialog        : return null;
                case WndObjDescription.WndObjType.PropertyDialog: return null;
                case WndObjDescription.WndObjType.PrintDialog   : return null;
				case WndObjDescription.WndObjType.FileDialog	: return null;

                default: return new TBPanel();
            }
        }

        //devo controllare se e' stato creato un webcontrol a partire dalla descrizione.
        //Alcune descrizioni non generano controlli (es. menu di frame e titolo di frame) ma vengono solo usate
        //per calcoli di tipo spaziale nel disegno della form
        //--------------------------------------------------------------------------------------
        public TBWebControl CreateControl(TBWebControl tbWebControl)
        {
            TBWebControl control = CreateControl();
            if (control != null)
                control.SetInitValues(tbWebControl.FormControl, tbWebControl, this);
            return control;
        }

		//--------------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			ParentId = parser.ParseString();
			Id = parser.ParseString();
			Cmd = parser.ParseString();
			Type = (WndObjType)parser.ParseInt();
			Text = parser.ParseString();
			Tooltip = parser.ParseString();
			HasTabIndex = parser.ParseBool();
			HasFocus = parser.ParseBool();
			State = (WndDescriptionState)parser.ParseInt();
			X = parser.ParseInt();
			Y = parser.ParseInt();
			Width = parser.ParseInt();
			Height = parser.ParseInt();
			Enabled = parser.ParseBool();
			if (parser.ParseBool())
			{
				Accelerator = new AcceleratorDescription();
				Accelerator.FromStream(parser);
			}
			if (parser.ParseBool())
			{
				Font = new FontDescription();
				Font.FromStream(parser);
			}
		}

		//--------------------------------------------------------------------------------------
		internal string HtmlTextAttribute { get { return Text.TrimAmpersand().TrimTab(); } }

		//--------------------------------------------------------------------------------------
		internal virtual WndObjDescription Clone ()
		{
			return this.MemberwiseClone() as WndObjDescription;
		}

		//--------------------------------------------------------------------------------------
		internal virtual void Assign (WndObjDescription clone)
		{
			this.Cmd = clone.Cmd;
			this.Enabled = clone.Enabled;
			this.Height = clone.Height;
			this.Id = clone.Id;
			this.Text = clone.Text;
			this.Tooltip = clone.Tooltip;
			this.HasTabIndex = clone.HasTabIndex;
            this.HasFocus = clone.HasFocus;
			this.Type = clone.Type;
			this.Width = clone.Width;
			this.X = clone.X;
			this.Y = clone.Y;
			this.ChildrenChanged = clone.ChildrenChanged;
			this.State = clone.State;
		}
	}


	//==============================================================================
	public class WndColoredObjDescription : WndObjDescription
	{
		public Color			BackgroundColor = Color.Empty;
		public Color			TextColor = Color.Empty;
		//Messo qui e non in TextObjDescription perche' anche la groupbox e' soggetta all'allineamento della caption,
		//ed e' mappata con una WndColoredObjDescription 
		public TextAlignment	TextAlignment = TextAlignment.LEFT ;


		//--------------------------------------------------------------------------------
		internal override void FromStream(BinaryParser parser)
		{
			base.FromStream(parser);
			
			if (parser.ParseBool())
				BackgroundColor = Color.FromArgb(parser.ParseInt());

			if (parser.ParseBool())
				TextColor = Color.FromArgb(parser.ParseInt());

			TextAlignment = (TextAlignment)parser.ParseInt();
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			WndColoredObjDescription coloredObjDesc = (WndColoredObjDescription)clone;
			BackgroundColor = coloredObjDesc.BackgroundColor;
			TextColor = coloredObjDesc.TextColor;
			TextAlignment = coloredObjDesc.TextAlignment;
		}
	}

	//==============================================================================
	public class WndImageDescription : WndColoredObjDescription
	{
		public ImageBuffer Image;

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			Image = new ImageBuffer();
			Image.FromStream(parser);
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			Image.Assign(((WndImageDescription)clone).Image);
		}
	}

	//==============================================================================
	public class WndButtonDescription : WndImageDescription
	{
	}


	//==============================================================================
	public class LinkDescription : GenericDescription
	{
		public int ObjectAlias;
		public int Row;
		public int X;
		public int Y;
		public int Width;
		public int Height;

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			ObjectAlias = parser.ParseInt();
			Row = parser.ParseInt();
			X = parser.ParseInt();
			Y = parser.ParseInt();
			Width = parser.ParseInt();
			Height = parser.ParseInt();
		}
	}
	
    //==============================================================================
	public class WndReportDescription : WndImageDescription
	{
		public List<LinkDescription> Links = new List<LinkDescription>();

		//--------------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			int count = parser.ParseInt();
			for (int i = 0; i < count; i++)
			{
				GenericDescription item = GenericDescription.GetWndObjDescription(parser);
				item.FromStream(parser);
				Links.Add((LinkDescription)item);
			}
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			Links = ((WndReportDescription)clone).Links;
		}
	}

	//==============================================================================
	public class WndPdfButtonDescription : WndButtonDescription
	{
		public string PdfFile;

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			PdfFile = parser.ParseString();
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			PdfFile = ((WndPdfButtonDescription)clone).PdfFile;
		}
	}

	//==============================================================================
	public class WndSaveFileButtonDescription : WndButtonDescription
	{
		public string Folder;

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			Folder = parser.ParseString();
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			Folder = ((WndSaveFileButtonDescription)clone).Folder;		
		}
	}

	//==============================================================================
	public class TextObjDescription : WndColoredObjDescription
	{
		public bool IsMultiline;
		public bool IsStatic;
		public bool IsHyperLink;
		public bool HasCalendar;
		
		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			IsMultiline = parser.ParseBool();
			IsStatic = parser.ParseBool();
			IsHyperLink = parser.ParseBool();
			HasCalendar = parser.ParseBool();
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			TextObjDescription textobjClone = (TextObjDescription)clone;
			IsMultiline = textobjClone.IsMultiline;
			IsStatic = textobjClone.IsStatic;
			IsHyperLink = textobjClone.IsHyperLink;
			HasCalendar = textobjClone.HasCalendar;
		}
	}

	//================================================================================
	public class TabberDescription : WndObjDescription
	{

	}

	//================================================================================
	public class TabDescription : WndImageDescription
	{
		public bool Active;
		public bool Protected; //indica se la tab e' protetta via security

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			Active = parser.ParseBool();
			Protected = parser.ParseBool();
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			Active = ((TabDescription)clone).Active;
			Protected = ((TabDescription)clone).Protected;
		}
	}

	//================================================================================
	public class WndCheckRadioDescription : WndButtonDescription
	{
		public bool Checked;

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			Checked = parser.ParseBool();
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			Checked = ((WndCheckRadioDescription)clone).Checked;
		}
	}

	//================================================================================
	public abstract class ItemDescription : GenericDescription
	{
	}

	//================================================================================
	public class ItemListBoxDescription : ItemDescription
	{
		public string Text;
		public bool Selected;

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			Text = parser.ParseString();
			Selected = parser.ParseBool();
		}
	}

	//================================================================================
	public class ItemCheckListDescription : ItemListBoxDescription
	{
		public bool Disabled;
		public bool Checked;

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			Checked = parser.ParseBool();
			Disabled = parser.ParseBool();
		}
	}

	//================================================================================
	public class ListDescription : WndObjDescription
	{
		public List<ItemDescription> Items = new List<ItemDescription>();

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);

			int count = parser.ParseInt();
			for (int i = 0; i < count; i++)
			{
				GenericDescription item = GenericDescription.GetWndObjDescription(parser);
				item.FromStream(parser);
				Items.Add((ItemDescription)item);
			}
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			ListDescription listClone = (ListDescription)clone;
			Items = listClone.Items;
		}
	}

	//================================================================================
	public class WndTreeCtrlDescription : WndImageDescription
	{
		public int IconHeight;
		public int Icons;

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			IconHeight = parser.ParseInt();
			Icons = parser.ParseInt();
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			WndTreeCtrlDescription treeClone = (WndTreeCtrlDescription)clone;
			IconHeight = treeClone.IconHeight;
			Icons = treeClone.Icons;
		}
	}
	//================================================================================
	class WndTreeNodeDescription : WndImageDescription
	{
		public bool Expanded;
		public bool Selected;
		public bool HasChild;
		public int IdxIcon;
		public int IdxSelectedIcon;
		public bool HasStateImg;

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			Expanded = parser.ParseBool();
			Selected = parser.ParseBool();
			HasChild = parser.ParseBool();
			IdxIcon = parser.ParseInt();
			IdxSelectedIcon = parser.ParseInt();
			HasStateImg = parser.ParseBool();
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			WndTreeNodeDescription treeNodeClone = (WndTreeNodeDescription)clone;
			Expanded = treeNodeClone.Expanded;
			Selected = treeNodeClone.Selected;
			HasChild = treeNodeClone.HasChild;
			IdxIcon = treeNodeClone.IdxIcon;
			IdxSelectedIcon = treeNodeClone.IdxSelectedIcon;
			HasStateImg = treeNodeClone.HasStateImg;
		}
	}

	//================================================================================
	public class ToolbarDescription : WndButtonDescription
	{
		public int ImageHeight;

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			ImageHeight = parser.ParseInt();
		}
	}

	//================================================================================
	public class ToolbarBtnDescription : WndObjDescription
	{
		public int Image;
		public bool Dropdown;
		public bool IsCheckButton;
		public bool IsPressed;
        public bool HasMenu;  //significato simile a DropDown, bottone che ha un menu apribile, ma della TabbedToolbar (compatibilita' con parte server di WebLook)
        public ImageBuffer ImageContent;
        public bool IsTabbedToolbarButton;

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			Image = parser.ParseInt();
			Dropdown = parser.ParseBool();
			IsCheckButton = parser.ParseBool();
			IsPressed = parser.ParseBool();
            HasMenu = parser.ParseBool();
            IsTabbedToolbarButton = parser.ParseBool();
            ImageContent = new ImageBuffer();
            ImageContent.FromStream(parser);
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			ToolbarBtnDescription toolbarBtnClone = (ToolbarBtnDescription)clone;
			Image = toolbarBtnClone.Image;
			Dropdown = toolbarBtnClone.Dropdown;
			IsCheckButton = toolbarBtnClone.IsCheckButton;
			IsPressed = toolbarBtnClone.IsPressed;
            HasMenu = toolbarBtnClone.HasMenu;
            IsTabbedToolbarButton = toolbarBtnClone.IsTabbedToolbarButton;
		}
	}

    //================================================================================
    //Tabbed Toolbar description 
    //================================================================================
    //================================================================================
    public class TabbedToolbarDescription : WndObjDescription
    {
        public int ActiveIndex;

        //--------------------------------------------------------------------------------
        internal override void FromStream(BinaryParser parser)
        {
            base.FromStream(parser);
            ActiveIndex = parser.ParseInt();
        }

        //--------------------------------------------------------------------------------
        internal override void Assign(WndObjDescription clone)
        {
            base.Assign(clone);
            ActiveIndex = ((TabbedToolbarDescription)clone).ActiveIndex;
        }
    }





	//================================================================================
	public class ComboDescription : TextObjDescription
	{
		public bool ReadOnly; //per combo che hanno stile CBS_DROPDOWN

		//--------------------------------------------------------------------------------
		internal override void FromStream(BinaryParser parser)
		{
			base.FromStream(parser);
			ReadOnly = parser.ParseBool();
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			ComboDescription comboClone = (ComboDescription)clone;
			ReadOnly = comboClone.ReadOnly;
		}
	}

	//================================================================================
	public class MSComboDescription : TextObjDescription
	{
		public string ComboTxt;
		public bool ReadOnly; //per combo che hanno stile CBS_DROPDOWN

		//--------------------------------------------------------------------------------
		internal override void FromStream(BinaryParser parser)
		{
			base.FromStream(parser);

			ComboTxt = parser.ParseString();	// combo box string cntenute
			int count = parser.ParseInt();		// number of items 
			for (int i = 0; i < count; i++)
			{
				bool item = parser.ParseBool();
			}
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			MSComboDescription comboMSClone = (MSComboDescription)clone;
			ReadOnly = comboMSClone.ReadOnly;
			ComboTxt = comboMSClone.ComboTxt;
		}
	}


	//==============================================================================
	public class WndBodyElementDescription : WndObjDescription
	{
		public enum TAlign { TA_LEFT = 0, TA_RIGHT = 2, TA_CENTER = 6 } //SEE WINGDI.H
		public bool ActiveCell;
		public bool ActiveRow;
		public TAlign TextAlign;
		public bool IsCheckBox;
		public bool CheckBoxValue;
		public bool IsHyperLink;
		public Color BackgroundColor = Color.Empty;
		public Color TextColor = Color.Empty;
		public int Row; //numero di riga della cella
		public int Col; //numero colonna della cella

		//--------------------------------------------------------------------------------
		public bool AlignLeft { get { return TextAlign == TAlign.TA_LEFT; } }
		//--------------------------------------------------------------------------------
		public bool AlignCenter { get { return TextAlign == TAlign.TA_CENTER; } }
		//--------------------------------------------------------------------------------
		public bool AlignRight { get { return TextAlign == TAlign.TA_RIGHT; } }

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			ActiveCell = parser.ParseBool();
			ActiveRow = parser.ParseBool();
			TextAlign = (TAlign)parser.ParseInt();
			Row = parser.ParseInt();
			Col = parser.ParseInt();
			IsCheckBox = parser.ParseBool();
			if (IsCheckBox)
				CheckBoxValue = parser.ParseBool();

			IsHyperLink = parser.ParseBool();

			if (parser.ParseBool())
				BackgroundColor = Color.FromArgb(parser.ParseInt());

			if (parser.ParseBool())
				TextColor = Color.FromArgb(parser.ParseInt());
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			WndBodyElementDescription bodyElementClone = (WndBodyElementDescription)clone;
			ActiveCell = bodyElementClone.ActiveCell;
			ActiveRow = bodyElementClone.ActiveRow;
			TextAlign = bodyElementClone.TextAlign;
			IsCheckBox = bodyElementClone.IsCheckBox;
			CheckBoxValue = bodyElementClone.CheckBoxValue;
			IsHyperLink = bodyElementClone.IsHyperLink;
			BackgroundColor = bodyElementClone.BackgroundColor;
			TextColor = bodyElementClone.TextColor;
			Row = bodyElementClone.Row;
			Col = bodyElementClone.Col;
		}
	}


	//Descrizione usata per la celle del body edit ad albero. Ha le informazione che 
	//dicono se e' un nodo con figli, se e' in stato expanded/collapsed, e di che livello e'
	//==============================================================================
	public class WndBodyTreeElementDescription : WndBodyElementDescription
	{
		public bool HasChild;
		public bool IsExpanded;
		public int	Level;
	
		//--------------------------------------------------------------------------------
		internal override void FromStream(BinaryParser parser)
		{
			base.FromStream(parser);
			HasChild = parser.ParseBool();
			IsExpanded = parser.ParseBool();
			Level = parser.ParseInt();
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			WndBodyTreeElementDescription bodyElementClone = (WndBodyTreeElementDescription)clone;
			HasChild = bodyElementClone.HasChild;
			IsExpanded = bodyElementClone.IsExpanded;
			Level = bodyElementClone.Level;
		}
	}


	//==============================================================================
	public class WndBodyTableDescription : WndObjDescription
	{
		public int Rows;
		public int WebRowStart;
		public bool ReadOnly;

		//--------------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			Rows = parser.ParseInt();
			WebRowStart = parser.ParseInt();
			ReadOnly = parser.ParseBool();
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			WndBodyTableDescription bodyTableClone = (WndBodyTableDescription)clone;
			Rows = bodyTableClone.Rows;
			WebRowStart = bodyTableClone.WebRowStart;
			ReadOnly = bodyTableClone.ReadOnly;
		}
	}

	///<summary>
	/// Classe che rapresenta la descrizione din un BodyEdit
	/// </summary>
	//==============================================================================
	public class WndBodyDescription : WndObjDescription
	{
		public string RowsIndicator;
		public bool PrevRows;
		public bool NextRows;

		//--------------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			RowsIndicator = parser.ParseString();
			PrevRows = parser.ParseBool();
			NextRows = parser.ParseBool();
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			WndBodyDescription bodyClone = (WndBodyDescription)clone;
			RowsIndicator = bodyClone.RowsIndicator;
			PrevRows = bodyClone.PrevRows;
			NextRows = bodyClone.NextRows;
		}
	}

	//==============================================================================
	public class MenuDescription : WndObjDescription
	{
		public string Hwnd;

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			//Il menu viene sempre passato in modo completo, e non come delta difference, nello stream sono quindi salvati anche 
			//i suoi figli (le voci di menu)
			Childs.FromStream(parser);

			Hwnd = parser.ParseString();
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			Hwnd = ((MenuDescription)clone).Hwnd;
		}
	}

	//==============================================================================
	public class MenuItemDescription : WndObjDescription
	{
		public bool Checked;
		public MenuDescription SubMenu = null;

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			Checked = parser.ParseBool();

			if (parser.ParseBool())
			{
				SubMenu = (MenuDescription)GenericDescription.GetWndObjDescription(parser);
				SubMenu.FromStream(parser);
			}
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			MenuItemDescription menuItemClone = (MenuItemDescription)clone;
			Checked = menuItemClone.Checked;
			SubMenu = menuItemClone.SubMenu;
		}
	}

	//================================================================================
	public class WndRadarTableDescription : WndObjDescription
	{
		//================================================================================
		public class RadarRow
		{

			public bool Active;
			public List<string> Cells;

			//--------------------------------------------------------------------------------
			public RadarRow (int count, bool active)
			{
				Active = active;
				Cells = new List<string>(count);
			}

		}

		public List<RadarRow> Table;
		public bool Active;
		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			int r = parser.ParseInt();
			Table = new List<RadarRow>(r);
			for (int i = 0; i < r; i++)
			{
				bool active = parser.ParseBool();
				int c = parser.ParseInt();
				RadarRow row = new RadarRow(c, active);
				for (int k = 0; k < c; k++)
					row.Cells.Add(parser.ParseString());
				Table.Add(row);
			}
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			WndRadarTableDescription radarTableClone = (WndRadarTableDescription)clone;
			Active = radarTableClone.Active;
			Table = radarTableClone.Table;
		}
	}

	//=========================================================================
	public class ListCtrlItemDescription : WndObjDescription
	{
		public int IdxItem;
		public bool Selected;
		public List<WndObjDescription> Cells = new List<WndObjDescription>();

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			IdxItem = parser.ParseInt();
			Selected = parser.ParseBool();

			int count = parser.ParseInt();
			for (int i = 0; i < count; i++)
			{
				WndObjDescription cell = (WndObjDescription)GenericDescription.GetWndObjDescription(parser);
				cell.FromStream(parser);
				Cells.Add(cell);
			}
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			ListCtrlItemDescription listCtrlItemClone = (ListCtrlItemDescription)clone;
			IdxItem = listCtrlItemClone.IdxItem;
			Selected = listCtrlItemClone.Selected;
			Cells = listCtrlItemClone.Cells;
		}
	}

	//=========================================================================
	public class ListCtrlDescription : WndImageDescription
	{
		public int IconHeight;
		public string ViewMode;
		public List<ListCtrlItemDescription> Items = new List<ListCtrlItemDescription>();
		public List<string> ColumnsHeaderText = new List<string>();

		//-------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			IconHeight = parser.ParseInt();
			ViewMode = parser.ParseString();

			int count = parser.ParseInt();
			for (int i = 0; i < count; i++)
				ColumnsHeaderText.Add(parser.ParseString());

			count = parser.ParseInt();
			for (int i = 0; i < count; i++)
			{
				GenericDescription item = GenericDescription.GetWndObjDescription(parser);
				item.FromStream(parser);
				Items.Add((ListCtrlItemDescription)item);
			}
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			ListCtrlDescription listCtrlClone = ((ListCtrlDescription)clone);
			IconHeight = listCtrlClone.IconHeight;
			ViewMode = listCtrlClone.ViewMode;
			Items = listCtrlClone.Items;
			ColumnsHeaderText = listCtrlClone.ColumnsHeaderText;
		}
	}

	///<summary>
	///Classe corrispondente alla classe c++ CWndMailAddressEditDescription. Visualizza l'indirizzo mail come 
	///cliccabile (mailto) se il link e' abilitato lato framework
	/// </summary>
	//================================================================================
	public class WndMailAddressEditDescription :TextObjDescription
	{
		public bool EnabledLink;

		//--------------------------------------------------------------------------------
		internal override void FromStream (BinaryParser parser)
		{
			base.FromStream(parser);
			EnabledLink = parser.ParseBool();
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			EnabledLink = ((WndMailAddressEditDescription)clone).EnabledLink;
		}
	}

	///<summary>
	///Classe corrispondente alla classe c++ CWndAddressEditDescription. Visualizza l'indirizzo  come 
	///cliccabile e rimanda sulla mappa di Google Maps
	/// </summary>
	//================================================================================
	public class WndAddressEditDescription : TextObjDescription
	{
		public string Url;

		//--------------------------------------------------------------------------------
		internal override void FromStream(BinaryParser parser)
		{
			base.FromStream(parser);
			Url = parser.ParseString();
		}

		//--------------------------------------------------------------------------------
		internal override void Assign(WndObjDescription clone)
		{
			base.Assign(clone);
			Url = ((WndAddressEditDescription)clone).Url;
		}
	}


	///<summary>
	///Classe corrispondente alla classe c++ CWndProgressBarDescription.
	/// </summary>
	//=========================================================================
	public class WndProgressBarDescription : WndObjDescription
	{
        public int Lower;
        public int Upper;
        public int Pos;

        //--------------------------------------------------------------------------------
        internal override void FromStream(BinaryParser parser)
        {
            base.FromStream(parser);
            Lower = parser.ParseInt();
            Upper = parser.ParseInt();
            Pos = parser.ParseInt();
        }

        //--------------------------------------------------------------------------------
        internal override void Assign(WndObjDescription clone)
        {
            base.Assign(clone);
            Lower = ((WndProgressBarDescription)clone).Lower;
            Upper = ((WndProgressBarDescription)clone).Upper;
            Pos = ((WndProgressBarDescription)clone).Pos;
        }

	}

    ///<summary>
    ///Classe corrispondente alla classe c++ CWndStatusBarDescription. Non aggiunge niente alla classe base, 
    ///ma lato TaskBuilder serve differenziarla per una diversa logica di aggiunta delle descrizioni degli
    ///elementi figli
    /// </summary>
    //=========================================================================
    public class WndStatusBarDescription : WndObjDescription
    {

    }

	///<summary>
	///Classe corrispondente alla classe c++ FileDialogDescription. Non aggiunge niente alla classe base, 
	///ma lato TaskBuilder serve differenziarla per una diversa logica di aggiunta delle descrizioni degli
	///elementi figli
	/// </summary>
	//=========================================================================
	public class FileDialogDescription : WndObjDescription
	{

	}

    ///<summary>
    ///Classe corrispondente alla classe c++ CWndTileDescription.
    /// </summary>
    //=========================================================================
    public class WndTileDescription : WndObjDescription
    {

    }

      ///<summary>
    ///Classe corrispondente alla classe c++ CLabelObjDescription.
    /// </summary>
    //=========================================================================
    public class LabelObjDescription : TextObjDescription
    {

    }

    

    ///<summary>
    ///Classe corrispondente alla classe c++ CCaptionBarDescription
    /// </summary>   
    //================================================================================
    public class CaptionBarDescription : WndButtonDescription
    {
        int textAlign;
        bool hasButton;
        int buttonAlign;
        int imageAlign;
        //--------------------------------------------------------------------------------
        internal override void FromStream(BinaryParser parser)
        {
            base.FromStream(parser);
            textAlign = parser.ParseInt();
            hasButton = parser.ParseBool();
            buttonAlign = parser.ParseInt();
            imageAlign = parser.ParseInt();
        }
    }

}
