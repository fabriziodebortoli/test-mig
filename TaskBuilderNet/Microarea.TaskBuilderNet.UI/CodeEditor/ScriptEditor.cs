using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.SoapCall;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;
using WeifenLuo.WinFormsUI.Docking;

namespace Microarea.TaskBuilderNet.UI.CodeEditor
{
	enum ImageIconIndex
	{
		Class = 0,
		Method = 1,
		Property = 2,
		Field = 3,
		Enum = 4,
		Namespace = 5,
		Events = 6,
		Table = 7,
		Column = 8,
		Container = 9,
		Keyword = 10,
		EnumTag = 11,
		EnumItem = 12
	}
	public delegate bool CheckExpression(string expression, out string errorMessage);

	//================================================================================
	public partial class ScriptEditor : DockContent
	{
		List<IRecord> tableInfos;
		SymbolTable symbolTable;
		List<IFunctionPrototype> functions;
		TreeNode mainNode = new TreeNode("Root");

		Enums enums = null;
		public event CheckExpression OnCheck;
	
		//--------------------------------------------------------------------------------
		protected virtual bool CanShowExternalFunctions { get { return true; } }
		//--------------------------------------------------------------------------------
		public string Expression { get { return codeEditor.TextEditor.Document.Text; } }
		//--------------------------------------------------------------------------------
		public ScriptEditor(
			string initialExpression,
			List<string> keywords,
			List<IRecord> tableInfos,
			SymbolTable symbolTable,
			List<IFunctionPrototype> functions,
			Enums enums)
		{
			InitializeComponent();

			codeEditor.TextEditor.Document.Insert(codeEditor.TextEditor.TextArea.Caret.Offset, initialExpression);

			this.tableInfos = tableInfos;
			this.symbolTable = symbolTable;
			this.functions = functions;
			this.enums = enums;

			treeCodeElements.BeginUpdate();

			AddTableNodes(tableInfos);
			AddSymbolTableNodes(symbolTable);
			AddFunctionNodes(functions);
			AddKeywordNodes(keywords);
			AddEnumNodes();

			foreach (TreeNode currentNode in mainNode.Nodes)
			{
				if (currentNode.Nodes.Count <= 0)
					continue;

				treeCodeElements.Nodes.Add(currentNode);
			}

			treeCodeElements.TreeViewNodeSorter = new CodeNodeComparer();
			treeCodeElements.Sort();

			treeCodeElements.EndUpdate();
		}

		//--------------------------------------------------------------------------------
		private void AddEnumNodes()
		{
			ContainerNode root = new ContainerNode(null, CodeEditorStrings.Enums);
			mainNode.Nodes.Add(root);

			foreach (EnumTag tag in enums.Tags)
			{
				CodeNode tagNode = new EnumTagNode
					(
					root,
					tag,
					tag.LocalizedName, CreateEnumItemDescription(tag),
					(int)ImageIconIndex.EnumTag
					);
				root.Nodes.Add(tagNode);
				foreach (EnumItem item in tag.EnumItems)
				{
					string name = item.LocalizedName;
					string description = CreateEnumItemDescription(item);
					CodeNode itemNode = new EnumItemNode
						(
						root,
						item,
						name,
						description,
						(int)ImageIconIndex.EnumItem
						);
					tagNode.Nodes.Add(itemNode);

					//codeEditor.CompletionDataProvider.CompletionData.Add(itemNode);
				}
			}
			
		}

		//--------------------------------------------------------------------------------
		private static string CreateEnumItemDescription(EnumItem item)
		{
			return string.Format(CodeEditorStrings.EnumItemDescription, item.FullValue, item.LocalizedDescription);
		}

		//--------------------------------------------------------------------------------
		private static string CreateEnumItemDescription(EnumTag tag)
		{
			return string.Format(CodeEditorStrings.EnumItemDescription, tag.Value, tag.LocalizedDescription);
		}
		//--------------------------------------------------------------------------------
		private void AddKeywordNodes(List<string> keywords)
		{
			if (keywords.Count == 0)
				return;

			ContainerNode root = new ContainerNode(null, CodeEditorStrings.Keywords);
			mainNode.Nodes.Add(root);

			foreach (string keyword in keywords)
			{
				CodeNode keywordNode = new KeywordNode(root, keyword, (int)ImageIconIndex.Keyword);
				root.Nodes.Add(keywordNode);
				//codeEditor.CompletionDataProvider.CompletionData.Add(keywordNode);
			}
			
		}

