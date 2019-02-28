using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Engine.Scripting.Entities;
using System.Reflection;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using Albo1125.Common.CommonLibrary;

namespace LSPDFR_.API
{
    public delegate void PedEvent(Ped ped);
    public static class Functions
    {
        #region courtsystem
        /// <summary>
        /// Adds a new court case to the court system.
        /// </summary>
        /// <param name="DefendantPersona">LSPDFR persona of the defendant</param>
        /// <param name="Crime">String describing the crime committed, e.g. 'stealing a police vehicle'.</param>
        /// <param name="GuiltyChance">Percentage based chance of the suspect being found guilty. 100 = always guilty, 0 = never guilty.</param>
        /// <param name="CourtVerdict">The decision the court will come to, e.g. 'Sentenced to 5 months in prison'</param>
        public static void CreateNewCourtCase(Persona DefendantPersona, string Crime, int GuiltyChance, string CourtVerdict)
        {
            if (DefendantPersona != null)
            {
                Game.LogTrivial("LSPDFR+ API adding new court case.");

                CourtSystem.CreateNewCourtCase(DefendantPersona.FullName, DefendantPersona.Birthday, Crime, DateTime.Now, GuiltyChance, CourtVerdict, CourtSystem.DetermineCourtHearingDate(), false);
            }
            else
            {
                Game.LogTrivial("LSPDFR+ API error: DefendantPersona null.");
            }
        }

        /// <summary>
        /// Adds a new court case to the court system (this overload is recommended only for debugging by setting an instant publish time).
        /// </summary>
        /// <param name="DefendantPersona">LSPDFR persona of the defendant</param>
        /// <param name="Crime">String describing the crime committed, e.g. 'stealing a police vehicle'.</param>
        /// <param name="GuiltyChance">Percentage based chance of the suspect being found guilty. 100 = always guilty, 0 = never guilty.</param>
        /// <param name="CourtVerdict">The decision the court will come to, e.g. 'Sentenced to 5 months in prison'</param>
        /// <param name="ResultsPublishTime">The DateTime when the results will become available to the player (use not recommended in release builds).</param>
        public static void CreateNewCourtCase(Persona DefendantPersona, string Crime, int GuiltyChance, string CourtVerdict, DateTime ResultsPublishTime)
        {
            if (DefendantPersona != null)
            {
                Game.LogTrivial("LSPDFR+ API adding new court case.");

                CourtSystem.CreateNewCourtCase(DefendantPersona.FullName, DefendantPersona.Birthday, Crime, DateTime.Now, GuiltyChance, CourtVerdict, ResultsPublishTime, false);
            }
            else
            {
                Game.LogTrivial("LSPDFR+ API error: DefendantPersona null.");
            }
        }

        /// <summary>
        /// Returns a court verdict for a prison sentence depending on the parameters.
        /// </summary>
        /// <param name="MinMonths"></param>
        /// <param name="MaxMonths"></param>
        /// <param name="SuspendedChance">Percentage based chance of the sentence being suspended. 100 = always suspended, 0 = never suspended.</param>
        /// <returns></returns>
        public static string DeterminePrisonSentence(int MinMonths, int MaxMonths, int SuspendedChance)
        {
            return CourtSystem.DeterminePrisonSentence(MinMonths, MaxMonths, SuspendedChance);
        }

        /// <summary>
        /// Returns a court verdict for a fine depending on the parameters.
        /// </summary>
        /// <param name="MinFine"></param>
        /// <param name="MaxFine"></param>
        /// <returns></returns>
        public static string DetermineFineSentence(int MinFine, int MaxFine)
        {
            return CourtSystem.DetermineFineSentence(MinFine, MaxFine);
        }
        #endregion

        #region TrafficstopQuestions
        /// <summary>
        /// Adds a custom question to the traffic stop questioning section.
        /// </summary>
        /// <param name="Suspect">The ped for whom the question should appear (must have this ped stopped for the question to appear).</param>
        /// <param name="Question"></param>
        /// <param name="Answer"></param>
        public static void AddQuestionToTrafficStop(Ped Suspect, string Question, string Answer)
        {
            Game.LogTrivial("LSPDFR+ API adding new question to Traffic Stop - 1.");
            EnhancedTrafficStop.PedsWithCustomTrafficStopQuestionsAndAnswers.Add(Suspect, Question, Answer);
            Menus.UpdateTrafficStopQuestioning();
            
        }

