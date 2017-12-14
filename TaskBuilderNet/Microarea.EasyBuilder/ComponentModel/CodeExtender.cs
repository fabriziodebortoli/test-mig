using Microarea.TaskBuilderNet.Core.EasyBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microarea.EasyBuilder.ComponentModel
{
    //===================================================================
    /// <summary>
    /// this object allows to extend objects
    /// </summary>
    public class CodeExtender
    {
        private EasyBuilderComponent extendedComponent;

        //------------------------------------------------------------
        /// <summary>
        /// constructs a CodeExtender
        /// </summary>
        public CodeExtender(EasyBuilderComponent extendedComponent)
        {
            this.extendedComponent = extendedComponent;
        }

        //------------------------------------------------------------
        /// <summary>
        /// gets extended components
        /// </summary>
        public bool CanBeUsedInBusinessObject
        {
            get { return true; }
        }

        //------------------------------------------------------------
        /// <summary>
        /// gets extended components
        /// </summary>
        protected EasyBuilderComponent ExtendedComponent
        {
            get
            {
                return extendedComponent;
            }
        }
    }
}
