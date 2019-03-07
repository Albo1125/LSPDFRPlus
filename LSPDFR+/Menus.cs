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
using Rage.Native;
using RAGENativeUI.PauseMenu;
using Albo1125.Common.CommonLibrary;

namespace LSPDFR_
{
    internal static class Menus
    {
        //private static UIMenu ChecksMenu;

        
        //private static UIMenuItem CheckPlateItem;
        
        //private static UIMenuItem CheckCourtResultsItem;
        private static MenuPool _MenuPool;

        


        //Speech, ID, ticket, warning, out of vehicle
        private static UIMenu TrafficStopMenu;
        private static UIMenuListItem SpeechItem;
        private static UIMenuListItem IDItem;
        private static UIMenuItem QuestionDriverItem;
        private static UIMenuItem PenaltyItem;
        private static UIMenuItem WarningItem;
        private static UIMenuListItem OutOfVehicleItem;
        private static List<dynamic> OccupantSelector = new List<dynamic>() { "Driver", "Passengers", "All occupants" };

        //public static UIMenuSwitchMenusItem MenuSwitchListItem;
        //private static UIMenu ActiveMenu = ChecksMenu;

        private static UIMenu TicketMenu;
        private static UIMenuListItem FineItem;
        private static List<string> FineList = new List<string>();

        private static UIMenuListItem PointsItem;
        private static List<string> PointsList = new List<string>();

        private static List<string> DefaultTicketReasonsList = new List<string>() { "Careless driving", "Speeding", "Mobile Phone", "Traffic light offence", "Illegal tyre", "Road Obstruction",
            "No insurance", "Expired registration", "No seat belt", "Expired licence", "Unroadworthy vehicle", "Lane splitting", "No helmet", "Failure to yield", "Tailgating", "Unsecure load" };

        private static UIMenuItem TicketOffenceSelectorItem = new UIMenuItem("Select Offences");


        private static UIMenuListItem IssueTicketItem;
        private static UIMenuCheckboxItem SeizeVehicleTicketCheckboxItem;
        

        private static UIMenu QuestioningMenu;
        private static UIMenuItem IllegalInVehQuestionItem;
        private static UIMenuItem DrinkingQuestionItem;
        private static UIMenuItem DrugsQuestionItem;
        private static UIMenuItem SearchPermissionItem;

        private static List<UIMenuItem> CustomQuestionsItems = new List<UIMenuItem>();
        private static List<UIMenuItem> CustomQuestionsCallbacksAnswersItems = new List<UIMenuItem>();
        private static List<UIMenuItem> CustomQuestionsAnswersCallbackItems = new List<UIMenuItem>();

        public static TabView CourtsMenu;

        public static TabSubmenuItem PendingResultsList;
        public static TabSubmenuItem PublishedResultsList;

        public static UIMenu PursuitTacticsMenu;
        public static UIMenuCheckboxItem AutomaticTacticsCheckboxItem;
        public static UIMenuListItem PursuitTacticsListItem;
        public static List<DisplayItem> PursuitTacticsOptionsList = new List<DisplayItem>() { new DisplayItem("Safe"), new DisplayItem("Slightly Aggressive"), new DisplayItem("Full-out aggressive") };

        public static List<UIMenu> OffenceCategoryMenus = new List<UIMenu>();
        public static UIMenuSwitchMenusItem OffenceCategorySwitchItem;
        public static List<UIMenuCheckboxItem> Offences = new List<UIMenuCheckboxItem>();
        public static TupleList<UIMenuCheckboxItem, Offence> CheckboxItems_Offences = new TupleList<UIMenuCheckboxItem, Offence>();

        public static bool TrafficStopMenuEnabled = true;

        public static bool StandardQuestionsInMenu = true;

