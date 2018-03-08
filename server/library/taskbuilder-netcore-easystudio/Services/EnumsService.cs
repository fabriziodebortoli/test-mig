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
		public bool Create(INameSpace moduleNameSpace, ushort value, string name, string description = "", bool hidden = false)
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
                Diagnostic.Add(DiagnosticType.Error, string.Concat(tag.Name, ": ", Strings.TagNotDeclared));
                return false;
            }

            ModuleInfo info = GetModuleInfo(moduleNameSpace);
            if (info == null)
                return false;

            EnumsTable.Tags.DeleteTag(name);

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
            if (info == null)
                return false;
            return EnumsSerializer.GenerateSourceCode(info);
        }

        //---------------------------------------------------------------
        public bool CreateItem(INameSpace moduleNameSpace, string tagName, int value, string name, string description)
        {
            return true;
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

