namespace Microarea {
	namespace Framework {
		namespace TBApplicationWrapper
		{
			/// <summary>
			/// Describes an EasyBuilder action dispatched via post message or send message.
			/// </summary>
			/// <value></value>
			//================================================================================
			public enum EasyBuilderAction
			{
				/// <summary>
				/// No action, default value.
				/// </summary>
				NoAction = 0,
				/// <summary>
				/// A row is changed, used for all grid controls.
				/// </summary>
				RowChanged = 1,
				/// <summary>
				/// A button was clicked.
				/// </summary>
				Clicked = 2,
				/// <summary>
				/// A value changed.
				/// </summary>
				ValueChanged = 3,
				/// <summary>
				/// State Button Clicked
				/// </summary>
				StateButtonClicked = 4
			};
		}
	}
}