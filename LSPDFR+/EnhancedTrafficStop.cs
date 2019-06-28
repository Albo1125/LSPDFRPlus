using LSPD_First_Response.Mod.API;
using LSPDFR_.ExtensionNamespace;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Albo1125.Common.CommonLibrary;
using System.IO;
using System.Xml.Serialization;

namespace LSPDFR_
{
    internal class EnhancedTrafficStop
    {
        public static bool EnhancedTrafficStopsEnabled = true;
        public static ControllerButtons BringUpTrafficStopMenuControllerButton = ControllerButtons.DPadRight;
        public static Keys BringUpTrafficStopMenuKey = Keys.D7;

        public static TupleList<Ped, string, string> PedsWithCustomTrafficStopQuestionsAndAnswers = new TupleList<Ped, string, string>();
        public static TupleList<Ped, string, Func<Ped, string>> PedsCustomTrafficStopQuestionsAndCallBackAnswer = new TupleList<Ped, string, Func<Ped, string>>();
        public static TupleList<Ped, string, string, Action<Ped, string>> PedsCustomQuestionsAnswerCallback = new TupleList<Ped, string, string, Action<Ped, string>>();
        public static List<Ped> PedsWhereStandardQuestionsAreHidden = new List<Ped>();

        public static TupleList<Ped, TrafficStopQuestionsInfo> SuspectsTrafficStopQuestionsInfo = new TupleList<Ped, TrafficStopQuestionsInfo>();

        public static bool HasShownKeybindHelp = false;


        public static void PedBackIntoVehicleLogic(Ped suspect, Vehicle suspectvehicle)
        {
            GameFiber.StartNew(delegate
            {
                try
                {
                    while (true)
                    {
                        GameFiber.Yield();
                        if (!suspect.Exists() || !suspectvehicle.Exists())
                        {
                            return;
                        }
                        else if (Functions.IsPedGettingArrested(suspect) || Functions.IsPedArrested(suspect) || suspect.IsDead)
                        {
                            return;
                        }
                        else if (Functions.IsPedStoppedByPlayer(suspect))
                        {
                            while (Functions.IsPedStoppedByPlayer(suspect))
                            {
                                GameFiber.Yield();
                                if (!suspect.Exists() || !suspectvehicle.Exists())
                                {
                                    return;
                                }
                                else if (Functions.IsPedGettingArrested(suspect) || Functions.IsPedArrested(suspect) || suspect.IsDead)
                                {
                                    return;
                                }
                            }
                            if (suspect.DistanceTo(suspectvehicle) < 25f)
                            {
                                suspect.IsPersistent = true;
                                suspect.BlockPermanentEvents = true;
                                suspectvehicle.IsPersistent = true;
                                suspect.Tasks.FollowNavigationMeshToPosition(suspectvehicle.GetOffsetPosition(Vector3.RelativeLeft * 2f), suspectvehicle.Heading, 1.45f).WaitForCompletion(10000);
                                if (suspectvehicle.GetFreeSeatIndex() != null)
                                {
                                    int? freeseat = suspectvehicle.GetFreeSeatIndex();
                                    suspect.Tasks.EnterVehicle(suspectvehicle, 6000, freeseat == null ? -1 : freeseat.Value).WaitForCompletion(6100);
                                }
                            }
                            suspect.Dismiss();
                            suspectvehicle.Dismiss();
                            return;
                        }

                    }
                }
                catch (System.Threading.ThreadAbortException e) { }
                catch (Exception e)
                {
                    Game.LogTrivial(e.ToString());
                    if (suspect.Exists()) { suspect.Dismiss(); }
                    if (suspectvehicle) { suspectvehicle.Dismiss(); }
                    
                }
            });
        }        

        public static void VerifySuspectIsInTrafficStopInfo(Ped Suspect)
        {
            if (!SuspectsTrafficStopQuestionsInfo.Select(x => x.Item1).Contains(Suspect))
            {
                SuspectsTrafficStopQuestionsInfo.Add(Suspect, new TrafficStopQuestionsInfo());
            }
        }        