		//--------------------------------------------------------------------------------
		private void AddFunctionNodes(List<IFunctionPrototype> functions)
		{
			if (functions.Count == 0)
				return;

			ContainerNode root = new ContainerNode(null, CodeEditorStrings.Functions);
			mainNode.Nodes.Add(root);

			foreach (FunctionPrototype function in functions)
			{
				CodeNode functionNode = null;
				if (function.NameSpace == null) //caso delle funzioni interne di woorm
				{
					CodeNode modNode = AddFunctionNode(root, "WOORM", typeof(CodeNode), null);
					functionNode = AddFunctionNode(modNode, function.Name, typeof(WoormFunctionCodeNode), function);
				}
				else if (CanShowExternalFunctions) //caso delle funzioni esterne
				{
                    INameSpace ns = function.NameSpace;

					CodeNode appNode = AddFunctionNode(root, ns.Application, typeof(CodeNode), null);
					CodeNode modNode = AddFunctionNode(appNode, ns.Module, typeof(CodeNode), null);
					CodeNode libNode = AddFunctionNode(modNode, ns.Library, typeof(CodeNode), null);
					
                    functionNode = AddFunctionNode(libNode, function.Name, typeof(FunctionCodeNode), function);
				}

				//codeEditor.CompletionDataProvider.CompletionData.Add(functionNode);
			}
			
		}

		//--------------------------------------------------------------------------------
		private string CreateFunctionDescription(IFunctionPrototype function)
		{
            return function.GetFunctionDescription();
		}

		//--------------------------------------------------------------------------------
		private CodeNode AddFunctionNode(CodeNode root, string name, Type type, IFunctionPrototype function)
		{
			CodeNode node = root.GetChildByName(name);
			if (node == null)
			{
				node = (CodeNode)Activator.CreateInstance
					(
					type,
					root,
					name,
					function == null ? "" : CreateFunctionDescription(function),
					function == null ? ImageIconIndex.Namespace : ImageIconIndex.Method);
				root.Nodes.Add(node);
			}
			return node;
		}

		//--------------------------------------------------------------------------------
		private void AddSymbolTableNodes(SymbolTable symbolTable)
		{
			if (symbolTable.Count == 0)
				return;

			ContainerNode root = new ContainerNode(null, CodeEditorStrings.AllVariables);
			mainNode.Nodes.Add(root);
			ContainerNode rootTableRule = new ContainerNode(null, CodeEditorStrings.TableRuleVariables);
			mainNode.Nodes.Add(rootTableRule);
			ContainerNode rootExpressionRule = new ContainerNode(null, CodeEditorStrings.ExpRuleVariables);
			mainNode.Nodes.Add(rootExpressionRule);
			ContainerNode rootFunction = new ContainerNode(null, CodeEditorStrings.FunctionVariables);
			mainNode.Nodes.Add(rootFunction);
			ContainerNode rootAsk = new ContainerNode(null, CodeEditorStrings.AskVariables);
			mainNode.Nodes.Add(rootAsk);
			ContainerNode rootGeneric = new ContainerNode(null, CodeEditorStrings.GenericVariables);
			mainNode.Nodes.Add(rootGeneric);

			AddSymbolTableVariables(symbolTable, root, rootTableRule, rootExpressionRule, rootFunction, rootAsk, rootGeneric);
		}

		//--------------------------------------------------------------------------------
		private void AddSymbolTableVariables(
			SymbolTable symbolTable,
			ContainerNode root,
			ContainerNode rootTableRule,
			ContainerNode rootExpressionRule,
			ContainerNode rootFunction,
			ContainerNode rootAsk,
			ContainerNode rootGeneric
			)
		{
			foreach (Variable v in symbolTable)
			{
				AddVariable(root, v, true);

				ExtendedVariable extVar = v as ExtendedVariable;
				if (extVar == null)
				{
					AddVariable(rootGeneric, v, false);
					continue;
				}

				if (extVar.IsTableRuleField)
					AddVariable(rootTableRule, v, false);
				if (extVar.IsFunctionField)
					AddVariable(rootFunction, v, false);
				if (extVar.IsExpressionRuleField)
					AddVariable(rootExpressionRule, v, false);
				if (extVar.IsAskField)
					AddVariable(rootAsk, v, false);
			}

			if (symbolTable.Parent != null)
				AddSymbolTableVariables(symbolTable.Parent, root, rootTableRule, rootExpressionRule, rootFunction, rootAsk, rootGeneric);

		}

