using System;
using System.Collections;
using System.Diagnostics;

namespace Microarea.Library.TBWizardProjects
{
	#region TableHistoryInfo Class

	//===============================================================================
	/// <summary>
	/// Summary description for TableHistoryInfo.
	/// </summary>
	public class TableHistoryInfo
	{
		#region TableHistoryInfo private data members

		private TableHistoryStepsCollection steps = null;
		
		private bool sortByDbReleaseNumber = true;
		
		#endregion // TableHistoryInfo private data members

		//---------------------------------------------------------------------------
		public TableHistoryInfo()
		{
		}

		//---------------------------------------------------------------------------
		public TableHistoryInfo(TableHistoryInfo aTableHistory)
		{
			sortByDbReleaseNumber = (aTableHistory != null) ? aTableHistory.SortByDbReleaseNumber : true;

			if (aTableHistory != null && aTableHistory.StepsCount > 0)
			{
				foreach(TableHistoryStep aStep in aTableHistory.Steps)
					this.AddHistoryStep(new TableHistoryStep(aStep));
			}
		}

		#region TableHistoryInfo public properties

		//---------------------------------------------------------------------------
		public TableHistoryStepsCollection Steps { get { return steps; } }

		//---------------------------------------------------------------------------
		public int StepsCount { get { return (steps != null) ? steps.Count : 0; } }

		//---------------------------------------------------------------------------
		public bool SortByDbReleaseNumber
		{
			get { return sortByDbReleaseNumber; } 
			set 
			{ 
				sortByDbReleaseNumber = value; 

				if (sortByDbReleaseNumber && steps != null && steps.Count > 0)
					steps.SortByDbReleaseNumber();
			} 
		}

		#endregion // TableHistoryInfo public properties
		
		#region TableHistoryInfo public methods

		//---------------------------------------------------------------------------
		public TableHistoryStep GetDbReleaseStep(uint aDbReleaseNumber)
		{
			if (steps == null || steps.Count == 0)
				return null;

			return steps.GetDbReleaseStep(aDbReleaseNumber);
		}

		//---------------------------------------------------------------------------
		public TableHistoryStep GetNearestDbReleaseStep(uint aDbReleaseNumber)
		{
			if (aDbReleaseNumber <= 0 || steps == null || steps.Count == 0)
				return null;

			TableHistoryStepsCollection orderedSteps = GetStepsSortedByDbReleaseNumber();

			foreach (TableHistoryStep aStep in orderedSteps)
			{
				if (aStep.DbReleaseNumber > aDbReleaseNumber)
					continue;
					
				return aStep;
			}
			return null;
		}

		//---------------------------------------------------------------------------
		public TableHistoryStepsCollection GetStepsSortedByDbReleaseNumber()
		{
			if (steps == null || steps.Count == 0)
				return null;

			if (sortByDbReleaseNumber)
				return steps;

			TableHistoryStepsCollection orderedSteps = new TableHistoryStepsCollection();
			orderedSteps.AddRange(steps);
			orderedSteps.SortByDbReleaseNumber();
			
			return orderedSteps;
		}
		
		//---------------------------------------------------------------------------
		public TableHistoryStepsCollection GetStepsBeforeDbRelease(uint aDbReleaseNumber)
		{
			if (aDbReleaseNumber <= 0 || steps == null || steps.Count == 0)
				return null;

			TableHistoryStepsCollection orderedSteps = GetStepsSortedByDbReleaseNumber();

			if (orderedSteps[orderedSteps.Count - 1].DbReleaseNumber <= aDbReleaseNumber)
				return orderedSteps;

			TableHistoryStepsCollection stepsToReturn = new TableHistoryStepsCollection();

			foreach(TableHistoryStep aStep in orderedSteps)
			{
				if (aStep.DbReleaseNumber > aDbReleaseNumber)
					break;

				stepsToReturn.Add(aStep);
			}

			return stepsToReturn;
		}

		//---------------------------------------------------------------------------
		public TableHistoryStepsCollection GetStepsAfterDbRelease(uint aDbReleaseNumber)
		{
			if (aDbReleaseNumber <= 0 || steps == null || steps.Count == 0)
				return null;

			TableHistoryStepsCollection orderedSteps = GetStepsSortedByDbReleaseNumber();
			if (orderedSteps[orderedSteps.Count - 1].DbReleaseNumber <= aDbReleaseNumber)
				return null;

			TableHistoryStepsCollection stepsToReturn = new TableHistoryStepsCollection();

			foreach(TableHistoryStep aStep in orderedSteps)
			{
				if (aStep.DbReleaseNumber <= aDbReleaseNumber)
					continue;

				stepsToReturn.Add(aStep);
			}

			return stepsToReturn;
		}
		
		//---------------------------------------------------------------------------
		public void ClearSteps()
		{
			if (steps != null && steps.Count > 0)
				steps.Clear();
		}
		
		//---------------------------------------------------------------------------
		public int AddHistoryStep(TableHistoryStep aHistoryStep)
		{
			if 
				(
				aHistoryStep == null || 
				aHistoryStep.DbReleaseNumber == 0 ||
				aHistoryStep.EventsCount == 0
				)
				return -1;

			TableHistoryStep existingStep = GetDbReleaseStep(aHistoryStep.DbReleaseNumber);
			if (existingStep != null)
			{
				int stepIndex = steps.IndexOf(existingStep);
				if (stepIndex >= 0)
				{
					if (existingStep != aHistoryStep)
						existingStep.UpdateWithNewerInfo(aHistoryStep);

					return stepIndex;
				}
			}

			if (steps == null || steps.Count == 0)
				steps = new TableHistoryStepsCollection();

			steps.Add(aHistoryStep);
			
			if (sortByDbReleaseNumber)
				steps.SortByDbReleaseNumber();

			return steps.IndexOf(aHistoryStep);
		}
		
		//---------------------------------------------------------------------------
		public uint GetMaximumDbReleaseNumberUsed()
		{
			if (steps == null || steps.Count == 0)
				return 0;

			if (sortByDbReleaseNumber)
				return steps[steps.Count -1].DbReleaseNumber;

			uint maxDbReleaseNumber = 0;
			foreach(TableHistoryStep aStep in steps)
			{
				if (aStep.DbReleaseNumber > maxDbReleaseNumber)
					maxDbReleaseNumber = aStep.DbReleaseNumber;
			}
			return maxDbReleaseNumber;
		}

		#endregion // TableHistoryInfo public methods
	}
	
	#endregion // TableHistoryInfo Class

	#region TableHistoryStep Class
		
