using GTANetworkAPI;
using NeptuneEvo.Handles;
using System;
using System.Collections.Generic;
using System.Linq;
using NeptuneEvo.Core;
using NeptuneEvo.Chars;
using NeptuneEvo.Functions;
using NeptuneEvo.Fractions.Models;
using NeptuneEvo.GUI;
using NeptuneEvo.Players;
using NeptuneEvo.Players.Models;
using NeptuneEvo.Character;
using NeptuneEvo.Character.Models;
using NeptuneEvo.Players.Popup.List.Models;
using Newtonsoft.Json;
using Redage.SDK;

namespace NeptuneEvo.Fractions
{
    /// <summary>
    /// Organization Clothing System
    /// Each org has unique uniforms accessible from their locker rooms
    /// Players can change into duty uniform and back to civilian clothes
    /// </summary>
    class OrgClothing : Script
    {
        private static readonly nLog Log = new nLog("Fractions.OrgClothing");

        #region Clothing Definitions

        // Locker room positions (same as BuildingInterior.LockerRoom)
        public static Dictionary<int, Vector3> LockerRooms = new Dictionary<int, Vector3>()
        {
            { 7, new Vector3(451.3f, -980.2f, 30.7f) },    // LSPD
            { 18, new Vector3(-441.9f, 6007.8f, 31.7f) },  // SAHP
            { 9, new Vector3(130.8f, -760.5f, 46.75f) },   // FIB
            { 15, new Vector3(-1075.3f, -252.4f, 37.76f) },// LifeInvader
            { 8, new Vector3(307.1f, -585.4f, 43.29f) },   // Hospital
            { 19, new Vector3(1840.1f, 2580.9f, 45.67f) }, // Jail
            { 14, new Vector3(-2355.2f, 3244.7f, 92.9f) }, // Army
        };

        // Uniform sets per organization
        // Format: [torso_drawable, torso_texture, legs_drawable, legs_texture, shoes_drawable, shoes_texture, hat_drawable, hat_texture]
        public static Dictionary<int, Dictionary<string, int[]>> OrgUniforms = new Dictionary<int, Dictionary<string, int[]>>()
        {
            // LSPD
            { 7, new Dictionary<string, int[]>()
                {
                    { "Patrol Officer",    new int[] { 55, 0, 35, 0, 25, 0, 46, 0 } },
                    { "Detective",         new int[] { 4, 0, 10, 0, 10, 0, -1, 0 } },
                    { "SWAT Tactical",     new int[] { 230, 0, 31, 0, 25, 0, 150, 0 } },
                    { "Traffic Unit",      new int[] { 55, 2, 35, 2, 25, 0, 46, 2 } },
                    { "Bike Patrol",       new int[] { 250, 0, 130, 0, 25, 0, 39, 0 } },
                    { "Civilian Clothes",  new int[] { 0, 0, 0, 0, 0, 0, -1, 0 } },
                }
            },
            // SAHP
            { 18, new Dictionary<string, int[]>()
                {
                    { "Highway Patrol",    new int[] { 55, 3, 35, 3, 25, 0, 46, 3 } },
                    { "Desert Patrol",     new int[] { 55, 8, 35, 4, 25, 0, 46, 4 } },
                    { "K-9 Unit",          new int[] { 55, 1, 35, 1, 25, 0, 46, 1 } },
                    { "Tactical Response", new int[] { 230, 2, 31, 2, 25, 0, 150, 2 } },
                    { "Civilian Clothes",  new int[] { 0, 0, 0, 0, 0, 0, -1, 0 } },
                }
            },
            // FIB
            { 9, new Dictionary<string, int[]>()
                {
                    { "Field Agent",       new int[] { 4, 0, 10, 0, 10, 0, -1, 0 } },
                    { "Tactical Agent",    new int[] { 230, 5, 31, 5, 25, 0, 150, 3 } },
                    { "Undercover",        new int[] { 11, 0, 4, 0, 12, 0, -1, 0 } },
                    { "Office Attire",     new int[] { 6, 0, 10, 1, 10, 0, -1, 0 } },
                    { "HRT Operator",      new int[] { 230, 10, 31, 10, 25, 0, 150, 5 } },
                    { "Civilian Clothes",  new int[] { 0, 0, 0, 0, 0, 0, -1, 0 } },
                }
            },
            // LifeInvader News
            { 15, new Dictionary<string, int[]>()
                {
                    { "Reporter",          new int[] { 6, 0, 10, 0, 10, 0, -1, 0 } },
                    { "Camera Crew",       new int[] { 11, 2, 4, 2, 1, 0, 13, 0 } },
                    { "Anchor",            new int[] { 4, 4, 10, 4, 10, 2, -1, 0 } },
                    { "Field Journalist",  new int[] { 14, 0, 4, 0, 1, 0, 13, 2 } },
                    { "Civilian Clothes",  new int[] { 0, 0, 0, 0, 0, 0, -1, 0 } },
                }
            },
            // Hospital EMS
            { 8, new Dictionary<string, int[]>()
                {
                    { "Paramedic",         new int[] { 23, 0, 36, 0, 25, 0, -1, 0 } },
                    { "Doctor",            new int[] { 70, 0, 10, 0, 10, 0, -1, 0 } },
                    { "Surgeon",           new int[] { 70, 1, 10, 1, 10, 1, -1, 0 } },
                    { "Nurse",             new int[] { 70, 2, 36, 1, 25, 0, -1, 0 } },
                    { "Flight Medic",      new int[] { 23, 2, 36, 2, 25, 0, 39, 5 } },
                    { "Civilian Clothes",  new int[] { 0, 0, 0, 0, 0, 0, -1, 0 } },
                }
            },
            // Jail / Corrections
            { 19, new Dictionary<string, int[]>()
                {
                    { "Corrections Officer", new int[] { 55, 6, 35, 6, 25, 0, 46, 6 } },
                    { "Guard Captain",       new int[] { 55, 7, 35, 7, 25, 0, 46, 7 } },
                    { "Riot Control",        new int[] { 230, 8, 31, 8, 25, 0, 150, 8 } },
                    { "Medical Staff",       new int[] { 70, 0, 36, 0, 25, 0, -1, 0 } },
                    { "Civilian Clothes",    new int[] { 0, 0, 0, 0, 0, 0, -1, 0 } },
                }
            },
            // National Armory / Army
            { 14, new Dictionary<string, int[]>()
                {
                    { "Infantry",          new int[] { 230, 3, 31, 3, 25, 0, 150, 1 } },
                    { "Special Forces",    new int[] { 230, 7, 31, 7, 25, 0, 150, 4 } },
                    { "Officer",           new int[] { 55, 5, 35, 5, 25, 0, 46, 5 } },
                    { "Pilot",             new int[] { 251, 0, 31, 0, 25, 0, 123, 0 } },
                    { "Combat Medic",      new int[] { 23, 3, 31, 3, 25, 0, 150, 1 } },
                    { "Dress Uniform",     new int[] { 53, 0, 35, 0, 10, 0, 44, 0 } },
                    { "Civilian Clothes",  new int[] { 0, 0, 0, 0, 0, 0, -1, 0 } },
                }
            },
        };