        public static void ToggleStandardQuestions(bool Enabled)
        {
            if (Enabled && !StandardQuestionsInMenu)
            {
                QuestioningMenu.Clear();
                QuestioningMenu.AddItem(IllegalInVehQuestionItem = new UIMenuItem(""));
                QuestioningMenu.AddItem(DrinkingQuestionItem = new UIMenuItem(""));
                QuestioningMenu.AddItem(DrugsQuestionItem = new UIMenuItem(""));
                QuestioningMenu.AddItem(SearchPermissionItem = new UIMenuItem(""));
                StandardQuestionsInMenu = true;
            }

            else if (!Enabled && StandardQuestionsInMenu)
            {

                QuestioningMenu.Clear();
                StandardQuestionsInMenu = false;

            }
        }

        public static void OpenOffencesMenu(UIMenu callingMenu, List<Offence> SelectedOffences)
        {            
            foreach (UIMenu men in OffenceCategoryMenus)
            {
                men.ParentMenu = callingMenu;         
                foreach (UIMenuItem it in men.MenuItems)
                {
                    if (it is UIMenuCheckboxItem)
                    {
                        ((UIMenuCheckboxItem)it).Checked = SelectedOffences.Contains(CheckboxItems_Offences.FirstOrDefault(x => x.Item1 == it).Item2);
                        
                    }
                }
            }
            OffenceCategoryMenus[0].Visible = true;
        }