        /// <summary>
        /// Adds a custom question to the traffic stop questioning section.
        /// </summary>
        /// <param name="Suspect">The ped for whom the question should appear (must have this ped stopped for the question to appear).</param>
        /// <param name="Question"></param>
        /// <param name="Answers">A list of possible answers. A random one will be selected.</param>
        public static void AddQuestionToTrafficStop(Ped Suspect, string Question, List<string> Answers)
        {
            Game.LogTrivial("LSPDFR+ API adding new question to Traffic Stop - 2.");
            string Answer = Answers[LSPDFRPlusHandler.rnd.Next(Answers.Count)];
            EnhancedTrafficStop.PedsWithCustomTrafficStopQuestionsAndAnswers.Add(Suspect, Question, Answer);
            Menus.UpdateTrafficStopQuestioning();

        }
        /// <summary>
        /// Adds a custom question to the traffic stop questioning section.
        /// </summary>
        /// <param name="Suspect">The ped for whom the question should appear (must have this ped stopped for the question to appear).</param>
        /// <param name="Questions">A list of possible questions. A random one will be selected.</param>
        /// <param name="Answers">A list of possible answers. A random one will be selected.</param>
        public static void AddQuestionToTrafficStop(Ped Suspect, List<string> Questions, List<string> Answers)
        {
            Game.LogTrivial("LSPDFR+ API adding new question to Traffic Stop - 3.");
            string Answer = Answers[LSPDFRPlusHandler.rnd.Next(Answers.Count)];
            string Question = Questions[LSPDFRPlusHandler.rnd.Next(Questions.Count)];
            EnhancedTrafficStop.PedsWithCustomTrafficStopQuestionsAndAnswers.Add(Suspect, Question, Answer);
            Menus.UpdateTrafficStopQuestioning();

        }

        /// <summary>
        /// Adds a custom question to the traffic stop questioning section.
        /// </summary>
        /// <param name="Suspect">The ped for whom the question should appear (must have this ped stopped for the question to appear).</param>
        /// <param name="Question"></param>
        /// <param name="CallbackAnswer">The function passed will be called when the question is asked. The suspect will be passed to the function. It must return a string, which will be used as an answer.</param>
        public static void AddQuestionToTrafficStop(Ped Suspect, string Question, Func<Ped, string> CallbackAnswer)
        {
            Game.LogTrivial("LSPDFR+ API adding new question to Traffic Stop - 4.");
            EnhancedTrafficStop.PedsCustomTrafficStopQuestionsAndCallBackAnswer.Add(Suspect, Question, CallbackAnswer);
            Menus.UpdateTrafficStopQuestioning();

        }

        /// <summary>
        /// Adds a custom question to the traffic stop questioning section.
        /// </summary>
        /// <param name="Suspect">The ped for whom the question should appear (must have this ped stopped for the question to appear).</param>
        /// <param name="Question"></param>
        /// <param name="Answers">A list of possible answers. A random one will be selected.</param>
        /// <param name="Callback">The function passed will be called when the question is asked. The suspect and the chosen answer will be passed to the function as parameters.</param>
        public static void AddQuestionToTrafficStop(Ped Suspect, string Question, List<string> Answers, Action<Ped, string> Callback)
        {
            Game.LogTrivial("LSPDFR+ API adding new question to Traffic Stop - 5.");
            string Answer = Answers[LSPDFRPlusHandler.rnd.Next(Answers.Count)];
            EnhancedTrafficStop.PedsCustomQuestionsAnswerCallback.Add(Suspect, Question, Answer, Callback);
            Menus.UpdateTrafficStopQuestioning();

        }

        /// <summary>
        /// Hides the standard traffic stop questions for the specified ped.
        /// </summary>
        /// <param name="Suspect">Traffic stop questions will be hidden if this ped is the current suspect.</param>
        /// <param name="Hide">If true, hides standard questions. If false, shows standard questions.</param>
        public static void HideStandardTrafficStopQuestions(Ped Suspect, bool Hide)
        {
            Game.LogTrivial("LSPDFR+ API hiding standard questions: " + Hide.ToString());
            if (Hide)
            {
                EnhancedTrafficStop.PedsWhereStandardQuestionsAreHidden.Add(Suspect);
            }
            else
            {
                EnhancedTrafficStop.PedsWhereStandardQuestionsAreHidden.RemoveAll(x => x == Suspect);
            }
            Menus.UpdateTrafficStopQuestioning();
        }