		//--------------------------------------------------------------------------------
		private void AddVariable(CodeNode root, Variable v, bool addToCompletionProvider)
		{
			CodeNode variableNode = new VariableNode(root, v.Name, "", (int)ImageIconIndex.Field);
			root.Nodes.Add(variableNode);
			//if (addToCompletionProvider)
			//	codeEditor.CompletionDataProvider.CompletionData.Add(variableNode);
		}
		//--------------------------------------------------------------------------------
		private void AddTableNodes(List<IRecord> tableInfos)
		{
			if (tableInfos.Count == 0)
				return;

			ContainerNode root = new ContainerNode(null, CodeEditorStrings.Tables);
			mainNode.Nodes.Add(root);

			foreach (IRecord table in tableInfos)
			{
				CodeNode tableNode = new TableNode(root, table.Name, "", (int)ImageIconIndex.Table);
				root.Nodes.Add(tableNode);
				//codeEditor.CompletionDataProvider.CompletionData.Add(tableNode);

				foreach (IRecordField column in table.Fields)
				{
					CodeNode columnNode = new ColumnNode(tableNode, column.Name, "", (int)ImageIconIndex.Column);
					tableNode.Nodes.Add(columnNode);
					//codeEditor.CompletionDataProvider.CompletionData.Add(columnNode);
				}
			}

		}

		//--------------------------------------------------------------------------------
		private void treeCodeElements_DoubleClick(object sender, EventArgs e)
		{
			CodeNode codeNode = (CodeNode)treeCodeElements.SelectedNode;
			codeEditor.TextEditor.Document.Insert (codeEditor.TextEditor.TextArea.Caret.Offset, codeNode.Script);
			//codeNode.InsertData(codeEditor.ActiveTextAreaControl.TextArea);

			codeEditor.Focus();
		}

		//--------------------------------------------------------------------------------
		private void treeCodeElements_MouseMove(object sender, MouseEventArgs e)
		{
			TreeNode node = treeCodeElements.GetNodeAt(e.Location);
			if (node == null)
				return;

			CodeNode codeNode = (CodeNode)node;
			{
				txtHelper.Text = codeNode.Description;
			}
		}

		//--------------------------------------------------------------------------------
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (DialogResult == System.Windows.Forms.DialogResult.OK && OnCheck != null)
			{
				string message;
				if (!OnCheck(Expression, out message))
				{
					e.Cancel = true;
					txtHelper.Text = message;
				}
			}
			base.OnFormClosing(e);
		}

		//--------------------------------------------------------------------------------
		private void operatorPad_OperatorSelected(object sender, Woorm.OperatorSelectedEventArgs e)
		{
			codeEditor.TextEditor.Document.Insert(codeEditor.TextEditor.TextArea.Caret.Offset, e.Operator);
			codeEditor.Focus();
		}

		//--------------------------------------------------------------------------------
		private void btnOk_Click(object sender, EventArgs e)
		{
			CloseDialog(sender);
		}

		//--------------------------------------------------------------------------------
		private void btnCancel_Click(object sender, EventArgs e)
		{
			CloseDialog(sender);
		}

		//--------------------------------------------------------------------------------
		private void CloseDialog(object sender)
		{
			DialogResult = ((Button)sender).DialogResult;
			Close();
		}
	}

	//================================================================================
	public class ScriptEditorManager
	{
		//--------------------------------------------------------------------------------
		public static bool DoEditor(
			ref string expression,
			List<string> keywords,
			List<IRecord> tableInfos,
			SymbolTable symbolTable,
			List<IFunctionPrototype> functions,
			Enums enums,
			CheckExpression OnCheck
			)
		{
			ScriptEditor editor = new ScriptEditor(expression, keywords, tableInfos, symbolTable, functions, enums);
			editor.OnCheck += OnCheck;
			if (DialogResult.OK == editor.ShowDialog())
			{
				expression = editor.Expression;
				return true;
			}
			return false;
		}
	}

}
