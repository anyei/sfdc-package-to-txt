using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace PackageToTxt
{
    /*
     * Created By : anyei
     * Created Date : 7/31/2017
     * Github: https://github.com/anyei/sfdc-package-to-txt
     * By a given sfdc package xml file, generates a txt file with a list of the types members.
     * For the people that doesn't like to read the package file in the xml format.
     */
    class Program
    {
        static void Main(string[] args)
        {
            string outputDir = getSetting("defaultOutputDir");
            
            string inputDir = Environment.CurrentDirectory;
            outputDir = string.IsNullOrEmpty(outputDir) ? Environment.CurrentDirectory : outputDir;

            string componentFileName = getSetting("outputFileName");
            string outputFileExtension = getSetting("outputFileExtension");
            string pattern = getSetting("defaultSearchPattern");

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
                byte[] bts = Encoding.UTF8.GetBytes(txtVersion);
                string componentFileWithNumber = componentFileName + (files.Length > 1 ? c.ToString() : "");
                string fullComponentsFilePath = (!string.IsNullOrEmpty(outputDir) ? outputDir + "\\" : "") + componentFileWithNumber+outputFileExtension;

                using (System.IO.FileStream physicalFile = System.IO.File.Create(fullComponentsFilePath))
                {
                    physicalFile.Write(bts, 0, bts.Length);
                    physicalFile.Flush();
                    physicalFile.Close();
                    filesGenerated.Add(componentFileWithNumber+outputFileExtension);
                }
                c += 1;
            }
            if (filesGenerated.Count > 0) Console.WriteLine("files " + string.Join(",", filesGenerated) + " generated in " + outputDir);
           
        }
        static string convertToTxt(string filePath)
        {
            string txtContentResult = "";
            List<string> typesResult = new List<string>();
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(filePath);
            XmlElement root = doc.DocumentElement;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(root.OwnerDocument.NameTable);
            nsmgr.AddNamespace("x", root.OwnerDocument.DocumentElement.NamespaceURI);

            XmlNodeList types = root.SelectNodes("//x:types", nsmgr);

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
        static string getSetting(string settingName)
        {
            return System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(settingName) ?
                System.Configuration.ConfigurationManager.AppSettings[settingName] : "";
        }
    }
}
