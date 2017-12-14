using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;
using Microarea.TaskBuilderNet.Core.EasyBuilder;

namespace Microarea.EasyBuilder
{
	//====================================================================================
	internal static class MethodDeclarationExtension
	{
		//--------------------------------------------------------------------------------
		public static string GetSignature(this MethodDeclaration @this)
		{
			return "UserMethods";
			//return String.Format(
			//	"{0} {1}({2})",
			//	@this.ReturnType.AstTypeToString(),
			//	@this.Name,
			//	BuildParametersString(@this)
			//	);
		}


		//--------------------------------------------------------------------------------
		public static bool IsEqualTo(this MethodDeclaration @this, MethodDeclaration toCompare)
		{
			bool areEquals = String.CompareOrdinal(@this.Name, toCompare.Name) == 0;
			if (areEquals && @this.Parameters.Count == toCompare.Parameters.Count)
			{
				List<ParameterDeclaration> sourceParameters = @this.Parameters.ToList();
				List<ParameterDeclaration> targetParamters = toCompare.Parameters.ToList();

				for (int i = 0; i < sourceParameters.Count; i++)
				{
					areEquals &=
					String.CompareOrdinal(sourceParameters[i].Type.AstTypeToString(), targetParamters[i].Type.AstTypeToString()) == 0;
					if (!areEquals)
						break;
				}
			}
			return areEquals;
		}

		//--------------------------------------------------------------------------------
		public static bool Contains(this SyntaxTree @this, NamespaceDeclaration namespaceDeclaration)
		{
			NamespaceDeclaration temp = null;
			foreach (var child in @this.Members)
			{
				temp = child as NamespaceDeclaration;
				if (temp != null && String.Compare(temp.Name, namespaceDeclaration.Name, StringComparison.InvariantCulture) == 0)
					return true;
			}

			return false;
		}

		//--------------------------------------------------------------------------------
		public static bool ContainsNamespaceDeclaration(
			this SyntaxTree @this,
			NamespaceDeclaration updatingNs,
			out NamespaceDeclaration targetNs
			)
		{
			targetNs = null;
			NamespaceDeclaration tempNs = null;
			foreach (var item in @this.Members)
			{
				tempNs = item as NamespaceDeclaration;
				if (tempNs == null)
					continue;

				if (String.Compare(tempNs.Name, updatingNs.Name, StringComparison.InvariantCulture) == 0)
				{
					targetNs = tempNs;
					return true;
				}
			}

			return false;
		}

	}

	//====================================================================================
	internal static class FieldDeclarationExtension
	{
		//--------------------------------------------------------------------------------
		public static bool Contains(this FieldDeclaration field, string fieldName)
		{
			foreach (var usingChild in field.Variables)
			{
				if (usingChild.Name == fieldName)
				{
					return true;
				}
			}

			return false;
		}
	}
}