	//===============================================================================
	/// <summary>
	/// Summary description for TableHistoryStep.
	/// </summary>
	public class TableHistoryStep
	{
		public enum EventType : ushort
		{
			Undefined					= 0x0000,
			AddColumn					= 0x0001,
			AlterColumnType				= 0x0002,
			DropColumn					= 0x0003,
			RenameColumn				= 0x0004,
			ChangeColumnDefaultValue	= 0x0005,
			ChangeColumnOrder			= 0x0006,
			ModifyPrimaryKey			= 0x0007,
			CreateIndex					= 0x0008,
			DropIndex					= 0x0009,
			CreateConstraint			= 0x000A,
			DropConstraint				= 0x000B,
			ChangeTBGuidExistence		= 0x000C,
			AddTable					= 0x000D,
			AddExtraColumnsToTable		= 0x000E
		}
		
		private uint dbReleaseNumber = 0;
		
		private string primaryKeyConstraintName = String.Empty;
		
		private ColumnHistoryEventCollection columnsEvents = null;
		private IndexHistoryEventCollection indexesEvents = null;
		private ForeignKeyHistoryEventCollection foreignKeyEvents = null;

		//---------------------------------------------------------------------------
		public TableHistoryStep(uint aDbReleaseNumber)
		{
			dbReleaseNumber = aDbReleaseNumber;
		}

		//---------------------------------------------------------------------------
		public TableHistoryStep(TableHistoryStep aStep)
		{
			dbReleaseNumber = (aStep != null) ? aStep.DbReleaseNumber : 0;
			primaryKeyConstraintName = (aStep != null) ? aStep.PrimaryKeyConstraintName : String.Empty;

			if (aStep != null)
			{
				if (aStep.ColumnsEventsCount > 0)
				{
					foreach(ColumnHistoryEvent aColumnEvent in aStep.ColumnsEvents)
						AddColumnEvent(aColumnEvent);
				}
				
				if (aStep.IndexesEventsCount > 0)
				{
					foreach(IndexHistoryEvent aIndexEvent in aStep.IndexesEvents)
						AddIndexEvent(aIndexEvent);
				}
			}
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is TableHistoryStep))
				return false;

			if (obj == this)
				return true;

			return 
				(
				dbReleaseNumber == ((TableHistoryStep)obj).DbReleaseNumber &&
				ColumnHistoryEventCollection.Equals(columnsEvents, ((TableHistoryStep)obj).ColumnsEvents) &&
				ColumnHistoryEventCollection.Equals(indexesEvents, ((TableHistoryStep)obj).IndexesEvents) &&
				ColumnHistoryEventCollection.Equals(foreignKeyEvents, ((TableHistoryStep)obj).ForeignKeyEvents)
				);
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
			
		#region TableHistoryStep public properties

		//---------------------------------------------------------------------------
		public uint DbReleaseNumber { get { return dbReleaseNumber; } } 

		//---------------------------------------------------------------------------
		public string PrimaryKeyConstraintName 
		{
			get 
			{
				return (primaryKeyConstraintName != null) ? primaryKeyConstraintName.Trim() : String.Empty; 
			}
			set 
			{
				if (Generics.IsValidDBObjectName(value))
					primaryKeyConstraintName = value.Trim(); 
			}
		}

		//---------------------------------------------------------------------------
		public ColumnHistoryEventCollection ColumnsEvents { get { return columnsEvents; } }
			
		//---------------------------------------------------------------------------
		public int ColumnsEventsCount { get { return (columnsEvents != null) ? columnsEvents.Count : 0; } }

		//---------------------------------------------------------------------------
		public IndexHistoryEventCollection IndexesEvents { get { return indexesEvents; } }

		//---------------------------------------------------------------------------
		public int IndexesEventsCount { get { return (indexesEvents != null) ? indexesEvents.Count : 0; } }

		//---------------------------------------------------------------------------
		public ForeignKeyHistoryEventCollection ForeignKeyEvents { get { return foreignKeyEvents; } }

		//---------------------------------------------------------------------------
		public int ForeignKeyEventsCount { get { return (foreignKeyEvents != null) ? foreignKeyEvents.Count : 0; } }

		//---------------------------------------------------------------------------
		public int EventsCount { get { return ColumnsEventsCount + IndexesEventsCount + ForeignKeyEventsCount; } }

		#endregion // TableHistoryStep public properties
			
		#region TableHistoryStep public methods

		//---------------------------------------------------------------------------
		public int AddColumnEvent
			(
			WizardTableColumnInfo		aColumn, 
			int							aColumnOrder, 
			WizardTableColumnInfo		aPreviousColumnInfo,
			int							aPreviousColumnOrder, 
			TableHistoryStep.EventType	aType
			)
		{
			if (aColumn == null || aType == EventType.Undefined)
				return -1;

			if 
				(
				aType == EventType.ChangeColumnDefaultValue && 
				(aPreviousColumnInfo == null || aColumn.HasSameDefaultValueAs(aPreviousColumnInfo))
				)
				return -1;

			if (columnsEvents != null && columnsEvents.Count > 0)
			{
				for (int i = 0; i < columnsEvents.Count; i++)
				{
					ColumnHistoryEvent aEvent = columnsEvents[i];
					if 
						(
						aEvent != null &&
						aType == aEvent.Type &&
						WizardTableColumnInfo.Equals(aColumn, aEvent.ColumnInfo) &&
						WizardTableColumnInfo.Equals(aPreviousColumnInfo, aEvent.PreviousColumnInfo)
						)
						return i; // l'evento è già stato inserito precedentemente
				}
			}

			if (columnsEvents == null)
				columnsEvents = new ColumnHistoryEventCollection();

			return columnsEvents.Add(new ColumnHistoryEvent(aColumn, aColumnOrder, aPreviousColumnInfo, aPreviousColumnOrder, aType));
		}
			
		//---------------------------------------------------------------------------
		public int AddColumnEvent(ColumnHistoryEvent aColumnEvent)
		{
			if (aColumnEvent == null)
				return -1;

			return AddColumnEvent(aColumnEvent.ColumnInfo, aColumnEvent.ColumnOrder, aColumnEvent.PreviousColumnInfo, aColumnEvent.PreviousColumnOrder, aColumnEvent.Type);
		}
		
		//---------------------------------------------------------------------------
		public int AddAddColumnEvent(WizardTableColumnInfo aColumn, int aColumnOrder)
		{
			return AddColumnEvent(aColumn, aColumnOrder, null, -1, EventType.AddColumn);
		}
			
		//---------------------------------------------------------------------------
		public int AddAlterColumnTypeEvent(WizardTableColumnInfo aColumn, int aColumnOrder, WizardTableColumnInfo aPreviousColumnInfo)
		{
			return AddColumnEvent(aColumn, aColumnOrder, aPreviousColumnInfo, -1, EventType.AlterColumnType);
		}