        public static List<TabItem> EmptyItems = new List<TabItem>() { new TabItem(" ") };
        public static void InitialiseMenus()
        {
            Game.FrameRender += Process;
            _MenuPool = new MenuPool();
            //ChecksMenu = new UIMenu("Checks", "");
            //_MenuPool.Add(ChecksMenu);
            TrafficStopMenu = new UIMenu("Traffic Stop", "LSPDFR+");
            _MenuPool.Add(TrafficStopMenu);
            TicketMenu = new UIMenu("Ticket", "");
            _MenuPool.Add(TicketMenu);

            PursuitTacticsMenu = new UIMenu("Pursuit Tactics", "");
            PursuitTacticsMenu.AddItem(AutomaticTacticsCheckboxItem = new UIMenuCheckboxItem("Automatic Tactics", EnhancedPursuitAI.DefaultAutomaticAI));
            PursuitTacticsMenu.AddItem(PursuitTacticsListItem = new UIMenuListItem("Current Tactic", "", PursuitTacticsOptionsList));
            PursuitTacticsListItem.Enabled = false;
            PursuitTacticsMenu.RefreshIndex();
            PursuitTacticsMenu.OnItemSelect += OnItemSelect;
            PursuitTacticsMenu.OnCheckboxChange += OnCheckboxChange;
            //TrafficStopMenu.OnListChange += OnListChange;
            PursuitTacticsMenu.MouseControlsEnabled = false;
            PursuitTacticsMenu.AllowCameraMovement = true;
            _MenuPool.Add(PursuitTacticsMenu);


            Dictionary<UIMenu, string> UIMenus_Categories = new Dictionary<UIMenu, string>();
            foreach (string category in Offence.CategorizedTrafficOffences.Keys)
            {
                UIMenu newcategorymenu = new UIMenu(category, "LSPDFR+ offences");
                OffenceCategoryMenus.Add(newcategorymenu);
                UIMenus_Categories.Add(newcategorymenu, category);

            }
            OffenceCategorySwitchItem = new UIMenuSwitchMenusItem("Categories", "", OffenceCategoryMenus);

            foreach (UIMenu newcategorymenu in OffenceCategoryMenus)
            {
                
                newcategorymenu.AddItem(OffenceCategorySwitchItem);
                string category = UIMenus_Categories[newcategorymenu];
                foreach (string reason in Offence.CategorizedTrafficOffences[category].Select(x => x.name))
                {
                    UIMenuCheckboxItem newcheckboxitem = new UIMenuCheckboxItem(reason, false);
                    newcategorymenu.AddItem(newcheckboxitem);
                    CheckboxItems_Offences.Add(new Tuple<UIMenuCheckboxItem, Offence>(newcheckboxitem, Offence.CategorizedTrafficOffences[category].FirstOrDefault(x => x.name == reason)));
                }

                newcategorymenu.OnMenuClose += OnMenuClose;
                newcategorymenu.RefreshIndex();
                newcategorymenu.AllowCameraMovement = true;
                newcategorymenu.MouseControlsEnabled = false;
                _MenuPool.Add(newcategorymenu);
            }



            var speech = new List<dynamic>() { "Hello", "Insult", "Kifflom", "Thanks", "Swear", "Warn", "Threaten" };

            TrafficStopMenu.AddItem(SpeechItem = new UIMenuListItem("Speech", "", speech));
            TrafficStopMenu.AddItem(IDItem = new UIMenuListItem("Ask for identification", "", OccupantSelector));
            TrafficStopMenu.AddItem(QuestionDriverItem = new UIMenuItem("Question driver"));
            TrafficStopMenu.AddItem(PenaltyItem = new UIMenuItem("Issue Penalty"));
            TrafficStopMenu.AddItem(WarningItem = new UIMenuItem("Issue warning", "Let the driver go with words of advice."));
            TrafficStopMenu.AddItem(OutOfVehicleItem = new UIMenuListItem("Order out of vehicle", "", OccupantSelector));

            TrafficStopMenu.RefreshIndex();
            TrafficStopMenu.OnItemSelect += OnItemSelect;

            TrafficStopMenu.MouseControlsEnabled = false;
            TrafficStopMenu.AllowCameraMovement = true;

            for (int i = 5; i<=Offence.maxFine;i+=5)
            {
                FineList.Add(Offence.currency + i.ToString());
            }

            for (int i = Offence.minpoints; i <= Offence.maxpoints; i += Offence.pointincstep)
            {
                PointsList.Add(i.ToString());
            }
            TicketMenu.AddItem(TicketOffenceSelectorItem);
            TicketMenu.AddItem(FineItem = new UIMenuListItem("Fine", "", FineList));

            PointsItem = new UIMenuListItem("Points", "", PointsList);
            if (Offence.enablePoints)
            {
                TicketMenu.AddItem(PointsItem);
            }
            
            //TicketMenu.AddItem(TicketReasonsListItem = new UIMenuListItem("Offence", TicketReasonsList, 0));            
            TicketMenu.AddItem(SeizeVehicleTicketCheckboxItem = new UIMenuCheckboxItem("Seize Vehicle", false));
            List<dynamic> PenaltyOptions = new List<dynamic>() { "Ticket", "Court Summons" };
            if (LSPDFRPlusHandler.BritishPolicingScriptRunning)
            {
                PenaltyOptions = new List<dynamic> { "Traffic Offence Report", "Fixed Penalty Notice", "Court Summons" };
            }
            TicketMenu.AddItem(IssueTicketItem = new UIMenuListItem("~h~Issue ", "", PenaltyOptions));
            IssueTicketItem.OnListChanged += OnIndexChange;
            TicketMenu.ParentMenu = TrafficStopMenu;
            TicketMenu.RefreshIndex();
            TicketMenu.OnItemSelect += OnItemSelect;

            TicketMenu.MouseControlsEnabled = false;
            TicketMenu.AllowCameraMovement = true;
            TicketMenu.SetMenuWidthOffset(80);


            QuestioningMenu = new UIMenu("Questioning", "");
            _MenuPool.Add(QuestioningMenu);
            QuestioningMenu.AddItem(IllegalInVehQuestionItem = new UIMenuItem(""));
            QuestioningMenu.AddItem(DrinkingQuestionItem = new UIMenuItem(""));
            QuestioningMenu.AddItem(DrugsQuestionItem = new UIMenuItem(""));
            QuestioningMenu.AddItem(SearchPermissionItem = new UIMenuItem(""));
            QuestioningMenu.ParentMenu = TrafficStopMenu;
            QuestioningMenu.RefreshIndex();
            QuestioningMenu.OnItemSelect += OnItemSelect;

            QuestioningMenu.MouseControlsEnabled = false;
            QuestioningMenu.AllowCameraMovement = true;
            QuestioningMenu.SetMenuWidthOffset(120);

            CourtsMenu = new TabView("~b~~h~San Andreas Court");


            
            CourtsMenu.AddTab(PendingResultsList = new TabSubmenuItem("Pending Results", EmptyItems));
            CourtsMenu.AddTab(PublishedResultsList = new TabSubmenuItem("Results", EmptyItems));

            CourtsMenu.RefreshIndex();

            MainLogic();
        }

