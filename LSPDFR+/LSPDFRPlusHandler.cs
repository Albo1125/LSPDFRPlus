using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.API;
using Rage;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System.Drawing;
using System.Threading;
using System.Management;
using System.Net;
using Rage.Native;

[assembly: Rage.Attributes.Plugin("LSPDFR+", Description = "INSTALL IN PLUGINS/LSPDFR FOLDER. Enhances policing in LSPDFR", Author = "Albo1125")]
namespace LSPDFR_
{
    public class EntryPoint
    {
        public static void Main()
        {
            Game.DisplayNotification("You have installed LSPDFR+ incorrectly. You must install it in GTAV/Plugins/LSPDFR. It will then be automatically loaded when going on duty - you must NOT load it yourself via RAGEPluginHook. This is also explained in the Readme and Documentation. You will now be redirected to the installation tutorial.");
            GameFiber.Wait(5000);
            System.Diagnostics.Process.Start("https://www.youtube.com/watch?v=af434m72rIo&list=PLEKypmos74W8PMP4k6xmVxpTKdebvJpFb");
            return;
        }
    }
    internal static class LSPDFRPlusHandler
    {
        
        public static bool BritishPolicingScriptRunning = false;
        public static bool ArrestManagerRunning = false;
        public static bool TrafficPolicerRunning = false;

        public static KeysConverter kc = new KeysConverter();
        public static void Initialise()
        {
            //ini stuff

            InitializationFile ini = new InitializationFile("Plugins/LSPDFR/LSPDFR+.ini");
            ini.Create();
            try
            {
                EnhancedTrafficStop.BringUpTrafficStopMenuControllerButton = ini.ReadEnum<ControllerButtons>("General", "BringUpTrafficStopMenuControllerButton", ControllerButtons.DPadRight);
                EnhancedTrafficStop.BringUpTrafficStopMenuKey = (Keys)kc.ConvertFromString(ini.ReadString("General", "BringUpTrafficStopMenuKey", "E"));
                CourtSystem.OpenCourtMenuKey = (Keys)kc.ConvertFromString(ini.ReadString("OnlyWithoutBritishPolicingScriptInstalled", "OpenCourtMenuKey", "F9"));
                CourtSystem.OpenCourtMenuModifierKey = (Keys)kc.ConvertFromString(ini.ReadString("OnlyWithoutBritishPolicingScriptInstalled", "OpenCourtMenuModifierKey", "None"));
                EnhancedTrafficStop.EnhancedTrafficStopsEnabled = ini.ReadBoolean("General", "EnhancedTrafficStopsEnabled", true);
                EnhancedPursuitAI.EnhancedPursuitAIEnabled = ini.ReadBoolean("General", "EnhancedPursuitAIEnabled", true);
                EnhancedPursuitAI.AutoPursuitBackupEnabled = ini.ReadBoolean("General", "AutoPursuitBackupEnabled", false);
                EnhancedPursuitAI.OpenPursuitTacticsMenuKey = (Keys)kc.ConvertFromString(ini.ReadString("General", "OpenPursuitTacticsMenuKey", "Q"));
                EnhancedPursuitAI.OpenPursuitTacticsMenuModifierKey = (Keys)kc.ConvertFromString(ini.ReadString("General", "OpenPursuitTacticsMenuModifierKey", "LShiftKey"));
                EnhancedPursuitAI.DefaultAutomaticAI = ini.ReadBoolean("General", "DefaultAutomaticAI", true);

                Offence.maxpoints = ini.ReadInt32("General", "MaxPoints", 12);
                Offence.pointincstep = ini.ReadInt32("General", "PointsIncrementalStep", 1);
                Offence.maxFine = ini.ReadInt32("General", "MaxFine", 5000);

                Offence.OpenTicketMenuKey = (Keys)kc.ConvertFromString(ini.ReadString("General", "OpenTicketMenuKey", "Q"));
                Offence.OpenTicketMenuModifierKey = (Keys)kc.ConvertFromString(ini.ReadString("General", "OpenTicketMenuModifierKey", "LShiftKey"));
                Offence.enablePoints = ini.ReadBoolean("General", "EnablePoints", true);

                CourtSystem.RealisticCourtDates = ini.ReadBoolean("OnlyWithoutBritishPolicingScriptInstalled", "RealisticCourtDates", true);
            }
            catch (Exception e)
            {
                Game.LogTrivial(e.ToString());
                Game.LogTrivial("Error loading LSPDFR+ INI file. Loading defaults");
                Game.DisplayNotification("~r~Error loading LSPDFR+ INI file. Loading defaults");
            }
            BetaCheck();
        }

