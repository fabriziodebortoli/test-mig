using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;

namespace ICSharpCode.NRefactory.Demo
{
	internal class ReferenceVisitor : CSharpOutputVisitor
	{
		List<string> allReferences;

		public List<string> AllReferences
		{
			get
			{
				return allReferences;
			}

			set
			{
				allReferences = value;
			}
		}

		public ReferenceVisitor(TextWriter tw, CSharpFormattingOptions fo) : base(tw, fo)
		{
			AllReferences = new List<string>();
		}

		public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
		{
			
			base.VisitMemberReferenceExpression(memberReferenceExpression);
			string m = memberReferenceExpression.MemberName;

			InvocationExpression e = memberReferenceExpression.Parent as InvocationExpression;
			if (e != null)
				return;

			if (!AllReferences.Contains(m))
				AllReferences.Add(m);
		}

		public override void VisitIdentifierExpression(IdentifierExpression identifierExpression)
		{
			//base.VisitIdentifierExpression(identifierExpression);
			//string m = identifierExpression.Identifier;

			//if (!AllReferences.Contains(m))
			//	AllReferences.Add(m);
		}

		public override void VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement)
		{
			base.VisitVariableDeclarationStatement(variableDeclarationStatement);

			if (variableDeclarationStatement.Type is PrimitiveType)
				return;

			string m = variableDeclarationStatement.Type.ToString();

			if (!AllReferences.Contains(m))
				AllReferences.Add(m);
		}

		public override void VisitVariableInitializer(VariableInitializer variableInitializer)
		{
			base.VisitVariableInitializer(variableInitializer);
		}

		public override void VisitTryCatchStatement(TryCatchStatement tryCatchStatement)
		{
			base.VisitTryCatchStatement(tryCatchStatement);
		}

		public override void VisitCatchClause(CatchClause catchClause)
		{
			
			base.VisitCatchClause(catchClause);
		}

		public override void VisitIdentifier(Identifier identifier)
		{
			base.VisitIdentifier(identifier);
		}

	}
}
