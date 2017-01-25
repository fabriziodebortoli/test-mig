namespace Microarea.RSWeb.Temp
{   //Temporary
    public class MyColor
    {
        public int A { get; set; }

        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public MyColor()
        {
            A = 255;
            R = 255;
            G = 255;
            B = 255;
        }

        public static MyColor FromArgb(int a, int r, int g, int b)
        {
            MyColor c = new MyColor();
            c.A = a;
            c.B = b;
            c.R = r;
            c.G = g;
            return c;
        }

    }
}