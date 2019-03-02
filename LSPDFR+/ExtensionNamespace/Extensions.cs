using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSPDFR_.ExtensionNamespace
{
    static class Extensions
    {

        public static void ShowDrivingLicence(this Ped p)
        {
            if (p)
            {
                if (LSPDFRPlusHandler.BritishPolicingScriptRunning)
                {
                    API.BritishPolicingScriptFunctions.RequestDetails(p);
                }
                else
                {
                    Persona pers = Functions.GetPersonaForPed(p);
                    Game.DisplayNotification("mpcharselect", "mp_generic_avatar", "STATE ISSUED IDENTIFICATION", pers.FullName, "~b~" + pers.FullName + "~n~~y~" + pers.Gender + "~s~. Born: ~y~" + pers.Birthday.ToShortDateString());
                }
            }
        }     
        
        public static float GetRandomFloat(float minimum, float maximum)
        {

            return (float)LSPDFRPlusHandler.rnd.NextDouble() * (maximum - minimum) + minimum;
        }

        public static float DistanceTo(this Vector3 start, Vector3 end)
        {
            return (end - start).Length();
        }

        public static Vector3 RandomXY()
        {


            Vector3 vector3 = new Vector3();
            vector3.X = (float)(LSPDFRPlusHandler.rnd.NextDouble() - 0.5);
            vector3.Y = (float)(LSPDFRPlusHandler.rnd.NextDouble() - 0.5);
            vector3.Z = 0.0f;
            vector3.Normalize();
            return vector3;
        }

        public static void DisableTrafficStopControls()
        {
            foreach (GameControl ctrl in trafficStopControls)
            {
                NativeFunction.Natives.DisableControlAction(0, (int)ctrl, true);
            }
        }

        private static readonly GameControl[] trafficStopControls =
        {
            GameControl.Context,
            GameControl.ContextSecondary,
            GameControl.WeaponWheelNext,
            GameControl.WeaponSpecialTwo,
            GameControl.VehicleGrapplingHook,
            GameControl.VehicleFlyVerticalFlightMode,

        };


        public static void DisEnableControls(bool enable)
        {
            if (enable)
            {
                NativeFunction.Natives.EnableAllControlActions(0);
                return;
            }
            else
            {
                NativeFunction.Natives.DisableAllControlActions(0);
            }
            //Controls we want
            // -Frontend
            // -Mouse
            // -Walk/Move
            // -
            

            for (int i = 0; i < menuNavigationNeededControls.Length; i++)
            {
                NativeFunction.Natives.EnableControlAction(0, (int)menuNavigationNeededControls[i], true);
            }
        }

        private static readonly GameControl[] menuNavigationNeededControls =
        {
            GameControl.FrontendAccept,
            GameControl.FrontendAxisX,
            GameControl.FrontendAxisY,
            GameControl.FrontendDown,
            GameControl.FrontendUp,
            GameControl.FrontendLeft,
            GameControl.FrontendRight,
            GameControl.FrontendCancel,
            GameControl.FrontendSelect,
            GameControl.FrontendPause,
            GameControl.FrontendPauseAlternate,
            GameControl.SelectWeapon,
            GameControl.SelectWeaponAutoRifle,
            GameControl.SelectWeaponHandgun,
            GameControl.SelectWeaponHeavy,
            GameControl.SelectWeaponMelee,
            GameControl.SelectWeaponShotgun,
            GameControl.SelectWeaponSmg,
            GameControl.SelectWeaponSniper,
            GameControl.SelectWeaponSpecial,
            GameControl.SelectWeaponUnarmed,
            GameControl.WeaponWheelLeftRight,
            GameControl.WeaponWheelUpDown,            
            GameControl.WeaponWheelPrev,
            GameControl.CursorScrollDown,
            GameControl.CursorScrollUp,
            GameControl.CursorX,
            GameControl.CursorY,
            GameControl.MoveUpDown,
            GameControl.MoveLeftRight,
            GameControl.Sprint,
            GameControl.Jump,
            GameControl.Enter,
            GameControl.VehicleExit,
            GameControl.VehicleAccelerate,
            GameControl.VehicleBrake,
            GameControl.VehicleMoveLeftRight,
            GameControl.VehicleFlyYawLeft,
            GameControl.ScriptedFlyLeftRight,
            GameControl.ScriptedFlyUpDown,
            GameControl.VehicleFlyYawRight,
            GameControl.VehicleHandbrake,
            GameControl.LookUpDown,
            GameControl.LookLeftRight,
            GameControl.Aim,
            GameControl.Attack,
            GameControl.ReplayStartStopRecording,
            GameControl.ReplayStartStopRecordingSecondary
        };
    }
}
