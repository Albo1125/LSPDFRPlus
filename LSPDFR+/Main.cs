using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Reflection;


namespace LSPDFR_
{
    internal class Main : Plugin
    {

        public Main()
        {

        }


        public override void Finally()
        {

        }


        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
            Game.LogTrivial("LSPDFR+ " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ", developed by Albo1125, has been initialised.");
            Game.LogTrivial("Go on duty to start LSPDFR+.");
            Albo1125.Common.UpdateChecker.VerifyXmlNodeExists(PluginName, FileID, DownloadURL, Path);
            Albo1125.Common.DependencyChecker.RegisterPluginForDependencyChecks(PluginName);

        }

        internal static Version Albo1125CommonVer= new Version("6.6.4.0");
        internal static Version MadeForGTAVersion = new Version("1.0.1604.1");
        internal const float MinimumRPHVersion = 0.51f;
        internal static string[] AudioFilesToCheckFor = new string[] { };
        internal static string[] OtherFilesToCheckFor = new string[] {  }; //"Plugins/LSPDFR/LSPDFR+/CourtCases.xml"
        internal static Version RAGENativeUIVersion = new Version("1.6.3.0");
        internal static Version MadeForLSPDFRVersion = new Version("0.4.4");

        internal const string DownloadURL = "https://www.lcpdfr.com/files/file/11930-lspdfr-improved-pursuit-ai-better-traffic-stops-court-system/";

        internal const string FileID = "11930";

        internal const string PluginName = "LSPDFR+";
        internal const string Path = "Plugins/LSPDFR/LSPDFR+.dll";

        internal static string[] ConflictingFiles = new string[] { "Plugins/LSPDFR/AutoPursuitBackupDisabler.dll", "Plugins/LSPDFR/SaferChasesRPH.dll" };

        static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
                Albo1125.Common.UpdateChecker.InitialiseUpdateCheckingProcess();
                if (Albo1125.Common.DependencyChecker.DependencyCheckMain(PluginName, Albo1125CommonVer, MinimumRPHVersion, MadeForGTAVersion, MadeForLSPDFRVersion, RAGENativeUIVersion, AudioFilesToCheckFor, OtherFilesToCheckFor))
                {
                    Albo1125.Common.DependencyChecker.CheckIfThereAreNoConflictingFiles(PluginName, ConflictingFiles);
                    LSPDFRPlusHandler.Initialise();                  
                }
            }
        }
        public static bool IsLSPDFRPluginRunning(string Plugin, Version minversion = null)
        {
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                AssemblyName an = assembly.GetName();
                if (an.Name.ToLower() == Plugin.ToLower())
                {
                    if (minversion == null || an.Version.CompareTo(minversion) >= 0) { return true; }
                }
            }
            return false;
        }
    }
}
