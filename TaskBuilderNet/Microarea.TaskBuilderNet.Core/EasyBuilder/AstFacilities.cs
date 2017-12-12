using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory;
using Ast = ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.Semantics;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	/// <summary>
	/// TODO: Update summary.
	/// </summary>
	public static class AstFacilities
	{
		//--------------------------------------------------------------------------------
		public static BlockStatement ConvertToCompilationUnit(System.CodeDom.CodeStatementCollection collection)
		{
			string text = GetCode(collection);
			CSharpParser parser = new CSharpParser();
			SyntaxTree syntaxTree = parser.Parse(new StringReader(DecorateForParsing(text)));
			if (parser.Errors.Count() > 0)
			{
				Debug.Assert(false);
			}
			return GetStatementsFromParserContent(syntaxTree); //TODOLUCA
		}

		//--------------------------------------------------------------------------------
		private static string DecorateForParsing(string text)
		{
			return String.Concat("class Pippo{public void OnPippo(){", text, "}}");
		}

		//--------------------------------------------------------------------------------
		private static string GetCode(System.CodeDom.CodeStatementCollection collection)
		{
			MemoryStream ms = new MemoryStream(); //TODOLUCA //TODOROBY
			TextWriter writer = new StreamWriter(ms);

			System.CodeDom.Compiler.CodeDomProvider codeProvider = new Microsoft.CSharp.CSharpCodeProvider();

			System.CodeDom.Compiler.CodeGeneratorOptions options = new System.CodeDom.Compiler.CodeGeneratorOptions();
			options.IndentString = "	";
			foreach (System.CodeDom.CodeStatement statement in collection)
			{
				writer.Write(options.IndentString);
				codeProvider.GenerateCodeFromStatement(statement, writer, options);
			}

			writer.Flush();

			ms.Seek(0L, SeekOrigin.Begin);
			return new StreamReader(ms).ReadToEnd();
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Parse a block of text given the language
		/// </summary>
		public static BlockStatement ParseMethod(
			string methodBody
			)
		{
			CSharpParser parser = new CSharpParser(); 
			{
				SyntaxTree syntaxTree = parser.Parse(new StringReader(DecorateForParsing(methodBody)));

				if (parser.Errors.Count() > 0)
				{
					//Debug.Assert(false);
				}

				BlockStatement blockStatement = GetStatementsFromParserContent(syntaxTree);

				if (blockStatement != null)
					return blockStatement;

				throw new Exception("Unable to parse " + methodBody);
			}
		}

		//--------------------------------------------------------------------------------
		private static BlockStatement GetStatementsFromParserContent(SyntaxTree syntaxTree)
		{
			TypeDeclaration dummyType = null;
			MethodDeclaration dummyMethod = null;
			BlockStatement content = null;
			foreach (var item in syntaxTree.Members)
			{
				dummyType = item as TypeDeclaration;
				if (dummyType != null)
				{
					foreach (EntityDeclaration member in dummyType.Members)
					{
						dummyMethod = member as MethodDeclaration;
						if (dummyMethod != null)
						{
							content = dummyMethod.Body;
							break;
						}
					}
				}
			}
			return content;
		}

		
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Parse a block of text given the language
		/// </summary>
		public static SyntaxTree Parse(string text, string fileName)
		{
			CSharpParser parser = new CSharpParser();

			SyntaxTree syntaxTree = parser.Parse(new StringReader(text), fileName);
			if (parser.Errors.Count() > 0)
			{
				Debug.Assert(false);
			}
			return syntaxTree;
		}

		//--------------------------------------------------------------------------------
		public static string GetVisitedText(AstNode node)
		{
			using (TextWriter tw = new StringWriter())
			{
				IAstVisitor outputVisitor = new CSharpOutputVisitor(tw, AstFacilities.GetCSharpFormattingOptions());
				node.AcceptVisitor(outputVisitor);
				return tw.ToString();
			}
		}

		//--------------------------------------------------------------------------------
		public static string GetVisitedText(AstNodeCollection<Statement> statements)
		{
			using (TextWriter tw = new StringWriter())
			{
				IAstVisitor outputVisitor = new CSharpOutputVisitor(tw, AstFacilities.GetCSharpFormattingOptions());
				statements.AcceptVisitor(outputVisitor);
				return tw.ToString();
			}
		}

		//--------------------------------------------------------------------------------
		public static CSharpFormattingOptions GetCSharpFormattingOptions()
		{
			CSharpFormattingOptions s = FormattingOptionsFactory.CreateSharpDevelop();
			s.ClassBraceStyle = BraceStyle.NextLine;
			s.ConstructorBraceStyle = BraceStyle.NextLine;
			s.DestructorBraceStyle = BraceStyle.NextLine;
			s.MethodBraceStyle = BraceStyle.NextLine;
			s.NamespaceBraceStyle = BraceStyle.NextLine;
			s.PropertyBraceStyle = BraceStyle.NextLine;
			s.PropertyGetBraceStyle = BraceStyle.NextLine;
			s.PropertySetBraceStyle = BraceStyle.NextLine;
			s.StatementBraceStyle = BraceStyle.NextLine;
			s.IndentBlocks = true;
			s.IndentMethodBody = true;
			return s;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		public static void GenerateCodeFromNamespaceDeclaration(
			NamespaceDeclaration namespaceDeclaration,
			TextWriter textWriter
			)
		{
			IAstVisitor outputVisitor = new CSharpOutputVisitor(textWriter, GetCSharpFormattingOptions());
			namespaceDeclaration.AcceptVisitor(outputVisitor);
		}


		//--------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// <param name="typeName"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public static ObjectCreateExpression GetObjectCreationExpression(
			string typeName,
			params Expression[] arguments
			)
		{
			return new ObjectCreateExpression
						(
							new SimpleType(typeName),
							new List<Expression>(arguments)
						);
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// <param name="typeReference"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public static ObjectCreateExpression GetObjectCreationExpression(
			AstType type,
			params Expression[] arguments
			)
		{
			return new ObjectCreateExpression
						(
							type,
							new List<Expression>(arguments)
						);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="varName">The object containing the method</param>
		/// <param name="invokee">Method name</param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		public static Statement GetInvocationStatement(
			string varName,
			string invokee,
			params Expression[] arguments
			)
		{
			return new ExpressionStatement(new InvocationExpression(
						(
							new MemberReferenceExpression(new IdentifierExpression(varName), invokee)),
							new List<Expression>(arguments)
						));
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="varName">The object containing the method</param>
		/// <param name="invokee">Method name</param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		public static ExpressionStatement GetInvocationStatement(
			Expression target,
			string invokee,
			params Expression[] arguments
			)
		{
			Expression copy = target.Clone();
			var argumentsCopy = arguments.Select(item => (Expression)item.Clone()).ToList();
			return new ExpressionStatement(new InvocationExpression(
						(
							new MemberReferenceExpression(copy, invokee)),
							new List<Expression>(argumentsCopy)
						));
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// <param name="varName"></param>
		/// <param name="invokee"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public static InvocationExpression GetInvocationExpression(
			string varName,
			string invokee,
			params Expression[] arguments
			)
		{
			return new InvocationExpression(
						(
							new MemberReferenceExpression(new IdentifierExpression(varName), invokee)),
							new List<Expression>(arguments)
						);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="varName">The object containing the method</param>
		/// <param name="invokee">Method name</param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		public static InvocationExpression GetInvocationExpression(
			Expression target,
			string invokee,
			params Expression[] arguments
			)
		{
			return new InvocationExpression(
						(
							new MemberReferenceExpression(target, invokee)),
							new List<Expression>(arguments)
						);
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="names"></param>
		/// <returns></returns>
		public static FieldDeclaration GetFieldsDeclaration(string type, params string[] names)
		{
			FieldDeclaration fieldDeclaration = new FieldDeclaration();
			fieldDeclaration.ReturnType = new SimpleType(type);
			fieldDeclaration.Modifiers = Modifiers.Public;

			foreach (var name in names)
			{
				fieldDeclaration.Variables.Add(new VariableInitializer(name));
			}

			return fieldDeclaration;
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// <param name="attributeNames"></param>
		/// <returns></returns>
		public static AttributeSection GetAttributeSection(params string[] attributeNames)
		{
			AttributeSection attributeSection = new AttributeSection();
			foreach (var attributeName in attributeNames)
			{
				Ast.Attribute attr = new Ast.Attribute();
				attr.Type = new SimpleType(attributeName);
				attributeSection.Attributes.Add(attr);

			}
			return attributeSection;
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// <param name="attributes"></param>
		/// <returns></returns>
		public static AttributeSection GetAttributeSection(params Ast.Attribute[] attributes)
		{
			AttributeSection attributeSection = new AttributeSection();
			foreach (var attribute in attributes)
			{
				attributeSection.Attributes.Add(attribute);
			}
			return attributeSection;
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// <param name="attributes"></param>
		/// <returns></returns>
		public static Ast.Attribute GetAttribute(string name, params Expression[] positionalArguments)
		{
			List<Expression> positionalArgumentsList = new List<Expression>();
			if (positionalArguments != null)
				positionalArgumentsList.AddRange(positionalArguments);

			Ast.Attribute attribute = new Ast.Attribute();
			attribute.Type = new SimpleType(name);
			attribute.Arguments.AddRange(positionalArguments);
			return attribute;
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static Statement GetAssignmentStatement(Expression left, Expression right)
		{
			return new ExpressionStatement(
				new AssignmentExpression(
					left, AssignmentOperatorType.Assign, right
					)
				);
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public static ConstructorInitializer GetConstructorInitializer(params Expression[] arguments)
		{
			ConstructorInitializer costrIn = new ConstructorInitializer();
			costrIn.ConstructorInitializerType = ConstructorInitializerType.Base;

			foreach (var arg in arguments)
			{
				costrIn.Arguments.Add(arg);
			}

			return costrIn;
		}

		//-----------------------------------------------------------------------------
		public static string AstTypeToString(this AstType s)
		{


			SimpleType simple = s as SimpleType;
			if (simple != null)
			{
				return simple.Identifier;
			}

			PrimitiveType primitive = s as PrimitiveType;
			if (primitive != null)
			{
				return primitive.Keyword;
			}

			MemberType member = s as MemberType;
			if (member != null)
			{
				return member.MemberName;
			}

			//Debug.Assert(false);
			return string.Empty;

		}

		//-----------------------------------------------------------------------------
		public static bool CompareType(this AstType s, string type)
		{
			SimpleType simple = s as SimpleType;
			if (simple != null)
			{
				return string.Compare(simple.Identifier, type, StringComparison.InvariantCulture) == 0;
			}

			PrimitiveType primitive = s as PrimitiveType;
			if (primitive != null)
			{
				return string.Compare(primitive.Keyword, type, StringComparison.InvariantCulture) == 0;
			}

			MemberType member = s as MemberType;
			if (member != null)
			{
				return string.Compare(member.MemberName, type, StringComparison.InvariantCulture) == 0;
			}

			Debug.Assert(false);
			return false;
		}
	}
}
