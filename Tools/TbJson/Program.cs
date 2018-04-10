using System;

namespace Microarea.TbJson
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var verboseOutput = false;
            try
            {
                if (args.Length < 2)
                {
                    throw new ApplicationException("Usage: TbJson /ts <file or folder name> -v");
                }
                if (args[args.Length - 1] == "-v")
                    verboseOutput = true;
                switch (args[0])
                {
                    case "/resetRoutes":
                        new WebInterfaceGenerator(verboseOutput).ResetRoutes(args[1]);
                        break;
                    case "/ts":
                        new WebInterfaceGenerator(verboseOutput).Generate(args[1], args.Length >= 3 ? args[2] : "", false, false);
                        break;
                    case "/merge":
                        new WebInterfaceGenerator(verboseOutput).Generate(args[1], args.Length >= 3 ? args[2] : "", true, false);
                        break;
                    case "/force":
                        new WebInterfaceGenerator(verboseOutput).Generate(args[1], args.Length >= 3 ? args[2] : "", true, true);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
