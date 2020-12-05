using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MysticLightController
{
    public class ColorArray
    {
        List<Color> colors = new List<Color>();
        bool dirty;
        uint[] red;
        uint[] green;
        uint[] blue;

        public ColorArray() { }

        public List<Color> Colors
        {
            get => colors;
        }

        void prepare()
        {
            if (!dirty) return;
            red = (from c in colors select c.R).ToArray();
            green = (from c in colors select c.G).ToArray();
            blue = (from c in colors select c.B).ToArray();
            dirty = false;
        }

        public void addColor(Color color)
        {
            colors.Add(color);
            dirty = true;
        }
        public void addColor(uint r, uint g, uint b)
        {
            addColor(new Color(r, g, b));
        }

        public void apply(string device, ref string[] names, uint area)
        {
            prepare();
            if (!LightController.API_OK(LightApiDLL.MLAPI_SetLedColors(device, area, ref names, red, green, blue), out string error))
            {
                Console.WriteLine("MLAPI_SetLedColorEx:\n\t" + error);
            }
        }
    }


    class Program
    {
        static bool exitSignal = false;
        #region Trap application termination
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool ExitHandler(CtrlType sig)
        {
            Console.WriteLine($"Got Exit Signal {sig}");
            exitSignal = true;

            return true;
        }
        #endregion

        public static void Run()
        {
            ChristmasMarquee3();
            Console.WriteLine("Thread exiting");
        }

        static string[] GetJRainbowNames(uint which, uint count)
        {
            string[] names = new string[count];

            for (int i = 0; i < count; i++)
            {
                names[i] = $"JRAINBOW{which}_{i + 1}";
            }
            return names;
        }
        static void ChristmasMarquee3()
        {
            string device = "MSI_MB";
            string[] names1 = GetJRainbowNames(1, 16);
            string[] names2 = GetJRainbowNames(2, 16);
            ColorArray[] ca = new ColorArray[] { new ColorArray(), new ColorArray(), new ColorArray() };
            int i;

            ca[0].addColor(64, 64, 64);
            ca[1].addColor(255, 0, 0);
            ca[2].addColor(0, 255, 0);
            for (i = 1; i < 16; i++)
            {
                Color first = ca[0].Colors.Last();
                ca[0].addColor(ca[1].Colors.Last());
                ca[1].addColor(ca[2].Colors.Last());
                ca[2].addColor(first);
            }

            LightApiDLL.MLAPI_SetLedStyle(device, 3, "Direct Lighting Control");
            LightApiDLL.MLAPI_SetLedStyle(device, 4, "Direct Lighting Control");

            i = 0;
            int delayMs = 125;
            while ( !exitSignal )
            {
                DateTime end = DateTime.Now.AddMilliseconds(delayMs);
                i = (i + 1) % ca.Length;
                ca[i].apply(device, ref names1, 1);
                ca[i].apply(device, ref names2, 2);
                int remaining = (int)(end - DateTime.Now).TotalMilliseconds;
                if (remaining > 0)
                    Thread.Sleep(remaining);
            }

            LightApiDLL.MLAPI_SetLedStyle(device, 3, "Marquee");
            LightApiDLL.MLAPI_SetLedStyle(device, 4, "Marquee");
        }
        static void Main(string[] args)
        {
            _handler += new EventHandler(ExitHandler);
            SetConsoleCtrlHandler(_handler, true);

            LightApiDLL.MLAPI_Initialize();

            Thread t = new Thread(new ThreadStart(Program.Run));

            t.Start();
            while ( !exitSignal )
            {
                Thread.Sleep(500);
            }
            t.Join();
            Console.WriteLine("Main exiting");
            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
        }
    }
}