        public Ped Suspect { get; private set; }
        public Vehicle SuspectVehicle { get; private set; }
        public List<Offence> SelectedOffences = new List<Offence>();
        public EnhancedTrafficStop()
        {
            if (Functions.IsPlayerPerformingPullover())
            {

                if (!HasShownKeybindHelp)
                {
                    string maybeControllerButton = BringUpTrafficStopMenuControllerButton != ControllerButtons.None ? $" or ~o~{BringUpTrafficStopMenuControllerButton}~w~" : "";

                    Game.DisplayNotification($"Press ~b~{BringUpTrafficStopMenuKey.FriendlyName()}~w~{maybeControllerButton} when standing at the side of the vehicle to bring up the ~g~LSPDFR+ Traffic Stop~w~ menu.");
                    HasShownKeybindHelp = true;
                }
                

                SuspectVehicle = Functions.GetPulloverSuspect(Functions.GetCurrentPullover()).CurrentVehicle;
                Suspect = Functions.GetPulloverSuspect(Functions.GetCurrentPullover());
                if (SuspectVehicle.IsBoat)
                {
                    Menus.TrafficStopMenuDistance = 6.0f;
                }
                else
                {
                    Menus.TrafficStopMenuDistance = 3.8f;
                }
                
                UpdateTrafficStopQuestioning();

            }
        }

        public void UpdateTrafficStopQuestioning()
        {
            CustomQuestionsWithAnswers.Clear();
            CustomQuestionsWithCallbacksAnswers.Clear();
            CustomQuestionsAnswerWithCallbacks.Clear();
            foreach (Tuple<Ped, string, string> tuple in PedsWithCustomTrafficStopQuestionsAndAnswers)
            {
                if (tuple.Item1 == Suspect)
                {
                    CustomQuestionsWithAnswers.Add(tuple.Item2, tuple.Item3);
                }
            }
            foreach (Tuple<Ped, string, Func<Ped, string>> tuple in PedsCustomTrafficStopQuestionsAndCallBackAnswer)
            {
                if (tuple.Item1 == Suspect)
                {
                    CustomQuestionsWithCallbacksAnswers.Add(tuple.Item2, tuple.Item3);
                }
            }

            foreach(Tuple<Ped, string, string, Action<Ped, string>> tuple in PedsCustomQuestionsAnswerCallback)
            {
                if (tuple.Item1 == Suspect)
                {
                    CustomQuestionsAnswerWithCallbacks.Add(tuple.Item2, tuple.Item3, tuple.Item4);
                }
            }
            StandardQuestionsEnabled = !PedsWhereStandardQuestionsAreHidden.Contains(Suspect);
        }

        public void PlaySpecificSpeech(string speech)
        {
            if (speech == "Hello")
            {
                Game.LogTrivial("Playing hello");
                Game.LocalPlayer.Character.PlayAmbientSpeech("GENERIC_HI");

            }
            else if (speech == "Insult")
            {
                Game.LogTrivial("Playing insult");
                if (LSPDFRPlusHandler.rnd.Next(2) == 0) { Game.LocalPlayer.Character.PlayAmbientSpeech("GENERIC_INSULT_MED"); }
                else
                {
                    Game.LocalPlayer.Character.PlayAmbientSpeech("GENERIC_INSULT_HIGH");
                }
            }
            else if (speech == "Kifflom")
            {
                Game.LogTrivial("Playing kifflom");
                Game.LocalPlayer.Character.PlayAmbientSpeech("KIFFLOM_GREET");
            }
            else if (speech == "Thanks")
            {
                Game.LogTrivial("Playing thanks");
                Game.LocalPlayer.Character.PlayAmbientSpeech("GENERIC_THANKS");
            }
            else if (speech == "Swear")
            {
                Game.LogTrivial("Playing swear");
                Game.LocalPlayer.Character.PlayAmbientSpeech("GENERIC_CURSE_HIGH");
            }
            else if (speech == "Warn")
            {
                Game.LogTrivial("Playing warn");
                Game.LocalPlayer.Character.PlayAmbientSpeech("CRIMINAL_WARNING");
               //NativeFunction.CallByHash<uint>(0x8E04FEDD28D42462, Game.LocalPlayer.Character, "SHOUT_THREATEN_PED", "SPEECH_PARAMS_FORCE_SHOUTED_CRITICAL");
            }
            else if (speech == "Threaten")
            {
                Game.LogTrivial("Playing threaten");
                Game.LocalPlayer.Character.PlayAmbientSpeech("CHALLENGE_THREATEN");
            }
        }

