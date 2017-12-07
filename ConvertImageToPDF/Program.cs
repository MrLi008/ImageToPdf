using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leadtools;
using Leadtools.Codecs;
using Leadtools.ImageProcessing;
using Leadtools.ImageProcessing.Core;
using Leadtools.Forms.Ocr;

namespace ConvertImageToPDF
{
    class Program
    {
        private static IOcrEngine ocrEngine;
        static void Main(string[] args)
        {
            System.Threading.Thread.GetDomain().UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            
            run(args);
        }
        private static void run(string[] args)
        {
           try
            {
                process(args);
            }
            catch (Exception e)
            {
                Console.WriteLine("..." + e);
                //Console.WriteLine(".....哈哈, 报错啦");
            }
            finally
            {
                //Console.WriteLine("stop");
            }
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
           // Console.WriteLine("error");
            //Console.ReadKey();
            Environment.Exit(-1);
        }
        private static void process(string[] args)
        {
            // args = new string[2] { @"D:\ocrengine\test", @"D:\ocrengine\test" };
            //args = new string[1] { @"1510583968646.jpg" };
            //args = new string[2]
            //{
            //    @"d:\ocrengine\test",
            //    @"d:\ocrengine\test"
            //};
            for (int i = 0; i < args.Length; i++)
            {
               // Console.WriteLine("args: " + args[i]);
            }
            if (args.Length < 1)
            {
                Console.WriteLine(@"provide at least two args: new filename and imagefilename or imagefileroot");
                return;
            }
            // begin
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

           
            //Console.WriteLine(@"load all Pics");
            List<string> files = new List<string>();
            if (args.Length > 1 && Directory.Exists(args[1]))
            {
                 files = Directory.GetFiles(args[1], "*.JPG", SearchOption.AllDirectories).ToList<string>();
            }
            else 
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (File.Exists(args[i]) && IsPicture(args[i])){
                        files.Add(args[i]);
                    }
                }

            }
            int filelength = files.ToArray<string>().Length;
            Console.WriteLine("file count: " + filelength);
            //for (int i=0; i<filelength; i++)
            //{
            //    Console.WriteLine("the file: " + files[i]);
            //    try
            //    {
            //        AutoRecongizeManage(files[i] + ".pdf", files[i]);

            //        Console.WriteLine("success...");
            //    }catch (AccessViolationException e)
            //    {
            //        Console.WriteLine("faild!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + e);
            //    }
            //    catch(OcrException e)
            //    {
            //        Console.WriteLine("faild!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + e);
            //    }catch(IOException e)
            //    {
            //        Console.WriteLine("faild!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + e);
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine("faild!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + e);
            //    }
            //    finally
            //    {
            //        Console.WriteLine("end.." + files[i] + ".pdf-----------------" + i);
            //    }
            //}
            int perImage = 1;
            try
            {
                IOcrDocument ocrDocument;
                for (int i = 0; i <= filelength / perImage; i++)
                {
                    int index = 0;
                    int rest = filelength - i * perImage;

                    ocrDocument = ocrEngine.DocumentManager.CreateDocument();
                    if (rest > perImage)
                    {
                        rest = perImage;
                    }
                    for (int j = 0; j < rest; j++)
                    {
                        index = i * perImage + j;
                        if (index >= filelength)
                        {
                            break;
                        }
                       // Console.WriteLine("at..." + index);
                        exe_ocr(files[index], ocrDocument);
                    }
                    Console.WriteLine("Begin ocr engine ....." + files[index] + i * perImage + ".html");
                    try
                    {
                        ocrDocument.Save(files[index] + i * perImage + ".html", Leadtools.Forms.DocumentWriters.DocumentFormat.Html, null);
                       // Console.WriteLine("finish this group image");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error when saving image as pdf type" + e);
                        // Console.ReadKey();
                    }
                    finally
                    {
                      //  Console.WriteLine("...." + i);
                        ocrDocument.Dispose();
                    }

                }

            }
            catch(Exception e)
            {
             //   Console.WriteLine("Error when saving image as pdf type" + e);
            }
            finally
            {
            //    Console.WriteLine("....");
            }
            try
            {
                ocrEngine.Shutdown();
           //     Console.Write("end ocr engine with result file: " + args[0] + ".pdf\nPress any key to continue...");
                //  Console.ReadKey();
            }
            catch (Exception e)
            {
            //    Console.WriteLine("err with shutdown");
            }
            finally
            {
                if (ocrEngine != null)
                {
                    ocrEngine.Dispose();
                }
            }
        }
        public static void exe_ocr(string filename, IOcrDocument ocrDocument)
        {
            Console.WriteLine(filename);
            RasterCodecs rasterCodecs = null;
            RasterImage rasterImage = null;
            try
            {
                rasterCodecs = new RasterCodecs();
                rasterImage = rasterCodecs.Load(filename);
                AutoBinarizeCommand command = new AutoBinarizeCommand();
                command.Run(rasterImage);

                IOcrPage page = ocrDocument.Pages.AddPage(rasterImage, null);
                if (page != null)
                {
                    page.UpdateNativeFillMethod();
                    page.Recognize(null);
                }

                // ocrDocument.Pages.zon(NativeOcrZoneType.AutoGraphic);
            }catch(Exception e)
            {
            //    Console.WriteLine("add image faild: " + e);
            }
            finally
            {
                if (rasterCodecs != null)
                {
                    rasterCodecs.Dispose();
                }
                if (rasterImage != null)
                {
                    rasterImage.Dispose();
                }
            }
        }
        private static void AutoRecongizeManage(string outfile, string infile)
        {
            try
            {
                ocrEngine.AutoRecognizeManager.Run(
                    infile,
                    outfile,
                    null,
                    Leadtools.Forms.DocumentWriters.DocumentFormat.Pdf,
                    null);
            }
            catch (AccessViolationException e)
            {
           //     Console.WriteLine("faild!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + e);
            }
            catch (OcrException e)
            {
          //      Console.WriteLine("faild!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + e);
            }
            catch (IOException e)
            {
           //     Console.WriteLine("faild!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + e);
            }
            catch (Exception e)
            {
          //      Console.WriteLine("faild!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + e);
            }
        }
    
        public static void SetLicense()
        {
            string p_Lic, p_Key;
            //1、读取License文件
            //License文件夹被我放到Debug文件夹和Release文件夹里，可以根据需要变更文件路径
            System.IO.TextReader reader = System.IO.File.OpenText(@"D:\ocrengine\License\full_license.key");
            p_Key = reader.ReadLine();
            reader = System.IO.File.OpenText(@"D:\ocrengine\License\full_license.lic");
            p_Lic = reader.ReadToEnd();

            //下面只是注册License的其中一中方式
            byte[] licenseBuffer = ASCIIEncoding.ASCII.GetBytes(p_Lic.ToCharArray());
            RasterSupport.SetLicense(licenseBuffer, p_Key);
        }
        private static bool IsPicture(string filePath)//filePath是文件的完整路径   
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(fs);
                string fileClass;
                byte buffer;
                byte[] b = new byte[2];
                buffer = reader.ReadByte();
                b[0] = buffer;
                fileClass = buffer.ToString();
                buffer = reader.ReadByte();
                b[1] = buffer;
                fileClass += buffer.ToString();


                reader.Close();
                fs.Close();
              //  Console.WriteLine(fileClass);
                if (fileClass == "255216" || fileClass == "7173" || fileClass == "13780")//255216是jpg;7173是gif;6677是BMP,13780是PNG;7790是exe,8297是rar   
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