        private static void updatePenaltyType(int index)
        {
            if (IssueTicketItem.Collection[index].Value.ToString().Contains("Court"))
            {
                FineItem.Description = "The estimated fine. A judge will decide on the final penalty.";
                PointsItem.Description = "The estimated number of points. A judge will decide on the final penalty.";
                PointsItem.Enabled = false;
                FineItem.Enabled = false;

            }
            else
            {
                FineItem.Description = "";
                PointsItem.Description = "";
                PointsItem.Enabled = true;
                FineItem.Enabled = true;
            }
        }
        private static void OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem changeditem, bool check)
        {
            if (sender == PursuitTacticsMenu && changeditem == AutomaticTacticsCheckboxItem) 
            {
                EnhancedPursuitAI.AutomaticAI = check;
                PursuitTacticsListItem.Enabled = !check;
            }           
        }

        private static void OnIndexChange(UIMenuItem changeditem, int index)
        {
            if (changeditem == IssueTicketItem)
            {
                updatePenaltyType(index);
            }
        }
        private static void OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            //if (sender == ChecksMenu)
            //{
                
            //    if (selectedItem == CheckCourtResultsItem)
            //    {
            //        sender.Visible = false;
            //        CourtsMenu.Visible = true;

            //    }
            //}
            if (sender == TrafficStopMenu)
            {
                if (selectedItem == SpeechItem)
                {
                    string speech = SpeechItem.Collection[SpeechItem.Index].Value.ToString();
                    CurrentEnhancedTrafficStop.PlaySpecificSpeech(speech);

                }
                else if (selectedItem == IDItem)
                {
                    //Ask for ID

                    CurrentEnhancedTrafficStop.AskForID((EnhancedTrafficStop.OccupantSelector)IDItem.Index);

                }
                else if (selectedItem == QuestionDriverItem)
                {
                    sender.Visible = false;

                    UpdateTrafficStopQuestioning();
                    QuestioningMenu.Visible = true;
                }
                else if (selectedItem == PenaltyItem)
                {
                    //Issue ticket(bind menu to item)?
                    sender.Visible = false;
                    //Menus.UpdateTicketReasons();
                    updatePenaltyType(IssueTicketItem.Index);
                    TicketMenu.Visible = true;
                    
                }
                else if (selectedItem == WarningItem)
                {
                    //Let driver go
                    CurrentEnhancedTrafficStop.IssueWarning();
                    _MenuPool.CloseAllMenus();
                }
                else if (selectedItem == OutOfVehicleItem)
                {
                    //Order driver out
                    CurrentEnhancedTrafficStop.OutOfVehicle((EnhancedTrafficStop.OccupantSelector)OutOfVehicleItem.Index);
                    _MenuPool.CloseAllMenus();
                }
            }

