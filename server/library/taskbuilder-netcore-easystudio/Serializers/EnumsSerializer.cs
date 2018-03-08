using Microarea.Common.Applications;
using Microarea.Common.NameSolver;

namespace TaskBuilderNetCore.EasyStudio.Serializers
{
    //====================================================================
    public class EnumsSerializer : Serializer
    {      
        //---------------------------------------------------------------
        public EnumsSerializer()
        {
        }

        //---------------------------------------------------------------
        public bool SaveDeclaration(ModuleInfo moduleInfo, Enums table)
        {
            string fileName = string.Empty;
            table.Tags.SaveXml(fileName, false);
            return true;
        }

        //---------------------------------------------------------------
        public bool GenerateSourceCode(ModuleInfo moduleInfo)
        {
            return true;
        }
    }
}
