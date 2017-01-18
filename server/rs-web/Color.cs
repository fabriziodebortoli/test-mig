namespace Microarea.RSWeb.Temp
{   //Temporary
    public class Color
    {
        public int A { get; set; }

        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public Color()
        {
            A = 255;
            R = 255;
            G = 255;
            B = 255;
        }

        public static Color FromArgb(int a, int r, int g, int b)
        {
            Color c = new Color();
            c.A = a;
            c.B = b;
            c.R = r;
            c.G = g;
            return c;
        }

    }
}