using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Model
{
    public class CallerContext
    {
        INameSpace nameSpace;

        public INameSpace NameSpace
        {
            get
            {
                return nameSpace;
            }

            set
            {
                nameSpace = value;
            }
        }
    }
}