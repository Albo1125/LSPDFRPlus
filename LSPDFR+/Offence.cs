using Rage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace LSPDFR_
{
    public class Offence
    {
        public string name = "Default";
        public int points = 0;
        public int fine = 5;
        public bool seizeVehicle = false;
        public string offenceCategory = "Default";
        public override string ToString()
        {
            return "OFFENCE<" + name + points + fine + seizeVehicle + offenceCategory + ">";
        }


        internal static Dictionary<string, List<Offence>> CategorizedTrafficOffences = new Dictionary<string, List<Offence>>();
        internal static List<Offence> DeserializeOffences()
        {
            List<Offence> AllOffences = new List<Offence>();
            if (Directory.Exists("Plugins/LSPDFR/LSPDFR+/Offences"))
            {
                foreach (string file in Directory.EnumerateFiles("Plugins/LSPDFR/LSPDFR+/Offences", "*.xml", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        using (var reader = new StreamReader(file))
                        {
                            XmlSerializer deserializer = new XmlSerializer(typeof(List<Offence>),
                                new XmlRootAttribute("Offences"));
                            AllOffences.AddRange((List<Offence>)deserializer.Deserialize(reader));
                        }
                    }
                    catch (Exception e)
                    {
                        Game.LogTrivial(e.ToString());
                        Game.LogTrivial("LSPDFR+ - Error parsing XML from " + file);
                    }
                }
            }
            else
            {
                
            }
            if (AllOffences.Count == 0)
            {
                AllOffences.Add(new Offence());
                Game.DisplayNotification("~r~~h~LSPDFR+ couldn't find a valid XML file with offences in Plugins/LSPDFR/LSPDFR+/Offences. Setting just the default offence.");
            }
            CategorizedTrafficOffences = AllOffences.GroupBy(x => x.offenceCategory).ToDictionary(x => x.Key, x => x.ToList());
            return AllOffences;
        }
        internal static int maxpoints = 12;
        internal static int minpoints = 0;
        internal static int pointincstep = 1;
        internal static int maxFine = 5000;

        internal static Keys OpenTicketMenuKey = Keys.Q;
        internal static Keys OpenTicketMenuModifierKey = Keys.LShiftKey;

        internal static string currency = "$";
        internal static bool enablePoints = true;
    }
}
