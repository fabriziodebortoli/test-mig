using System;
using System.Threading;

namespace Microarea.Console.Core.TaskSchedulerObjects
{
	//============================================================================================
	public class TaskInScheduledSequence : IComparable
	{
		public const string ScheduledSequencesTableName	= "MSD_ScheduledSequences";
		
		public const string SequenceIdColumnName		= "SequenceId";
		public const string TaskIdColumnName			= "TaskId";
		public const string TaskIndexColumnName			= "TaskIndex";
		public const string BlockingModeColumnName		= "BlockingMode";
		public static string[] ScheduledSequenceTableColumns = new string[4]{
																			 SequenceIdColumnName,
																			 TaskIdColumnName,
																			 TaskIndexColumnName,
																			 BlockingModeColumnName
																		 };
		private Guid				sequenceId = Guid.Empty;
		private Guid				taskInSequenceId = Guid.Empty;
		private int					taskInSequenceIndex = 0;
		private bool				blockingMode = false;
		private bool				tBLoaderConnectionNecessaryForRun = false;
		private bool				closeOnEnd = false;
		private ManualResetEvent	executionEndedEvent = null;

		//--------------------------------------------------------------------------------------------------------------------------------
		public TaskInScheduledSequence(ScheduledTask aSequenceTask, Guid aTaskId, int aTaskInSequenceIndex, bool aBlockingMode, bool aTBLoaderConnectionNecessaryForRun, bool aCloseOnEndFlag)
		{
			if (aSequenceTask == null || !aSequenceTask.IsSequence)
			{
				throw new ScheduledTaskException(TaskSchedulerObjectsStrings.InvalidSequenceOperationRequestMsg);
			}

			if (aTaskId == Guid.Empty)
			{
				throw new ScheduledTaskException(TaskSchedulerObjectsStrings.UndefinedTaskIdMsg);
			}
			sequenceId = aSequenceTask.Id;
			taskInSequenceId = aTaskId;
			taskInSequenceIndex = aTaskInSequenceIndex;
			blockingMode = aBlockingMode;
			tBLoaderConnectionNecessaryForRun = aTBLoaderConnectionNecessaryForRun;
			closeOnEnd = aCloseOnEndFlag;
		}

		//------------------------------------------------------------------------------------------------
		public TaskInScheduledSequence(TaskInScheduledSequence aTaskInSequence)
		{
			sequenceId = aTaskInSequence.SequenceId;
			taskInSequenceId = aTaskInSequence.TaskInSequenceId;
			taskInSequenceIndex = aTaskInSequence.TaskInSequenceIndex;
			blockingMode = aTaskInSequence.BlockingMode;
			tBLoaderConnectionNecessaryForRun = aTaskInSequence.TBLoaderConnectionNecessaryForRun;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void ResetExecutionEndedEvent()
		{
			if (executionEndedEvent == null)
				executionEndedEvent = new ManualResetEvent(false);//the initial state is set to nonsignaled
			else
				executionEndedEvent.Reset();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void WaitForExecutionEnd()
		{
			if (executionEndedEvent == null)
				return;
			// The caller blocks indefinitely until executionEndedEvent receives a signal
			executionEndedEvent.WaitOne();
		}
		
		//------------------------------------------------------------------------------------------------
		public Guid SequenceId { get { return sequenceId; } }
		//------------------------------------------------------------------------------------------------
		public Guid TaskInSequenceId { get { return taskInSequenceId; } }
		//------------------------------------------------------------------------------------------------
		public int TaskInSequenceIndex { get { return taskInSequenceIndex; } }
		//------------------------------------------------------------------------------------------------
		public bool BlockingMode { get { return blockingMode; } }
		//------------------------------------------------------------------------------------------------
		public bool TBLoaderConnectionNecessaryForRun { get { return tBLoaderConnectionNecessaryForRun; } }
		//------------------------------------------------------------------------------------------------
		public ManualResetEvent ExecutionEndedEvent { get { return executionEndedEvent; } }

		//------------------------------------------------------------------------------------------------
		// Implement IComparable CompareTo to provide default sort order.
		int IComparable.CompareTo(object obj)
		{
			if (obj == null || !(obj is TaskInScheduledSequence))
				throw new ArgumentNullException();

			return taskInSequenceIndex - ((TaskInScheduledSequence)obj).taskInSequenceIndex;
		}

	}

	//============================================================================
	public class TasksInScheduledSequenceCollection : System.Collections.CollectionBase
	{
		// Restricts to TaskInScheduledSequence types, items that can be added to the collection
		//-------------------------------------------------------------------------------------------
		public void Add(TaskInScheduledSequence aTaskInSequence)
		{
			List.Add(aTaskInSequence);
		}

		//-------------------------------------------------------------------------------------------
		public void Remove(int index)
		{
			// Check to see if there is a recipient at the supplied index.
			if (index > Count - 1 || index < 0)
			{
				throw new IndexOutOfRangeException("The supplied index is out of range");
			}
			else
			{
				List.RemoveAt(index); 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public TaskInScheduledSequence this[int index]
		{
			get 
			{
				// The appropriate item is retrieved from the List object and explicitly cast 
				// to the TaskInScheduledSequence type, then returned to the caller.
				return (TaskInScheduledSequence) List[index];
			}
			set
			{
				List.RemoveAt(index);
				List.Insert(index, value);
			}
		}

		//-------------------------------------------------------------------------------------------
		public void ReorderByTaskIndex()
		{
			this.InnerList.Sort();
		}
	}
}