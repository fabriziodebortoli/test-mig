using System;
using System.ComponentModel;
using System.Reflection;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyBuilder
{
	/// <summary>
	/// It represents a component declaration request
	/// </summary>
	//================================================================================
	public class ComponentDeclarationRequest
	{
		/// <summary>
		/// Gets request type
		/// </summary>
		public enum Action
		{
			/// <summary>
			/// Add
			/// </summary>
			Add,
			/// <summary>
			/// Remove
			/// </summary>
			Remove,
			/// <summary>
			/// Update
			/// </summary>
			Update,
			/// <summary>
			/// Update and recalculates references
			/// </summary>
			UpdateWithReferences
		};

		private Action action;
		private Type type;
		private NameSpace nameSpace;
		private ReferenceableComponent referenceableComponent;
		private IComponent componentInstanceToDeclare;

		/// <summary>
		/// Gets type to add
		/// </summary>
		//--------------------------------------------------------------------------------
		public Type Type { get { return type; } }

		/// <summary>
		/// Gets request type
		/// </summary>
		//--------------------------------------------------------------------------------
		public Action RequestAction { get { return action; } }

		/// <summary>
		/// Gets namespace
		/// </summary>
		//--------------------------------------------------------------------------------
		public NameSpace Namespace { get { return nameSpace; } }

		/// <summary>
		/// Gets the referenced component to use
		/// </summary>
		//--------------------------------------------------------------------------------
		internal ReferenceableComponent ReferenceableComponent
		{
			get { return referenceableComponent; }
		}

		//--------------------------------------------------------------------------------
		internal IComponent ComponentInstanceToDeclare
		{
			get { return componentInstanceToDeclare; }
			set { componentInstanceToDeclare = value; }
		}

		//--------------------------------------------------------------------------------
		internal ComponentDeclarationRequest
			(
			Action action,
			Type type,
			NameSpace nameSpace,
			IComponent componentInstanceToDeclare
			)
		{
			this.action = action;
			this.type = type;
			this.nameSpace = nameSpace;

			this.componentInstanceToDeclare = componentInstanceToDeclare;
		}
		/// <summary>
		/// Initializes a new instance of the ComponentDeclarationRequest
		/// </summary>
		//--------------------------------------------------------------------------------
		internal ComponentDeclarationRequest(Action action, Type type, NameSpace nameSpace)
		{
			this.action = action;
			this.type = type;
			this.nameSpace = nameSpace;
		}
		//--------------------------------------------------------------------------------
		internal ComponentDeclarationRequest(Action action, ReferenceableComponent refComponent)
		{
			this.referenceableComponent = refComponent;
			this.action = action;
			Assembly asm = typeof(MDocument).Assembly;
			string fullName = string.Format("{0}.{1}", typeof(MDocument).Namespace, refComponent.ContentType);
			this.type = asm.GetType(fullName);
			this.nameSpace = refComponent.Component;
		}
	}
}
