using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form form=null;
            Console.WriteLine("All test:");
            Console.WriteLine("1 PureUdp test");
            Console.WriteLine("2 PureKcp test");
            Console.WriteLine("3 Udp+Kcp mix test");
            Console.WriteLine("other: exit");
            string input = "";           
            Console.WriteLine("input 1~3:");
            input = Console.ReadLine();
            switch (input.Trim())
            {
                case "0":
                    form = new ClientForm1();
                    break;
                case "1":
                    form = new ClientUdp();
                    break;
                case "2":                
                    form = new ClientKcp();
                    break;
                case "3":
                    form = new ClientMix();
                    break;
                default:
                    return;
                    break;
            }

            Application.Run(form);
        }
    }
}