/*
//static LightController controller;
static void ChristmasMarquee1()
{
    string device = "MSI_MB";
    string error;
    string[] names = new string[16];
    uint[][] r = new uint[2][];
    uint[][] g = new uint[2][];
    uint[][] b = new uint[2][];

    r[0] = new uint[16];
    g[0] = new uint[16];
    b[0] = new uint[16];
    r[1] = new uint[16];
    g[1] = new uint[16];
    b[1] = new uint[16];
    for (int i = 0; i < 16; i++)
    {
        names[i] = $"JRAINBOW1_{i + 1}";
        r[0][i] = (i & 1) == 0 ? 255 : (uint)0;
        g[0][i] = (i & 1) == 0 ? (uint)0 : 255;
        b[0][i] = 0;
        r[1][i] = (i & 1) == 0 ? (uint)0 : 255;
        g[1][i] = (i & 1) == 0 ? 255 : (uint)0;
        b[1][i] = 0;
    }

    controller.SetLedStyle(device, 3, "Direct Lighting Control");

    Stopwatch clock = new Stopwatch();

    for (int i = 0; i < 100; i++)
    {
        clock.Restart();
        if (!LightController.API_OK(LightApiDLL.MLAPI_SetLedColors(device, 1, ref names, r[i & 1], g[i & 1], b[i & 1]), out error))
        {
            Console.WriteLine("MLAPI_SetLedColorEx:\n\t" + error);
            break;
        }
        Thread.Sleep((int)(250 - clock.ElapsedMilliseconds));
    }

    controller.SetLedStyle(device, 3, "Marquee");
}
static void ChristmasMarquee2()
{
    string device = "MSI_MB";
    string error;
    string[] names = new string[16];
    uint[][] r = new uint[2][];
    uint[][] g = new uint[2][];
    uint[][] b = new uint[2][];

    r[0] = new uint[16];
    g[0] = new uint[16];
    b[0] = new uint[16];
    r[1] = new uint[16];
    g[1] = new uint[16];
    b[1] = new uint[16];
    for (int i = 0; i < 16; i++)
    {
        names[i] = $"JRAINBOW2_{i + 1}";
        r[0][i] = (i & 1) == 0 ? 255 : (uint)0;
        g[0][i] = (i & 1) == 0 ? (uint)0 : 255;
        b[0][i] = 0;
        r[1][i] = (i & 1) == 0 ? (uint)0 : 255;
        g[1][i] = (i & 1) == 0 ? 255 : (uint)0;
        b[1][i] = 0;
    }

    controller.SetLedStyle(device, 4, "Direct Lighting Control");

    Stopwatch clock = new Stopwatch();

    for (int i = 0; i < 100; i++)
    {
        clock.Restart();
        if (!LightController.API_OK(LightApiDLL.MLAPI_SetLedColors(device, 2, ref names, r[i & 1], g[i & 1], b[i & 1]), out error))
        {
            Console.WriteLine("MLAPI_SetLedColorEx:\n\t" + error);
            break;
        }
        Thread.Sleep((int)(250 - clock.ElapsedMilliseconds));
    }

    controller.SetLedStyle(device, 4, "Marquee");
}
void main()
{
    //string error;
    //controller = new LightController();
    //Console.WriteLine("Light controller was created");
    //string device = controller.Devices[0];
    //ChristmasMarquee1();

    /*
    Console.WriteLine($"Getting color 0 for {device}");
    Color[] colors = controller.GetAllLedColors(device);
    Color oldColor;
    if (colors.Length > 0)
    {
        oldColor = colors[0];
    }
    */
    /*
    if (LightController.API_OK(LightApiDLL.MLAPI_GetLedName(device, out string[] names), out string error) && names != null)
    {
        foreach (string name in names)
        {
            Console.WriteLine($"{name}");
        }
    }
    else
    {
        Console.WriteLine("Failed getting LED names:\n\t" + error);
    }
    */
    /*
    controller.SetLedStyle(device, 3, "Direct Lighting Control");
    string[] names = new string[16];
    uint[][] r = new uint[2][];
    uint[][] g = new uint[2][];
    uint[][] b = new uint[2][];

    r[0] = new uint[16];
    g[0] = new uint[16];
    b[0] = new uint[16];
    r[1] = new uint[16];
    g[1] = new uint[16];
    b[1] = new uint[16];
    for (int i = 0; i < 16; i++)
    {
        names[i] = $"JRAINBOW1_{i+1}";
        r[0][i] = (i & 1) == 0 ? 255 : (uint)0;
        g[0][i] = (i & 1) == 0 ? (uint)0 : 255;
        b[0][i] = 0;
        r[1][i] = (i & 1) == 0 ? (uint)0 : 255;
        g[1][i] = (i & 1) == 0 ? 255 : (uint)0;
        b[1][i] = 0;
    }

    for (int i = 0; i < 100; i++)
    {
        if (!LightController.API_OK(LightApiDLL.MLAPI_SetLedColors(device, 1, ref names, r[i&1], g[i&1], b[i&1]), out error))
        {
            Console.WriteLine("MLAPI_SetLedColorEx:\n\t" + error);
            break;
        }
        Thread.Sleep(250);
    }

    controller.SetLedStyle(device, 3, "Marquee");
    controller.SetLedStyle(device, 4, "Marquee");
    */
    /*
    for ( int i = 1; i <= 16; i++ )
    {
        if (!LightController.API_OK(LightApiDLL.MLAPI_SetLedColorEx(device, 1, $"JRAINBOW1_{i}", 0, 255, 0, 1), out error))
        {
            Console.WriteLine("MLAPI_SetLedColorEx:\n\t" + error);
            break;
        }
    }
    */
    /*
    //controller.SetAllLedColors(device, new Color(255, 0, 0));
    //controller.SetAllLedColors(device, oldColor);
}
*/