        public enum OccupantSelector { Driver, Passengers, AllOccupants }
        public void AskForID(OccupantSelector occupantselect)
        {
            GameFiber.StartNew(delegate
            {

                PlaySpecificSpeech("Kifflom");

                Game.LocalPlayer.Character.Tasks.AchieveHeading(Game.LocalPlayer.Character.CalculateHeadingTowardsEntity(Suspect));
                GameFiber.Wait(1500);

                if (occupantselect == OccupantSelector.Driver)
                {
                    Suspect.ShowDrivingLicence();
                }
                else if (occupantselect == OccupantSelector.Passengers)
                {
                    foreach (Ped occupant in SuspectVehicle.Passengers)
                    {
                        occupant.ShowDrivingLicence();
                    }
                }
                else if (occupantselect == OccupantSelector.AllOccupants)
                {
                    foreach (Ped occupant in SuspectVehicle.Occupants)
                    {
                        occupant.ShowDrivingLicence();
                    }
                }
                Game.LocalPlayer.Character.Tasks.Clear();
            });

        }

        public void IssueWarning()
        {
            GameFiber.StartNew(delegate
            {
                PlaySpecificSpeech("Warn");
                GameFiber.Wait(2500);
                Functions.ForceEndCurrentPullover();
                StatisticsCounter.AddCountToStatistic("Traffic Stop - Warnings Issued", "LSPDFR+");
            });
        }

        public void OutOfVehicle(OccupantSelector occupantselect)
        {
            GameFiber.StartNew(delegate
            {
                try
                {
                    Vehicle veh = Functions.GetPulloverSuspect(Functions.GetCurrentPullover()).CurrentVehicle;
                    if (occupantselect == OccupantSelector.Driver)
                    {
                        if (Functions.GetPulloverSuspect(Functions.GetCurrentPullover()).IsInAnyVehicle(false))
                        {
                            ProcessOrderOutOfVehicle(veh, Suspect);
                        }
                    }
                    else if (occupantselect == OccupantSelector.Passengers)
                    {
                        foreach (Ped pas in veh.Passengers)
                        {
                            ProcessOrderOutOfVehicle(veh, pas);

                        }
                    }
                    else if (occupantselect == OccupantSelector.AllOccupants)
                    {
                        foreach (Ped occ in veh.Occupants)
                        {
                            ProcessOrderOutOfVehicle(veh, occ);
                        }
                    }
                }
                catch(Exception e)
                {
                    Game.LogTrivial(e.ToString());
                    Game.LogTrivial("Error in getout handled - LSPDFR+");
                }
            });
        }

        private void ProcessOrderOutOfVehicle(Vehicle v, Ped p)
        {
            if (v.IsBoat)
            {

                Functions.ForceEndCurrentPullover();
                Vector3 pos = p.GetBonePosition(0);
                p.Tasks.Clear();
                p.Position = pos;

            }
            else
            {
                Functions.ForceEndCurrentPullover();
                p.Tasks.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion(5000);
                PedBackIntoVehicleLogic(p, SuspectVehicle);
            }
            NativeFunction.Natives.RESET_PED_LAST_VEHICLE(p);
            API.Functions.OnPedOrderedOutOfVehicle(p);
            Functions.SetPedAsStopped(p, true);
        }

        public void IssueTicket(bool SeizeVehicle)
        {
            GameFiber.StartNew(delegate
            {

                Game.LocalPlayer.Character.Tasks.AchieveHeading(Game.LocalPlayer.Character.CalculateHeadingTowardsEntity(Suspect));
                Functions.GetPersonaForPed(Suspect).Citations++;
                GameFiber.Wait(1500);
                Game.LocalPlayer.Character.Tasks.Clear();
                NativeFunction.Natives.TASK_START_SCENARIO_IN_PLACE(Game.LocalPlayer.Character, "CODE_HUMAN_MEDIC_TIME_OF_DEATH", 0, true);
                
                //Do animation
                while (!NativeFunction.Natives.IS_PED_ACTIVE_IN_SCENARIO<bool>(Game.LocalPlayer.Character))
                {
                    GameFiber.Yield();
                }
                int Waitcount = 0;
                while (NativeFunction.Natives.IS_PED_ACTIVE_IN_SCENARIO<bool>(Game.LocalPlayer.Character))
                {
                    GameFiber.Yield();
                    Waitcount++;
                    if (Waitcount >= 300) { Game.LocalPlayer.Character.Tasks.Clear(); }
                }
                GameFiber.Wait(4000);

                if (SeizeVehicle && SuspectVehicle.Exists())
                {

                    //Game.LogTrivial("Debug 4");
                    foreach (Ped occupant in SuspectVehicle.Occupants)
                    {
                        occupant.Tasks.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion(6000);
                        if (occupant.IsInAnyVehicle(false)) { occupant.Tasks.LeaveVehicle(LeaveVehicleFlags.WarpOut).WaitForCompletion(100); }
                        occupant.Tasks.Wander();
                    }

                    GameFiber.Wait(1000);
                    Game.LocalPlayer.Character.Tasks.ClearImmediately();
                    if (LSPDFRPlusHandler.ArrestManagerRunning)
                    {
                        API.ArrestManagerFuncs.RequestTowForVeh(SuspectVehicle);
                    }
                    

                }
                else
                {
                    GameFiber.Wait(2500);
                    Functions.ForceEndCurrentPullover();
                    
                }
                
                StatisticsCounter.AddCountToStatistic("Traffic Stop - Tickets Issued", "LSPDFR+");
            });
        }