        private static Stopwatch TimeOnDutyStopWatch = new Stopwatch();
        private static void MainLoop()
        {
            GameFiber.StartNew(delegate
            {
                Game.LogTrivial("LSPDFR+ has been initialised successfully and is now loading INI, XML and dependencies. Standby...");
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LSPDFRResolveEventHandler);
                GameFiber.Sleep(5000);
                BritishPolicingScriptRunning = IsLSPDFRPluginRunning("British Policing Script", new Version("0.9.0.0"));
                ArrestManagerRunning = IsLSPDFRPluginRunning("Arrest Manager", new Version("7.6.8.0"));
                TrafficPolicerRunning = IsLSPDFRPluginRunning("Traffic Policer", new Version("6.13.6.0"));
                if (BritishPolicingScriptRunning)
                {
                    API.BritishPolicingScriptFunctions.RegisterBPSOutOfVehicleEvent();
                    Offence.currency = "£";
                }
                if (ArrestManagerRunning)
                {
                    API.ArrestManagerFuncs.PlayerArrestedPedHandler();
                }
                else
                {
                    Game.DisplayNotification("To optimally use LSPDFR+, you are advised to install Arrest Manager by Albo1125.");
                }

                if (!TrafficPolicerRunning)
                {
                    Game.DisplayNotification("To optimally use LSPDFR+, you are advised to install Traffic Policer by Albo1125.");
                }

                Offence.DeserializeOffences();
                Game.LogTrivial("TrafficOffences:");
                Offence.CategorizedTrafficOffences.Values.ToList().ForEach(x => x.ForEach(y => Game.LogTrivial(y.ToString())));
                Menus.InitialiseMenus();

                CourtSystem.CourtSystemMainLogic();
                
                EnhancedPursuitAI.MainLoop();
                StatisticsCounter.AddCountToStatistic("Times gone on duty", "LSPDFR+");
                
                Game.LogTrivial("LSPDFR+ has been fully initialised successfully and is now working.");
                
                TimeOnDutyStopWatch.Start();
                while (true)
                {
                    GameFiber.Yield();

                    if (Functions.IsPlayerPerformingPullover() && NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>() == 0)
                    {
                        Game.DisplaySubtitle("~h~Stopped Vehicle: " + Functions.GetPulloverSuspect(Functions.GetCurrentPullover()).CurrentVehicle.LicensePlate, 50);
                    }

                    if (Game.IsPaused && TimeOnDutyStopWatch.IsRunning)
                    {
                        TimeOnDutyStopWatch.Stop();
                    }
                    else if (!Game.IsPaused && !TimeOnDutyStopWatch.IsRunning)
                    {
                        TimeOnDutyStopWatch.Start();
                    }

                    if (TimeOnDutyStopWatch.ElapsedMilliseconds > 60000)
                    {
                        StatisticsCounter.AddCountToStatistic("Minutes spent on duty", "LSPDFR+");
                        
                        TimeOnDutyStopWatch.Restart();
                    }
                    



                }
            });

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
        public static Assembly LSPDFRResolveEventHandler(object sender, ResolveEventArgs args) { foreach (Assembly assembly in Functions.GetAllUserPlugins()) { if (args.Name.ToLower().Contains(assembly.GetName().Name.ToLower())) { return assembly; } } return null; }

        public static Random rnd = new Random();
        public static void BetaCheck()
        {
            GameFiber.StartNew(delegate
            {

                Game.LogTrivial("LSPDFR+, developed by Albo1125, has been loaded successfully!");
                GameFiber.Wait(6000);
                Game.DisplayNotification("~b~LSPDFR+~s~, developed by ~b~Albo1125, ~s~has been loaded ~g~successfully.");


            });
            Game.LogTrivial("LSPDFR+ is not in beta.");

            MainLoop();
        }        
    }
}
