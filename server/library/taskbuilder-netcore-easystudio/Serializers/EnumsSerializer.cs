using Microarea.Common.Applications;
using Microarea.Common.NameSolver;
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.EasyStudio.Services;
using System.Collections.Generic;
using System;
using System.Text;

namespace TaskBuilderNetCore.EasyStudio.Serializers
{
    //======================================================================
    public class EnumsSerializerFormatter
    {
        //------------------------------------------------------------------------------------------------
        public static string FormatterCSharp(string arg)
        {
            if (arg == null)
                return "Null";

            if (arg.Trim().Length == 0)
                return "Blank";

            StringBuilder builder = new StringBuilder();
            bool first = true;
            foreach (var c in arg)
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

        //----------------------------------------------------------------------------
        public static string FormatterCPlusPluss(string arg)
        {
            return arg.Replace(" ", "_").ToUpper();
        }
    }

    //======================================================================
    public class EnumItemSerializer
    {
        EnumItem item = null;
        EnumTagSerializer parent = null;
        
        //-------------------------------------------------------------------------
        public EnumItemSerializer()
        {

        }

        //-----------------------------------------------------------------------------------------
        public EnumItem Item
        {
            get { return item; }
            set { item = value; }
        }

        //------------------------------------------------------------------------------
        public EnumTagSerializer Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        //-----------------------------------------------------------------------------------
        public string FormattedNameCPlusPlus
        {
            get
            {
                if (item == null)
                    return string.Empty;

                return EnumsSerializerFormatter.FormatterCPlusPluss(item.Name);
            }
        }

        //-----------------------------------------------------------------------------------
        public string FormattedNameCSharp
        {
            get
            {
                if (item == null)
                    return "Null";

                return EnumsSerializerFormatter.FormatterCSharp(item.Name);
            }
        }

        //----------------------------------------------------------------------------
        public ushort OwnerValue
        {
            get
            {
                if (item == null)
                    return 0;

                return item.Owner.Value;
            }
        }
    }

    //======================================================================
    public class EnumTagSerializer
    {
        EnumTag tag = null;
        List<EnumItemSerializer> items = null;
        string formattedName = string.Empty;
        string formattedDefaultItem = string.Empty;
        
        //---------------------------------------------------------------------
        public EnumTag Tag
        {
            set
            {
                EnumTag valueTag = value as EnumTag;
                tag = valueTag;
                foreach(EnumItem ei in tag.EnumItems)
                {
                    EnumItemSerializer eiSer = new EnumItemSerializer();
                    eiSer.Item = ei;
                    eiSer.Parent = this;
                    this.items.Add(eiSer);
                }
            }
            get { return tag; }
        }

        //-----------------------------------------------------------------------
        public List<EnumItemSerializer> Items
        {
            get { return items; }
        }

        //-----------------------------------------------------------------------
        public string FormattedNameCPlusPlus
        {
            get
            {
                if (tag == null)
                    return string.Empty;

                return EnumsSerializerFormatter.FormatterCPlusPluss(tag.Name);
            }
            
         }

        //-----------------------------------------------------------------------------------
        public string FormattedNameCSharp
        {
            get
            {
                if (tag == null)
                    return "Null";

                return EnumsSerializerFormatter.FormatterCSharp(tag.Name);
            }
        }

        //-------------------------------------------------------------------------
        public string FormattedDefaultItemCPlusPlus
        {
            get
            {
                if (tag == null || tag.EnumItems == null)
                    return string.Empty;
                
                foreach (EnumItem ei in tag.EnumItems)
                {
                    if (ei.Value == tag.DefaultValue)
                        return EnumsSerializerFormatter.FormatterCPlusPluss(ei.Name);
                }

                return string.Empty;
            }
        }

        //--------------------------------------------------------------------
        public EnumTagSerializer()
        {
            items = new List<EnumItemSerializer>();
        }
    }

    //====================================================================
    public class EnumsSerializer : Serializer
    {
        const string templateTBApplicationEnums = "Enums.h";
        const string templateCustomizationEnums = "Enums.cs";
        const string prefix = "Microarea.EasyBuilder.";

        public List<EnumTagSerializer> myTags = null;

        //---------------------------------------------------------------
        public EnumsSerializer()
        {
            myTags = new List<EnumTagSerializer>();
        }

        //---------------------------------------------------------------
        public bool SaveDeclaration(ModuleInfo moduleInfo, Enums table)
        {
            string fileName = moduleInfo.GetEnumsPath();
            string path = System.IO.Path.GetDirectoryName(fileName);
            if (!PathFinder.ExistPath(path))
                PathFinder.CreateFolder(path, true);
            return table.Tags.SaveXml(fileName, false, false, moduleInfo);
        }

        //---------------------------------------------------------------
        public bool GenerateSourceCode(ModuleInfo moduleInfo, ApplicationType appType, /*Enums enums*/EnumTags enumTags)
        {
            string code = string.Empty;
            string fileName = string.Empty;
            string destFullFileName = string.Empty;
            bool bFilterTags = false;

            ServicesManager servMng = new ServicesManager();
            TemplateCodeService templateService = servMng.GetService<TemplateCodeService>();

            switch (appType)
            {
                case ApplicationType.TaskBuilderApplication:
                    fileName = templateTBApplicationEnums;
                    destFullFileName = System.IO.Path.Combine(moduleInfo.Path, string.Concat(moduleInfo.Name, templateTBApplicationEnums));
                    bFilterTags = true;
                    break;
                case ApplicationType.Customization:
                    fileName = templateCustomizationEnums;
                    destFullFileName = System.IO.Path.Combine(PathFinder.GetEasyStudioReferencedAssembliesPath(), string.Concat(prefix, templateCustomizationEnums));
                    break;
                default:
                    return false;
            }

            if (string.IsNullOrEmpty(fileName))
                return false;

            myTags.Clear();
            foreach(EnumTag eTag in enumTags)
            {
                
                if (!bFilterTags || bFilterTags && string.Compare(eTag.OwnerModule.NameSpace.ToString(), moduleInfo.NameSpace.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    EnumTagSerializer myTag = new EnumTagSerializer();
                    myTag.Tag = eTag;
                    myTags.Add(myTag);
                }
            }

            templateService.ManageSerialization(appType, fileName, this, destFullFileName);

            return true;
        }
    }
}