        /// <summary>
        /// Resets the traffic stop questions to default for the specified suspect.
        /// </summary>
        /// <param name="Suspect"></param>
        public static void ResetTrafficStopQuestions(Ped Suspect)
        {
            EnhancedTrafficStop.PedsWhereStandardQuestionsAreHidden.RemoveAll(x => x == Suspect);
            EnhancedTrafficStop.PedsCustomTrafficStopQuestionsAndCallBackAnswer.RemoveAll(x => x.Item1 == Suspect);
            EnhancedTrafficStop.PedsWithCustomTrafficStopQuestionsAndAnswers.RemoveAll(x => x.Item1 == Suspect);
            Menus.UpdateTrafficStopQuestioning();
        }

        /// <summary>
        /// Raised whenever the player orders a ped out of a vehicle on a traffic stop.
        /// </summary>
        public static event PedEvent PedOrderedOutOfVehicle;
        internal static void OnPedOrderedOutOfVehicle(Ped ped)
        {

            if (PedOrderedOutOfVehicle != null)
            {
                PedOrderedOutOfVehicle(ped);
            }
        }

        /// <summary>
        /// Raised whenever the player initiates a traffic stop on a suspect.
        /// </summary>
        public static event PedEvent TrafficStopInitiated;
        internal static void OnTrafficStopInitiated (Ped ped)
        {
            if (TrafficStopInitiated != null)
            {
                TrafficStopInitiated(ped);
            }
        }
        #endregion

        /// <summary>
        /// Raised whenever the player joins a pursuit that's active.
        /// </summary>
        public static event Action PlayerJoinedActivePursuit;
        internal static void OnPlayerJoinedActivePursuit()
        {
            if (PlayerJoinedActivePursuit != null)
            {
                PlayerJoinedActivePursuit();
            }
        }

        /// <summary>
        /// Returns the current pursuit tactic.
        /// </summary>
        /// <returns></returns>
        public static PursuitTactics GetCurrentPursuitTactics()
        {
            return EnhancedPursuitAI.CurrentPursuitTactic;
        }

        /// <summary>
        /// Returns true if automatic tactics are enabled for pursuits, and false if not.
        /// </summary>
        /// <returns></returns>
        public static bool ArePursuitTacticsAutomatic()
        {
            return EnhancedPursuitAI.AutomaticAI;
        }
    }

    /// <summary>
    /// These functions require verification with me (Albo1125) beforehand to ensure fairness. To get verified, please contact me.
    /// </summary>
    public static class ProtectedFunctions
    {

        private static List<string> ProtectedStatistics = new List<string>()
        {
            "people arrested", "times gone on duty", "pursuits", "traffic stops", "traffic stop - tickets issued", "minutes spent on duty"
        };

        /// <summary>
        /// Increases the specified statistic by one.
        /// </summary>
        /// <param name="SecurityGuid">Use GenerateSecurityGuid().</param>
        /// <param name="Statistic">The statistic to increase.</param>
        [Obsolete("Security GUIDs are no longer used, use other overload.", true)]
        public static void AddCountToStatistic(Guid SecurityGuid, string Statistic)
        {
            Game.LogTrivial("LSPDFR+ API: Security GUIDs are no longer used, use other overload.");            
        }

        /// <summary>
        /// Increases the specified statistic by one.
        /// </summary>
        /// <param name="PluginName">The name of your plugin.</param>
        /// <param name="Statistic">The statistic to increase.</param>
        public static void AddCountToStatistic(string PluginName, string Statistic)
        {           
            if (!ProtectedStatistics.Contains(Statistic.ToLower()))
            {
                Game.LogTrivial("LSPDFR+ API: Plugin is increasing statistic: " + Statistic);
                StatisticsCounter.AddCountToStatistic(Statistic, PluginName);
            }           
        }

        /// <summary>
        /// If Signature matches with the passed ExecutingAssembly, PluginName and AuthorName, returns a security Guid to allow use of this class's functions.
        /// </summary>
        /// <param name="ExecutingAssembly">Pass the following: System.Reflection.Assembly.GetExecutingAssembly()</param>
        /// <param name="PluginName">Exact PluginName as agreed between you and me.</param>
        /// <param name="AuthorName">Exact AuthorName as agreed between you and me.</param>
        /// <param name="Signature">The Signature as you obtained from me.</param>
        /// <returns>If verification is successful, returns a security Guid. If unsuccessful, returns an empty Guid.</returns>
        [Obsolete("Security GUIDs are no longer used. Call methods directly.", true)]
        public static Guid GenerateSecurityGuid(Assembly ExecutingAssembly, string PluginName, string AuthorName, string Signature)
        {
            Game.LogTrivial("LSPDFR+: Security GUIDs are now obsolete.");
            return Guid.Empty;           
        }
    }
}
