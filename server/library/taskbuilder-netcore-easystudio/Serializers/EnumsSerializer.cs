using Microarea.Common.Applications;
using Microarea.Common.NameSolver;
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.EasyStudio.Services;
using System.Collections.Generic;

namespace TaskBuilderNetCore.EasyStudio.Serializers
{
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
        public string FormattedName
        {
            get
            {
                if (item == null)
                    return string.Empty;

                return item.Name.Replace(" ", "_").ToUpper();
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
        public string FormattedName
        {
            get
            {
                if (tag == null)
                    return string.Empty;

                return tag.Name.Replace(" ", "_").ToUpper();
            }
            
         }

        //-------------------------------------------------------------------------
        public string FormattedDefaultItem
        {
            get
            {
                if (tag == null || tag.EnumItems == null)
                    return string.Empty;
                
                foreach (EnumItem ei in tag.EnumItems)
                {
                    if (ei.Value == tag.DefaultValue)
                        return ei.Name.Replace(" ", "_").ToUpper();
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

            ServicesManager servMng = new ServicesManager();
            TemplateCodeService templateService = servMng.GetService<TemplateCodeService>();

            switch (appType)
            {
                case ApplicationType.TaskBuilderApplication:
                    fileName = templateTBApplicationEnums;
                    destFullFileName = System.IO.Path.Combine(moduleInfo.Path, string.Concat(moduleInfo.Name, templateTBApplicationEnums));
                    break;
                case ApplicationType.Customization:
                    fileName = templateCustomizationEnums;
                    //!!!!TODO - percorso corretto!!!!
                    destFullFileName = destFullFileName = System.IO.Path.Combine(moduleInfo.Path, templateCustomizationEnums);
                    break;
                default:
                    return false;
            }

            if (string.IsNullOrEmpty(fileName))
                return false;

            myTags.Clear();
            foreach(EnumTag eTag in enumTags)
            {
                EnumTagSerializer myTag = new EnumTagSerializer();
                myTag.Tag = eTag;
                myTags.Add(myTag);
            }

            templateService.ManageSerialization(appType, fileName, this, destFullFileName);

            return true;
        }
    }
}