        public static void performTicketAnimation()
        {
            Game.LocalPlayer.Character.Tasks.Clear();
            NativeFunction.Natives.TASK_START_SCENARIO_IN_PLACE(Game.LocalPlayer.Character, "CODE_HUMAN_MEDIC_TIME_OF_DEATH", 0, true);

            //Do animation
            while (!NativeFunction.Natives.IS_PED_ACTIVE_IN_SCENARIO<bool>(Game.LocalPlayer.Character))
            {
                GameFiber.Yield();
            }
            int Waitcount = 0;
            while (NativeFunction.Natives.IS_PED_ACTIVE_IN_SCENARIO<bool>(Game.LocalPlayer.Character))
            {
                GameFiber.Yield();
                Waitcount++;
                if (Waitcount >= 300) { Game.LocalPlayer.Character.Tasks.Clear(); }
            }
            GameFiber.Wait(6000);
            Game.LocalPlayer.Character.Tasks.ClearImmediately();
        }

        public TupleList<string, string> CustomQuestionsWithAnswers = new TupleList<string, string>();
        public TupleList<string, Func<Ped, string>> CustomQuestionsWithCallbacksAnswers = new TupleList<string, Func<Ped, string>>();
        public TupleList<string, string, Action<Ped, string>> CustomQuestionsAnswerWithCallbacks = new TupleList<string, string,Action<Ped, string>>();

        public bool StandardQuestionsEnabled = true;

        private static List<List<string>> Questions = new List<List<string>>()
        {
            new List<string>(){ "Do you have anything illegal in the vehicle?", "Anything in the vehicle that shouldn't be?", "Got anything illegal in your vehicle?" },
            new List<string>() {"Have you been drinking?", "Have you had a drink today?", "Have you had a drink recently?" },
            new List<string>() {"Have you done any illegal drugs recently?", "Have you taken any drugs in the past hours?", "Have you taken any drugs recently?" },
            new List<string>() {"Can I search your vehicle?", "Would you mind if I searched your vehicle?", "Any objection to me searching your vehicle?" }
        };


        private static List<List<string>> InnocentAnswers = new List<List<string>>()
        {
            new List<string>() {"Not that I know of, officer...", "Nope", "That's none of your business - I know my rights!", "Perhaps.", "You never know. Sometimes people borrow my car.", "No idea.", "Ummm, no?", "Of course not!", "Depends, I guess.", "No, why?", "Do I look like a criminal?",  },
            new List<string>() {"No", "Nope", "Only one.", "I don't need to answer that.", "I want my lawyer.", "Got nothing better to do? Of course not!", "Yup. Stay hydrated!" },
            new List<string>() {"Nope", "Not recently, no.", "I'm not obliged to answer that.", "Am  I being detained?", "No, why?" },
            new List<string>() {"No thanks!", "Well, sure, I guess.", "Why? No!", "Why are you messing with me? Go find real crime!", "You cops always take away my rights!", "Pff, hoping to find something? Have at it.", "I don't mind.", "I'd prefer it if you didn't.", "Sure, have at it.", "I don't have any issues with that. Go ahead.", "No. Can I search yours?" }
        };


