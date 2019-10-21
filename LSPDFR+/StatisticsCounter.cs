using Rage;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;

namespace LSPDFR_
{
    internal static class StatisticsCounter
    {
        public static string StatisticsFilePath = "Plugins/LSPDFR/LSPDFR+/Statistics.xml";
       

        
        public static void AddCountToStatistic(string Statistic, string PluginName)
        {

            try
            {
                SimpleAES StringEncrypter = new SimpleAES();
                Directory.CreateDirectory(Directory.GetParent(StatisticsFilePath).FullName);
                if (!File.Exists(StatisticsFilePath))
                {

                    new XDocument(
                        new XElement("LSPDFRPlus")
                    )
                    .Save(StatisticsFilePath);

                }              
                
                string pswd = Environment.UserName;
                
                string EncryptedStatistic = XmlConvert.EncodeName(StringEncrypter.EncryptToString(Statistic + PluginName + pswd));

                string EncryptedPlugin = XmlConvert.EncodeName(StringEncrypter.EncryptToString(PluginName + pswd));
                
                XDocument xdoc = XDocument.Load(StatisticsFilePath);
                char[] trim = new char[] { '\'', '\"', ' ' };
                XElement LSPDFRPlusElement;
                if (xdoc.Element("LSPDFRPlus") == null)
                {
                    LSPDFRPlusElement = new XElement("LSPDFRPlus");
                    xdoc.Add(LSPDFRPlusElement);
                }

                LSPDFRPlusElement = xdoc.Element("LSPDFRPlus");
                XElement StatisticElement;
                if (LSPDFRPlusElement.Elements(EncryptedStatistic).Where(x => (string)x.Attribute("Plugin") == EncryptedPlugin).ToList().Count == 0)
                {
                    //Game.LogTrivial("Creating new statistic entry.");
                    StatisticElement = new XElement(EncryptedStatistic);
                    StatisticElement.Add(new XAttribute("Plugin", EncryptedPlugin));
                    LSPDFRPlusElement.Add(StatisticElement);
                }
                StatisticElement = LSPDFRPlusElement.Elements(EncryptedStatistic).Where(x => (string)x.Attribute("Plugin") == EncryptedPlugin).FirstOrDefault();
                int StatisticCount;
                if (StatisticElement.IsEmpty)
                {
                    StatisticCount = 0;
                }
                else
                {
                    string DecryptedStatistic = StringEncrypter.DecryptString(XmlConvert.DecodeName(StatisticElement.Value));
                    //Game.LogTrivial("Decryptedstatistic: " + DecryptedStatistic);
                    int index = DecryptedStatistic.IndexOf(EncryptedStatistic);
                    string cleanPath = (index < 0)
                        ? "0"
                        : DecryptedStatistic.Remove(index, EncryptedStatistic.Length);
                    //if (cleanPath == "0") { Game.LogTrivial("Cleanpath debug 2"); }
                    
                    index = cleanPath.IndexOf(pswd);
                    cleanPath = (index < 0)
                        ? "0"
                        : cleanPath.Remove(index, pswd.Length);
                    //if (cleanPath == "0") { Game.LogTrivial("Cleanpath debug 1"); }
                    StatisticCount = int.Parse(cleanPath);


                }
                //Game.LogTrivial("Statisticscount: " + StatisticCount.ToString());
                StatisticCount++;
                string ValueToWrite = StatisticCount.ToString() + pswd;
                int indextoinsertat = LSPDFRPlusHandler.rnd.Next(ValueToWrite.Length);
                ValueToWrite = ValueToWrite.Substring(0, indextoinsertat) + EncryptedStatistic + ValueToWrite.Substring(indextoinsertat);
                //Game.LogTrivial("Valueotwrite: " + ValueToWrite);
                StatisticElement.Value = XmlConvert.EncodeName(StringEncrypter.EncryptToString(ValueToWrite));

                xdoc.Save(StatisticsFilePath);

            }
            catch (System.Threading.ThreadAbortException e) { }
            catch (Exception e)
            {
                Game.LogTrivial("LSPDFR+ encountered a statistics exception. It was: " + e.ToString());
                Game.DisplayNotification("~r~LSPDFR+: Statistics error.");
            }
        }

        public static void OnPedArrested(Ped ped)
        {
            Game.LogTrivial("LSPDFR+ Ped arrested");
            AddCountToStatistic("People Arrested", "LSPDFR+");
        }

        internal const string PublicAPIKey = "<RSAKeyValue><Modulus>yeGjbl4QaPJ2izTMnXC6hpXGQB/J6xlNbvRhMsxc+BjgO58VLrwd4C6eA0CY8SbLLClm+eoH5rvM1didx91zK7RklksFOzc7aDh4Y9tQ7+Eym4ZlPhI5kjLtrgg4kc3Aq8Yt8Tu/3H1bl2Sn+EHtnWflOvo3HXebUeiOA5rltMc=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        internal static RSACryptoServiceProvider ReadPublicKey()
        {
            
            CspParameters cspParams = new CspParameters();
            cspParams.ProviderType = 1;
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
            rsaProvider.FromXmlString(PublicAPIKey);
            return rsaProvider;
        }
    }
}
