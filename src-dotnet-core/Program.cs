using System;
using Microsoft.Extensions.Configuration;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Xml.XPath;

namespace packagetotxt
{
    class Program
    {
    	public static IConfigurationRoot Config {get;set;}
    	static Program(){
    		loadConfig();
    	}
        
        static void Main(string[] args)
        {
        	string outputDir = Config != null ? Config["defaultOutputDir"] : "";
            outputDir = string.IsNullOrEmpty(outputDir) ? Directory.GetCurrentDirectory() : outputDir;            
            string inputDir = Directory.GetCurrentDirectory();
            string dir = null;

            string componentFileName = Config != null ? Config["outputFileName"] : null;
            string outputFileExtension =Config != null ? Config["outputFileExtension"] : null;
            string pattern = Config != null ? Config["defaultSearchPattern"] : null;
            Console.WriteLine(pattern);
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            bool optionStart = false;
            string prevArg = "";
            foreach(string arg in args)
            {
                if (optionStart == false && arg.StartsWith("--") && string.IsNullOrEmpty(prevArg))
                {
                    prevArg = arg;
                    optionStart = true; continue;
                }
                if(optionStart == true)
                {
                    arguments.Add(prevArg.ToLower(), arg);
                    optionStart = false;
                    prevArg = "";
                    continue;
                }
              
            }
            outputDir = arguments.ContainsKey("--outputdir") ? arguments["--outputdir"] : outputDir;
            inputDir = arguments.ContainsKey("--inputdir") ? arguments["--inputdir"] : inputDir;
            dir = arguments.ContainsKey("--dir") ? arguments["--dir"] : dir;
            outputDir = !string.IsNullOrEmpty(dir) ? dir : outputDir;
            inputDir = !string.IsNullOrEmpty(dir) ? dir : inputDir;

            componentFileName = arguments.ContainsKey("--outputfilename") ? arguments["--outputfilename"] : componentFileName;
            pattern = arguments.ContainsKey("--pattern") ? arguments["--pattern"] : pattern;
            outputFileExtension = arguments.ContainsKey("--outputextension") ? arguments["--outputextension"] : outputFileExtension;


            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))  Console.WriteLine("Directory {0} does not exists", outputDir);
            if (!Directory.Exists(inputDir)) Console.WriteLine("Directory {0} does not exists", inputDir);


            if (!Directory.Exists(inputDir) || !Directory.Exists(outputDir))
            return;

            int c = 1;
            string[] files = System.IO.Directory.GetFiles(inputDir, pattern, System.IO.SearchOption.TopDirectoryOnly);
            List<string> filesGenerated = new List<string>();
           foreach (string file in files )
            {
                string txtVersion = convertToTxt(file);

                if (!string.IsNullOrEmpty(txtVersion))
                {
                    byte[] bts = Encoding.UTF8.GetBytes(txtVersion);
                    string componentFileWithNumber = componentFileName + (files.Length > 1 ? c.ToString() : "");
                    string fullComponentsFilePath = (!string.IsNullOrEmpty(outputDir) ? outputDir + "\\" : "") + componentFileWithNumber + outputFileExtension;

                    using (System.IO.FileStream physicalFile = System.IO.File.Create(fullComponentsFilePath))
                    {
                        physicalFile.Write(bts, 0, bts.Length);
                        physicalFile.Flush();
                        filesGenerated.Add(componentFileWithNumber + outputFileExtension);
                    }
                }
                c += 1;
            }
            if (filesGenerated.Count > 0) Console.WriteLine("files " + string.Join(",", filesGenerated) + " generated in " + outputDir);
           
        }
        static string convertToTxt(string filePath)
        {
            string txtContentResult = "";
            List<string> typesResult = new List<string>();
            XmlDocument doc = new XmlDocument();
            FileStream fs = File.OpenRead(filePath);
            try
            {
                doc.Load(fs);
            }catch(Exception err) { Console.WriteLine("Not an xml file: "+filePath); return ""; }
            XmlElement root = doc.DocumentElement;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(root.OwnerDocument.NameTable);
            nsmgr.AddNamespace("x", root.OwnerDocument.DocumentElement.NamespaceURI);

            XmlNodeList types =root.SelectNodes("//x:types", nsmgr);

            if(types != null)
                foreach (XmlNode node in types)
                {
                    
                    XmlNode nameNode = node.SelectSingleNode("x:name",nsmgr);
                    string typeName = (nameNode != null) ? nameNode.InnerText : "";
                    string memberList = "";
                    XmlNodeList members = node.SelectNodes("x:members",nsmgr);
                    if(members != null)
                        foreach(XmlNode member in members)
                        { memberList += "\t\t-" + member.InnerText+"\r\n";}

                    typesResult.Add(typeName + ":\r\n" + memberList);

                }
            
            txtContentResult = string.Join("\r\n", typesResult);
            return txtContentResult;
            
        }
        static void loadConfig(){

            try
            {
                var builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json");
                Config = builder.Build();
            }catch(Exception err) { Console.WriteLine("No appsettings.json or bad format"); }
        }

    }
}