		//---------------------------------------------------------------------------
		public int AddChangeColumnDefaultValueEvent(WizardTableColumnInfo aColumn, int aColumnOrder, WizardTableColumnInfo aPreviousColumnInfo)
		{
			if 
				(
				aColumn == null || 
				aPreviousColumnInfo == null || 
				aColumn.HasSameDefaultValueAs(aPreviousColumnInfo)
				)
				return -1;
			
			return AddColumnEvent(aColumn, aColumnOrder, aPreviousColumnInfo, -1, EventType.ChangeColumnDefaultValue);
		}

		//---------------------------------------------------------------------------
		public int AddDropColumnEvent(WizardTableColumnInfo aColumn, int aColumnOrder)
		{
			return AddColumnEvent(aColumn, aColumnOrder, null, -1, EventType.DropColumn);
		}

		//---------------------------------------------------------------------------
		public int AddChangeColumnOrderEvent(WizardTableColumnInfo aColumn, int aColumnOrder, int aPreviousColumnOrder)
		{
			return AddColumnEvent(aColumn, aColumnOrder, null, aPreviousColumnOrder, EventType.ChangeColumnOrder);
		}

		//---------------------------------------------------------------------------
		public int AddIndexEvent(WizardTableIndexInfo aIndex, WizardTableIndexInfo aPreviousIndexInfo, EventType aType)
		{
			if (aIndex == null || aType == EventType.Undefined)
				return -1;

			if (indexesEvents != null && indexesEvents.Count > 0)
			{
				for (int i = 0; i < indexesEvents.Count; i++)
				{
					IndexHistoryEvent aEvent = indexesEvents[i];
					if 
						(
						aEvent != null &&
						aType == aEvent.Type &&
						WizardTableIndexInfo.Equals(aIndex, aEvent.IndexInfo) &&
						WizardTableIndexInfo.Equals(aPreviousIndexInfo, aEvent.PreviousIndexInfo)
						)
						return i; // l'evento è già stato inserito precedentemente
				}
			}

			if (indexesEvents == null)
				indexesEvents = new IndexHistoryEventCollection();

			return indexesEvents.Add(new IndexHistoryEvent(aIndex, aPreviousIndexInfo, aType));
		}

		//---------------------------------------------------------------------------
		public int AddIndexEvent(IndexHistoryEvent aIndexEvent)
		{
			if (aIndexEvent == null)
				return -1;

			return AddIndexEvent(aIndexEvent.IndexInfo, aIndexEvent.PreviousIndexInfo, aIndexEvent.Type);
		}

		//---------------------------------------------------------------------------
		public int AddForeignKeyConstraintEvent(WizardForeignKeyInfo aForeignKeyInfo, EventType aType)
		{
			if (aForeignKeyInfo == null || aType == EventType.Undefined)
				return -1;

			if (foreignKeyEvents != null && foreignKeyEvents.Count > 0)
			{
				for (int i = 0; i < foreignKeyEvents.Count; i++)
				{
					ForeignKeyHistoryEvent aEvent = foreignKeyEvents[i];
					if 
						(
						aEvent != null &&
						aType == aEvent.Type &&
						WizardForeignKeyInfo.Equals(aForeignKeyInfo, aEvent.ForeignKeyInfo)
						)
						return i; // l'evento è già stato inserito precedentemente
				}
			}

			if (foreignKeyEvents == null)
				foreignKeyEvents = new ForeignKeyHistoryEventCollection();

			return foreignKeyEvents.Add(new ForeignKeyHistoryEvent(aForeignKeyInfo, aType));
		}

		//---------------------------------------------------------------------------
		public int AddCreateForeignKeyConstraintEvent(WizardForeignKeyInfo aForeignKeyInfo)
		{
			return AddForeignKeyConstraintEvent(aForeignKeyInfo, EventType.CreateConstraint);
		}

		//---------------------------------------------------------------------------
		public int AddDropForeignKeyConstraintEvent(WizardForeignKeyInfo aForeignKeyInfo)
		{
			return AddForeignKeyConstraintEvent(aForeignKeyInfo, EventType.DropConstraint);
		}

		//---------------------------------------------------------------------------
		public int AddChangeTBGuidExistenceColumnEvent(WizardTableColumnInfo guidColumnInfo, bool isTBGuidPresent)
		{
			if (guidColumnInfo == null)
				return -1;

			if (isTBGuidPresent)
				return AddAddColumnEvent(guidColumnInfo, -1);
	
			return AddDropColumnEvent(guidColumnInfo, -1);
		}
		
		//---------------------------------------------------------------------------
		public ColumnHistoryEventCollection GetColumnEventsByName(string aColumnName)
		{
			if 
				(
				aColumnName == null ||
				aColumnName.Length == 0 ||
				columnsEvents == null ||
				columnsEvents.Count == 0
				)
				return null;

			ColumnHistoryEventCollection eventsToReturn = new ColumnHistoryEventCollection();

			foreach (ColumnHistoryEvent aEvent in columnsEvents)
			{
				if (aEvent.ColumnInfo != null && String.Compare(aEvent.ColumnInfo.Name, aColumnName, true) == 0)
					eventsToReturn.Add(aEvent);
			}
			
			return (eventsToReturn != null && eventsToReturn.Count > 0) ? eventsToReturn : null;
		}

		//---------------------------------------------------------------------------
		public ColumnHistoryEventCollection GetColumnEventsByType(EventType aEventType)
		{
			if (columnsEvents == null || columnsEvents.Count == 0)
				return null;

			ColumnHistoryEventCollection eventsToReturn = new ColumnHistoryEventCollection();

			foreach (ColumnHistoryEvent aEvent in columnsEvents)
			{
				if (aEvent.Type == aEventType)
					eventsToReturn.Add(aEvent);
			}
			
			return (eventsToReturn != null && eventsToReturn.Count > 0) ? eventsToReturn : null;
		}

		//---------------------------------------------------------------------------
		public ColumnHistoryEvent GetColumnEventByNameAndType(string aColumnName, EventType aEventType)
		{
			if 
				(
				aColumnName == null ||
				aColumnName.Length == 0 ||
				columnsEvents == null ||
				columnsEvents.Count == 0 ||
				aEventType == EventType.Undefined
				)
				return null;

			foreach (ColumnHistoryEvent aEvent in columnsEvents)
			{
				if
					(
					aEvent.Type == aEventType &&
					aEvent.ColumnInfo != null && 
					String.Compare(aEvent.ColumnInfo.Name, aColumnName, true) == 0
					)
					return aEvent;
			}
			
			return null;
		}
		
		//---------------------------------------------------------------------------
		public ColumnHistoryEvent GetAddColumnEventByColumnName(string aColumnName)
		{
			return GetColumnEventByNameAndType(aColumnName, EventType.AddColumn);
		}

		//---------------------------------------------------------------------------
		public ColumnHistoryEvent GetDropColumnEventByColumnName(string aColumnName)
		{
			return GetColumnEventByNameAndType(aColumnName, EventType.DropColumn);
		}

