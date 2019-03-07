using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;
using System.Windows.Forms;
using Albo1125.Common.CommonLibrary;
using Rage.Native;

namespace LSPDFR_
{
    public enum PursuitTactics { Safe, SlightlyAggressive, FulloutAggressive }
    [Obsolete("LSPDFR 0.4 appears to use custom AI rather than pursuit natives, this no longer works well unless disabling it.")]
    internal static class EnhancedPursuitAI
    {
        
        public static bool EnhancedPursuitAIEnabled = true;
        public static bool AutoPursuitBackupEnabled = false;
        public static Keys OpenPursuitTacticsMenuKey = Keys.Q;
        public static Keys OpenPursuitTacticsMenuModifierKey = Keys.LShiftKey;
        public static bool DefaultAutomaticAI = true;


        public static PursuitTactics CurrentPursuitTactic = PursuitTactics.Safe;
        public static bool AutomaticAI = true;

        public static bool SetSafePursuit = true;
        static List<Ped> CopsInPursuit = new List<Ped>();
        public static bool InPursuit = false;



        
        public static void MainLoop()
        {
            GameFiber.StartNew(delegate
            {


                while (true)
                {
                    try
                    {
                        GameFiber.Yield();

                        if (Functions.GetActivePursuit() != null)
                        {
                            if (!AutoPursuitBackupEnabled)
                            {
                                Functions.SetPursuitCopsCanJoin(Functions.GetActivePursuit(), false);
                            }

                            if (!InPursuit)
                            {
                                StatisticsCounter.AddCountToStatistic("Pursuits", "LSPDFR+");
                                InPursuit = true;
                                API.Functions.OnPlayerJoinedActivePursuit();
                                if (EnhancedPursuitAIEnabled)
                                {
                                    Game.DisplayHelp("Press ~b~" + ExtensionMethods.GetKeyString(OpenPursuitTacticsMenuKey, OpenPursuitTacticsMenuModifierKey) + " ~s~to open the pursuit tactics menu.");
                                    //Menus.AutomaticTacticsCheckboxItem.Checked = true;
                                }
                            }


                            if (EnhancedPursuitAIEnabled)
                            {
                                if (Functions.GetPursuitPeds(Functions.GetActivePursuit()).Length <= 0) { Functions.ForceEndPursuit(Functions.GetActivePursuit()); continue; }
                                Ped[] pursuitpeds = (from x in Functions.GetPursuitPeds(Functions.GetActivePursuit()) where x.Exists() orderby Game.LocalPlayer.Character.DistanceTo(x) select x).ToArray();
                                if (pursuitpeds.Length > 0)
                                {
                                    EnhancedPursuitAI.AutomaticAI = Menus.AutomaticTacticsCheckboxItem.Checked;
                                    Menus.PursuitTacticsListItem.Enabled = !Menus.AutomaticTacticsCheckboxItem.Checked;
                                    if (AutomaticAI)
                                    {
                                        if (Game.LocalPlayer.Character.IsInAnyVehicle(false) && pursuitpeds[0].IsInAnyVehicle(false))
                                        {
                                            if (Game.LocalPlayer.Character.DistanceTo(pursuitpeds[0].GetOffsetPosition(Vector3.RelativeFront * 4f)) < Game.LocalPlayer.Character.DistanceTo(pursuitpeds[0].GetOffsetPosition(Vector3.RelativeBack * 4f)) && Game.LocalPlayer.Character.DistanceTo(pursuitpeds[0]) < 40f)
                                            {
                                                CurrentPursuitTactic = PursuitTactics.SlightlyAggressive;
                                            }
                                            else if (Game.LocalPlayer.Character.DistanceTo(pursuitpeds[0].Position) > 130f)
                                            {
                                                CurrentPursuitTactic = PursuitTactics.FulloutAggressive;
                                            }
                                            else
                                            {
                                                CurrentPursuitTactic = PursuitTactics.Safe;
                                            }
                                        }
                                        else
                                        {
                                            CurrentPursuitTactic = PursuitTactics.FulloutAggressive;
                                        }
                                        Menus.PursuitTacticsListItem.Index = (int)CurrentPursuitTactic;
                                    }
                                    else
                                    {

                                        CurrentPursuitTactic = (PursuitTactics)Menus.PursuitTacticsListItem.Index;
                                    }

                                    foreach (Ped ped in Game.LocalPlayer.Character.GetNearbyPeds(16))
                                    {

                                        if (ped.Exists() && !CopsInPursuit.Contains(ped))
                                        {

                                            if (ped.IsInAnyPoliceVehicle && ped.IsPolicePed())
                                            {
                                                if (!ped.CurrentVehicle.IsInAir && !ped.CurrentVehicle.IsHelicopter && !ped.CurrentVehicle.IsBoat)
                                                {
                                                    CopsInPursuit.Add(ped);
                                                }

                                            }
                                            else if (!Functions.GetPursuitPeds(Functions.GetActivePursuit()).Contains(ped) && ped.IsInAnyVehicle(false))
                                            {
                                                Rage.Native.NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(ped, 786603);
                                            }
                                        }
                                    }
                                    //Game.LogTrivial("Pursuittactic: " + CurrentPursuitTactic);
                                    float distance = Vector3.Distance(pursuitpeds[0].Position, Game.LocalPlayer.Character.Position) + 10f;
                                    
                                    Ped[] CopsInPursuitOrdered = (from x in CopsInPursuit where x.Exists() orderby x.DistanceTo(pursuitpeds[0].Position) select x).ToArray();
                                    //Ped[] CopsInPursuitOrdered = (from x in (from y in CopsInPursuit where y.Exists() select y) orderby x.DistanceTo(pursuitpeds[0].Position) select x).ToArray();
                                    //Ped[] NearbyPedsInOrder = (from x in (from y in NearbyPeds where y.Exists() select y) orderby x.DistanceTo(Game.LocalPlayer.Character.Position) select x).ToArray();
                                    foreach (Ped ped in CopsInPursuitOrdered)
                                    {
                                        GameFiber.Yield();
                                        if (ped.Exists())
                                        {
                                            if (ped.IsInAnyVehicle(false) && !ped.CurrentVehicle.IsInAir)
                                            {
                                                NativeFunction.Natives.SET_DRIVER_ABILITY(ped, 1.0f);
                                                int flag = 1;

                                                //8: Medium-aggressive boxing tactic with a bit of PIT
                                                //1: Aggressive ramming of suspect
                                                //2: Ram attempts
                                                //32: Stay back from suspect, no tactical contact. Convoy-like.
                                                //16: Ramming, seems to be slightly less aggressive than 1-2.

                                                distance += 15;



                                                if (Game.LocalPlayer.Character.IsInAnyVehicle(false) && pursuitpeds[0].IsInAnyVehicle(false))
                                                {
                                                    if (CurrentPursuitTactic == PursuitTactics.Safe)
                                                    {
                                                        Rage.Native.NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_IDEAL_PURSUIT_DISTANCE(ped, distance);
                                                        flag = 32;
                                                        //Game.LogTrivial("Setting pursuit distance " + distance.ToString());
                                                        Rage.Native.NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(ped, flag, true);
                                                        //Game.LogTrivial("Setting behaviour flag to " + flag.ToString());
                                                        NativeFunction.Natives.SET_DRIVER_AGGRESSIVENESS(ped, 0.1f);
                                                    }
                                                    else if (CurrentPursuitTactic == PursuitTactics.SlightlyAggressive)
                                                    {
                                                        Rage.Native.NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_IDEAL_PURSUIT_DISTANCE(ped, 0f);
                                                        //Game.LogTrivial("Setting pursuit distance 0");
                                                        flag = 8;
                                                        Rage.Native.NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(ped, flag, true);
                                                        //Game.LogTrivial("Setting behaviour flag to " + flag.ToString());
                                                        NativeFunction.Natives.SET_DRIVER_AGGRESSIVENESS(ped, 0.5f);
                                                    }
                                                    else
                                                    {
                                                        ped.Tasks.Clear();
                                                        ped.Tasks.ChaseWithGroundVehicle(pursuitpeds[0]);
                                                        NativeFunction.Natives.SET_DRIVER_AGGRESSIVENESS(ped, 1.0f);

                                                    }
                                                }
                                                else
                                                {
                                                    ped.Tasks.Clear();
                                                    ped.Tasks.ChaseWithGroundVehicle(pursuitpeds[0]);
                                                    NativeFunction.Natives.SET_DRIVER_AGGRESSIVENESS(ped, 1.0f);


                                                }
                                                //else
                                                //{
                                                //    Rage.Native.NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_IDEAL_PURSUIT_DISTANCE(ped, 0f);
                                                //    Game.LogTrivial("Setting pursuit distance 0");
                                                //    flag = 8;
                                                //}



                                                //Works with: 32, 16, 8, 

                                            }
                                        }
                                    }
                                    if (CurrentPursuitTactic == PursuitTactics.FulloutAggressive)
                                    {
                                        GameFiber.Wait(600);
                                    }
                                }
                            }
                        }
                        else
                        {
                            InPursuit = false;
                            if (CopsInPursuit.Count > 0)
                            {
                                CopsInPursuit.Clear();
                            }
                        }
                    }
                    catch (System.Threading.ThreadAbortException e) { break; }
                    catch (Exception e)
                    {
                        Game.LogTrivial(e.ToString());
                        Game.LogTrivial("Handled");
                    }
                }
            });
        }
    }
}
