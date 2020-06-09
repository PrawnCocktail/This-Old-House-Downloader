using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace thisoldhouse
{
    class Program
    {
        static void Main(string[] args)
        {
            checkfFmpeg();

            //get url from console
            Console.WriteLine("Please enter the video url.");
            string url = Console.ReadLine();


            //get embed link from main url
            var web = new HtmlWeb();
            var doc = web.Load(url);
            string embedlink = doc.DocumentNode.SelectSingleNode("//iframe").Attributes["src"].Value;


            //follow link and get location of actual player url
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(embedlink);
            request.AllowAutoRedirect = false;
            var response = request.GetResponse();
            string location = response.Headers["Location"];


            //open player url and get the m3u8 link
            doc = web.Load(location);
            string m3u8Link = doc.DocumentNode.SelectSingleNode("//script[contains(.,'m3u8?expand')]").InnerText;
            m3u8Link = m3u8Link
                .Split(new string[] { "src : UrlDimensionsParser.parse(container.offsetWidth, container.offsetHeight, '" }, StringSplitOptions.None)[1]
                .Split(new string[] { "')," }, StringSplitOptions.None)[0];


            //get page title as video name and make sure its valid
            string title = doc.DocumentNode.SelectSingleNode("//head/title").InnerText.Replace("Zype | ", "");
            title = MakeValidFileName(title);


            //download the video with ffmpeg
            if (File.Exists("ffmpeg.exe"))
            {
                Console.Write("Downloading: " + title);

                Process ffmpeg = new Process();
                ffmpeg.StartInfo.FileName = "ffmpeg.exe";
                ffmpeg.StartInfo.Arguments = "-i \"" + m3u8Link + "\" -c copy \"" + title + ".mp4\"";

                //uncommment to hide ffmpeg output
                //ffmpeg.StartInfo.UseShellExecute = false;
                //ffmpeg.StartInfo.CreateNoWindow = true;

                ffmpeg.Start();
                ffmpeg.WaitForExit();
            }
            else
            {
                Console.WriteLine("ffmpeg missing, Cant download video.");
            }

            Console.Write("Download Finished.");
            Console.WriteLine("Exiting in 10 seconds.");
            Thread.Sleep(10000);
            Environment.Exit(0);
        }

        static void checkfFmpeg()
        {
            if (!File.Exists("ffmpeg.exe"))
            {
                Console.WriteLine("Downloading FFMPEG");
                using (WebClient client = new WebClient())
                {
                    string json = client.DownloadString("https://ffbinaries.com/api/v1/version/latest");
                    var ffmpegResult = JsonConvert.DeserializeObject<ffmpeg.Json>(json);

                    client.DownloadFile(ffmpegResult.bin.windows32.ffmpeg, "ffmpeg.zip");

                    string path = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) + "\\";

                    if (File.Exists(path + "ffmpeg.zip"))
                    {
                        Console.WriteLine("Extracting FFMPEG");
                        ZipFile.ExtractToDirectory(path + "ffmpeg.zip", path);

                        if (Directory.Exists(path + "__MACOSX"))
                        {
                            Directory.Delete(path + "__MACOSX", true);
                        }

                        File.Delete(path + "ffmpeg.zip");
                    }
                }
                Console.WriteLine("FFMPEG Download Finished.");
            }
            else
            {
                Console.WriteLine("FFMPEG Found.");
            }
        }

        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "");
        }
    }
}
