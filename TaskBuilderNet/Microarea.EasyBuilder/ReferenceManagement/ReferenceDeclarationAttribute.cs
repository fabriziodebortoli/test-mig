using System;
using System.Linq;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Ast = ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp;
using System.Collections.Generic;

namespace Microarea.EasyBuilder
{
	/// <summary>
	/// declaration attribute
	/// </summary>
	//=============================================================================
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public sealed class ReferenceDeclarationAttribute : ContentDescriptionAttribute
	{
		/// <summary>
		/// constructs a declaration attribute
		/// </summary>
		//--------------------------------------------------------------------------------
		public ReferenceDeclarationAttribute(string contentType, string nameSpace, string mainClass)
			:
			base(contentType, nameSpace, mainClass)
		{
		}

		//--------------------------------------------------------------------------------
		internal static ReferenceDeclarationAttribute Create(Ast.Attribute attribute)
		{
			// class type check
			if (
					attribute.Type.AstTypeToString() != typeof(ReferenceDeclarationAttribute).Name ||
					attribute.Arguments == null ||
					attribute.Arguments.Count == 0
				)
				return null;

			List<Expression> arguments = attribute.Arguments.ToList();

			// type
			PrimitiveExpression typeArg = arguments[0] as PrimitiveExpression;
			if (typeArg == null)
				return null;

			// namespace
			PrimitiveExpression nsArg = arguments[1] as PrimitiveExpression;
			if (nsArg == null)
				return null;

			// main class
			PrimitiveExpression classArg = arguments[2] as PrimitiveExpression;
			if (classArg == null)
				return null;

			return new ReferenceDeclarationAttribute(typeArg.Value as string, nsArg.Value as string, classArg.Value as string);
		}
	}
}