        private static List<List<string>> GuiltyAnswers = new List<List<string>>()
        {
            new List<string>(),
            new List<string>() {"Yes, I've had a few.", "Breathalyze me, then...", "Yarr, want some too?", "Just a few beers.", "Umm, no..?", "Well, I can't really remember...", "I don't think so, at least.", "Ughh...Such a headache...", "No.", "Nope.", "Yes. Got a problem with that?" },
            new List<string>() {"Yes, officer.", "My lawyer! Now!", "I can't remember, officer...Ugh.", "My vision... Those... flowers... Hell no", "I'm high as hell!", "Nope", "Am I being detained?", "I'm in the clouds!", "Yes. Any problems with that?", "I don't need to answer that.", "What you do care?" },
            new List<string>()
        };

        private string _AnythingIllegalInVehQuestion;
        public string AnythingIllegalInVehQuestion
        {
            get
            {
                if (_AnythingIllegalInVehQuestion == null)
                {
                    _AnythingIllegalInVehQuestion = Questions[0][LSPDFRPlusHandler.rnd.Next(Questions[0].Count)];
                }
                return _AnythingIllegalInVehQuestion;
            }
        }

        private string _AnythingIllegalInVehAnswer;
        public string AnythingIllegalInVehAnswer
        {
            get
            {
                if (_AnythingIllegalInVehAnswer == null)
                {
                    _AnythingIllegalInVehAnswer = InnocentAnswers[0][LSPDFRPlusHandler.rnd.Next(InnocentAnswers[0].Count)];
                }
                return _AnythingIllegalInVehAnswer;
            }
        }

        private string _DrinkingQuestion;
        public string DrinkingQuestion
        {
            get
            {
                if (_DrinkingQuestion == null)
                {
                    _DrinkingQuestion = Questions[1][LSPDFRPlusHandler.rnd.Next(Questions[1].Count)];
                }
                return _DrinkingQuestion;
            }
        }

        private string _DrinkingAnswer;
        public string DrinkingAnswer
        {
            get
            {
                if (_DrinkingAnswer == null)
                {
                    if (LSPDFRPlusHandler.TrafficPolicerRunning && API.TrafficPolicerFuncs.IsPedOverAlcoholLimit(Suspect))
                    {
                        _DrinkingAnswer = GuiltyAnswers[1][LSPDFRPlusHandler.rnd.Next(GuiltyAnswers[1].Count)];
                    }
                    else
                    {
                        _DrinkingAnswer = InnocentAnswers[1][LSPDFRPlusHandler.rnd.Next(InnocentAnswers[1].Count)];
                    }
                }
                return _DrinkingAnswer;
            }
        }
        private string _DrugsQuestion;
        public string DrugsQuestion
        {
            get
            {
                if (_DrugsQuestion == null)
                {
                    _DrugsQuestion = Questions[2][LSPDFRPlusHandler.rnd.Next(Questions[2].Count)];
                }
                return _DrugsQuestion;
            }
        }

        private string _DrugsAnswer;
        public string DrugsAnswer
        {
            get
            {
                if (_DrugsAnswer == null)
                {
                    if (LSPDFRPlusHandler.TrafficPolicerRunning && API.TrafficPolicerFuncs.IsPedOverDrugsLimit(Suspect))
                    {
                        _DrugsAnswer = GuiltyAnswers[2][LSPDFRPlusHandler.rnd.Next(GuiltyAnswers[2].Count)];
                    }
                    else
                    {
                        _DrugsAnswer = InnocentAnswers[2][LSPDFRPlusHandler.rnd.Next(InnocentAnswers[2].Count)];
                    }
                }
                return _DrugsAnswer;
            }
        }

        private string _SearchVehQuestion;
        public string SearchVehQuestion
        {
            get
            {
                if (_SearchVehQuestion == null)
                {
                    _SearchVehQuestion = Questions[3][LSPDFRPlusHandler.rnd.Next(Questions[3].Count)];
                }
                return _SearchVehQuestion;
            }
        }

        private string _SearchVehAnswer;
        public string SearchVehAnswer
        {
            get
            {
                if (_SearchVehAnswer == null)
                {
                    _SearchVehAnswer = InnocentAnswers[3][LSPDFRPlusHandler.rnd.Next(InnocentAnswers[3].Count)];
                }
                return _SearchVehAnswer;
            }
        }
    }

    internal class TrafficStopQuestionsInfo
    {
        public List<string> CustomReasons = new List<string>();
        public bool HideDefaultReasons = false;
    }

}
