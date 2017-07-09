using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CreateVideoFromHLS
{
    public partial class Form1 : Form
    {
        ArrayList tsFiles = new ArrayList();

        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {                
            backgroundWorker1.RunWorkerAsync();        
        }

        private void doWork()
        {
            string m3u8File = textBox1.Text;
            string fileNameWithoutM3u8 = m3u8File.Replace("index.m3u8", "");

            var result = GetFileViaHttp(m3u8File.ToString());
            string str = Encoding.UTF8.GetString(result);
            string[] words = str.Split('\n');

            Regex r = new Regex(@"(.*)\.ts");
            MatchCollection collectionCount = r.Matches(str);
            progressBar1.Maximum = collectionCount.Count;

            int myCounter = 0;
            foreach (var line in collectionCount)
            {
                ++myCounter;
                string tsFileName = line.ToString();
                string tsFullPathFileName = fileNameWithoutM3u8 + tsFileName;
                tsFiles.Add(tsFileName);
                textBox2.AppendText(tsFileName + " indiriliyor..." + Environment.NewLine);
                downloadFile(tsFullPathFileName, tsFileName);
                progressBar1.Value = myCounter;
            }

            /*for (Int64 i = 0; i < words.Length; i++)
            {
                string line = words[i].ToString() + "\n\n";
                
                if (r.IsMatch(line.ToString()))
                {
                    MatchCollection myMatchCollection = r.Matches(line);
                    string tsFileName = myMatchCollection[0].ToString();
                    string tsFullPathFileName = fileNameWithoutM3u8 + tsFileName;
                    tsFiles.Add(tsFileName);
                    textBox2.AppendText(tsFileName + " indiriliyor..." + Environment.NewLine);
                    downloadFile(tsFullPathFileName, tsFileName);
                    progressBar1.Value = Convert.ToInt32(i);             
                }
                
                label1.Text = i.ToString();
            }*/

            concatTsFiles(tsFiles);
        }

        public byte[] GetFileViaHttp(string url)
        {
            using (WebClient client = new WebClient())
            {
                return client.DownloadData(url);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            doWork();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox2.ScrollBars = ScrollBars.Both;
        }

        private void downloadFile(string fullPath, string fileName)
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(new System.Uri(fullPath), fileName);
            }
        }

        static void FFmpegConversion(string command)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = "ffmpeg\\bin\\ffmpeg.exe";
            proc.StartInfo.Arguments = command.ToString();
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;

            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;

            if (!proc.Start())
            {
                Console.WriteLine("Error!");
                return;
            }
            StreamReader reader = proc.StandardError;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
            proc.Close();
        }

        static void concatTsFiles(ArrayList tsFiles)
        {
            string concatString = "";
            string ffmpegCommand = "";
            foreach (string file in tsFiles)
            {
                concatString = concatString + file + "|";
            }
            concatString = concatString.Remove(concatString.Length - 1, 1);
            ffmpegCommand = "-i \"concat:" + concatString + "\" -c copy output.ts";
            Console.WriteLine(ffmpegCommand);
            FFmpegConversion(ffmpegCommand);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            textBox2.AppendText("BİTTİ!");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (string file in tsFiles)
            {
                if (Directory.Exists(Path.GetDirectoryName(file)))
                {
                    File.Delete(file);
                }
            }
        }
    }
}
