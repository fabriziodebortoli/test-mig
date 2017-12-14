using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.EasyBuilder.MVC;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Core.EasyBuilder.Refactor;
using ICSharpCode.NRefactory.CSharp;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.EasyBuilder
{
    internal class DeltaCreator
    {
		private List<string> classesToRemove = new List<string>();
        private List<string> classesNotToRemove = new List<string>();
        private bool enableDelta = false;
        private bool preserveEntireRecord = true;

		ControllerSources sources;

        //-----------------------------------------------------------------------------
        public bool EnableDelta
        {
            get
            {
                return enableDelta;
            }

            set
            {
                enableDelta = value && IsEnabledBySetting();
            }
        }

        //-----------------------------------------------------------------------------
        public bool PreserveEntireRecord
        {
            get
            {
                return preserveEntireRecord;
            }

            set
            {
                preserveEntireRecord = value;
            }
        }

        //-----------------------------------------------------------------------------
        internal DeltaCreator(ControllerSources sources)
        {
            this.sources = sources;
        }

        //-----------------------------------------------------------------------------
        internal void CreateDelta(DocumentController controller)
		{
            if (!enableDelta)
                return;

            // parto con le classi delle eventuali conversioni
            classesToRemove.Clear();
            foreach (string className in BaseCustomizationContext.ApplicationChanges.GetOldClassesNamesFor(controller.Document.Namespace, controller.Version))
            {
                classesToRemove.Add(className);
            }

            classesNotToRemove.Clear();

            PurgeUnusedFields(controller.View);
		    HasPurgedUnusedContainer(controller.View);

			PurgeUnusedFields(controller.Document);
            HasPurgedUnusedContainer(controller.Document);

            int idx = -1;
            foreach (string singleClass in classesToRemove)
            {
                idx = classesNotToRemove.IndexOf(singleClass);
                if (idx == -1)
                {
                    sources.RemoveClass(singleClass);
                }
            }
        }

        //-----------------------------------------------------------------------------
        internal void PurgeUnusedFields(IEasyBuilderContainer container)
        {
            if (container == null)
                return;

            TypeDeclaration decl = sources.FindClass(container.SerializedType);
            if (decl != null)
                PurgeUnusedClassFields(decl);
            // devo sistemare il keep alive delle tab
            foreach (IComponent component in container.Components)
            {
                IEasyBuilderContainer ebContainer = component as IEasyBuilderContainer;
                if (ebContainer == null)
                    continue;
                PurgeUnusedFields(ebContainer);
            }
        }

        //-----------------------------------------------------------------------------
        private void PurgeUnusedClassFields(TypeDeclaration classStructure)
        {
            MethodDeclaration createComponents =
                EasyBuilderSerializer.FindMember<MethodDeclaration>(
                classStructure,
                EasyBuilderSerializer.CreateComponentsMethodName
                );


			List<EntityDeclaration> members = classStructure.Members.ToList();

			for (int i = classStructure.Members.Count - 1; i >= 0; i--)
            {
                FieldDeclaration field = members[i] as FieldDeclaration;
				if (field == null)
					continue;

				if (EasyBuilderSerializer.HasPreserveFieldAttribute(field))
					continue;

                if (createComponents != null && IsFieldConstructedInMethod(createComponents, field))
                    continue;

				classStructure.Members.Remove(field); 

                sources.RemoveClass(field.ReturnType);
            }
        }
        //-----------------------------------------------------------------------------
        private bool IsFieldConstructedInMethod(MethodDeclaration method, FieldDeclaration field)
        {
            foreach (Statement currentStatement in method.Body.Statements)
            {
                ExpressionStatement ass = currentStatement as ExpressionStatement;
                if (ass == null)
                    continue;

                //tra gli statements della create controls cerco gli statements di assegnazione
                AssignmentExpression cas = ass.Expression as AssignmentExpression;
                if (cas == null)
                    continue;

                //Degli statements di assegnazione mi interessa solamente il right side
                ObjectCreateExpression coce = (cas.Right as ObjectCreateExpression);
                if (coce == null)
                    continue;

				//DbtNotes = new DBTNotes()
                IdentifierExpression exp = cas.Left as IdentifierExpression;
				if (exp != null)
				{
					if (field.Contains(exp.Identifier))
						return true;
				}

				//this.fld_prova = new MDataStr()
				MemberReferenceExpression membExp = cas.Left as MemberReferenceExpression;
				if (membExp == null)
					continue;

				if (field.Contains(membExp.MemberName))
					return true;
			}
			return false;
        }

        //-----------------------------------------------------------------------------
        private EasyBuilderComponent GetComponentFromField(IEasyBuilderContainer container, FieldDeclaration field)
        {
            List<VariableInitializer> declaration = field.Variables.ToList();
            if (declaration.Count == 0)
                return null;

            string fieldName = declaration[0].Name; 
            foreach (IComponent component in container.Components)
            {
                EasyBuilderComponent ebComponent = component as EasyBuilderComponent;
                if (ebComponent.SerializedName == fieldName)
                    return ebComponent;
            }
            return null;
        }

        //-----------------------------------------------------------------------------
        private bool IsInvokeMethodUsing(ExpressionStatement expressionStatement, string varName)
        {
            InvocationExpression invExpression = expressionStatement.Expression as InvocationExpression;
            if (invExpression == null)
                return false;

            foreach (Expression expression in invExpression.Arguments)
            {

				//l'arguments potrebbe essere 
				// this.AttachMaster(DBTProspectiveSuppliers); o 
				// this.AttachMaster(this.DBTProspectiveSuppliers);  
				IdentifierExpression idExp = expression as IdentifierExpression;
                if (idExp != null && idExp.Identifier == varName)
                    return true;

				MemberReferenceExpression membExp = expression as MemberReferenceExpression;
				if (membExp != null && membExp.MemberName == varName)
					return true;
			}

			return false;
        }

        //-----------------------------------------------------------------------------
        private bool IsConstructorMethodUsing(ExpressionStatement expressionStatement, string varType, string varName)
        {
            //tra gli statements della create controls cerco gli statements di assegnazione
            AssignmentExpression cas = expressionStatement.Expression as AssignmentExpression;
            if (cas == null)
                return false;

            // costruttore
            ObjectCreateExpression coce = (cas.Right as ObjectCreateExpression);
			IdentifierExpression idExp = (cas.Left as IdentifierExpression);

			// dubbio, il left side potrebbe essere non una IdentifierExpression ma una memberReferenceExpression
			// prendo il type di creazione e il name 
			return
				(coce != null && varType == coce.Type.AstTypeToString()) &&
				(idExp != null && idExp.Identifier == varName);
        }

        //-----------------------------------------------------------------------------
        private bool IsAssignmentUsing(ExpressionStatement expressionStatement, string varName)
        {
            AssignmentExpression cas = expressionStatement.Expression as AssignmentExpression;
            if (cas == null)
                return false;

            IdentifierExpression leftExpression = cas.Left as IdentifierExpression;
            IdentifierExpression rightExpression = cas.Right as IdentifierExpression;

            return (leftExpression != null && leftExpression.Identifier == varName) ||
                    (rightExpression != null && rightExpression.Identifier == varName);
        }

        //-----------------------------------------------------------------------------
        private List<ExpressionStatement> GetStatementListOf(BlockStatement blockStatement, string classCreated, string varName)
        {
            List<ExpressionStatement> list = new List<ExpressionStatement>();

            foreach (Statement currentStatement in blockStatement.Statements)
            {

                // prima cerco le espressioni di assegnazione
                ExpressionStatement expressionStatement = currentStatement as ExpressionStatement;
                if (expressionStatement == null)
                    continue;

                // methodo che invoca qualcosa sulla variabile
                if (IsInvokeMethodUsing(expressionStatement, varName))
                {
                    list.Add(expressionStatement);
                    continue;
                }

                // identifica il costruttore
                if (IsConstructorMethodUsing(expressionStatement, classCreated, varName))
                {
                    list.Add(expressionStatement);
                    continue;
                }

                // identifica l'uso della variabile in un assegnazione 
                if (IsAssignmentUsing(expressionStatement, varName))
                {
                    list.Add(expressionStatement);
                    continue;
                }
            }
            return list;
        }

        //-----------------------------------------------------------------------------
        private bool IsToDelete(EasyBuilderComponent component)
        {
            IChangedEventsSource temp = component as IChangedEventsSource;
			EasyBuilderComponent relatedComponent = temp.EventSourceComponent as EasyBuilderComponent;
			
			// tutto cio' che e' creato dall'utente non si tocca
			if (!component.HasCodeBehind)
                return false;

            // se devo preservare tutto il record salto direttamente la ricorsione
            if (PreserveEntireRecord && relatedComponent is IDataManager && !relatedComponent.IsToDelete())
                return false;

            // considera solo le container con certe caratteristiche
            IEasyBuilderContainer ebContainer = relatedComponent as IEasyBuilderContainer;
			bool isToDelete = ebContainer != null ? HasPurgedUnusedContainer(ebContainer) : true;
			return isToDelete && relatedComponent.IsToDelete(); 
		}

        //-----------------------------------------------------------------------------
        private void DeleteStatementsFrom(MethodDeclaration method, EasyBuilderComponent ebComponent)
        {
            // il metodo non e' stato trovato
            if (method == null)
                return;
            
            List<ExpressionStatement> statementToDelete = GetStatementListOf(method.Body, ebComponent.SerializedType, ebComponent.SerializedName);
            foreach (ExpressionStatement statement in statementToDelete)
                method.Body.Statements.Remove(statement);
        }

        //-----------------------------------------------------------------------------
        internal bool HasPurgedUnusedContainer(IEasyBuilderContainer container)
        {
            // cerca la classe con il suo metodo CreateComponents
            TypeDeclaration theClass = sources.FindClass(container.SerializedType);
            if (theClass == null)
                return true;

            MethodDeclaration createComponents =
                    EasyBuilderSerializer.FindMember<MethodDeclaration>(
                    theClass,
                    EasyBuilderSerializer.CreateComponentsMethodName
                    );

            MethodDeclaration clearComponents =
                            EasyBuilderSerializer.FindMember<MethodDeclaration>(
                            theClass,
                            EasyBuilderSerializer.ClearComponentsMethodName
                            );

            MethodDeclaration applyResources =
                                    EasyBuilderSerializer.FindMember<MethodDeclaration>(
                                    theClass,
                                    EasyBuilderSerializer.ApplyResourcesMethodName
                                    );

            List<FieldDeclaration> fieldToRemove = new List<FieldDeclaration>();
			List<EntityDeclaration> members = theClass.Members.ToList();
			// esplora i data members rimasti nella classe e decide se sono usati nella
			// createComponents
			for (int i = theClass.Members.Count - 1; i >= 0; i--)
            {
                FieldDeclaration field = members[i] as FieldDeclaration;
				if (field == null)
					continue;

				if (EasyBuilderSerializer.HasPreserveFieldAttribute(field))
					continue;
                
				// non c'e' la new
                if (!IsFieldConstructedInMethod(createComponents, field))
                    continue;

                EasyBuilderComponent ebComponent = GetComponentFromField(container, field);

                // adesso decide se deve cancellarlo o meno
                if (ebComponent != null)
                {
                    if (IsToDelete(ebComponent))
                    {
                        // mi segno che andrà rimosso fuori dal ciclo
                        fieldToRemove.Add(field);

                        // elimino le istruzioni dai metodi
                        DeleteStatementsFrom(createComponents, ebComponent);
                        DeleteStatementsFrom(clearComponents, ebComponent);
                        DeleteStatementsFrom(applyResources, ebComponent);

                        if (!classesToRemove.Contains(ebComponent.SerializedType))
                            classesToRemove.Add(ebComponent.SerializedType);
                    }
                    else
                    {
                        if (!classesNotToRemove.Contains(ebComponent.SerializedType))
                            classesNotToRemove.Add(ebComponent.SerializedType);
                    }
                }

            }

            // infine tolgo le dichiarazioni di field
            foreach (FieldDeclaration field in fieldToRemove)
				theClass.Members.Remove(field);

            return createComponents == null || createComponents.Body.Statements.Count == 0;
        }

        //-----------------------------------------------------------------------------
        private bool IsEnabledBySetting()
        {
            try
            {

                SettingItem i = BasePathFinder.BasePathFinderInstance.GetSettingItem("Extensions", "EasyBuilder", "EasyStudio", "DeltaEnabled");

                if (i == null)
                    return true;

                bool b = false;
                try { b = (string)i.Values[0] != "0"; }
                catch { }

                return b;
            }
            catch
            {
                return false;
            }
        }
    }
}
  
