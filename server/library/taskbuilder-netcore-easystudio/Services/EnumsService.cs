using System.ComponentModel;
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.Common.CustomAttributes;
using Microarea.Common.NameSolver;
using Microarea.Common.Applications;
using TaskBuilderNetCore.EasyStudio.Serializers;

namespace TaskBuilderNetCore.EasyStudio.Services
{
	//====================================================================
	[Name("enumsSvc"), Description("This service manages enums structure info and serialization.")]
	[DefaultSerializer(typeof(EnumsSerializer))]
	public class EnumsService : Service
	{
        //=========================================================================
        internal class Strings
        {
            internal static readonly string MissingModule       = "Module namespace unknown, module not declared";
            internal static readonly string MissingModuleParameter = "Module namespace parameter";
            internal static readonly string TagAlreadyDeclared  = "Tag is already declared";
            internal static readonly string CannotAddTagInTable = "Cannot Add Tag into enums table";
            internal static readonly string MissingOrEmptyParameters = "Missing parameters: value and name are mandatory";
            internal static readonly string TagNotDeclared = "Tag Not Declared";
            internal static readonly string TagNotDeclaredInModule = "Tag Not Declared in Module";
            internal static readonly string ItemNotDeclared = "Item Not Declared";
            internal static readonly string ItemNotDeclaredInModule = "Item not declared in Module";
            internal static readonly string ItemVerifyDefaultValue = "Verify item value and tag default value";
            internal static readonly string CannotChangeDefaultValueTag = "Cannot Change Default Value Tag";
            internal static readonly string Or = "Or";
        }

        //---------------------------------------------------------------
        Enums enumsTable = null;
        EnumsSerializer EnumsSerializer { get => Serializer as EnumsSerializer; }

        //---------------------------------------------------------------
        public Enums EnumsTable
        {
            get 
            {
                if (enumsTable == null)
                {
                    enumsTable = new Enums();
                    enumsTable.LoadXml();
                }
                return enumsTable;
            }
        }

        //---------------------------------------------------------------
        public EnumsService()
		{
		}

		//---------------------------------------------------------------
		public bool CreateTag(INameSpace moduleNameSpace, ushort value, string name, string description = "", bool hidden = false)
		{
            if (value == 0 || string.IsNullOrEmpty(name))
            {
                Diagnostic.Add(DiagnosticType.Error, Strings.MissingOrEmptyParameters);
                return false;
            }

            EnumTag tag = EnumsTable.GetTagByStoredValue(value);
            if (tag != null)
            {
                Diagnostic.Add(DiagnosticType.Error, string.Concat(tag.Name, ": ", value.ToString(), Strings.TagAlreadyDeclared));
                return false;
            }

            ModuleInfo info = GetModuleInfo(moduleNameSpace);
            if (info == null)
                return false;

            tag = EnumsTable.AddEnumTag(info, name, value);
            if (tag == null)
            {
                Diagnostic.Add(DiagnosticType.Error, string.Concat(name, ": ", value.ToString(), Strings.CannotAddTagInTable));
                return false;
            }

            tag.Description = description;
            tag.Hidden = hidden;

            return EnumsSerializer.SaveDeclaration(info, EnumsTable);
        }