		//---------------------------------------------------------------------------
		public ColumnHistoryEvent GetAlterColumnTypeEventByColumnName(string aColumnName)
		{
			return GetColumnEventByNameAndType(aColumnName, EventType.AlterColumnType);
		}

		//---------------------------------------------------------------------------
		public ColumnHistoryEvent GetChangeColumnDefaultValueEventByColumnName(string aColumnName)
		{
			return GetColumnEventByNameAndType(aColumnName, EventType.ChangeColumnDefaultValue);
		}

		//---------------------------------------------------------------------------
		public ForeignKeyHistoryEvent GetForeignKeyEventByConstraintAndType(string aConstraintName, EventType aEventType)
		{
			if 
				(
				aConstraintName == null ||
				aConstraintName.Length == 0 ||
				foreignKeyEvents == null ||
				foreignKeyEvents.Count == 0 ||
				aEventType == EventType.Undefined
				)
				return null;

			foreach (ForeignKeyHistoryEvent aEvent in foreignKeyEvents)
			{
				if
					(
					aEvent.Type == aEventType &&
					aEvent.ForeignKeyInfo != null && 
					String.Compare(aEvent.ForeignKeyInfo.ConstraintName, aConstraintName, true) == 0
					)
					return aEvent;
			}
			
			return null;
		}
		
		//---------------------------------------------------------------------------
		public ForeignKeyHistoryEvent GetCreateForeignKeyEventByConstraintName(string aConstraintName)
		{
			return GetForeignKeyEventByConstraintAndType(aConstraintName, EventType.CreateConstraint);
		}

		//---------------------------------------------------------------------------
		public ForeignKeyHistoryEvent GetDropForeignKeyEventByConstraintName(string aConstraintName)
		{
			return GetForeignKeyEventByConstraintAndType(aConstraintName, EventType.DropConstraint);
		}

		//---------------------------------------------------------------------------
		public void UpdateWithNewerInfo(TableHistoryStep aHistoryStep)
		{
			if 
				(
				aHistoryStep == null || 
				aHistoryStep.EventsCount == 0 || 
				aHistoryStep.DbReleaseNumber != dbReleaseNumber
				)
				return;

			if (aHistoryStep.ColumnsEventsCount > 0)
			{
				foreach (ColumnHistoryEvent aColumnEvent in aHistoryStep.ColumnsEvents)
				{
					if (aColumnEvent.ColumnInfo == null || aColumnEvent.ColumnInfo.Name == null || aColumnEvent.ColumnInfo.Name.Length == 0)
						continue;

					// Se il nuovo step riaggiunge una colonna che era stata eliminata dallo step 
					// corrente, devo ovviamente cancellare l'evento di eliminazione della 
					// colonna. Prima, però, devo verificare che le definizioni delle due colonne 
					// coincidano, perchè altrimenti devo aggiungere un evento di modifica della
					// colonna
					if (aColumnEvent.Type == EventType.AddColumn)
					{
						ColumnHistoryEvent existingDropEvent = GetDropColumnEventByColumnName(aColumnEvent.ColumnInfo.Name);
						if (existingDropEvent != null)
						{
							columnsEvents.Remove(existingDropEvent);
								
							if 
								(
								existingDropEvent.ColumnInfo.DataType != aColumnEvent.ColumnInfo.DataType ||
								existingDropEvent.ColumnInfo.DataLength != aColumnEvent.ColumnInfo.DataLength ||
								existingDropEvent.ColumnInfo.IsPrimaryKeySegment != aColumnEvent.ColumnInfo.IsPrimaryKeySegment
								)
							{
								aColumnEvent.Type = EventType.AlterColumnType;
								aColumnEvent.PreviousColumnInfo = existingDropEvent.ColumnInfo;
								aColumnEvent.PreviousColumnOrder = existingDropEvent.ColumnOrder;

								columnsEvents.Add(aColumnEvent);
							}
							else if (!existingDropEvent.ColumnInfo.HasSameDefaultValueAs(aColumnEvent.ColumnInfo))
							{
								aColumnEvent.Type = EventType.ChangeColumnDefaultValue;
								aColumnEvent.PreviousColumnInfo = existingDropEvent.ColumnInfo;
								aColumnEvent.PreviousColumnOrder = existingDropEvent.ColumnOrder;

								columnsEvents.Add(aColumnEvent);
							}
							continue;

						}

						if (columnsEvents == null)
							columnsEvents = new ColumnHistoryEventCollection();
						
						columnsEvents.Add(aColumnEvent);				
						
						continue;
					}
						
					// Se il nuovo step elimina una colonna che è stata aggiunta dallo step 
					// corrente, devo semplicemente cancellare l'evento di aggiunta della colonna
					if (aColumnEvent.Type == EventType.DropColumn)
					{
						ColumnHistoryEvent existingAddEvent = GetAddColumnEventByColumnName(aColumnEvent.ColumnInfo.Name);
						if (existingAddEvent == null)
						{
							if (columnsEvents == null)
								columnsEvents = new ColumnHistoryEventCollection();
							
							columnsEvents.Add(aColumnEvent);
						}
						else
							columnsEvents.Remove(existingAddEvent);
						continue;
					}

					// Se il nuovo step rinomina una colonna che è stata aggiunta dallo step 
					// corrente, devo rinominare la colonna nell'evento di aggiunta
					if (aColumnEvent.Type == EventType.RenameColumn)
					{
						ColumnHistoryEvent existingAddEvent = GetAddColumnEventByColumnName(aColumnEvent.ColumnInfo.Name);
						if (existingAddEvent == null)
						{
							if (columnsEvents == null)
								columnsEvents = new ColumnHistoryEventCollection();
							
							columnsEvents.Add(aColumnEvent);
						}
						else
							existingAddEvent.ColumnInfo = aColumnEvent.ColumnInfo;

						continue;
					}

					if (aColumnEvent.Type == EventType.AlterColumnType)
					{
						ColumnHistoryEvent existingAddEvent = GetAddColumnEventByColumnName(aColumnEvent.ColumnInfo.Name);
						if (existingAddEvent != null)
						{
							existingAddEvent.ColumnInfo = aColumnEvent.ColumnInfo;
							existingAddEvent.ColumnOrder = aColumnEvent.ColumnOrder;
							continue;
						}

						ColumnHistoryEvent existingAlterEvent = GetAlterColumnTypeEventByColumnName(aColumnEvent.ColumnInfo.Name);
						if (existingAlterEvent != null)
						{
							existingAlterEvent.ColumnInfo = aColumnEvent.ColumnInfo;
							existingAlterEvent.ColumnOrder = aColumnEvent.ColumnOrder;
							continue;
						}

						ColumnHistoryEvent existingChangeDefaultEvent = GetChangeColumnDefaultValueEventByColumnName(aColumnEvent.ColumnInfo.Name);
						if (existingChangeDefaultEvent != null)
						{
							aColumnEvent.PreviousColumnInfo = existingChangeDefaultEvent.PreviousColumnInfo;
							columnsEvents.Remove(existingChangeDefaultEvent);
						}
		
						if (columnsEvents == null)
							columnsEvents = new ColumnHistoryEventCollection();
						
						columnsEvents.Add(aColumnEvent);
						
						continue;
					}
				
					if (aColumnEvent.Type == EventType.ChangeColumnDefaultValue)
					{
						ColumnHistoryEvent existingAddEvent = GetAddColumnEventByColumnName(aColumnEvent.ColumnInfo.Name);
						if (existingAddEvent != null)
						{
							existingAddEvent.ColumnInfo = aColumnEvent.ColumnInfo;
							existingAddEvent.ColumnOrder = aColumnEvent.ColumnOrder;
							continue;
						}

						ColumnHistoryEvent existingAlterEvent = GetAlterColumnTypeEventByColumnName(aColumnEvent.ColumnInfo.Name);
						if (existingAlterEvent != null)
						{
							existingAlterEvent.ColumnInfo = aColumnEvent.ColumnInfo;
							existingAlterEvent.ColumnOrder = aColumnEvent.ColumnOrder;
							continue;
						}

						ColumnHistoryEvent existingChangeDefaultEvent = GetColumnEventByNameAndType(aColumnEvent.ColumnInfo.Name, aColumnEvent.Type);
						if (existingChangeDefaultEvent == null)
						{
							if (columnsEvents == null)
								columnsEvents = new ColumnHistoryEventCollection();
							
							columnsEvents.Add(aColumnEvent);
						}
						else
							existingChangeDefaultEvent.ColumnInfo = aColumnEvent.ColumnInfo;
						continue;
					}
					
					if (aColumnEvent.Type == EventType.ChangeColumnOrder)
					{
						ColumnHistoryEvent existingChangeOrderEvent = GetColumnEventByNameAndType(aColumnEvent.ColumnInfo.Name, EventType.ChangeColumnOrder);
						if (existingChangeOrderEvent != null)
						{
							existingChangeOrderEvent.ColumnOrder = aColumnEvent.ColumnOrder;
							continue;
						}
						
						if (columnsEvents == null)
							columnsEvents = new ColumnHistoryEventCollection();

						columnsEvents.Add(aColumnEvent);
						
						continue;
					}
				}
			}
		
			if (aHistoryStep.ForeignKeyEventsCount > 0)
			{
				foreach (ForeignKeyHistoryEvent aForeignKeyEvent in aHistoryStep.ForeignKeyEvents)
				{
					if (aForeignKeyEvent.ForeignKeyInfo == null)
						continue;

					// Se il nuovo step riaggiunge una chiave esterna che era stata eliminata dallo  
					// step corrente, devo ovviamente cancellare l'evento di eliminazione della chiave
					// esterna.
					if (aForeignKeyEvent.Type == EventType.CreateConstraint)
					{
						ForeignKeyHistoryEvent existingDropEvent = GetDropForeignKeyEventByConstraintName(aForeignKeyEvent.ForeignKeyInfo.ConstraintName);
						if (existingDropEvent != null)
							foreignKeyEvents.Remove(existingDropEvent);
					
						if (foreignKeyEvents == null)
							foreignKeyEvents = new ForeignKeyHistoryEventCollection();
						
						foreignKeyEvents.Add(aForeignKeyEvent);
			
						continue;
					}
					// Se il nuovo step elimina una chiave esterna che è stata aggiunta dallo step 
					// corrente, devo semplicemente cancellare l'evento di aggiunta della chiave esterna
					if (aForeignKeyEvent.Type == EventType.DropConstraint)
					{
						ForeignKeyHistoryEvent existingAddEvent = GetCreateForeignKeyEventByConstraintName(aForeignKeyEvent.ForeignKeyInfo.ConstraintName);
						if (existingAddEvent == null)
						{
							if (foreignKeyEvents == null)
								foreignKeyEvents = new ForeignKeyHistoryEventCollection();
							
							foreignKeyEvents.Add(aForeignKeyEvent);
						}
						else
							foreignKeyEvents.Remove(existingAddEvent);
						continue;
					}
				}
			}
		}
		
