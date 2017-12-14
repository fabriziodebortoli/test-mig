using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.EasyBuilder.Refactor;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	//=========================================================================
	/// <summary>
	/// The customization context is the virtual space where all changes to the standard
	/// application are saved.
	/// </summary>
	//-----------------------------------------------------------------------------
	public partial class BaseCustomizationContext 
	{
		public static event EventHandler<CustomizationContextEventArgs> CustomizationContextCreation;
		protected static readonly object lockObject = new object();
		private static ICustomizationContext customizationContextInstance;
        private static Changes applicationChanges;

        //-----------------------------------------------------------------------------
        public static Changes ApplicationChanges
        {
            get
            {
                if (applicationChanges == null)
                    applicationChanges = new Changes();
                return applicationChanges;
            }

            set
            {
                applicationChanges = value;
            }
        }

        //-----------------------------------------------------------------------------
        public static void OnCustomizationContextCreation(CustomizationContextEventArgs arg)
		{
			if (CustomizationContextCreation != null)
				CustomizationContextCreation(null, arg);
		}


		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		public static ICustomizationContext CustomizationContextInstance
		{
			get
			{
				lock (lockObject)
				{
					if (customizationContextInstance == null)
					{
						CustomizationContextEventArgs arg = new CustomizationContextEventArgs();
						OnCustomizationContextCreation(arg);

						customizationContextInstance = arg.CustomizationContext;
					}
					return customizationContextInstance;
				}
			}
		}
    }

	//=========================================================================
	public class CustomizationContextEventArgs
	{
		ICustomizationContext customizationContext = null;

		//-----------------------------------------------------------------------------
		public CustomizationContextEventArgs()
		{
		}

		//-----------------------------------------------------------------------------
		public ICustomizationContext CustomizationContext { get { return customizationContext; } set { customizationContext = value; } }
	}

}