            else if (sender == TicketMenu)
            {
                if (selectedItem == IssueTicketItem)
                {
                    //Issue TOR 
                                  
                    bool SeizeVehicle = SeizeVehicleTicketCheckboxItem.Checked;
                    if (Functions.IsPlayerPerformingPullover())
                    {
                        CurrentEnhancedTrafficStop.IssueTicket(SeizeVehicle);
                    }
                    else
                    {
                        GameFiber.StartNew(delegate
                        {
                            EnhancedTrafficStop.performTicketAnimation();
                        });
                    }

                    _MenuPool.CloseAllMenus();
                }
                else if (selectedItem == TicketOffenceSelectorItem)
                {
                    sender.Visible = false;
                    OpenOffencesMenu(sender, CurrentEnhancedTrafficStop.SelectedOffences);
                }
            }
            else if (sender == QuestioningMenu)
            {
                if (selectedItem == IllegalInVehQuestionItem)
                {
                    Game.DisplaySubtitle("~h~" + CurrentEnhancedTrafficStop.AnythingIllegalInVehAnswer);
                }
                else if (selectedItem == DrinkingQuestionItem)
                {
                    Game.DisplaySubtitle("~h~" + CurrentEnhancedTrafficStop.DrinkingAnswer);
                }
                else if (selectedItem == DrugsQuestionItem)
                {
                    Game.DisplaySubtitle("~h~" + CurrentEnhancedTrafficStop.DrugsAnswer);
                }
                else if (selectedItem == SearchPermissionItem)
                {
                    Game.DisplaySubtitle("~h~" + CurrentEnhancedTrafficStop.SearchVehAnswer);
                }
                else if (CustomQuestionsItems.Contains(selectedItem))
                {
                    Game.DisplaySubtitle("~h~" + CurrentEnhancedTrafficStop.CustomQuestionsWithAnswers[CustomQuestionsItems.IndexOf(selectedItem)].Item2);
                }
                else if (CustomQuestionsCallbacksAnswersItems.Contains(selectedItem))
                {
                    Game.DisplaySubtitle("~h~" + CurrentEnhancedTrafficStop.CustomQuestionsWithCallbacksAnswers[CustomQuestionsCallbacksAnswersItems.IndexOf(selectedItem)].Item2(CurrentEnhancedTrafficStop.Suspect));
                }
                else if (CustomQuestionsAnswersCallbackItems.Contains(selectedItem))
                {
                    string Text = CurrentEnhancedTrafficStop.CustomQuestionsAnswerWithCallbacks[CustomQuestionsAnswersCallbackItems.IndexOf(selectedItem)].Item2;
                    Game.DisplaySubtitle("~h~" + Text);

                    CurrentEnhancedTrafficStop.CustomQuestionsAnswerWithCallbacks[CustomQuestionsAnswersCallbackItems.IndexOf(selectedItem)].Item3(CurrentEnhancedTrafficStop.Suspect, Text);
                }
            }
        }

        private static void OnMenuClose(UIMenu sender)
        {
            if (OffenceCategoryMenus.Contains(sender))
            {
                CurrentEnhancedTrafficStop.SelectedOffences.Clear();
                foreach (UIMenu men in OffenceCategoryMenus)
                {
                    foreach (UIMenuItem it in men.MenuItems)
                    {
                        if (it is UIMenuCheckboxItem)
                        {
                            if (((UIMenuCheckboxItem)it).Checked)
                            {
                                CurrentEnhancedTrafficStop.SelectedOffences.Add(CheckboxItems_Offences.FirstOrDefault(x => x.Item1 == it).Item2);
                            }
                        }
                    }
                }
                int fine = CurrentEnhancedTrafficStop.SelectedOffences.Sum(x => x.fine);
                fine = fine - (fine % 5);
                if (fine >  5000) { fine = 5000; }
                else if (fine < 5) { fine = 5; }

                FineItem.Index = fine / 5 - 1;
                int points = CurrentEnhancedTrafficStop.SelectedOffences.Sum(x => x.points);
                points = points - (points % Offence.pointincstep);
                if (points > Offence.maxpoints) { points = Offence.maxpoints; }
                else if (points < Offence.minpoints) { points = Offence.minpoints; }
                PointsItem.Index = PointsList.IndexOf(points.ToString());
                SeizeVehicleTicketCheckboxItem.Checked = CurrentEnhancedTrafficStop.SelectedOffences.Any(x => x.seizeVehicle);

            }
        }

        private static void MainLogic()
        {
            GameFiber.StartNew(delegate
            {
                try
                {
                    while (true)
                    {
                        GameFiber.Yield();
                        if (EnhancedPursuitAI.InPursuit && Game.LocalPlayer.Character.IsInAnyVehicle(false))
                        {
                            if (ExtensionMethods.IsKeyCombinationDownComputerCheck(EnhancedPursuitAI.OpenPursuitTacticsMenuKey, EnhancedPursuitAI.OpenPursuitTacticsMenuModifierKey))
                            {
                                PursuitTacticsMenu.Visible = !PursuitTacticsMenu.Visible;
                            }
                        }
                        else
                        {
                            PursuitTacticsMenu.Visible = false;
                        }

                        if (Functions.IsPlayerPerformingPullover())
                        {
                            if (Functions.GetPulloverSuspect(Functions.GetCurrentPullover()) != CurrentEnhancedTrafficStop.Suspect)
                            {
                                CurrentEnhancedTrafficStop = new EnhancedTrafficStop();

                                StatisticsCounter.AddCountToStatistic("Traffic Stops", "LSPDFR+");
                                Game.LogTrivial("Adding traffic stop count - LSPDFR+");
                                API.Functions.OnTrafficStopInitiated(Functions.GetPulloverSuspect(Functions.GetCurrentPullover()));

                            }
                        }
                        //Shift Q ticket menu handler.
                        else if (!_MenuPool.IsAnyMenuOpen() && !Game.LocalPlayer.Character.IsInAnyVehicle(false) && ExtensionMethods.IsKeyCombinationDownComputerCheck(Offence.OpenTicketMenuKey, Offence.OpenTicketMenuModifierKey)
                        && Game.LocalPlayer.Character.GetNearbyPeds(1)[0].Exists() && Game.LocalPlayer.Character.DistanceTo(Game.LocalPlayer.Character.GetNearbyPeds(1)[0]) < 5f)
                        {

                            Game.LocalPlayer.Character.Tasks.ClearImmediately();
                            _MenuPool.ResetMenus(true, true);
                            CurrentEnhancedTrafficStop.SelectedOffences.Clear();
                            SeizeVehicleTicketCheckboxItem.Enabled = false;
                            TicketMenu.ParentMenu = null;
                            foreach (UIMenu m in OffenceCategoryMenus)
                            {
                                m.Visible = false;
                            }
                            TicketMenu.Visible = true;
                        }

                        if (!LSPDFRPlusHandler.BritishPolicingScriptRunning && ExtensionMethods.IsKeyDownComputerCheck(CourtSystem.OpenCourtMenuKey) && (ExtensionMethods.IsKeyDownRightNowComputerCheck(CourtSystem.OpenCourtMenuModifierKey) || CourtSystem.OpenCourtMenuModifierKey == Keys.None))
                        {
                            if (!CourtsMenu.Visible) { CourtsMenu.Visible = true; }
                        }

                        if (_MenuPool.IsAnyMenuOpen()) { NativeFunction.Natives.SET_PED_STEALTH_MOVEMENT(Game.LocalPlayer.Character, 0, 0); }

                        //Prevent the traffic stop menu from being used when it shouldn't be.
                        if (TrafficStopMenu.Visible)
                        {                           
                            if (!Functions.IsPlayerPerformingPullover())
                            {
                                if (TrafficStopMenuEnabled)
                                {
                                    ToggleUIMenuEnabled(TrafficStopMenu, false);
                                    TrafficStopMenuEnabled = false;
                                }
                            }
                            else if (Vector3.Distance2D(Game.LocalPlayer.Character.Position, Functions.GetPulloverSuspect(Functions.GetCurrentPullover()).Position) > TrafficStopMenuDistance)
                            {
                                if (TrafficStopMenuEnabled)
                                {
                                    ToggleUIMenuEnabled(TrafficStopMenu, false);
                                    TrafficStopMenuEnabled = false;
                                }
                            }
                            else if (!TrafficStopMenuEnabled)
                            {
                                ToggleUIMenuEnabled(TrafficStopMenu, true);
                                TrafficStopMenuEnabled = true;
                            }
                        }

                        if (CourtsMenu.Visible)
                        {

                            if (!CourtsMenuPaused)
                            {
                                CourtsMenuPaused = true;
                                Game.IsPaused = true;
                            }
                            if (ExtensionMethods.IsKeyDownComputerCheck(Keys.Delete))
                            {
                                if (PendingResultsList.Active)
                                {
                                    if (CourtCase.PendingResultsMenuCleared)
                                    {
                                        CourtSystem.DeleteCourtCase(CourtSystem.PendingCourtCases[PendingResultsList.Index]);
                                        PendingResultsList.Index = 0;
                                    }
                                }
                                else if (PublishedResultsList.Active)
                                {
                                    if (CourtCase.ResultsMenuCleared)
                                    {
                                        CourtSystem.DeleteCourtCase(CourtSystem.PublishedCourtCases[PublishedResultsList.Index]);

                                        PublishedResultsList.Index = 0;
                                    }
                                }
                            }

                            if (ExtensionMethods.IsKeyDownComputerCheck(Keys.Insert))
                            {
                                if (PendingResultsList.Active)
                                {
                                    if (CourtCase.PendingResultsMenuCleared)
                                    {
                                        CourtSystem.PendingCourtCases[PendingResultsList.Index].ResultsPublishTime = DateTime.Now;
                                        PendingResultsList.Index = 0;
                                    }
                                }
                            }
                        }
                        else if (CourtsMenuPaused)
                        {
                            CourtsMenuPaused = false;
                            Game.IsPaused = false;
                        }

                    }
                }
                catch (System.Threading.ThreadAbortException e) { }
                catch (Exception e) { Game.LogTrivial(e.ToString()); }
            });
        }

        //Huge method to handle the traffic stop questioning layout.
        public static void UpdateTrafficStopQuestioning()
        {
            if (Functions.IsPlayerPerformingPullover())
            {
                CurrentEnhancedTrafficStop.UpdateTrafficStopQuestioning();
                ToggleStandardQuestions(CurrentEnhancedTrafficStop.StandardQuestionsEnabled);
                if (CurrentEnhancedTrafficStop.StandardQuestionsEnabled)
                {
                    IllegalInVehQuestionItem.Text = CurrentEnhancedTrafficStop.AnythingIllegalInVehQuestion;
                    DrinkingQuestionItem.Text = CurrentEnhancedTrafficStop.DrinkingQuestion;
                    DrugsQuestionItem.Text = CurrentEnhancedTrafficStop.DrugsQuestion;
                    SearchPermissionItem.Text = CurrentEnhancedTrafficStop.SearchVehQuestion;
                }
                if (CustomQuestionsItems.Count > 0)
                {
                    foreach (UIMenuItem item in QuestioningMenu.MenuItems.ToArray())
                    {
                        if (CustomQuestionsItems.Contains(item))
                        {
                            QuestioningMenu.RemoveItemAt(QuestioningMenu.MenuItems.IndexOf(item));
                        }
                    }
                    CustomQuestionsItems.Clear();
                }
                foreach (Tuple<string, string> tuple in CurrentEnhancedTrafficStop.CustomQuestionsWithAnswers)
                {
                    UIMenuItem customquestionitem = new UIMenuItem(tuple.Item1);
                    QuestioningMenu.AddItem(customquestionitem);
                    CustomQuestionsItems.Add(customquestionitem);

                }
                if (CustomQuestionsCallbacksAnswersItems.Count > 0)
                {
                    foreach (UIMenuItem item in QuestioningMenu.MenuItems.ToArray())
                    {
                        if (CustomQuestionsCallbacksAnswersItems.Contains(item))
                        {
                            QuestioningMenu.RemoveItemAt(QuestioningMenu.MenuItems.IndexOf(item));
                        }
                    }
                    CustomQuestionsCallbacksAnswersItems.Clear();
                }
                foreach (Tuple<string, Func<Ped, string>> tuple in CurrentEnhancedTrafficStop.CustomQuestionsWithCallbacksAnswers)
                {
                    UIMenuItem customquestionitem = new UIMenuItem(tuple.Item1);
                    QuestioningMenu.AddItem(customquestionitem);
                    CustomQuestionsCallbacksAnswersItems.Add(customquestionitem);

                }

                if (CustomQuestionsAnswersCallbackItems.Count > 0)
                {
                    foreach (UIMenuItem item in QuestioningMenu.MenuItems.ToArray())
                    {
                        if (CustomQuestionsAnswersCallbackItems.Contains(item))
                        {
                            QuestioningMenu.RemoveItemAt(QuestioningMenu.MenuItems.IndexOf(item));
                        }
                    }
                    CustomQuestionsAnswersCallbackItems.Clear();
                }
                foreach (Tuple<string,string, Action<Ped, string>> tuple in CurrentEnhancedTrafficStop.CustomQuestionsAnswerWithCallbacks)
                {
                    UIMenuItem customquestionitem = new UIMenuItem(tuple.Item1);
                    QuestioningMenu.AddItem(customquestionitem);
                    CustomQuestionsAnswersCallbackItems.Add(customquestionitem);

                }


            }
        }
        public static float TrafficStopMenuDistance = 3.7f;
        private static EnhancedTrafficStop CurrentEnhancedTrafficStop = new EnhancedTrafficStop();
        private static bool CourtsMenuPaused = false;
        private static void Process(object sender, GraphicsEventArgs e)
        {
            try
            {
                if (Functions.IsPlayerPerformingPullover() && !_MenuPool.IsAnyMenuOpen() && EnhancedTrafficStop.EnhancedTrafficStopsEnabled)
                {
                    Ped pulloverSuspect = Functions.GetPulloverSuspect(Functions.GetCurrentPullover());
                    if (pulloverSuspect &&
                        pulloverSuspect.IsInAnyVehicle(false) &&
                        Vector3.Distance2D(Game.LocalPlayer.Character.Position, pulloverSuspect.CurrentVehicle.Position) < TrafficStopMenuDistance + 0.1f
                        )
                    {
                        //ExtensionNamespace.Extensions.DisEnableControls(false);
                        //ExtensionNamespace.Extensions.DisableTrafficStopControls();

                        if (ExtensionMethods.IsKeyDownComputerCheck(EnhancedTrafficStop.BringUpTrafficStopMenuKey) || Game.IsControllerButtonDown(EnhancedTrafficStop.BringUpTrafficStopMenuControllerButton))
                        {
                            _MenuPool.ResetMenus(true, true);
                            SeizeVehicleTicketCheckboxItem.Enabled = true;
                            TicketMenu.ParentMenu = TrafficStopMenu;
                            TicketMenu.Visible = false;
                            foreach (UIMenu m in OffenceCategoryMenus)
                            {
                                m.Visible = false;
                            }
                            TrafficStopMenu.Visible = true;

                        }
                    }
                }

                _MenuPool.ProcessMenus();
                if (CourtsMenu.Visible)
                {
                    Game.IsPaused = true;
                    CourtsMenu.Update();
                }
            }
            catch (Exception exception)
            {
                Game.LogTrivial($"Handled {exception}");
            }
           
        }

        private static void ToggleUIMenuEnabled(UIMenu menu, bool Enabled)
        {

            foreach (UIMenuItem item in menu.MenuItems)
            {
                item.Enabled = Enabled;               
            }

        }
    }
}