		//---------------------------------------------------------------------------
		public bool HasColumnEventsOfType(EventType aEventType)
		{
			if (columnsEvents == null || columnsEvents.Count == 0)
				return false;

			foreach (ColumnHistoryEvent aColumnEvent in columnsEvents)
			{
				if (aColumnEvent.Type == aEventType)
					return true;
			}

			return false;
		}
		
		//---------------------------------------------------------------------------
		public bool HasAddColumnEvents()
		{
			return HasColumnEventsOfType(EventType.AddColumn);
		}

		//---------------------------------------------------------------------------
		public bool HasAlterColumnEvents()
		{
			return (HasColumnEventsOfType(EventType.AlterColumnType) || HasColumnEventsOfType(EventType.ChangeColumnDefaultValue));
		}

		//---------------------------------------------------------------------------
		public bool HasDropColumnEvents()
		{
			return HasColumnEventsOfType(EventType.DropColumn);
		}

		//---------------------------------------------------------------------------
		public bool HasRenameColumnEvents()
		{
			return HasColumnEventsOfType(EventType.RenameColumn);
		}

		//---------------------------------------------------------------------------
		public bool IsPrimaryKeyToModify()
		{
			if (indexesEvents != null && indexesEvents.Count > 0)
			{
				foreach (IndexHistoryEvent aIndexEvent in indexesEvents)
				{
					if (aIndexEvent.Type == EventType.ModifyPrimaryKey)
						return true;
				}
			}
			
			if (columnsEvents != null && columnsEvents.Count > 0)
			{
				foreach (ColumnHistoryEvent aColumnEvent in columnsEvents)
				{
					if 
						(
						aColumnEvent.ColumnInfo == null || 
						aColumnEvent.ColumnInfo.Name == null || 
						aColumnEvent.ColumnInfo.Name.Length == 0 ||
						aColumnEvent.Type == EventType.Undefined
						)
						continue;

					if (aColumnEvent.Type == EventType.AddColumn)
					{
						if (aColumnEvent.ColumnInfo.IsPrimaryKeySegment)
							return true;
						continue;
					}
					if (aColumnEvent.Type == EventType.DropColumn)
					{
						if (aColumnEvent.ColumnInfo.IsPrimaryKeySegment)
							return true;
						continue;
					}
					if (aColumnEvent.PreviousColumnInfo != null)
					{
						if (aColumnEvent.ColumnInfo.IsPrimaryKeySegment != aColumnEvent.PreviousColumnInfo.IsPrimaryKeySegment)
							return true;

						continue;
					}
				}
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public bool AreForeignKeysToAdd()
		{
			if (foreignKeyEvents != null && foreignKeyEvents.Count > 0)
			{
				foreach (ForeignKeyHistoryEvent aForeignKeyEvent in foreignKeyEvents)
				{
					if (aForeignKeyEvent.Type == EventType.CreateConstraint)
						return true;
				}
			}
			
			return false;
		}

		//---------------------------------------------------------------------------
		public bool AreForeignKeysToDrop()
		{
			if (foreignKeyEvents != null && foreignKeyEvents.Count > 0)
			{
				foreach (ForeignKeyHistoryEvent aForeignKeyEvent in foreignKeyEvents)
				{
					if (aForeignKeyEvent.Type == EventType.DropConstraint)
						return true;
				}
			}
			
			return false;
		}

		#endregion // TableHistoryStep public methods
	}
	
	#endregion // TableHistoryStep Class

	#region TableHistoryStepsCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for TableHistoryStepsCollection.
	/// </summary>
	public class TableHistoryStepsCollection : ReadOnlyCollectionBase, IList
	{
		//============================================================================
		private class TableHistoryStepsSorter : IComparer
		{
			public int Compare(object x, object y)
			{
				Debug.Assert(x != null && x is TableHistoryStep);
				Debug.Assert(y != null && y is TableHistoryStep);

				TableHistoryStep step1 = (TableHistoryStep)x;
				TableHistoryStep step2 = (TableHistoryStep)y;

				return ((int)step1.DbReleaseNumber - (int)step2.DbReleaseNumber);
			}
		}
			
		//---------------------------------------------------------------------------
		public TableHistoryStepsCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is TableHistoryStep))
					throw new NotSupportedException();

