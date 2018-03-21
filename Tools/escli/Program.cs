using System;

namespace escli
{
    class Program
    {
        static void Main(string[] args)
        {
            Engine engine = new Engine();
            engine.Execute(args);
        }
    }
}
