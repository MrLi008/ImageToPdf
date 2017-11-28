using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leadtools;
using Leadtools.Codecs;
using Leadtools.ImageProcessing;
using Leadtools.Forms.Ocr;

namespace ConvertImageToPDF
{
    class Program
    {
        private static IOcrEngine ocrEngine;
        static void Main(string[] args)
        {
           // args = new string[1] {  @"1.bmp" };
            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine("args: " + args[i]);
            }
            if (args.Length <= 1)
            {
                Console.WriteLine(@"provide at least two args: new filename and imagefilename or imagefileroot");
                return;
            }
            SetLicense();
            ocrEngine = OcrEngineManager.CreateEngine(OcrEngineType.Professional, false);
            ocrEngine.Startup(null, null, null, null);
            RasterCodecs codecs;
            codecs = ocrEngine.RasterCodecsInstance;
            codecs.Options.RasterizeDocument.Load.XResolution = 300;
            codecs.Options.RasterizeDocument.Load.YResolution = 300;
            ocrEngine.LanguageManager.EnableLanguages(new string[]
				{
					"zh-Hans"
				});

            IOcrDocument ocrDocument;
            ocrDocument = ocrEngine.DocumentManager.CreateDocument();
            Console.WriteLine(@"load all Pics");
            List<string> files = new List<string>();
            if (Directory.Exists(args[1]))
            {
                 files = Directory.GetFiles(args[1], "*.JPG", SearchOption.AllDirectories).ToList<string>();
            }
            else 
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (File.Exists(args[i])){
                        files.Add(args[i]);
                    }
                }

            }
            for (int i = 0; i < files.ToArray<string>().Length; i++ )
            {
                exe_ocr(files[i], ocrDocument);
            }
            Console.WriteLine("Begin ocr engine .....");
            ocrDocument.Save(args[0] + ".pdf", Leadtools.Forms.DocumentWriters.DocumentFormat.Pdf, null);

            ocrEngine.Shutdown();
            Console.Write("end ocr engine with result file: " + args[0] + ".pdf\nPress any key to continue...");
           //  Console.ReadKey();
           
        }
        public static void exe_ocr(string filename, IOcrDocument ocrDocument)
        {
            Console.WriteLine(filename);

           
            ocrDocument.Pages.AddPage(filename, null);

            ocrDocument.Pages.AutoZone(null);
            ocrDocument.Pages.Recognize(null);
        }
        public static void SetLicense()
        {
            string p_Lic, p_Key;
            //1、读取License文件
            //License文件夹被我放到Debug文件夹和Release文件夹里，可以根据需要变更文件路径
            System.IO.TextReader reader = System.IO.File.OpenText(@"License\full_license.key");
            p_Key = reader.ReadLine();
            reader = System.IO.File.OpenText(@"License\full_license.lic");
            p_Lic = reader.ReadToEnd();

            //下面只是注册License的其中一中方式
            byte[] licenseBuffer = ASCIIEncoding.ASCII.GetBytes(p_Lic.ToCharArray());
            RasterSupport.SetLicense(licenseBuffer, p_Key);
        }
    }
}
