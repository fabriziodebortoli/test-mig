using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.EasyBuilder.ComponentModel
{
	//================================================================================
	class EasyBuilderSelectionService : ISelectionService
	{
		IList<Object> selectedComponents = new List<Object>();

		//--------------------------------------------------------------------------------
		public void ListenTo(IComponentChangeService compChangeSvc)
		{
			compChangeSvc.ComponentRemoved += new ComponentEventHandler(OnComponentRemoved);
		}

		//--------------------------------------------------------------------------------
		public void StopListeningTo(IComponentChangeService compChangeSvc)
		{
			compChangeSvc.ComponentRemoved -= new ComponentEventHandler(OnComponentRemoved);
		}

		//--------------------------------------------------------------------------------
		private void OnComponentRemoved(object sender, ComponentEventArgs e)
		{
			SetSelectedComponents(new IComponent[] { e.Component }, SelectionTypes.Remove);
		}

		#region ISelectionService Members

		public event EventHandler SelectionChanged;
		public event EventHandler SelectionChanging;

		//--------------------------------------------------------------------------------
		protected virtual void OnSelectionChanging(object sender, EventArgs e)
		{
			if (SelectionChanging != null)
				SelectionChanging(this, e);
		}

		//--------------------------------------------------------------------------------
		protected virtual void OnSelectionChanged(object sender, EventArgs e)
		{
			if (SelectionChanged != null)
				SelectionChanged(this, e);
		}

		//--------------------------------------------------------------------------------
		public bool GetComponentSelected(object component)
		{
			return ContainsComponent(component);
		}

		//--------------------------------------------------------------------------------
		public ICollection GetSelectedComponents()
		{
			return selectedComponents.ToArray();
		}

		//--------------------------------------------------------------------------------
		public object PrimarySelection
		{
			get
			{
				return (selectedComponents.Count > 0) ? selectedComponents[selectedComponents.Count - 1] : null;
			}
		}

		//--------------------------------------------------------------------------------
		public int SelectionCount
		{
			get { return selectedComponents.Count; }
		}

		//--------------------------------------------------------------------------------
		public void SetSelectedComponents(ICollection components, SelectionTypes selectionType)
		{
			Keys controlOrShiftKeys = (Keys.Control | Keys.Shift);
			bool controlOrShiftPressed = ((Control.ModifierKeys & controlOrShiftKeys) == controlOrShiftKeys);

			OnSelectionChanging(this, EventArgs.Empty);

			if (components == null || components.Count == 0)
				components = new List<Object>();

			bool collectionChanged = false;
			switch (selectionType)
			{
				case SelectionTypes.Add:
					collectionChanged = AddComponentsToSelection(components);
					break;
				case SelectionTypes.Auto:
					{
						collectionChanged = controlOrShiftPressed ?
							AddComponentsToSelection(components) :
							ReplaceComponentsInSelection(components);
							
						break;
					}
				case SelectionTypes.Remove:
					collectionChanged = RemoveComponentsFromSelection(components);
					break;
				case SelectionTypes.Replace://Sostituisco gli oggetti selezionati
					{
						collectionChanged = ReplaceComponentsInSelection(components);
						break;
					}
				case SelectionTypes.Toggle:
					{
						int idx = -1;
						foreach (Object component in components)
						{
							idx = IndexOfComponent(component);
							if (idx > -1)
								collectionChanged |= RemoveComponentFromSelection(component);
							else
								collectionChanged |= AddComponentToSelection(component);
						}
						break;
					}
				case SelectionTypes.Primary:
					{
						if (components != null)
						{
							//Promuovo il component a componente primario (l'ultimo della lista per come è fatta PrimarySelection
							IEnumerator componentsEnumerator = components.GetEnumerator();
							if (componentsEnumerator.MoveNext())
							{
								collectionChanged = RemoveComponentFromSelection(componentsEnumerator.Current);
								collectionChanged |= AddComponentToSelection(componentsEnumerator.Current);
							}
						}
						break;
					}
				default:
					break;
			}

			if (collectionChanged)
				OnSelectionChanged(this, EventArgs.Empty);
		}

		//--------------------------------------------------------------------------------
		public void SetSelectedComponents(ICollection components)
		{
			SetSelectedComponents(components, SelectionTypes.Replace);
		}

		#endregion

		//--------------------------------------------------------------------------------
		/// <returns>True se ha modificato la collezione, false altrimenti</returns>
		private bool ReplaceComponentsInSelection(ICollection components)
		{
			selectedComponents.Clear();

			foreach (Object item in components)
				AddComponentToSelection(item);

			return true;
		}

		//--------------------------------------------------------------------------------
		/// <returns>True se ha modificato la collezione, false altrimenti</returns>
		private bool AddComponentsToSelection(ICollection components)
		{
			bool added = false;
			foreach (Object component in components)
				added |= AddComponentToSelection(component);

			return added;
		}

		//--------------------------------------------------------------------------------
		/// <returns>True se ha modificato la collezione, false altrimenti</returns>
		private bool AddComponentToSelection(Object component)
		{
			bool added = false;
			if (component != null && !ContainsComponent(component))
			{
				selectedComponents.Add(component);
				added = true;
			}

			return added;
		}

		//--------------------------------------------------------------------------------
		/// <returns>True se ha modificato la collezione, false altrimenti</returns>
		private bool RemoveComponentsFromSelection(ICollection components)
		{
			bool removed = false;
			foreach (Object component in components)
				removed |= RemoveComponentFromSelection(component);

			return removed;
		}

		//--------------------------------------------------------------------------------
		/// <returns>True se ha modificato la collezione, false altrimenti</returns>
		private bool RemoveComponentFromSelection(Object component)
		{
			int idx = -1;
			if (component != null && (idx = IndexOfComponent(component)) != -1)
				selectedComponents.RemoveAt(idx);

			return idx != -1;
		}

		//--------------------------------------------------------------------------------
		private bool ContainsComponent(Object component)
		{
			return (component != null && IndexOfComponent(component) != -1);
		}

		//--------------------------------------------------------------------------------
		private int IndexOfComponent(Object component)
		{
			return selectedComponents.IndexOf(component);
		}
	}

	//=========================================================================
	class SelectionHelper
	{
		//---------------------------------------------------------------------
		/// <summary>
		/// Calcola l'area dell'intersezione di tutti i controlli.
		/// Il controllo da selezionare è quello contenuto in maggior parte
		/// dall'area dell'intersezione.
		/// </summary>
		/// <param name="wrappers">Collezione di controlli candidati ad essere quello selezionato</param>
		/// <param name="editor">oggetto FormEditor per scremare meglio i candidati wrappers</param>
		/// <returns>Il controllo che deve essere seleizonato tra quelli presi in ingresso</returns>
		/// <remarks>
		/// La collezione wrappers può contenere controlli IContainer e non IContainer
		/// contemporaneamente.
		/// </remarks>
		public static IWindowWrapper CalculateSelected(IList<IWindowWrapper> wrappers, FormEditor editor)
		{
			//prima elimino eventuali contenitori
			wrappers = RemoveParents(wrappers);
			//quindi applico l'argoritmo basato sul calcolo delle aree alle foglie
			List<SelectionInfo> selectionInfos = new List<SelectionInfo>();

			IEnumerator<IWindowWrapper> enumerator = wrappers.GetEnumerator();
			IWindowWrapper current = null;
			Rectangle intersection = new Rectangle(Int32.MinValue / 2, Int32.MinValue / 2, Int32.MaxValue, Int32.MaxValue);
			SelectionInfo currentSelectionInfo = null;

			while (enumerator.MoveNext())
			{
				//Ciclo sulla collezione di controlli selezionati e, per ogni controllo, istanzio la classe
				//SelectionInfo che memorizza informazioni utili.
				current = enumerator.Current;
				currentSelectionInfo = new SelectionInfo(current);
				selectionInfos.Add(currentSelectionInfo);

				//Contemporaneamente calcolo l'interseizone tra tutti i controlli.
				intersection.Intersect(currentSelectionInfo.Rectangle);
			}

			int intersectionArea = intersection.Height * intersection.Width;

			IWindowWrapper candidateForSelection = null;
			float candidateForSelectionProbOfSel = 0F;
			float probOfSel = 0F;
			enumerator.Reset();

			foreach (SelectionInfo selectionInfo in selectionInfos)
			{
				//La probabilità che il controllo sia quello da selezionare è
				//pari alla misura in cui l'intersezione "contiene" il controllo.
				probOfSel = (float)intersectionArea / (float)selectionInfo.Area;

				//Tengo da parte il controllo con maggiore probabilità di essere quello selezionato.
				//ma se il controllo non appartiene alla tile attualmente in editing, lo escludo
				if (probOfSel > candidateForSelectionProbOfSel && editor.CanBeSelected(selectionInfo.Selected))
				{
					candidateForSelectionProbOfSel = probOfSel;
					candidateForSelection = selectionInfo.Selected;
				}
			}

			return candidateForSelection;
		}

		//---------------------------------------------------------------------
		private static IList<IWindowWrapper> RemoveParents(IList<IWindowWrapper> wrappers)
		{
			for (int i = wrappers.Count - 1; i >= 0; i--)
			{
				IWindowWrapperContainer potentialParent = wrappers[i] as IWindowWrapperContainer;
				if (potentialParent == null)
					continue;
				for (int j = wrappers.Count - 1; j >= 0; j--)
				{
					if (i == j)
						continue;
					IWindowWrapper potentialChild = wrappers[j];
					if (HasParent(potentialParent, potentialChild))
					{
						wrappers.RemoveAt(i);
						break;
					}
				}
			}
			return wrappers;
		}

		//---------------------------------------------------------------------
		private static bool HasParent(IWindowWrapperContainer potentialParent, IWindowWrapper potentialChild)
		{
			while ((potentialChild = potentialChild.Parent) != null)
			{
				if (potentialChild == potentialParent)
					return true;
			}
			return false;
		}

	}

	/// <summary>
	/// Encapsulates data about user selection.
	/// </summary>
	//=========================================================================
	class SelectionInfo
	{
		IWindowWrapper selected;
		Rectangle rectangle;
		int area;

		/// <summary>
		/// Gets or sets the selected control.
		/// </summary>
		/// <seealso cref="Microarea.TaskBuilderNet.Interfaces.View.IWindowWrapper"/>
		//-----------------------------------------------------------------------------
		public IWindowWrapper Selected
		{
			get { return selected; }
			set { selected = value; }
		}

		/// <summary>
		/// Gets or sets the Rectangle of the selected control.
		/// </summary>
		/// <seealso cref="System.Drawing.Rectangle"/>
		//-----------------------------------------------------------------------------
		public Rectangle Rectangle
		{
			get { return rectangle; }
			set { rectangle = value; }
		}

		/// <summary>
		/// Gets or sets the area of the selected control.
		/// </summary>
		//-----------------------------------------------------------------------------
		public int Area
		{
			get { return area; }
			set { area = value; }
		}

		/// <summary>
		/// Initializes a new instance of the SelectionInfo with the given IWindowWrapper.
		/// </summary>
		/// <seealso cref="Microarea.TaskBuilderNet.Interfaces.View.IWindowWrapper"/>
		//-----------------------------------------------------------------------------
		public SelectionInfo(IWindowWrapper selected)
		{
			this.selected = selected;
			this.rectangle = selected.Rectangle;
			this.area = this.rectangle.Width * this.rectangle.Height;
		}
	}
}
