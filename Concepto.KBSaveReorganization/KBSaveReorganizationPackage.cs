
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;
using System.Security.Principal;
using Artech.Architecture.Common.Events;
using Artech.Architecture.Common.Services;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Wiki;
using Artech.Architecture.BL.Framework.Packages;
using Artech.Common.Diagnostics;
using Artech.Architecture.Common.Packages;
using Microsoft.Practices.CompositeUI.EventBroker;
using System;
using System.Xml.Xsl;
using System.Xml.Linq;
using System.Reflection;
using Artech.Architecture.BL.Framework.Services;
using System.Text.RegularExpressions;

namespace Concepto.Packages.KBSaveReorganization
{

    public class KBSaveReorganizationClass : AbstractPackage, IGxPackageBL

    {
        public override string Name
        {
            get { return "KBSaveReorganization"; }
        }

        public override void PostInitialize()
        {
            base.PostInitialize();
            //CommonServices.Output.Add(new OutputError("KBSaveReorganization was loaded!!", MessageLevel.Information));
        }

        [EventSubscription("event://Genexus/AfterReorganize")]
        public void OnAfterReorganize(object sender, KBModelEventArgs e)
        {
            DateTime now = DateTime.Now;
            CommonServices.Output.AddLine("KBSaveReorganization start");

            KnowledgeBase kB = e.KB;

            KBModel model = e.KBModel;

            string location = "";
            if (kB != null && model != null)
            {
                location = kB.Location;

                string targetPath = kB.DesignModel.Environment.TargetModel.TargetPath;

                string dirKB_targetPath = location + @"\" + targetPath;

                string dirWeb = @"\web\";
                string Reorgtext = dirKB_targetPath + dirWeb + "ReorganizationScript.txt";
                if (!File.Exists(Reorgtext))
                {
                    dirWeb = @"\";
                    Reorgtext = dirKB_targetPath + dirWeb +  "ReorganizationScript.txt";
                }
                string strDateTime = now.ToString("s");
                strDateTime = strDateTime.Replace(":", "");
                string dirReorg2 = dirKB_targetPath +  @"\Reorg_" + strDateTime + @"\";

                string Username = WindowsIdentity.GetCurrent().Name;
                string ReorganizationFileNameWOExtension = "Reorg-" + Username + "-" + strDateTime;

                var invalids = System.IO.Path.GetInvalidFileNameChars();
                ReorganizationFileNameWOExtension = String.Join("_", ReorganizationFileNameWOExtension.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

                string ReorganizationFileName = string.Concat(new string[] {  dirReorg2, ReorganizationFileNameWOExtension, ".sql" });


                string headerReorg = string.Format("/* KBSaveReorganization {0} {1}  */ {2}", DateTime.Now, Username, Environment.NewLine);
                string contentReorg = string.Empty;
                if (File.Exists(Reorgtext))
                {
                    contentReorg = File.ReadAllText(Reorgtext, Encoding.GetEncoding(1252));
                    contentReorg = contentReorg.Replace(System.Environment.NewLine, ";" + System.Environment.NewLine);

                }
                File.WriteAllText(Reorgtext, headerReorg + contentReorg, Encoding.GetEncoding(1252));
                try
                {
                    Directory.CreateDirectory(dirReorg2);
                    File.Copy(Reorgtext, ReorganizationFileName);
                    SaveReorganizationFilesNET(dirKB_targetPath, dirReorg2);
               
                    string ReorganizationHTML = string.Concat(dirReorg2, ReorganizationFileNameWOExtension, ".html");
                    GenerarHTML_IAR(location, ReorganizationHTML);

                    KBModel design = model.GetDesignModel();

                    KBObject kBObject = BlobKBObjectHelper.CreateFileObject(design, ReorganizationFileName, ReorganizationFileNameWOExtension);
                    KBObjectSavePreferences kbsp = new KBObjectSavePreferences();

                    kbsp.AllowSaveInNoDesign = true;
                    kbsp.ForceSave = true;
                    kBObject.Save(kbsp);
              
                   }
                   catch (Exception ex)
                   {
                       CommonServices.Output.AddErrorLine(string.Format("KBSaveReorganization: Error when saving file {0} in KB. Exception : {1}", ReorganizationFileNameWOExtension, ex.Message));
                   }
                CommonServices.Output.AddLine(string.Format("KBSaveReorganization: Reoganization saved in Reorg_{0} folder. ", strDateTime));
            }
        }

        private void SaveReorganizationFilesNET(string dirkb, string dirReorg)
        {
            string[] FileList = new string[] {"gxclasses.dll", "client.exe.config", "log4net.dll", "reor.exe", "Reorganization.dll",
                                                "messages.spa.dll","messages.eng.dll", "Jayrock-JSON.dll" , "runx86.exe", "ReorganizationScript.txt" };

            string dirName = dirReorg;

            foreach (string fileCopy in FileList)
            {
                string sourceFile = Path.Combine(dirkb + @"\web\bin\", fileCopy);
                if (File.Exists(sourceFile))
                {
                    string destinationFile = Path.Combine(dirName, fileCopy);
                    File.Copy(sourceFile, destinationFile, true);
                }

            }

            File.WriteAllText(dirName + @"\__Reorg_force.cmd", "Reor.exe -force " + Environment.NewLine + "pause");
            File.WriteAllText(dirName + @"\__Reorg_force_batch.cmd", "Reor.exe -force -nogui" + Environment.NewLine + "pause");
            File.WriteAllText(dirName + @"\__Reorg_force_recordcount.cmd", "Reor.exe -nogui  -recordcount -noverifydatabaseschema" + Environment.NewLine + "pause");

            File.WriteAllText(dirName + @"\reorgpgm.gen", "");
            

        }



        private void GenerarHTML_IAR(string kbpath, string outputFile)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string gxpath = assemblyPath.Replace(@"Packages", string.Empty);
            
            string iarSource = kbpath + @"\iar_objs.xml";

            string xmlstring = File.ReadAllText(iarSource);
            xmlstring = "<?xml version='1.0' encoding='iso-8859-1'?> <Objects>" + xmlstring + "</Objects>";

            var xml = XDocument.Parse(xmlstring);

            var query = from c in xml.Root.Descendants("Object")
                        select c.Element("ObjNavig").Value;

            foreach (string name in query)
            {
                string xmlFile = kbpath + @"\" + name;
                GenerarHTML(xmlFile, outputFile+".tmp", gxpath);
            }

            string html = File.ReadAllText(outputFile+".tmp");
            Regex rRemScript = new Regex(@"<script[^>]*>[\s\S]*?</script>");
            html = rRemScript.Replace(html, string.Empty);

            Regex rRemKey = new Regex(@"<img.*Key.+?>");
            html = rRemKey.Replace(html, "&raquo;");

            Regex rRemIdxUp = new Regex(@"<img.*IdxAscending.+?>");
            html = rRemIdxUp.Replace(html, "&uarr;");

            Regex rRemIdxDown = new Regex(@"<img.*IdxDescending.+?>");
            html = rRemIdxDown.Replace(html, "&darr;");

            Regex rRemCollapse = new Regex(@"<img.*Collapse.+?>");
            html = rRemCollapse.Replace(html, string.Empty);

            Regex rRemWarning = new Regex(@"<img.*Warning.+?>");
            html = rRemWarning.Replace(html, " Warning:" );

            Regex rRemMeta = new Regex(@"<meta.*.+?>");
            html = rRemMeta.Replace(html, "<meta charset=\"windows-1252\"><style>" + File.ReadAllText(gxpath + @"\gxxml\genexus.css")+"</style>");

            File.WriteAllText(outputFile, html);
            File.Delete(outputFile + ".tmp");
        }
        private static void GenerarHTML(string xmlSource, string outputFile, string gxpath)
        {
            string pathxsl = gxpath + @"gxxml\spec.xsl";

            XmlDocument docsource = new XmlDocument();

            string xmlstring = File.ReadAllText(xmlSource);
            xmlstring = "<?xml version='1.0' encoding='iso-8859-1'?> <ObjSpecs> <gxpath>" + gxpath + "</gxpath>" + xmlstring + "</ObjSpecs>";

            docsource.LoadXml(xmlstring);

            XmlNodeReader xmlr = new XmlNodeReader(docsource);
            MemoryStream ms = new MemoryStream();

            XslCompiledTransform myXslTransform = new XslCompiledTransform();
            XsltSettings xsltSettings = new XsltSettings();
            xsltSettings.EnableDocumentFunction = true;

            if (File.Exists(pathxsl))
            {
                myXslTransform.Load(pathxsl, xsltSettings, new XmlUrlResolver());
                myXslTransform.Transform(xmlr, new XsltArgumentList(), ms);

                ms.Position = 0;

                using (FileStream file = new FileStream(outputFile, FileMode.Append, FileAccess.Write))
                    ms.CopyTo(file);

            }
        }
    }
}



