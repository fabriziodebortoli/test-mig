
namespace Microarea.TaskBuilderNet.Core.NameSolver
{
   

    //=========================================================================
    public class NewObject
    {
        private bool isBatch = false;
        private bool isDataEntry = false;
        private bool isFinder = false;
        private bool isReport = false;

        protected string nameSpace;

        public string NameSpace { get { return nameSpace; } }

        public bool IsBatch { get { return isBatch; } }
        public bool IsDataEntry { get { return isDataEntry; } }
        public bool IsFinder { get { return isFinder; } }
        public bool IsReport { get { return isReport; } }

        //---------------------------------------------------------------------
        public NewObject(string aNamePace, bool aIsBatch, bool aIsDataEntry, bool aIsFinder, bool aIsReport)
        {
            nameSpace = aNamePace;

            isBatch = aIsBatch;
            isFinder = aIsFinder;
            isDataEntry = aIsDataEntry;
            isReport = aIsReport;
        }
    }

 
	//---------------------------------------------------------------------------
	public class TbServicesErrorCodes
	{
		public const int NoError								= 0;
		public const int XmlDataFileNotValid					= -1;
		public const int XmlDataFileSaveFailed					= -2;
		public const int CreateTBFailed							= -3;
		public const int SaveXMLTBDocumentFailed				= -4;
		public const int SaveXMLTBDocumentFailedWithNoMsg		= -5;
		public const int SaveXMLTBDocumentFailedWithWrongMsg	= -6;
		public const int XmlParametersFileNotValid				= -7;
		public const int XmlParamsFileSaveFailed				= -8;
		public const int LoadXMLTBDocumentFailedWithNoMsg		= -9;
		public const int LoadXMLTBDocumentFailed				= -10;
		public const int LoadXMLTBDocFailedToSaveFiles			= -11;
		public const int GetXMLParametersFailed					= -12;
		public const int GetXMLParametersFailedWithNoMsg		= -13;
		public const int GetXMLParametersFailedWithWrongMsg		= -14;
	}
}
