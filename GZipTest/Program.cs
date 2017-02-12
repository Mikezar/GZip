using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    class Program
    {
       static GZip zipper;

        static int Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelKeyPress);

            ShowInfo();

            try
            {
                //UNCOMMENT ON WORKING WITH CONSOLE!
                //args = new string[3];
                //args[0] = @"compress";
                //args[1] = @"C:\Users\Mike\Desktop\olifer_ru.pdf";
                //args[2] = @"C:\Users\Mike\Desktop\new.pdf";


                Validation.StringReadValidation(args);

                switch (args[0].ToLower())
                {
                    case "compress":
                        zipper = new Compressor(args[1], args[2]);
                    break;
                    case "decompress":
                        zipper = new Decompressor(args[1], args[2]);
                    break;
                }

                zipper.Launch();
                return zipper.CallBackResult();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error is occured!\n Method: {0}\n Error description {1}", ex.TargetSite, ex.Message);
                return 1;
            }
        }

       static void ShowInfo()
        {
            Console.WriteLine("To zip or unzip files please proceed with the following pattern to type in:\n" + 
                              "Zipping: GZipTest.exe compress [Source file path] [Destination file path]\n" +
                              "Unzipping: GZipTest.exe decompress [Compressed file path] [Destination file path]\n" +
                              "To complete the program correct please use the combination CTRL + C");
        }


       static void CancelKeyPress(object sender, ConsoleCancelEventArgs _args)
        {
            if (_args.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                Console.WriteLine("\nCancelling...");
                _args.Cancel = true;
                zipper.Cancel();
                
            }
        }
    }
}
