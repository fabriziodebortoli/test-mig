using System;

namespace Microarea.TbJson
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    throw new ApplicationException("Usage: TbJson /ts <file or folder name>");
                }
                switch (args[0])
                {
                    case "/ts":
                        new WebInterfaceGenerator().Generate(args[1], args.Length >= 3 ? args[2] : "", false);
                        break;
                    case "/merge":
                        new WebInterfaceGenerator().Generate(args[1], args.Length >= 3 ? args[2] : "", true);
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
