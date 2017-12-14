using System;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.EasyBuilder.MVC;

namespace Microarea.EasyBuilder
{
    //================================================================================
    /// <summary>
    /// </summary>
    public class ObjectModel
    {
        private MDocument document;
        private DocumentView view;
        private DocumentController controller;
        private DocumentControllers controllers;

        //-----------------------------------------------------------------------------
        /// <summary>
        /// </summary>
        public DocumentController Controller
        {
            get { return controller; }
            set { controller = value; }
        }

        //-----------------------------------------------------------------------------
        /// <summary>
        /// </summary>
        public MDocument Document
        {
            get { return document; }
            set { document = value; }
        }

        //-----------------------------------------------------------------------------
        /// <summary>
        /// </summary>
        public DocumentView View
        {
            get { return view; }
            set { view = value; }
        }

        //-----------------------------------------------------------------------------
        /// <summary>
        /// </summary>
        public DocumentControllers Controllers
        {
            get { return controllers; }
            set { controllers = value; }
        }

        //-----------------------------------------------------------------------------
        /// <summary>
        /// </summary>
         public ObjectModel
            (
                MDocument document,
                DocumentView view,
                DocumentController controller,
                DocumentControllers controllers
            )
        {
            this.document = document;
            this.view = view;
            this.controller = controller;
            this.controllers = controllers;
        }
    }
}