        #endregion

        #region Initialization

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                foreach (var locker in LockerRooms)
                {
                    CustomColShape.CreateCylinderColShape(locker.Value, 1.5f, 2, 0, ColShapeEnums.OrgLocker);
                    NAPI.TextLabel.CreateTextLabel("~g~Locker Room\n~w~Change Uniform",
                        locker.Value + new Vector3(0, 0, 0.5), 8F, 0.4F, 0, new Color(0, 200, 0));
                    NAPI.Marker.CreateMarker(1, locker.Value - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 1f, new Color(0, 200, 0, 200));
                }

                Log.Write("Organization Clothing system initialized.");
            }
            catch (Exception e)
            {
                Log.Write($"onResourceStart Exception: {e.ToString()}");
            }
        }

        #endregion

        #region Locker Room Interaction

        [Interaction(ColShapeEnums.OrgLocker)]
        public static void OnLockerInteraction(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                int fractionId = characterData.FractionID;

                // Find which locker they're at
                int lockerFraction = GetNearestLockerFraction(player.Position);

                if (fractionId != lockerFraction)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        "This locker room is restricted to organization members only.", 3000);
                    return;
                }

                if (!OrgUniforms.ContainsKey(fractionId))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        "No uniforms available for your organization.", 3000);
                    return;
                }

                var uniforms = OrgUniforms[fractionId];
                var frameList = new FrameListData();
                frameList.Header = "Locker Room - Select Uniform";
                frameList.Callback = callback_clothingMenu;

                foreach (var uniform in uniforms)
                {
                    frameList.List.Add(new ListData(uniform.Key, uniform.Key));
                }

                Players.Popup.List.Repository.Open(player, frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnLockerInteraction Exception: {e.ToString()}");
            }
        }

        private static void callback_clothingMenu(ExtPlayer player, object listItem)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                string uniformName = listItem.ToString();
                int fractionId = characterData.FractionID;

                if (!OrgUniforms.ContainsKey(fractionId)) return;
                if (!OrgUniforms[fractionId].ContainsKey(uniformName)) return;

                var uniformData = OrgUniforms[fractionId][uniformName];

                if (uniformName == "Civilian Clothes")
                {
                    // Reset to civilian
                    player.TriggerEvent("client.clothing.resetToCivilian");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Changed to civilian clothes.", 2000);
                }
                else
                {
                    // Apply uniform
                    ApplyUniform(player, uniformData);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                        $"Changed to: {uniformName}", 2000);
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_clothingMenu Exception: {e.ToString()}");
            }
        }

        private static void ApplyUniform(ExtPlayer player, int[] uniformData)
        {
            // uniformData format: [torso, torso_tex, legs, legs_tex, shoes, shoes_tex, hat, hat_tex]
            player.SetClothes(11, uniformData[0], uniformData[1]); // Torso
            player.SetClothes(4, uniformData[2], uniformData[3]);  // Legs
            player.SetClothes(6, uniformData[4], uniformData[5]);  // Shoes
            
            if (uniformData[6] >= 0)
                player.SetAccessories(0, uniformData[6], uniformData[7]); // Hat
        }

        private static int GetNearestLockerFraction(Vector3 position)
        {
            int nearest = 0;
            float minDist = float.MaxValue;
            foreach (var kvp in LockerRooms)
            {
                float dist = position.DistanceTo(kvp.Value);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = kvp.Key;
                }
            }
            return nearest;
        }

        #endregion
    }
}
