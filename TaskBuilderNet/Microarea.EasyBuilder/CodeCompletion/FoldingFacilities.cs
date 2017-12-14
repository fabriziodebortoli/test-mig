using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.TypeSystem;

namespace Microarea.EasyBuilder.CodeCompletion
{
	internal class FoldingFacilities
	{
		//-------------------------------------------------------------------------------
		public static IEnumerable<NewFolding> GetFoldings(IDocument document, IUnresolvedFile file, out int firstErrorOffset)
		{
			firstErrorOffset = -1;
			List<NewFolding> newFoldMarkers = new List<NewFolding>();
			foreach (var c in file.TopLevelTypeDefinitions)
			{
				AddClassMembers(document, c, newFoldMarkers);
			}
			return newFoldMarkers;

		}

		//-------------------------------------------------------------------------------
		public static IEnumerable<NewFolding> CreateNewFoldings(SyntaxTree syntaxTree, IDocument document)
		{
			FoldingVisitor v = new FoldingVisitor();
			v.document = document;
			syntaxTree.AcceptVisitor(v);
			return v.foldings;
		}

		//-------------------------------------------------------------------------------
		static void AddClassMembers(IDocument document, IUnresolvedTypeDefinition c, List<NewFolding> newFoldMarkers)
		{
			if (c.Kind == TypeKind.Delegate)
			{
				return;
			}
			DomRegion cRegion = c.BodyRegion;
			if (c.BodyRegion.BeginLine < c.BodyRegion.EndLine)
			{
				newFoldMarkers.Add(new NewFolding(GetStartOffset(document, c.BodyRegion), GetEndOffset(document, c.BodyRegion)));
			}
			foreach (var innerClass in c.NestedTypes)
			{
				AddClassMembers(document, innerClass, newFoldMarkers);
			}

			foreach (var m in c.Members)
			{
				if (m.BodyRegion.BeginLine < m.BodyRegion.EndLine)
				{
					newFoldMarkers.Add(new NewFolding(GetStartOffset(document, m.BodyRegion), GetEndOffset(document, m.BodyRegion))
					{ IsDefinition = true });
				}
			}
		}

		//-------------------------------------------------------------------------------
		static int GetStartOffset(IDocument document, DomRegion bodyRegion)
		{
			if (bodyRegion.BeginLine < 1)
				return 0;
			if (bodyRegion.BeginLine > document.LineCount)
				return document.TextLength;
			var line = document.GetLineByNumber(bodyRegion.BeginLine);
			int lineStart = line.Offset;
			int bodyStartOffset = lineStart + bodyRegion.BeginColumn - 1;
			for (int i = lineStart; i < bodyStartOffset; i++)
			{
				if (!char.IsWhiteSpace(document.GetCharAt(i)))
				{
					// Non-whitespace in front of body start:
					// Use the body start as start offset
					return bodyStartOffset;
				}
			}
			// Only whitespace in front of body start:
			// Use the end of the previous line as start offset
			return line.PreviousLine != null ? line.PreviousLine.EndOffset : bodyStartOffset;
		}

		//-------------------------------------------------------------------------------
		static int GetEndOffset(IDocument document, DomRegion region)
		{
			if (region.EndLine < 1)
				return 0;
			if (region.EndLine > document.LineCount)
				return document.TextLength;
			return document.GetOffset(region.End);
		}
	}
}