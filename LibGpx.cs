using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace GpxcApplication
{
    class LibGpx
    {
        public static void conv_nuvi(string file_name,bool nuvi=false,bool nuvi2=false){
            string tmp_filename = file_name+".tmp";
            if (System.IO.File.Exists(tmp_filename)) System.IO.File.Delete(tmp_filename);
            System.IO.File.Move(file_name,tmp_filename);
            using(System.IO.StreamReader sr = new System.IO.StreamReader(tmp_filename,System.Text.Encoding.GetEncoding("utf-8"))){
                 using (System.IO.StreamWriter sw = new System.IO.StreamWriter(file_name, false, System.Text.Encoding.GetEncoding("utf-8")))
                {
                     List<string> lines = new List<string> { };
                     string name = null;
                     string urlname;
                     bool wpt = false;
                     while (sr.EndOfStream==false)
                     {
                         string line = sr.ReadLine();
                         if(line.IndexOf("<wpt") > 0)wpt = true;
                         var match = Regex.Match(line,@"<name>(.*)</name>");
                         if(match.Groups.Count > 1 && wpt){
                             name = match.Groups[1].Value;
                         }else{
                             match = Regex.Match(line,@"<urlname>(.*)</urlname>");
                             if(match.Groups.Count > 1 && name != null){
                                 urlname = match.Groups[1].Value;
                                 if (nuvi)
                                 {
                                     lines.Insert(0, "    <name>" + urlname + "</name>");
                                 }
                                 else if (nuvi2)
                                 {
                                     lines.Insert(0, "    <name>" + name + " "+urlname+"</name>");
                                 }
                                 lines.Add("    <urlname>" + name + "</urlname>");

                                 foreach(string each_line in lines){
                                    sw.WriteLine(each_line);
                                 }
                                 lines.Clear();
                                 name = null;
                             }
                             else if(name != null)lines.Add(line);
                             else sw.WriteLine(line);
                         }
                     }
                     sw.Flush();
                }
            }
            System.IO.File.Delete(tmp_filename);
        }
   

        public static string decode(string str)
        {
            //return System.Net.WebUtility.HtmlDecode(str);

            int index_of_amp = str.IndexOf("&amp;", StringComparison.OrdinalIgnoreCase);
            
            if (index_of_amp == 0)
            {
                var match = Regex.Match(str, @"(.*?)(&amp;#(?:(\d*?)|(?:[xX]([0-9a-fA-F]{4})));)(.*)$"); //GC3XFVGで延々ループ

                if (match.Groups.Count > 1)
                {
                    return match.Groups[1].Value + decode_main(match.Groups[3].Value, match.Groups[4].Value) + decode(match.Groups[5].Value);
                }
                else
                {
                    return str;
                }
            }
            else if (index_of_amp > 0)
            {
                return str.Substring(0, index_of_amp) + decode(str.Substring(index_of_amp));
            }
            else
            {
                return str;
            }

        }
        //10進数の場合: arg1に数字:26397
        //16進数の場合: arg1はnull,argsに16進 例:671d
        //参考： http://dobon.net/vb/dotnet/string/getencoding.html
        // ポイント： GPXのEntityは、文字をUnicodeの文字コードの文字列に変えている。
        //            よって、一旦Unicodeのbyte配列に変える。それをutf8に変換。

        static string decode_main(string arg1, string arg2)
        {
            Byte[] temp_bytes;
            Byte[] return_bytes = new Byte[2];
            System.Text.Encoding src = System.Text.Encoding.Unicode;
            System.Text.Encoding dest = System.Text.Encoding.UTF8;
            int unicode_code;
            if (arg1.ToString() != "")
            {
                unicode_code = int.Parse(arg1);
            }
            else
            {
                unicode_code = Convert.ToInt32(arg2, 16);
            }

            //int[] skip_codes = { 62,60,38 };    //処理しないコード
            //if (skip_codes.Contains(unicode_code))
            if (unicode_code == 62 || unicode_code == 60 || unicode_code == 38)
            {
                return "&amp;#" + unicode_code.ToString() + ";";
            }
            else if ((0xD800 <= unicode_code && unicode_code <= 0xDBFF) ||	//上位サロゲート
           (0xDC00 <= unicode_code && unicode_code <= 0xDFFF))	//下位サロゲート
            {
                return "";
            }
            else
            {
                temp_bytes = BitConverter.GetBytes(unicode_code);

                return_bytes[0] = temp_bytes[0];
                return_bytes[1] = temp_bytes[1];

                byte[] utf8_byte = System.Text.Encoding.Convert(src, dest, return_bytes);
                return System.Text.Encoding.UTF8.GetString(utf8_byte);
            }
        }
    }
}
