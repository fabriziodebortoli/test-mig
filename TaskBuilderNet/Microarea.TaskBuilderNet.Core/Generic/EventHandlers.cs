using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	//======================================================================================
	public class EventHandlers
	{
		/// <summary>
		/// 
		/// </summary>
		//----------------------------------------------------------------------------------
		public static void RemoveEventHandlers(ref EventHandler eventToDelete)
		{
			if (eventToDelete != null)
			{
				Delegate[] invList = eventToDelete.GetInvocationList();

				foreach (EventHandler handler in invList)
				{
					eventToDelete -= handler;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="eventToDelete"></param>
		//----------------------------------------------------------------------------------
		public static void RemoveEventHandlers<T>(ref EventHandler<T> eventToDelete) where T : EventArgs
		{
			if (eventToDelete != null)
			{
				Delegate[] invList = eventToDelete.GetInvocationList();

				foreach (EventHandler<T> handler in invList)
				{
					eventToDelete -= handler;
				}
			}
		}


		static Dictionary<Type, List<FieldInfo>> dicEventFieldInfos = new Dictionary<Type, List<FieldInfo>>();

		static BindingFlags AllBindings
		{
			get { return BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static; }
		}

		//--------------------------------------------------------------------------------
		static List<FieldInfo> GetTypeEventFields(Type t)
		{
			if (dicEventFieldInfos.ContainsKey(t))
				return dicEventFieldInfos[t];

			List<FieldInfo> lst = new List<FieldInfo>();
			BuildEventFields(t, lst);
			dicEventFieldInfos.Add(t, lst);
			return lst;
		}

		//--------------------------------------------------------------------------------
		static void BuildEventFields(Type t, List<FieldInfo> lst)
		{
			// Type.GetEvent(s) gets all Events for the type AND it's ancestors
			// Type.GetField(s) gets only Fields for the exact type.
			//  (BindingFlags.FlattenHierarchy only works on PROTECTED & PUBLIC
			//   doesn't work because Fieds are PRIVATE)

			// NEW version of this routine uses .GetEvents and then uses .DeclaringType
			// to get the correct ancestor type so that we can get the FieldInfo.
			foreach (System.Reflection.EventInfo ei in t.GetEvents(AllBindings))
			{
				Type dt = ei.DeclaringType;
				FieldInfo fi = dt.GetField(ei.Name, AllBindings);
				if (fi != null)
					lst.Add(fi);
			}

			// OLD version of the code - called itself recursively to get all fields
			// for 't' and ancestors and then tested each one to see if it's an EVENT
			// Much less efficient than the new code
			/*
				  foreach (FieldInfo fi in t.GetFields(AllBindings))
				  {
					EventInfo ei = t.GetEvent(fi.Name, AllBindings);
					if (ei != null)
					{
					  lst.Add(fi);
					  Console.WriteLine(ei.Name);
					}
				  }
				  if (t.BaseType != null)
					BuildEventFields(t.BaseType, lst);*/
		}

		//--------------------------------------------------------------------------------
		static EventHandlerList GetStaticEventHandlerList(Type t, object obj)
		{
			MethodInfo mi = t.GetMethod("get_Events", AllBindings);
			return (EventHandlerList)mi.Invoke(obj, new object[] { });
		}

		//--------------------------------------------------------------------------------
		public static void RemoveAllEventHandlers(object obj)
		{
			PropertyInfo pi = obj.GetType().GetProperty("Events", BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			if (pi != null)
			{
				System.ComponentModel.EventHandlerList ehl = pi.GetGetMethod(true).Invoke(obj, new object[] { }) as EventHandlerList;
				ehl.Dispose();
			}
			RemoveEventHandler(obj, "");
		}

		//--------------------------------------------------------------------------------
		public static void RemoveEventHandler(object obj, string EventName)
		{
			if (obj == null)
				return;

			Type t = obj.GetType();
			List<FieldInfo> event_fields = GetTypeEventFields(t);
			EventHandlerList static_event_handlers = null;

			foreach (FieldInfo fi in event_fields)
			{
				if (EventName != "" && string.Compare(EventName, fi.Name, true) != 0)
					continue;

				// After hours and hours of research and trial and error, it turns out that
				// STATIC Events have to be treated differently from INSTANCE Events...
				if (fi.IsStatic)
				{
					// STATIC EVENT
					if (static_event_handlers == null)
						static_event_handlers = GetStaticEventHandlerList(t, obj);

					object idx = fi.GetValue(obj);
					Delegate eh = static_event_handlers[idx];
					if (eh == null)
						continue;

					Delegate[] dels = eh.GetInvocationList();
					if (dels == null)
						continue;

					System.Reflection.EventInfo ei = t.GetEvent(fi.Name, AllBindings);
					foreach (Delegate del in dels)
						ei.RemoveEventHandler(obj, del);
				}
				else
				{
					// INSTANCE EVENT
					System.Reflection.EventInfo ei = t.GetEvent(fi.Name, AllBindings);
					if (ei != null)
					{
						object val = fi.GetValue(obj);
						Delegate mdel = (val as Delegate);
						if (mdel != null)
						{
							foreach (Delegate del in mdel.GetInvocationList())
								ei.RemoveEventHandler(obj, del);
						}
					}
				}
			}
		}
	}

	//======================================================================================
	public class SafeCalls
	{
		public static void SafeDelete(IDisposable disposeObject)
		{
			if (disposeObject != null)
			{
				disposeObject.Dispose();
				disposeObject = null;
			}
		}
	}
}
