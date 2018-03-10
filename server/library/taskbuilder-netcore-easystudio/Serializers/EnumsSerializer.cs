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
            string fileName = moduleInfo.GetEnumsPath();
            string path = System.IO.Path.GetDirectoryName(fileName);
            if (!PathFinder.ExistPath(path))
                PathFinder.CreateFolder(path, true);
            return table.Tags.SaveXml(fileName, false, false, moduleInfo);
        }

        //---------------------------------------------------------------
        public bool GenerateSourceCode(ModuleInfo moduleInfo)
        {
            return true;
        }
    }
}
