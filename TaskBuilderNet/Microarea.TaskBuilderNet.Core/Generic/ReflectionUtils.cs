using System;
//using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.Model;



namespace Microarea.TaskBuilderNet.Core.Generic
{
	//=========================================================================
	public sealed class ReflectionUtils
	{
		//-----------------------------------------------------------------------------
		private ReflectionUtils()
		{ }

		/// <summary>
		/// Ritorna il nome del metodo desiderato (evitando la necessità di utilizzare una 
		/// string cablata: in questo modo eventuali cambiamenti alla struttura del metodo
		/// a livello di codice produrranno un errore di compilazione invece di un errore a runtime.
		/// Uso: ReflectionUtils.GetMethodName( (TextBox p) => p.Invalidate());  
		/// ritorna "Invalidate"
		//-----------------------------------------------------------------------------
		public static string GetMethodName<T>(Expression<Action<T>> expression)
		{
			if (expression == null)
				return string.Empty;

			try
			{
				MethodCallExpression callExpression = ((MethodCallExpression)expression.Body);
				if (callExpression == null)
					return string.Empty;

				return callExpression.Method.Name;
			}
			catch
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// /// Ritorna il nome del metodo desiderato (evitando la necessità di utilizzare una 
		/// string cablata: in questo modo eventuali cambiamenti alla struttura del metodo
		/// a livello di codice produrranno un errore di compilazione invece di un errore a runtime.
		/// Uso: ReflectionUtils.GetMethodName(TextBox p) => p.SetFocus());  will return "SetFocus"
		/// ritorna "SetFocus"
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="U"></typeparam>
		/// <param name="expression"></param>
		/// <returns></returns>
		//-----------------------------------------------------------------------------
		public static string GetMethodName<T, U>(Expression<Func<T, U>> expression)
		{
			if (expression == null)
				return string.Empty;

			try
			{
				MethodCallExpression callExpression = ((MethodCallExpression)expression.Body);
				if (callExpression == null)
					return string.Empty;

				return callExpression.Method.Name;
			}
			catch
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Ritorna il nome della property desiderata (evitando la necessità di utilizzare una 
		/// string cablata: in questo modo eventuali cambiamenti alla proprietà a livello di codice
		/// produrranno un errore di compilazione invece di un errore a runtime.
		/// Uso: ReflectionUtils.GetPropertyName(() => new MParsedControl().Name); ritorna 
		/// "Name"
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="expression"></param>
		/// <returns></returns>
		//-----------------------------------------------------------------------------
		public static string GetPropertyName<T>(Expression<Func<T>> expression)
		{
			if (expression == null)
				return string.Empty;

			try
			{
				MemberExpression body = (MemberExpression)expression.Body;
				return body.Member.Name;
			}
			catch
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Ritorna la catena di object parent dell' oggetto di cui voliamo registare l'evento
		/// (Es. Document.DBTCompany.MA_Company.CompanyId.PropertyChanged)
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static string GetEventSource(IComponent component, string eventName)
		{
			string path = GetComponentFullPath(component);

			bool eventNameNullOrEmpty = String.IsNullOrEmpty(eventName);
			bool pathNullOrEmpty = String.IsNullOrEmpty(path);

			if (!pathNullOrEmpty && !eventNameNullOrEmpty)
				return String.Concat(path, ".", eventName); 

			if (pathNullOrEmpty && eventNameNullOrEmpty)
				return String.Empty;
			
			if (eventNameNullOrEmpty)
				return path;

			return eventName;
		}

		/// <summary>
		/// Ritorna il componente a partire dal suo percorso
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static IComponent GetComponentFromPath(IEasyBuilderContainer container, string componentPath)
		{
			string[] tokens = componentPath.Split('.');
			
			return GetComponentFromPath(container, tokens, 0);
		}
		//---------------------------------------------------------------------
		private static IComponent GetComponentFromPath(IEasyBuilderContainer container, string[] componentPath, int idx)
		{
			string token = componentPath[idx];
			IComponent cmp = null;
			foreach (IComponent child in container.Components)
			{
				if (child.Site != null && child.Site.Name == token)
				{
					cmp = child;
					break;
				}
			}
			if (cmp == null)
				return null;

			if (idx >= componentPath.Length - 1)
				return cmp;
			
			IEasyBuilderContainer cnt = cmp as IEasyBuilderContainer;
			if (cnt == null)
				return null;
			return GetComponentFromPath(cnt, componentPath, ++idx);

		}

		/// <summary>
		/// Ritorna la catena di object parent dell' oggetto component
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static string GetComponentFullPath(IComponent component)
		{
			if (component == null || component.Site == null)
				return String.Empty;

			Stack<string> tokens = new Stack<string>();

			//risale la catena component container fino a trovare il container nullo
			IContainer aContainer = component.Site.Container;
			while (component != null)
			{
				tokens.Push(component.Site.Name);
				component = aContainer as IComponent;

				if (component != null && component.Site != null)
					aContainer = component.Site.Container;
				else
					aContainer = null;
			}

			StringBuilder sb = new StringBuilder();
			string aToken = null;

			//buttiamo via DocumentController. È brutto, ma per il momento va bene così.
			if (string.Compare(tokens.Peek(), "DocumentController", true) == 0)
				tokens.Pop();

			//ricompongo il path a partire dai token
			while (tokens.Count > 0)
			{
				aToken = tokens.Pop();
				if (aToken.Trim().Length > 0)
				{
					sb.Append(aToken);
					if (tokens.Count > 0 && !string.IsNullOrEmpty(tokens.Peek()))
						sb.Append(".");
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Ritorna l'oggetto radice dell'object model a cui appartiene
		/// il component passato come parametro
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static IModelRoot GetModelRoot(EasyBuilderComponent component)
		{
			if (component == null)
				return null;

			if (component.ParentComponent == null)
				return component as IModelRoot;

			EasyBuilderComponent workingComponent = component;
			IModelRoot modelRoot = null;
			while (workingComponent != null)
			{
				workingComponent = workingComponent.ParentComponent;
				modelRoot = workingComponent as IModelRoot;

				if (modelRoot != null)
					return modelRoot;

				if (workingComponent == null)
					return modelRoot;
			}

			return modelRoot;
		}

		/// <summary>
		/// Ritorna il IComponent top parent del corrente
		/// </summary>
		/// <remarks>
		/// Il IComponent top parent è il IComponent che non ha IContainer
		/// </remarks>
		/// <param name="component"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static IComponent GetTopComponent(IComponent component)
		{
			if (component == null)
				return null;

			if (component.Site == null || component.Site.Container == null)
				return component;

			IComponent workingComponent = component;
			IContainer aContainer = workingComponent.Site.Container;
			while (workingComponent != null)
			{
				workingComponent = aContainer as IComponent;

				if (workingComponent == null)
					throw new InvalidOperationException("IContainer not found");

				if (workingComponent.Site != null && workingComponent.Site.Container != null)
					aContainer = workingComponent.Site.Container;
				else
					break;
			}

			return workingComponent;
		}

		/// <summary>
		/// Ritorna il metodo del tipo <code>type</code> con il nome specificato
		/// da <code>name</code>, tipo di ritorno specificato da <code>returnType</code>
		/// e i parametri in ingresso specificati da <code>args</code>.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="args"></param>
		/// <param name="returnType"></param>
		/// <param name="publicOnly"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		public static MethodInfo FindMethod(
			Type type,
			string name,
			Type[] args,
			Type returnType,
			bool publicOnly
			)
		{
			MethodInfo method = null;
			if (publicOnly)
				method = type.GetMethod(name, args);
			else
				method = type.GetMethod(name, BindingFlags.Public, null, args, null);
			if ((method != null) && (method.ReturnType != returnType))
				method = null;

			return method;
		}

		/// <summary>
		/// Restituisce il nome del metodo in base a nome tabella e nome evento
		/// (ad es. TaxIdNumber_PropertyChanged)
		/// </summary>
		//--------------------------------------------------------------------------------
		public static string GenerateName(IComponent component, EventDescriptor e)
		{
            IEventNameGenerator eventNameGenerator = e as IEventNameGenerator;
            if (eventNameGenerator != null)
                return eventNameGenerator.GenerateEventName(component);

            throw new NotSupportedException("Unable to generate event name");
        }

		/// <summary>
		/// Recupera il tipo dell'event args legato al delegate da cui viene generato un event
		/// (Es. se l'eventdescriptor riferiesce a OnPropertyChanged(object sender, MyEventArgs e) ritorna MyEventArgs
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		public static Type GetEventArgsType(EventDescriptor e)
		{
			MethodInfo method = e.EventType.GetMethod("Invoke");
			Type eventArgsType = null;
			foreach (ParameterInfo param in method.GetParameters())
			{
				if (
					param.ParameterType == typeof(EventArgs) ||
					param.ParameterType.IsSubclassOf(typeof(EventArgs))
					)
				{
					eventArgsType = param.ParameterType;
					break;
				}
			}
			return eventArgsType;
		}

		/// <summary>
		/// Recupera il tipo dell'event args legato al delegate da cui viene generato un event
		/// (Es. se l'eventdescriptor riferiesce a OnPropertyChanged(object sender, MyEventArgs e) ritorna MyEventArgs
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		public static Type[] GetParametersTypes(EventDescriptor e)
		{
			List<Type> parameters = new List<Type>();

			MethodInfo method = e.EventType.GetMethod("Invoke");
			foreach (ParameterInfo param in method.GetParameters())
				parameters.Add(param.ParameterType);

			return parameters.ToArray();
		}

		/// <summary>
		/// Ritorna tutte le proprietà pubbliche di istanza estratte da
		/// <code>workingType</code> dei tipi specificati dall'array
		/// <code>typesToBeSearched</code>
		/// </summary>
		//---------------------------------------------------------------------
		public static MemberInfo[] GetMemberInfos(
			Type workingType,
			params Type[] typesToBeSearched
			)
		{
			MemberInfo[] dataObjMemberInfos = workingType.FindMembers(
				MemberTypes.Property | MemberTypes.Field,  BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public,
				(MemberInfo m, object filterCriteria)
					=>
				{
					PropertyInfo aPropInfo = m as PropertyInfo;
					FieldInfo aFieldInfo = m as FieldInfo;
					Type type = null;
					if (aPropInfo != null)
						type = aPropInfo.PropertyType;
					else if (aFieldInfo != null)
						type = aFieldInfo.FieldType;

					if (type == null)
						return false;

					bool found = false;
					foreach (Type toBeSearched in typesToBeSearched)
					{
						if (toBeSearched.IsInterface)
						{ 
							if (type.GetInterface(toBeSearched.FullName) == toBeSearched)
							{
								found = true;
								break;
							}
						}
						else if (type == toBeSearched || type.IsSubclassOf(toBeSearched))
						{
							found = true;
							break;
						}
					}
					return found;
				},
				null
			);
			return dataObjMemberInfos;
		}

		/// <summary>
		/// Ritorna tutti i field pubblici di istanza estratti da
		/// <code>workingType</code>
		/// </summary>
		//---------------------------------------------------------------------
		public static FieldInfo[] GetFieldInfo(Type workingType)
		{
			return workingType.GetFields(
				BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public
				);
		}

        //-------------------------------------------------------------------------
        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda) where TSource : IRecord
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }
	}
}
