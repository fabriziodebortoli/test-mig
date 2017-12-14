using System;
using System.Collections.Generic;
using System.Drawing;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;
using System.ComponentModel;

namespace Microarea.TaskBuilderNet.Interfaces.View
{
    //=============================================================================
    public interface ILayoutComponent
    {
        IComponent LinkedComponent { get; set; }
        INameSpace Namespace { get; }
        string LayoutDescription { get; }

        ILayoutComponent LayoutObject { get; }
    }

    //=============================================================================
    public interface IWindowWrapperContainer : IWindowWrapper, IEasyBuilderContainer
	{
        bool HasControl			(IntPtr handle);
		IWindowWrapper	GetControl			(INameSpace nameSpace);
		IWindowWrapper  GetControl			(IntPtr handle);
		IWindowWrapper	GetControl			(string name);
		void			GetChildrenFromPos	(Point p, IntPtr handleToSkip, ICollection<IWindowWrapper> foundChildren);
		IntPtr			GetChildFromOriginalPos	(Point clientPosition, String controlClass);
		void			SaveChildOriginalPos(IntPtr hwnd, Point clientPosition);
		Point			GetScrollPosition();

        void IntegrateLayout(ILayoutComponent layoutObject);
        }
    }
