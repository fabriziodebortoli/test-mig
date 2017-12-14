using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using Microarea.TaskBuilderNet.Core.NameSolver;
using System.Collections;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.Applications;
using System.Reflection;
using Microarea.TaskBuilderNet.Core.EasyBuilder;

using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.CSharp;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	//=========================================================================
	/// <remarks />
	public class EnumsDeclarator : BaseDeclarator
	{
		public const string EnumsClassName = "Enums";

		//---------------------------------------------------------------------
		/// <remarks />
		public void GenerateEnumsCodeDom(NamespaceDeclaration codeNamespace)
		{
			Enums enums = new Enums();
			enums.LoadXml(false);

			if (enums.Tags == null || enums.Tags.Count == 0)
				return;

			codeNamespace.Members.Add(new UsingDeclaration("System"));

			TypeDeclaration enumsClass = EasyBuilderSerializer.FindClass(codeNamespace, EnumsClassName);
			if (enumsClass == null)
			{
				enumsClass = GenerateEnumCodeTypeDeclaration(EnumsClassName);
				codeNamespace.Members.Add(enumsClass);
			}

			string enumTypeName = null;
			foreach (EnumTag enumTag in enums.Tags)
			{
				enumTypeName = Purge(enumTag.Name);

				TypeDeclaration enumTagClass = EasyBuilderSerializer.FindClass(codeNamespace, enumTypeName);
				if (enumTagClass == null)
				{
					enumTagClass = GenerateEnumCodeTypeDeclaration(enumTypeName);
					enumsClass.Members.Add(enumTagClass);
				}
	
				foreach (EnumItem enumItem in enumTag.EnumItems)
				{
					PropertyDeclaration prop = new PropertyDeclaration();
					prop.Modifiers = Modifiers.Public | Modifiers.Static;
					prop.Name = Purge(enumItem.Name);
					prop.ReturnType = new SimpleType("Int32");
					prop.Getter = new Accessor();
					prop.Getter.Body = new BlockStatement();
					prop.Getter.Body.Add(new ReturnStatement(new PrimitiveExpression(enumItem.Stored)));

					if (!HasCodeTypeMemberIndex(enumTagClass, prop))
						enumTagClass.Members.Add(prop);
				}
			}
		}

		//---------------------------------------------------------------------
		private static bool HasCodeTypeMemberIndex(TypeDeclaration td, PropertyDeclaration pd)
		{
			AstNode aMemberNode = null;
			foreach (EntityDeclaration codeTypeMember in td.Members)
			{
				if (aMemberNode != null && String.CompareOrdinal(codeTypeMember.Name, pd.Name) == 0)
					return true;
			}

			return false;
		}

		//---------------------------------------------------------------------
		private static TypeDeclaration GenerateEnumCodeTypeDeclaration(string typeName)
		{
			TypeDeclaration ctde = new TypeDeclaration();
			ctde.Modifiers =  Modifiers.Sealed | Modifiers.Public;
			ctde.ReturnType = new SimpleType(typeName);
			ctde.Name = typeName;

			//Aggiungo il costruttore privato perchè non voglio vengano create istanze.
			ConstructorDeclaration privateCtor = new ConstructorDeclaration();
			privateCtor.Modifiers = Modifiers.Private;
			privateCtor.Name = typeName;
			privateCtor.Body = new BlockStatement();
			ctde.Members.Add(privateCtor);
			return ctde;
		}

		//---------------------------------------------------------------------
		public static string Purge(string varName)
		{
			if (varName == null)
				return "Null";

			if (varName.Trim().Length == 0)
				return "Blank";

			StringBuilder builder = new StringBuilder();
			bool first = true;
			foreach (var c in varName)
			{
				switch (c)
				{
                    case '=':
                        builder.Append("Equals");
                        if (first)
                            first = false;
                        break;
                    case '>':
						builder.Append("GreaterThan");
						if (first)
							first = false;
						break;
					case '<':
						builder.Append("LowerThan");
						if (first)
							first = false;
						break;
					case '-':
						builder.Append("Minus");
						if (first)
							first = false;
						break;
					case '+':
						builder.Append("Plus");
						if (first)
							first = false;
						break;
					case '/':
						builder.Append("Slash");
						if (first)
							first = false;
						break;
					case '\\':
						builder.Append("BackSlash");
						if (first)
							first = false;
						break;
                    case ',':
                        builder.Append("Comma");
                        if (first)
                            first = false;
                        break;
                    case ';':
                        builder.Append("Semicolon");
                        if (first)
                            first = false;
                        break;
                    case '.':
                        builder.Append("Dot");
                        if (first)
                            first = false;
                        break;
                    case ' ':
                        builder.Append("_");
                        if (first)
                            first = false;
                        break;
                    default:
						if (Char.IsLetter(c) || Char.IsDigit(c) && !first)
						{
							builder.Append(c);
							if (first)
								first = false;
							break;
						}
						if (first && Char.IsDigit(c))
						{
							switch (c)
							{
								case '0':
									builder.Append("Zero");
									break;
								case '1':
									builder.Append("One");
									break;
								case '2':
									builder.Append("Two");
									break;
								case '3':
									builder.Append("Three");
									break;
								case '4':
									builder.Append("Four");
									break;
								case '5':
									builder.Append("Five");
									break;
								case '6':
									builder.Append("Six");
									break;
								case '7':
									builder.Append("Seven");
									break;
								case '8':
									builder.Append("Eight");
									break;
								case '9':
									builder.Append("Nine");
									break;
								default:
									break;
							}
						}
						break;
				}
			}
			return builder.ToString();
		}
	}

	//=========================================================================
	/// <summary>
	/// Exception generating Microarea.EasyBuilder.Enums.dll
	/// </summary>
	public class EnumsGenerationException : Exception
	{
		private CompilerResults compilerResults;

		/// <summary>
		/// Results of compilation process
		/// </summary>
		public CompilerResults CompilerResults { get { return compilerResults; } }

		//---------------------------------------------------------------------
		/// <summary>
		/// Creates a new instance of EnumsGenerationException
		/// </summary>
		public EnumsGenerationException(string message, CompilerResults results)
			: base(message)
		{
			compilerResults = results;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Creates a new instance of EnumsGenerationException
		/// </summary>
		public EnumsGenerationException(string message, Exception exc)
			: base(message, exc)
		{ }
	}
}
