using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;    //http://www.ndesk.org/Options
using Ionic.Zip;
// Ionic.Zip.dll を利用。
//   http://dotnetzip.codeplex.com/

namespace GpxcApplication
{
    class Gpxc
    {
        static List<string> get_target_files(List<string> args)
        {
            List<string> ret = new List<string>();
            foreach(string pattern in args)
            {
                
                string dirname = System.IO.Path.GetDirectoryName(pattern);
                if (dirname == null || dirname == "") dirname = ".";
                string filename_pattern = System.IO.Path.GetFileName(pattern);
                try
                {
                    string[] filenames = System.IO.Directory.GetFiles(dirname, filename_pattern);
                    for (var i = 0; i < filenames.Length; i++)
                    {
                        ret.Add(filenames[i]);
                    }
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    //引数に指定したフォルダが無かったらエラーとなる。
                    //このエラーは、単なる指定ミスとして無視する。
                    return new List<string>();
                }
            }
            return ret;
        }
  
        static void convert_gpx(string file_path, GpxcOptions options)
        {
            string new_gpx_file_path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(file_path),"gpxc." + System.IO.Path.GetFileName(file_path));
            if(!options.Silent)Console.WriteLine("{0}を処理中...",file_path);
            using(System.IO.StreamReader sr = new System.IO.StreamReader(file_path,System.Text.Encoding.GetEncoding("utf-8"))){
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(new_gpx_file_path, false, System.Text.Encoding.GetEncoding("utf-8")))
                {
                    while (sr.EndOfStream==false)
                    {
                        sw.WriteLine(LibGpx.decode(sr.ReadLine()));
                    };
                }
              
            }
            if (options.Nuvi)
            {
                LibGpx.conv_nuvi(new_gpx_file_path, nuvi: true);
            }
            else if (options.Nuvi2)
            {
                LibGpx.conv_nuvi(new_gpx_file_path, nuvi2: true);
            }
        }
        
        static void Main(string[] args)
        {
            GpxcOptions options = new GpxcOptions();
            List<string> extraCommandLineArgs = new List<string>();
            var p = new OptionSet()
            .Add("v|version", dummy => { ShowVersion(); Environment.Exit(0); })
                .Add("?|h|help", dummy => { ShowHelp(); Environment.Exit(0); })
                .Add("silent", v => { if (v != null) options.Silent = true; })
                .Add("n|nuvi", r => { if (r != null) options.Nuvi = true; })
                .Add("n2|nuvi2", r => { if (r != null) options.Nuvi2 = true; });
                
            //解析
            try
            {
                // extraCommandLineArgs には、上記オプションを取り除いた残りの引数が入る
                extraCommandLineArgs = p.Parse(Environment.GetCommandLineArgs());
            }
            catch (Exception ex)
            {
                Console.WriteLine("オプションの解析が失敗しました。\n" + ex.Message);
                Environment.Exit(0);
            }

            extraCommandLineArgs.RemoveAt(0);
            List<string> target_files = get_target_files(extraCommandLineArgs);
            foreach(string each_target_file in target_files)
            {
                switch (System.IO.Path.GetExtension(each_target_file).ToUpper())
                {
                    case ".GPX":
                        convert_gpx(each_target_file,options);
                        continue;
                    case ".ZIP":
                        using (ZipFile zip = ZipFile.Read(each_target_file))
                        {
                            foreach (ZipEntry entry in zip)
                            {
                                if (System.IO.Path.GetExtension(entry.FileName).ToUpper() == ".GPX")
                                {
                                    string dirname = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(each_target_file),System.IO.Path.GetFileNameWithoutExtension(each_target_file));
                                    if (!System.IO.Directory.Exists(dirname))
                                    {
                                        System.IO.Directory.CreateDirectory(dirname);
                                    }
                                    string gpx_filepath = System.IO.Path.Combine(dirname , entry.FileName);
                                    entry.Extract(dirname, ExtractExistingFileAction.OverwriteSilently);
                                    convert_gpx(gpx_filepath,options);
                                    System.IO.File.Delete(gpx_filepath);
                                }
                            }
                        }
                        continue;
                    default:
                        continue;
                }
            }

        }
        //バージョン表示
        static void ShowVersion()
        {
            Console.WriteLine(@"2012.09.05版 初版リリース
2013.01.14版 < > &を含む記述があると、GPSrでキャッシュの数が減って表示される問題に対処。
2013.01.20版 nuvi,nuvi2オプションを導入
2013.01.25版 サロゲートペアを無視するよう変更
2013.01.26版 zipファイルに対応
2013.01.28版 処理コードのミスマッチによりnuviオプションが効かない場合に対処。
2013.01.30版 zlib1.dllがロードできないと言う不具合に対処
2013.02.24版 C#で再作成");
        }
        //ヘルプの表示//
        static void ShowHelp()
        {
            Console.WriteLine(@"使い方)
gpxc ファイル [--nuvi|--nuvi2]

GPXファイルをGPSrでも日本語が読めるように変換するツールです。
GPXファイルの他に、GPXファイルの入ったZIPファイルも指定できます。
オプションとして、nuvi,nuvi2を指定できます。

・nuviオプション
GPSrでのキャッシュの見出しが、GCコードではなくキャッシュ名になります。
(例)
GC1ZMBH → Walk in the Park Yoyogi 

・nuvi2オプション
GPSrでのキャッシュの見出しが、GCコード + キャッシュ名になります。
GC1ZMBH → GC1ZMBH Walk in the Park Yoyogi

・ファイルの指定
ファイルには、フォルダ名、ファイル名を指定する事もできます。
 (例) gpxfiles\2012394.gpx
また、複数のファイルを並べて指定する事もできます。
ファイルのファイル名部分には、ワイルドカード(*)などが使えます。

* 任意の0個以上の文字
? 任意の一文字");
        }
    }
}
