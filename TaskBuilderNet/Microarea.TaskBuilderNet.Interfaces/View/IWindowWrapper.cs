using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Interfaces.View
{


    //-----------------------------------------------------------------------------
    [Flags]
    public enum EditingMode
    {
        None =0,
        Moving =1,
		ResizingMidRight = 2,
        ResizingTopLeft = 4,
        ResizingBottomLeft = 8,
        ResizingTopRight = 16,
        ResizingBottomRight = 32,
        ResizingMidTop = 64,
        ResizingMidBottom =128,
        ResizingMidLeft = 256,
		OnlyResizing = ResizingBottomRight | ResizingMidBottom | ResizingMidRight,
		All = ResizingTopLeft| ResizingBottomLeft| ResizingTopRight|
                ResizingBottomRight| ResizingMidTop| ResizingMidBottom|
                ResizingMidLeft| ResizingMidRight | Moving
    }

    //=============================================================================
    public interface IWindowWrapper : IWin32Window
	{
		string			Id					{ get; set; }
		INameSpace		Namespace			{ get; }
		IWindowWrapperContainer Parent		{ get; set; }
		int				AncestorCount		{ get; }
		string			ClassName			{ get; }
		string			Name				{ get; }
		Point			Location			{ get; set; }
		Size			Size				{ get; set; }
		bool			Visible				{ get; set; }
		Rectangle		Rectangle			{ get; }
		bool			DesignMode			{ get; }
		EditingMode		DesignerMovable		{ get;  }

		void			UpdateWindow	();
		void			Invalidate		();
		void			Invalidate		(Rectangle r);
		void			ScreenToClient	(ref Rectangle rect);
		void			ClientToScreen	(ref Rectangle rect);
		void			ScreenToClient	(ref Point point);
		void			ClientToScreen	(ref Point point);
		IntPtr			SendMessage		(int msg, IntPtr wParam, IntPtr lParam );
		bool			PostMessage		(int msg, IntPtr wParam, IntPtr lParam );
		IntPtr			GetWndPtr		();


	}

	//================================================================================
	public interface IDesignerCurrentStatusObject
	{
	}

	//================================================================================
	// consente di salvare/applicare agli oggetti uno stato corrente 
	public interface IDesignerCurrentStatus : IDictionary<string, IDesignerCurrentStatusObject>
	{ 
	}

	//interfaccia utilizzata per far passare alla finestra che la implementa i messaggi arrivati 
	//al designer del form manager (ad esempio, per far passare il click al tabmanager per effettuare
	//il cambio di tab)
	public interface IDesignerTarget
	{
        EditingMode DesignerMovable	{ get; }
		bool	MouseDownTarget { get; }
		void	OnMouseDown(Point p);
		void	Activate();
		void	SaveCurrentStatus	(IDesignerCurrentStatus status);
		void	ApplyCurrentStatus	(IDesignerCurrentStatus status);
		void	OnDesignerControlCreated();
		bool	CanDropTarget	    (Type droppedObject);
		bool	CanDropData		    (IDataBinding dataBinding);
        bool    CanUpdateTarget     (Type droppedObject);
        void    UpdateTargetFromDrop(Type droppedObject);
		void AfterTargetDrop(Type droppedObject);
    }
}
