using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	//=========================================================================
	public partial class MultiStateCheckBox : UserControl, IComparable
	{
		private Rectangle imageRectangle;

		private string undoTag;
		private Image undoImage;

		private MultiStateCheckBoxState currentState;
		private MultiStateCheckBoxState initialState;
		private ITransitionEnabler transitionEnabler;
		private ITransitionsBag transitionsBag;

		public event EventHandler<StateChangedEventArgs> ValueChanged;

		//---------------------------------------------------------------------
		public MultiStateCheckBox()
			: this (null, null, null)
		{}

		//---------------------------------------------------------------------
		public MultiStateCheckBox(
			MultiStateCheckBoxState initialState,
			ITransitionEnabler transitionEnabler,
			ITransitionsBag transitionsBag
			)
		{
			InitializeComponent();

			this.initialState = initialState;
			if (initialState != null)
				this.currentState = initialState.Clone() as MultiStateCheckBoxState;

			this.transitionEnabler = transitionEnabler;
			this.transitionsBag = transitionsBag;
		}

		#region Protected methods

		//---------------------------------------------------------------------
		protected virtual void OnValueChanged(StateChangedEventArgs args)
		{
			if (ValueChanged != null)
				ValueChanged(this, args);
		}

		//---------------------------------------------------------------------
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			contextMenuStrip.Items.Clear();
			InitContextMenuStrip();

			contextMenuStrip.Show(
				this,
				new Point(0, Height),
				ToolStripDropDownDirection.Default
				);
		}

		//---------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

            SolidBrush bkgndBrush = new SolidBrush(this.BackColor);
            e.Graphics.FillRectangle(bkgndBrush, this.ClientRectangle);
            bkgndBrush.Dispose();

			if (currentState == null || currentState.StateImage == null)
				return;

			int startX = (Width - currentState.StateImage.Width) / 2;
			int startY = (Height - currentState.StateImage.Height) / 2;

			imageRectangle = new Rectangle(
				startX,
				startY,
				currentState.StateImage.Width,
				currentState.StateImage.Height
				);

			e.Graphics.DrawImageUnscaled(currentState.StateImage, imageRectangle);
		}

		#endregion

		#region Private Methods

		//---------------------------------------------------------------------
		private void InitContextMenuStrip()
		{
			if (transitionsBag != null)
			{
				MultiStateCheckBoxStateTransition[] availableTransitions =
					transitionsBag.GetTransitions();

				ToolStripButton aToolStripButton = null;
				if (availableTransitions != null && availableTransitions.Length > 0)
				{
					foreach (MultiStateCheckBoxStateTransition transition in availableTransitions)
					{
						aToolStripButton = new ToolStripButton(
								transition.TransitionTag,
								transition.ToState.StateImage,
								new EventHandler(MenuStripItemClick)
								);
						aToolStripButton.Tag = transition.ToState;

						aToolStripButton.Enabled =
							transitionEnabler == null ||
							transitionEnabler.IsToBeEnabled(currentState, transition);

						contextMenuStrip.Items.Add(aToolStripButton);
					}
				}

				if (initialState.CompareTo(currentState) != 0)
				{
					aToolStripButton = new ToolStripButton(
								undoTag,
								undoImage,
								new EventHandler(MenuStripItemClick)
								);
					aToolStripButton.Tag = initialState;

					contextMenuStrip.Items.Add(aToolStripButton);
				}
			}
		}

		//---------------------------------------------------------------------
		private void MenuStripItemClick(object sender, EventArgs e)
		{
			ToolStripButton aToolStripButton = sender as ToolStripButton;
			if (aToolStripButton == null)
				return;

			object aTag = aToolStripButton.Tag;
			MultiStateCheckBoxState aState = aTag as MultiStateCheckBoxState;
			if (aState == null)
				return;

			CurrentState = aState.Clone() as MultiStateCheckBoxState;
		}

		#endregion

		#region Public methods

		//---------------------------------------------------------------------
		public void ResetState()
		{
			CurrentState = initialState.Clone() as MultiStateCheckBoxState;
		}

		//---------------------------------------------------------------------
		public virtual void Initialize(MultiStateCheckBox aMultiStateCheckBox)
		{
			if (aMultiStateCheckBox == null)
				return;

			if (aMultiStateCheckBox.initialState != null)
				initialState = aMultiStateCheckBox.initialState.Clone() as MultiStateCheckBoxState;

			if (aMultiStateCheckBox.currentState != null)
				currentState = aMultiStateCheckBox.currentState.Clone() as MultiStateCheckBoxState;

			if (aMultiStateCheckBox.undoImage != null)
				undoImage = aMultiStateCheckBox.undoImage.Clone() as Image;

			if (aMultiStateCheckBox.undoTag != null)
				undoTag = aMultiStateCheckBox.undoTag.Clone() as string;

			if (aMultiStateCheckBox.transitionEnabler != null)
				transitionEnabler = aMultiStateCheckBox.transitionEnabler;

			if (aMultiStateCheckBox.transitionsBag != null)
				transitionsBag = aMultiStateCheckBox.transitionsBag;
		}

		#region Object class overrides

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			MultiStateCheckBox aCheckBox = obj as MultiStateCheckBox;

			if (aCheckBox == null)
				return false;

			bool areEqual = false;

			if (initialState == null)
				areEqual = (aCheckBox.initialState == null);
			else
				areEqual = initialState.Equals(aCheckBox.initialState);

			if (!areEqual)
				return false;

			if (currentState == null)
				areEqual = (aCheckBox.currentState == null);
			else
				areEqual = currentState.Equals(aCheckBox.currentState);

			return areEqual;
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			if (currentState != null)
				return currentState.ToString();

			if (initialState != null)
				return initialState.ToString();

			return GetType().Name;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			if (currentState != null)
				return currentState.GetHashCode();

			if (initialState != null)
				return initialState.GetHashCode();

			return base.GetHashCode();
		}

		#endregion

		#region IComparable Members

		//---------------------------------------------------------------------
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			MultiStateCheckBox aCheckBox = obj as MultiStateCheckBox;

			if (aCheckBox == null)
				throw new ArgumentException("'obj' is not a 'MultiStateCheckBox'", "obj");

			int compareResult = 0;

			if (initialState != null)
			{
				compareResult = initialState.CompareTo(aCheckBox.initialState);
			}
			else if (aCheckBox.initialState != null)
			{
				compareResult = 1;
			}

			if (compareResult != 0)
				return compareResult;

			if (currentState != null)
			{
				compareResult = currentState.CompareTo(aCheckBox.currentState);
			}
			else if (aCheckBox.currentState != null)
			{
				compareResult = 1;
			}

			return compareResult;
		}

		#endregion

		#endregion

		#region Public properties

		//---------------------------------------------------------------------
		protected override Size DefaultSize
		{
			get
			{
				return new System.Drawing.Size(36, 36);
			}
		}

		//---------------------------------------------------------------------
		[Browsable(false)]
		public ITransitionEnabler TransitionEnabler
		{
			get
			{
				return transitionEnabler;
			}
			set
			{
				transitionEnabler = value;
			}
		}

		//---------------------------------------------------------------------
		[Browsable(false)]
		public ITransitionsBag TransitionsBag
		{
			get
			{
				return transitionsBag;
			}
			set
			{
				transitionsBag = value;
			}
		}

		//---------------------------------------------------------------------
		[Browsable(false)]
		public MultiStateCheckBoxState CurrentState
		{
			get
			{
				return currentState;
			}
			set
			{
				currentState = value;

				int currentStateId = (currentState != null) ? currentState.StateId : -1;
				OnValueChanged(new StateChangedEventArgs(currentStateId));
				Invalidate(imageRectangle);
			}
		}

		//---------------------------------------------------------------------
		[Browsable(false)]
		public MultiStateCheckBoxState InitialState
		{
			get { return initialState; }
			set { initialState = value; }
		}

		//---------------------------------------------------------------------
		public string UndoTag
		{
			get { return undoTag; }
			set { undoTag = value; }
		}

		//---------------------------------------------------------------------
		public Image UndoImage
		{
			get { return undoImage; }
			set { undoImage = value; }
		}

		#endregion
	}

	//=========================================================================
	public class StateChangedEventArgs : EventArgs
	{
		private int state;

		//---------------------------------------------------------------------
		public int State
		{
			get { return state; }
		}

		//---------------------------------------------------------------------
		public StateChangedEventArgs(int state)
		{
			this.state = state;
		}
	}

	//=========================================================================
	public class MultiStateCheckBoxState : ICloneable, IComparable
	{
		private int stateId;
		private Image stateImage;
		private string stateTag = string.Empty;

		//---------------------------------------------------------------------
		public int StateId
		{
			get { return stateId; }
			set { stateId = value; }
		}

		//---------------------------------------------------------------------
		public Image StateImage
		{
			get { return stateImage; }
			set { stateImage = value; }
		}

		//---------------------------------------------------------------------
		public string StateTag
		{
			get { return stateTag; }
			set { stateTag = value; }
		}

		//---------------------------------------------------------------------
		public MultiStateCheckBoxState()
			: this (0, null, string.Empty)
		{ }

		//---------------------------------------------------------------------
		public MultiStateCheckBoxState(
			int stateId,
			Image stateImage,
			string stateTag
			)
		{
			this.stateId = stateId;
			this.stateImage = stateImage;
			this.stateTag = stateTag;
		}

		//---------------------------------------------------------------------
		protected MultiStateCheckBoxState(MultiStateCheckBoxState aMultiStateCheckBoxState)
		{
			if (aMultiStateCheckBoxState != null)
			{
				this.stateId = aMultiStateCheckBoxState.stateId;
				if (aMultiStateCheckBoxState.stateImage != null)
					this.stateImage = aMultiStateCheckBoxState.stateImage.Clone() as Image;
				this.stateTag = aMultiStateCheckBoxState.stateTag;
			}
		}

		#region ICloneable Members

		//---------------------------------------------------------------------
		public object Clone()
		{
			return new MultiStateCheckBoxState(this);
		}

		#endregion

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			MultiStateCheckBoxState aMultiStateCheckBoxState = obj as MultiStateCheckBoxState;
			if (aMultiStateCheckBoxState == null)
				return false;

			return String.Compare(
				this.stateTag,
				aMultiStateCheckBoxState.stateTag,
				StringComparison.InvariantCulture
				) == 0
				&&
				(this.stateId == aMultiStateCheckBoxState.stateId);
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return this.stateTag;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return this.stateTag.GetHashCode();
		}

		#region IComparable Members

		//---------------------------------------------------------------------
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			MultiStateCheckBoxState aState = obj as MultiStateCheckBoxState;
			if (aState == null)
				throw new ArgumentException(
					"Cannot compare if 'obj' is not a MultiStateCheckBoxState",
					"obj"
					);

			return stateId.CompareTo(aState.stateId);
		}

		#endregion
	}

	//=========================================================================
	public class MultiStateCheckBoxStateTransition : ICloneable, IComparable
	{
		private MultiStateCheckBoxState fromState;
		private MultiStateCheckBoxState toState;
		private string transitionTag;

		//---------------------------------------------------------------------
		public MultiStateCheckBoxState FromState
		{
			get { return fromState; }
		}

		//---------------------------------------------------------------------
		public MultiStateCheckBoxState ToState
		{
			get { return toState; }
		}

		//---------------------------------------------------------------------
		public string TransitionTag
		{
			get { return transitionTag; }
		}

		//---------------------------------------------------------------------
		public MultiStateCheckBoxStateTransition(
			MultiStateCheckBoxState fromState,
			MultiStateCheckBoxState toState,
			string transitionTag
			)
		{
			this.fromState = fromState;
			this.toState = toState;
			this.transitionTag = transitionTag;
		}

		//---------------------------------------------------------------------
		protected MultiStateCheckBoxStateTransition(
			MultiStateCheckBoxStateTransition aMultiStateCheckBoxStateTransition
			)
		{
			if (aMultiStateCheckBoxStateTransition != null)
			{
				this.fromState =
					aMultiStateCheckBoxStateTransition.fromState.Clone() as MultiStateCheckBoxState;
				this.toState =
					aMultiStateCheckBoxStateTransition.toState.Clone() as MultiStateCheckBoxState;

				this.transitionTag = aMultiStateCheckBoxStateTransition.transitionTag.Clone() as string;
			}
		}

		#region IComparable Members

		//---------------------------------------------------------------------
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			MultiStateCheckBoxStateTransition transition =
				obj as MultiStateCheckBoxStateTransition;

			if (transition == null)
				throw new ArgumentException(
					"'obj' is not of MultiStateCheckBoxStateTransition type",
					"obj"
					);

			int compareResult = fromState.CompareTo(transition.fromState);

			if (compareResult != 0)
				return compareResult;

			compareResult = toState.CompareTo(transition.toState);

			if (compareResult != 0)
				return compareResult;

			return transitionTag.CompareTo(transition.transitionTag);
		}

		#endregion

		#region ICloneable Members

		//---------------------------------------------------------------------
		public object Clone()
		{
			return new MultiStateCheckBoxStateTransition(this);
		}

		#endregion
	}

	//=========================================================================
	public interface ITransitionEnabler
	{
		//---------------------------------------------------------------------
		bool IsToBeEnabled(
			MultiStateCheckBoxState currentState,
			MultiStateCheckBoxStateTransition toBeEnabled
			);
	}

	//=========================================================================
	public interface ITransitionsBag
	{
		//---------------------------------------------------------------------
		MultiStateCheckBoxStateTransition[] GetTransitions();
	}
}