				this[index] = (TableHistoryStep)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is TableHistoryStep))
				throw new NotSupportedException();

			return this.Contains((TableHistoryStep)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((TableHistoryStep)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is TableHistoryStep))
				throw new NotSupportedException();

			return this.IndexOf((TableHistoryStep)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is TableHistoryStep))
				throw new NotSupportedException();

			Insert(index, (TableHistoryStep)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is TableHistoryStep))
				throw new NotSupportedException();

			Remove((TableHistoryStep)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion
		
		//---------------------------------------------------------------------------
		public TableHistoryStep this[int index]
		{
			get {  return (TableHistoryStep)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (TableHistoryStep)value; 
			}
		}

		//---------------------------------------------------------------------------
		public TableHistoryStep[] ToArray()
		{
			return (TableHistoryStep[])InnerList.ToArray(typeof(TableHistoryStep));
		}

		//---------------------------------------------------------------------------
		public int Add(TableHistoryStep aHistoryStepToAdd)
		{
			if (Contains(aHistoryStepToAdd))
				return IndexOf(aHistoryStepToAdd);

			return InnerList.Add(aHistoryStepToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(TableHistoryStepsCollection aStepsCollectionTo)
		{
			if (aStepsCollectionTo == null || aStepsCollectionTo.Count == 0)
				return;

			foreach (TableHistoryStep aHistoryStepToAdd in aStepsCollectionTo)
				Add(aHistoryStepToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, TableHistoryStep aHistoryStepToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aHistoryStepToInsert))
				return;

			InnerList.Insert(index, aHistoryStepToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(TableHistoryStep beforeStep, TableHistoryStep aHistoryStepToInsert)
		{
			if (beforeStep == null)
				Add(aHistoryStepToInsert);

			if (!Contains(beforeStep))
				return;

			if (Contains(aHistoryStepToInsert))
				return;

			Insert(IndexOf(beforeStep), aHistoryStepToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(TableHistoryStep aHistoryStepToRemove)
		{
			if (!Contains(aHistoryStepToRemove))
				return;

			InnerList.Remove(aHistoryStepToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(TableHistoryStep aHistoryStepToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aHistoryStepToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(TableHistoryStep aHistoryStepToSearch)
		{
			if (!Contains(aHistoryStepToSearch))
				return -1;
			
			return InnerList.IndexOf(aHistoryStepToSearch);
		}
		
		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is TableHistoryStepsCollection))
				return false;

			if (obj == this)
				return true;

			if (((TableHistoryStepsCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!TableHistoryStep.Equals(this[i], ((TableHistoryStepsCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
	
		//---------------------------------------------------------------------------
		public TableHistoryStep GetDbReleaseStep(uint aDbReleaseNumber)
		{
			if (aDbReleaseNumber <= 0 || this.Count == 0)
				return null;

			for (int i = 0; i < this.Count; i++)
			{
				if (this[i].DbReleaseNumber == aDbReleaseNumber)
					return this[i];
			}
			return null;
		}

		//---------------------------------------------------------------------------
		public void SortByDbReleaseNumber()
		{
			if (this.Count == 0)
				return;

			InnerList.Sort(new TableHistoryStepsSorter());
		}
	}

	#endregion // TableHistoryStepsCollection Class
		
	#region ColumnHistoryEvent Class

	//===============================================================================
	/// <summary>
	/// Summary description for ColumnHistoryEvent.
	/// </summary>
	public class ColumnHistoryEvent
	{		
		private WizardTableColumnInfo columnInfo = null;
		private int columnOrder = -1;
		private WizardTableColumnInfo previousColumnInfo = null;
		private int previousColumnOrder = -1;
		private TableHistoryStep.EventType type = TableHistoryStep.EventType.Undefined;

		//---------------------------------------------------------------------------
		public ColumnHistoryEvent
			(
			WizardTableColumnInfo		aColumn, 
			int							aColumnOrder, 
			WizardTableColumnInfo		aPreviousColumnInfo,
			int							aPreviousColumnOrder, 
			TableHistoryStep.EventType	aType
			)
		{
			columnInfo = aColumn;
			columnOrder = aColumnOrder;
			previousColumnInfo = aPreviousColumnInfo;
			previousColumnOrder = aPreviousColumnOrder;
			type = aType;
		}
			
		//---------------------------------------------------------------------------
		public ColumnHistoryEvent(WizardTableColumnInfo aColumn, int aColumnOrder) : this(aColumn, aColumnOrder, null, -1, TableHistoryStep.EventType.AddColumn)
		{
		}

//		//---------------------------------------------------------------------------
//		public ColumnHistoryEvent(WizardTableColumnInfo aColumn) : this(aColumn, -1, null, -1, TableHistoryStep.EventType.AddColumn)
//		{
//		}
//
		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is ColumnHistoryEvent))
				return false;

			if (obj == this)
				return true;

			return 
				(
				WizardTableColumnInfo.Equals(columnInfo, ((ColumnHistoryEvent)obj).ColumnInfo) &&
				columnOrder == ((ColumnHistoryEvent)obj).ColumnOrder &&
				WizardTableColumnInfo.Equals(previousColumnInfo, ((ColumnHistoryEvent)obj).PreviousColumnInfo) &&
				previousColumnOrder == ((ColumnHistoryEvent)obj).PreviousColumnOrder &&
				type == ((ColumnHistoryEvent)obj).Type
				);
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
				
		//---------------------------------------------------------------------------
		public TableHistoryStep.EventType Type { get { return type; } set { type = value; } }
		//---------------------------------------------------------------------------
		public WizardTableColumnInfo ColumnInfo { get { return columnInfo; } set { columnInfo = value; } }
		//---------------------------------------------------------------------------
		public int ColumnOrder { get { return columnOrder; } set { columnOrder = value; } }
		//---------------------------------------------------------------------------
		public WizardTableColumnInfo PreviousColumnInfo { get { return previousColumnInfo; } set { previousColumnInfo = value; } }
		//---------------------------------------------------------------------------
		public int PreviousColumnOrder { get { return previousColumnOrder; } set { previousColumnOrder = value; } }
	}
	
	#endregion //ColumnHistoryEvent Class

	#region ColumnHistoryEventCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for ColumnHistoryEventCollection.
	/// </summary>
	public class ColumnHistoryEventCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public ColumnHistoryEventCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is ColumnHistoryEvent))
					throw new NotSupportedException();

				this[index] = (ColumnHistoryEvent)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is ColumnHistoryEvent))
				throw new NotSupportedException();

			return this.Contains((ColumnHistoryEvent)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((ColumnHistoryEvent)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is ColumnHistoryEvent))
				throw new NotSupportedException();

			return this.IndexOf((ColumnHistoryEvent)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is ColumnHistoryEvent))
				throw new NotSupportedException();

			Insert(index, (ColumnHistoryEvent)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is ColumnHistoryEvent))
				throw new NotSupportedException();

			Remove((ColumnHistoryEvent)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion
		
		//---------------------------------------------------------------------------
		public ColumnHistoryEvent this[int index]
		{
			get {  return (ColumnHistoryEvent)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (ColumnHistoryEvent)value; 
			}
		}

		//---------------------------------------------------------------------------
		public ColumnHistoryEvent[] ToArray()
		{
			return (ColumnHistoryEvent[])InnerList.ToArray(typeof(ColumnHistoryEvent));
		}

		//---------------------------------------------------------------------------
		public int Add(ColumnHistoryEvent aColumnEventToAdd)
		{
			if (Contains(aColumnEventToAdd))
				return IndexOf(aColumnEventToAdd);

			return InnerList.Add(aColumnEventToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(ColumnHistoryEventCollection aEventsCollectionToAdd)
		{
			if (aEventsCollectionToAdd == null || aEventsCollectionToAdd.Count == 0)
				return;

			foreach (ColumnHistoryEvent aColumnEventToAdd in aEventsCollectionToAdd)
				Add(aColumnEventToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, ColumnHistoryEvent aColumnEventToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aColumnEventToInsert))
				return;

			InnerList.Insert(index, aColumnEventToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(ColumnHistoryEvent beforeEvent, ColumnHistoryEvent aColumnEventToInsert)
		{
			if (beforeEvent == null)
				Add(aColumnEventToInsert);

			if (!Contains(beforeEvent))
				return;

			if (Contains(aColumnEventToInsert))
				return;

			Insert(IndexOf(beforeEvent), aColumnEventToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(ColumnHistoryEvent aColumnEventToRemove)
		{
			if (!Contains(aColumnEventToRemove))
				return;

			InnerList.Remove(aColumnEventToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(ColumnHistoryEvent aColumnEventToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aColumnEventToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(ColumnHistoryEvent aColumnEventToSearch)
		{
			if (!Contains(aColumnEventToSearch))
				return -1;
			
			return InnerList.IndexOf(aColumnEventToSearch);
		}
		
		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is ColumnHistoryEventCollection))
				return false;

			if (obj == this)
				return true;

			if (((ColumnHistoryEventCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!ColumnHistoryEvent.Equals(this[i], ((ColumnHistoryEventCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
	}

	#endregion // ColumnHistoryEventCollection Class

	#region IndexHistoryEvent Class

	//===============================================================================
	/// <summary>
	/// Summary description for IndexHistoryEvent.
	/// </summary>
	public class IndexHistoryEvent
	{		
		private WizardTableIndexInfo indexInfo = null;
		private WizardTableIndexInfo previousIndexInfo = null;
		private TableHistoryStep.EventType type = TableHistoryStep.EventType.Undefined;

		//---------------------------------------------------------------------------
		public IndexHistoryEvent(WizardTableIndexInfo aIndex, WizardTableIndexInfo aPreviousIndexInfo, TableHistoryStep.EventType aType)
		{
			indexInfo = aIndex;
			previousIndexInfo = aPreviousIndexInfo;
			type = aType;
		}
			
		//---------------------------------------------------------------------------
		public IndexHistoryEvent(WizardTableIndexInfo aIndex) : this(aIndex, null, TableHistoryStep.EventType.CreateIndex)
		{
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is IndexHistoryEvent))
				return false;

			if (obj == this)
				return true;

			return 
				(
				WizardTableIndexInfo.Equals(indexInfo, ((IndexHistoryEvent)obj).IndexInfo) &&
				WizardTableIndexInfo.Equals(previousIndexInfo, ((IndexHistoryEvent)obj).PreviousIndexInfo) &&
				type == ((IndexHistoryEvent)obj).Type
				);
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
				
		//---------------------------------------------------------------------------
		public TableHistoryStep.EventType Type { get { return type; } }
		//---------------------------------------------------------------------------
		public WizardTableIndexInfo IndexInfo { get { return indexInfo; } }
		//---------------------------------------------------------------------------
		public WizardTableIndexInfo PreviousIndexInfo { get { return previousIndexInfo; } }
	}
	
	#endregion //IndexHistoryEvent Class

	#region IndexHistoryEventCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for IndexHistoryEventCollection.
	/// </summary>
	public class IndexHistoryEventCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public IndexHistoryEventCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is IndexHistoryEvent))
					throw new NotSupportedException();

				this[index] = (IndexHistoryEvent)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is IndexHistoryEvent))
				throw new NotSupportedException();

			return this.Contains((IndexHistoryEvent)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((IndexHistoryEvent)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is IndexHistoryEvent))
				throw new NotSupportedException();

			return this.IndexOf((IndexHistoryEvent)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is IndexHistoryEvent))
				throw new NotSupportedException();

			Insert(index, (IndexHistoryEvent)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is IndexHistoryEvent))
				throw new NotSupportedException();

			Remove((IndexHistoryEvent)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion
		
		//---------------------------------------------------------------------------
		public IndexHistoryEvent this[int index]
		{
			get {  return (IndexHistoryEvent)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (IndexHistoryEvent)value; 
			}
		}

		//---------------------------------------------------------------------------
		public IndexHistoryEvent[] ToArray()
		{
			return (IndexHistoryEvent[])InnerList.ToArray(typeof(IndexHistoryEvent));
		}

		//---------------------------------------------------------------------------
		public int Add(IndexHistoryEvent aIndexEventToAdd)
		{
			if (Contains(aIndexEventToAdd))
				return IndexOf(aIndexEventToAdd);

			return InnerList.Add(aIndexEventToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(IndexHistoryEventCollection aEventsCollectionToAdd)
		{
			if (aEventsCollectionToAdd == null || aEventsCollectionToAdd.Count == 0)
				return;

			foreach (IndexHistoryEvent aIndexEventToAdd in aEventsCollectionToAdd)
				Add(aIndexEventToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, IndexHistoryEvent aIndexEventToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aIndexEventToInsert))
				return;

			InnerList.Insert(index, aIndexEventToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(IndexHistoryEvent beforeEvent, IndexHistoryEvent aIndexEventToInsert)
		{
			if (beforeEvent == null)
				Add(aIndexEventToInsert);

			if (!Contains(beforeEvent))
				return;

			if (Contains(aIndexEventToInsert))
				return;

			Insert(IndexOf(beforeEvent), aIndexEventToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(IndexHistoryEvent aIndexEventToRemove)
		{
			if (!Contains(aIndexEventToRemove))
				return;

			InnerList.Remove(aIndexEventToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(IndexHistoryEvent aIndexEventToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aIndexEventToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(IndexHistoryEvent aIndexEventToSearch)
		{
			if (!Contains(aIndexEventToSearch))
				return -1;
			
			return InnerList.IndexOf(aIndexEventToSearch);
		}
		
		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is IndexHistoryEventCollection))
				return false;

			if (obj == this)
				return true;

			if (((IndexHistoryEventCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!IndexHistoryEvent.Equals(this[i], ((IndexHistoryEventCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
	}

	#endregion // IndexHistoryEventCollection Class

	#region ForeignKeyHistoryEvent Class

	//===============================================================================
	/// <summary>
	/// Summary description for ForeignKeyHistoryEvent.
	/// </summary>
	public class ForeignKeyHistoryEvent
	{		
		private WizardForeignKeyInfo foreignKeyInfo = null;
		private TableHistoryStep.EventType type = TableHistoryStep.EventType.Undefined;

		//---------------------------------------------------------------------------
		public ForeignKeyHistoryEvent(WizardForeignKeyInfo aForeignKeyInfo, TableHistoryStep.EventType aType)
		{
			foreignKeyInfo = aForeignKeyInfo;
			type = aType;
		}
			
		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is ForeignKeyHistoryEvent))
				return false;

			if (obj == this)
				return true;

			return 
				(
				WizardForeignKeyInfo.Equals(foreignKeyInfo, ((ForeignKeyHistoryEvent)obj).ForeignKeyInfo) &&
				type == ((ForeignKeyHistoryEvent)obj).Type
				);
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
				
		//---------------------------------------------------------------------------
		public TableHistoryStep.EventType Type { get { return type; } }
		//---------------------------------------------------------------------------
		public WizardForeignKeyInfo ForeignKeyInfo { get { return foreignKeyInfo; } }
	}
	
	#endregion //ForeignKeyHistoryEvent Class

	#region ForeignKeyHistoryEventCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for ForeignKeyHistoryEventCollection.
	/// </summary>
	public class ForeignKeyHistoryEventCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public ForeignKeyHistoryEventCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is ForeignKeyHistoryEvent))
					throw new NotSupportedException();

				this[index] = (ForeignKeyHistoryEvent)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is ForeignKeyHistoryEvent))
				throw new NotSupportedException();

			return this.Contains((ForeignKeyHistoryEvent)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((ForeignKeyHistoryEvent)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is ForeignKeyHistoryEvent))
				throw new NotSupportedException();

			return this.IndexOf((ForeignKeyHistoryEvent)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is ForeignKeyHistoryEvent))
				throw new NotSupportedException();

			Insert(index, (ForeignKeyHistoryEvent)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is ForeignKeyHistoryEvent))
				throw new NotSupportedException();

			Remove((ForeignKeyHistoryEvent)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion
		
		//---------------------------------------------------------------------------
		public ForeignKeyHistoryEvent this[int index]
		{
			get {  return (ForeignKeyHistoryEvent)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (ForeignKeyHistoryEvent)value; 
			}
		}

		//---------------------------------------------------------------------------
		public ForeignKeyHistoryEvent[] ToArray()
		{
			return (ForeignKeyHistoryEvent[])InnerList.ToArray(typeof(ForeignKeyHistoryEvent));
		}

		//---------------------------------------------------------------------------
		public int Add(ForeignKeyHistoryEvent aForeignKeyEventToAdd)
		{
			if (Contains(aForeignKeyEventToAdd))
				return IndexOf(aForeignKeyEventToAdd);

			return InnerList.Add(aForeignKeyEventToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(ForeignKeyHistoryEventCollection aEventsCollectionToAdd)
		{
			if (aEventsCollectionToAdd == null || aEventsCollectionToAdd.Count == 0)
				return;

			foreach (ForeignKeyHistoryEvent aForeignKeyEventToAdd in aEventsCollectionToAdd)
				Add(aForeignKeyEventToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, ForeignKeyHistoryEvent aForeignKeyEventToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aForeignKeyEventToInsert))
				return;

			InnerList.Insert(index, aForeignKeyEventToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(ForeignKeyHistoryEvent beforeEvent, ForeignKeyHistoryEvent aForeignKeyEventToInsert)
		{
			if (beforeEvent == null)
				Add(aForeignKeyEventToInsert);

			if (!Contains(beforeEvent))
				return;

			if (Contains(aForeignKeyEventToInsert))
				return;

			Insert(IndexOf(beforeEvent), aForeignKeyEventToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(ForeignKeyHistoryEvent aForeignKeyEventToRemove)
		{
			if (!Contains(aForeignKeyEventToRemove))
				return;

			InnerList.Remove(aForeignKeyEventToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(ForeignKeyHistoryEvent aForeignKeyEventToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aForeignKeyEventToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(ForeignKeyHistoryEvent aForeignKeyEventToSearch)
		{
			if (!Contains(aForeignKeyEventToSearch))
				return -1;
			
			return InnerList.IndexOf(aForeignKeyEventToSearch);
		}
		
		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is ForeignKeyHistoryEventCollection))
				return false;

			if (obj == this)
				return true;

			if (((ForeignKeyHistoryEventCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!ForeignKeyHistoryEvent.Equals(this[i], ((ForeignKeyHistoryEventCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
	}

	#endregion // ForeignKeyHistoryEventCollection Class
}
