using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;

namespace LSPDFR_.API
{
    internal static class BritishPolicingScriptFunctions
    {

        public static void RegisterBPSOutOfVehicleEvent()
        {
            British_Policing_Script.API.Functions.PedOrderedOutOfVehicle += Functions.OnPedOrderedOutOfVehicle;
        }

        public static void RequestDetails(Ped p)
        {
            if (p.Exists())
            {
                British_Policing_Script.API.Functions.GetBritishPersona(p).RequestDetails();
            }
        }

    }

    internal static class ArrestManagerFuncs
    {
        public static void PlayerArrestedPedHandler()
        {
            Arrest_Manager.API.Functions.PlayerArrestedPed += StatisticsCounter.OnPedArrested;
        }

        public static void RequestTowForVeh(Vehicle veh)
        {
            Arrest_Manager.API.Functions.RequestTowTruck(veh);
        }
    }

    internal static class TrafficPolicerFuncs
    {
        public static bool IsPedOverAlcoholLimit(Ped p)
        {
            return Traffic_Policer.API.Functions.IsPedOverTheAlcoholLimit(p);
        }

        public static bool IsPedOverDrugsLimit(Ped p)
        {
            return Traffic_Policer.API.Functions.DoesPedHaveDrugsInSystem(p);
        }
    }
}
