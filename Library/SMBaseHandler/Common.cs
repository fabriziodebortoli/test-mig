
namespace Microarea.Library.SMBaseHandler
{
    public class CommonFactory
    {
        public static Common GetCommon(string pname)
        {
            if (pname.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "valorestudio" || pname.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "abitat" || pname.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "mbooknet")
            { return new CommonVS(); }//todo ilaria verifica sta schifezza dei nomi camblati man non vedo come fare meglio 
            return new Common();
        }

    }

    public class Common
    {
        // Settings.xml replacement
        protected virtual byte[] GetInnerVal()
        {
            return new byte[] { 104, 116, 116, 112, 58, 47, 47, 119, 119, 119, 46, 109, 105, 99, 114, 111, 97, 114, 101, 97, 46, 105, 116, 47, 65, 99, 116, 105, 118, 97, 116, 105, 111, 110, 75, 101, 121, 47, 65, 99, 116, 105, 118, 97, 116, 105, 111, 110, 75, 101, 121, 46, 97, 115, 109, 120 };
        }
        protected virtual byte[] GetOuterVal()
        {
            return
              new byte[] { 77, 105, 99, 114, 111, 97, 114, 101, 97, 83, 101, 114, 118, 101, 114 };
        }
        public string InnerBytes { get { return Helper.Evaluate(GetInnerVal()); } }
        public string OuterBytes { get { return Helper.Evaluate(GetOuterVal()); } }

        public static string SMExtension = ".csm";
    }

    public class CommonVS : Common//codici diversi da mago per criptare i mduli di valore studio ed apogeo.
    {
        protected override byte[] GetInnerVal()
        {
            return new byte[] { 105, 116, 116, 112, 58, 47, 47, 119, 119, 119, 46, 109, 105, 99, 114, 111, 97, 114, 101, 97, 46, 105, 116, 47, 65, 99, 116, 105, 118, 97, 116, 105, 111, 110, 75, 101, 121, 47, 65, 99, 116, 105, 118, 97, 116, 105, 111, 110, 75, 101, 121, 46, 97, 115, 109, 120 };
        }
        protected override byte[] GetOuterVal()
        {
            return new byte[] { 78, 105, 99, 114, 111, 97, 114, 101, 97, 83, 101, 114, 118, 101, 114 };
        }

    }
}
