﻿// (C) Copyright 2023 by Autodesk, Inc. 
//
// Permission to use, copy, modify, and distribute this software
// in object code form for any purpose and without fee is hereby
// granted, provided that the above copyright notice appears in
// all copies and that both that copyright notice and the limited
// warranty and restricted rights notice below appear in all
// supporting documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK,
// INC. DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL
// BE UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is
// subject to restrictions set forth in FAR 52.227-19 (Commercial
// Computer Software - Restricted Rights) and DFAR 252.227-7013(c)
// (1)(ii)(Rights in Technical Data and Computer Software), as
// applicable.
//

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using DesignAutomationFramework;
using Newtonsoft.Json;
using Revit.IFC.Import;
using Revit.IFC.Import.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitImportAddin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Command : IExternalDBApplication
    {
        public ExternalDBApplicationResult OnStartup(ControlledApplication application)
        {
            DesignAutomationBridge.DesignAutomationReadyEvent += HandleDesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
        {
            return ExternalDBApplicationResult.Succeeded;
        }

        [Obsolete]
        public void HandleApplicationInitializedEvent(object sender, Autodesk.Revit.DB.Events.ApplicationInitializedEventArgs e)
        {
            var app = sender as Autodesk.Revit.ApplicationServices.Application;
            DesignAutomationData data = new DesignAutomationData(app, "InputFile.rvt");
            this.DoExport(data);
        }

        private void HandleDesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
        {
            LogTrace("Design Automation Ready event triggered...");
            // Hook up the CustomFailureHandling failure processor.
            Application.RegisterFailuresProcessor(new ExportIfcFailuresProcessor());

            e.Succeeded = true;
            e.Succeeded = this.DoExport(e.DesignAutomationData);
        }

        private bool DoExport(DesignAutomationData data)
        {
            if (data == null)
                return false;

            Application app = data.RevitApp;
            if (app == null)
            {
                LogTrace("Error occured");
                LogTrace("Invalid Revit App");
                return false;
            }

            LogTrace("Creating ouput folder...");

            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "output");
            if (!Directory.Exists(outputPath))
            {
                try
                {
                    Directory.CreateDirectory(outputPath);
                }
                catch (Exception ex)
                {
                    this.PrintError(ex);
                    return false;
                }
            }

            LogTrace(string.Format("Output Path: `{0}`", outputPath));

            var folder = new DirectoryInfo(Directory.GetCurrentDirectory());

            if (folder.Exists)
            {
                LogTrace("Moving files into `output` folder");

                var ifcFiles = folder.GetFiles("*.ifc");
                ifcFiles.ToList().ForEach(f => File.Move(f.FullName, Path.Combine(outputPath, f.Name)));

                var IfcLinkFiles = folder.GetFiles("*.rvt");
                IfcLinkFiles.ToList().ForEach(f => File.Move(f.FullName, Path.Combine(outputPath, f.Name)));
            }

            LogTrace("Opening the `host.rvt` ...");

            var hostModelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(Path.Combine(outputPath, "host.rvt"));
            var hostDoc = app.OpenDocumentFile(hostModelPath, new OpenOptions());
            if (hostDoc == null)
            {
                LogTrace("Error occured");
                LogTrace("Invalid Revit DB Document");
                return false;
            }

            LogTrace("Linking IFC...");

            var outputFolder = new DirectoryInfo(outputPath);
            var ifcFilenames = outputFolder.GetFiles("*.ifc").Select(f => f.Name);

            IDictionary<string, string> options = new Dictionary<string, string>();
            options["Action"] = "Link";   // default is Open.
            options["Intent"] = "Reference"; // This is the default.

            foreach (var ifcName in ifcFilenames)
            {
                LogTrace($"Linkuing `{ifcName}` ...");
                try
                {
                    string fullIFCFileName = Path.Combine(outputPath, ifcName);
                    Importer importer = Importer.CreateImporter(hostDoc, fullIFCFileName, options);

                    importer.ReferenceIFC(hostDoc, fullIFCFileName, options);
                }
                catch (Exception ex)
                {
                    LogTrace("Exception in linking IFC document. " + ex.Message);
                    if (Importer.TheLog != null)
                        Importer.TheLog.LogError(-1, ex.Message, false);

                    return false;
                }
                finally
                {
                    if (Importer.TheLog != null)
                        Importer.TheLog.Close();
                    if (IFCImportFile.TheFile != null)
                        IFCImportFile.TheFile.Close();
                }

                LogTrace($"The `{ifcName}` linked...");
            }

            LogTrace("Saving changes into `host.rvt` ...");

            ModelPath path = ModelPathUtils.ConvertUserVisiblePathToModelPath(Path.Combine(outputPath, "output.rvt"));
            hostDoc.SaveAs(path, new SaveAsOptions());

            LogTrace("IFC link completed...");

            return true;
        }

        private void PrintError(Exception ex)
        {
            LogTrace("Error occured");
            LogTrace(ex.Message);

            if (ex.InnerException != null)
                LogTrace(ex.InnerException.Message);
        }

        /// <summary>
        /// This will appear on the Design Automation output
        /// </summary>
        private static void LogTrace(string format, params object[] args)
        {
#if DEBUG
            System.Diagnostics.Trace.WriteLine(string.Format(format, args));
#endif
            System.Console.WriteLine(format, args);
        }

    }
}