        //---------------------------------------------------------------
        public bool Delete(INameSpace moduleNameSpace, string name, string itemName = "")
        {
            if (string.IsNullOrEmpty(name))
            {
                Diagnostic.Add(DiagnosticType.Error, Strings.MissingOrEmptyParameters);
                return false;
            }

            EnumTag tag = EnumsTable.Tags.GetTag(name);
            if (tag == null)
            {
                Diagnostic.Add(DiagnosticType.Error, string.Concat(name, ": ", Strings.TagNotDeclared));
                return false;
            }

            ModuleInfo info = GetModuleInfo(moduleNameSpace);
            if (info == null)
                return false;

            // cancellazione del tag
            if (string.IsNullOrEmpty(itemName))
            {
                if (!EnumsTable.Tags.DeleteTag(moduleNameSpace.ToString(), name))
                {
                    Diagnostic.Add(DiagnosticType.Error, string.Concat(tag.Name, ": ", Strings.TagNotDeclaredInModule, ": ", moduleNameSpace.ToString()));
                    return false;
                }
            }
            else // cancellazione dell'item
                if (!tag.DeleteItem(moduleNameSpace.ToString(), itemName))
                {
                    Diagnostic.Add
                        (
                            DiagnosticType.Error, 
                            string.Concat
                                (
                                    itemName, ": ", 
                                    Strings.ItemNotDeclared, " ", 
                                    Strings.Or, " ", 
                                    Strings.ItemNotDeclaredInModule, ": ", 
                                    moduleNameSpace.ToString(), " ",
                                    Strings.Or, " ",
                                    Strings.ItemVerifyDefaultValue
                                )
                        );
                    return false;
                }
            
            return EnumsSerializer.SaveDeclaration(info, EnumsTable);
        }

        //-----------------------------------------------------------------------------------------
        public bool ChangeTagDefaultValue(INameSpace moduleNamespace, string name, ushort defaultValue)
        {
            if (string.IsNullOrEmpty(name))
            {
                Diagnostic.Add(DiagnosticType.Error, Strings.MissingOrEmptyParameters);
                return false;
            }

            EnumTag tag = EnumsTable.Tags.GetTag(name);
            if (tag == null)
            {
                Diagnostic.Add(DiagnosticType.Error, string.Concat(tag.Name, ": ", Strings.TagNotDeclared));
                return false;
            }

            ModuleInfo info = GetModuleInfo(moduleNamespace);
            if (info == null)
                return false;

            if (!tag.ChangeTagDefaultValue(moduleNamespace.ToString(), defaultValue))
            {
                Diagnostic.Add(DiagnosticType.Error, string.Concat(tag.Name, ": ", Strings.CannotChangeDefaultValueTag));
                return false;
            }

            return EnumsSerializer.SaveDeclaration(info, EnumsTable);
        }

        //---------------------------------------------------------------
        private ModuleInfo GetModuleInfo(INameSpace moduleNameSpace)
        {
            if (moduleNameSpace == null)
            {
                Diagnostic.Add(DiagnosticType.Error, Strings.MissingModuleParameter);
                return null;
            }

            ModuleInfo info = PathFinder.GetModuleInfo(moduleNameSpace);
            if (info == null)
                Diagnostic.Add(DiagnosticType.Error, string.Concat(moduleNameSpace.ToString(), Strings.MissingModule));

            return info;
        }

        //---------------------------------------------------------------
        public bool GenerateSourceCode(INameSpace moduleNameSpace)
		{
            ModuleInfo info = GetModuleInfo(moduleNameSpace);
            return info != null ? EnumsSerializer.GenerateSourceCode(info) : false;
        }

        //---------------------------------------------------------------
        public bool CreateItem(INameSpace moduleNameSpace, string tagName, ushort value, string name, string description, bool hidden = false)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(tagName))
            {
                Diagnostic.Add(DiagnosticType.Error, Strings.MissingOrEmptyParameters);
                return false;
            }

            ModuleInfo info = GetModuleInfo(moduleNameSpace);
            if (info == null)
                return false;

            EnumTag tag = EnumsTable.Tags.GetTag(name);
            if (tag == null)
            {
                Diagnostic.Add(DiagnosticType.Error, string.Concat(tag.Name, ": ", Strings.TagNotDeclared));
                return false;
            }

            EnumItem item = tag.AddItem(name, value, description, info);
            if (item != null)
                item.Hidden = hidden;

            return item != null;
        }

        //---------------------------------------------------------------
        public bool Refresh()
        {
            if (enumsTable != null)
                enumsTable = null;

            return EnumsTable != null;
        }
    }
}

