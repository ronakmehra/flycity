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
using Redage.SDK;

namespace NeptuneEvo.Fractions
{
    /// <summary>
    /// Special Vehicles System - Organization-restricted vehicles
    /// Normal players CANNOT drive these. Only org members with proper rank can use them.
    /// </summary>
    class SpecialVehicles : Script
    {
        private static readonly nLog Log = new nLog("Fractions.SpecialVehicles");

        #region Vehicle Definitions Per Organization

        /// <summary>
        /// All special vehicles per org. Civilians cannot enter/drive these.
        /// Key = Fraction ID, Value = list of vehicle definitions
        /// </summary>
        public static Dictionary<int, List<OrgVehicleData>> OrgVehicles = new Dictionary<int, List<OrgVehicleData>>()
        {
            // LSPD - Police Department
            { 7, new List<OrgVehicleData>()
                {
                    new OrgVehicleData("police", "LSPD Cruiser", 1, new Vector3(451.2f, -993.4f, 25.7f), 90f),
                    new OrgVehicleData("police2", "LSPD Interceptor", 3, new Vector3(447.8f, -993.4f, 25.7f), 90f),
                    new OrgVehicleData("police3", "LSPD Unmarked", 5, new Vector3(444.5f, -993.4f, 25.7f), 90f),
                    new OrgVehicleData("policet", "LSPD Transport Van", 4, new Vector3(441.1f, -993.4f, 25.7f), 90f),
                    new OrgVehicleData("riot", "LSPD SWAT Truck", 8, new Vector3(437.8f, -993.4f, 25.7f), 90f),
                    new OrgVehicleData("polmav", "LSPD Helicopter", 10, new Vector3(449.7f, -981.4f, 43.7f), 0f),
                    new OrgVehicleData("fbi2", "LSPD SUV", 6, new Vector3(434.4f, -993.4f, 25.7f), 90f),
                    new OrgVehicleData("policeb", "LSPD Motorcycle", 2, new Vector3(460.1f, -990.2f, 25.7f), 180f),
                }
            },
            // SAHP - Highway Patrol
            { 18, new List<OrgVehicleData>()
                {
                    new OrgVehicleData("sheriff", "SAHP Cruiser", 1, new Vector3(-452.3f, 6008.1f, 31.7f), 45f),
                    new OrgVehicleData("sheriff2", "SAHP SUV", 3, new Vector3(-455.8f, 6005.4f, 31.7f), 45f),
                    new OrgVehicleData("pranger", "SAHP Ranger", 2, new Vector3(-459.2f, 6002.7f, 31.7f), 45f),
                    new OrgVehicleData("policeb", "SAHP Motorcycle", 2, new Vector3(-462.6f, 6000.1f, 31.7f), 45f),
                    new OrgVehicleData("polmav", "SAHP Helicopter", 8, new Vector3(-475.3f, 5988.5f, 35.2f), 0f),
                    new OrgVehicleData("riot", "SAHP Armored Van", 7, new Vector3(-466.1f, 5997.4f, 31.7f), 45f),
                }
            },
            // FIB - Federal Investigation Bureau
            { 9, new List<OrgVehicleData>()
                {
                    new OrgVehicleData("fbi", "FIB Sedan", 1, new Vector3(127.5f, -757.2f, 45.75f), 160f),
                    new OrgVehicleData("fbi2", "FIB SUV", 2, new Vector3(124.1f, -760.8f, 45.75f), 160f),
                    new OrgVehicleData("cogcabrio", "FIB Undercover", 4, new Vector3(120.7f, -764.4f, 45.75f), 160f),
                    new OrgVehicleData("granger", "FIB Tactical", 5, new Vector3(117.3f, -768.0f, 45.75f), 160f),
                    new OrgVehicleData("buzzard2", "FIB Attack Helicopter", 9, new Vector3(142.8f, -738.5f, 65.0f), 0f),
                    new OrgVehicleData("insurgent", "FIB Armored Vehicle", 10, new Vector3(113.9f, -771.6f, 45.75f), 160f),
                    new OrgVehicleData("fbi", "FIB Pursuit Vehicle", 6, new Vector3(110.5f, -775.2f, 45.75f), 160f),
                }
            },
            // LifeInvader News
            { 15, new List<OrgVehicleData>()
                {
                    new OrgVehicleData("rumpo", "News Van", 1, new Vector3(-1089.3f, -252.8f, 37.76f), 30f),
                    new OrgVehicleData("rumpo", "News Van 2", 1, new Vector3(-1092.7f, -255.4f, 37.76f), 30f),
                    new OrgVehicleData("maverick", "News Helicopter", 5, new Vector3(-1075.2f, -233.5f, 50.0f), 0f),
                    new OrgVehicleData("premier", "Reporter Car", 1, new Vector3(-1096.1f, -258.0f, 37.76f), 30f),
                    new OrgVehicleData("blista", "Camera Crew Car", 2, new Vector3(-1099.5f, -260.6f, 37.76f), 30f),
                }
            },
            // Emergency Hospital (EMS)
            { 8, new List<OrgVehicleData>()
                {
                    new OrgVehicleData("ambulance", "Ambulance", 1, new Vector3(325.5f, -587.2f, 43.29f), 70f),
                    new OrgVehicleData("ambulance", "Ambulance 2", 1, new Vector3(329.1f, -584.6f, 43.29f), 70f),
                    new OrgVehicleData("lguard", "EMS Lifeguard", 2, new Vector3(332.7f, -582.0f, 43.29f), 70f),
                    new OrgVehicleData("polmav", "Medical Helicopter", 6, new Vector3(338.8f, -575.3f, 55.0f), 0f),
                    new OrgVehicleData("firetruk", "Fire Engine", 3, new Vector3(336.3f, -579.4f, 43.29f), 70f),
                    new OrgVehicleData("pbus", "Medical Transport Bus", 4, new Vector3(339.9f, -576.8f, 43.29f), 70f),
                }
            },
            // Jail / Corrections
            { 19, new List<OrgVehicleData>()
                {
                    new OrgVehicleData("policet", "Prison Transport", 1, new Vector3(1853.2f, 2592.5f, 45.67f), 270f),
                    new OrgVehicleData("riot", "Prison Riot Van", 3, new Vector3(1856.8f, 2595.1f, 45.67f), 270f),
                    new OrgVehicleData("police", "Corrections Cruiser", 1, new Vector3(1860.4f, 2597.7f, 45.67f), 270f),
                    new OrgVehicleData("sheriff", "Corrections SUV", 2, new Vector3(1864.0f, 2600.3f, 45.67f), 270f),
                    new OrgVehicleData("insurgent", "Armed Transport", 6, new Vector3(1867.6f, 2602.9f, 45.67f), 270f),
                }
            },
            // National Armory / Army
            { 14, new List<OrgVehicleData>()
                {
                    new OrgVehicleData("barracks", "Military Truck", 1, new Vector3(-2368.2f, 3256.5f, 92.9f), 315f),
                    new OrgVehicleData("insurgent", "Insurgent APC", 3, new Vector3(-2371.8f, 3259.1f, 92.9f), 315f),
                    new OrgVehicleData("crusader", "Military Jeep", 2, new Vector3(-2375.4f, 3261.7f, 92.9f), 315f),
                    new OrgVehicleData("rhino", "Rhino Tank", 10, new Vector3(-2379.0f, 3264.3f, 92.9f), 315f),
                    new OrgVehicleData("valkyrie", "Valkyrie Helicopter", 7, new Vector3(-2390.5f, 3275.8f, 95.0f), 0f),
                    new OrgVehicleData("buzzard2", "Buzzard Attack", 5, new Vector3(-2395.1f, 3280.4f, 95.0f), 0f),
                    new OrgVehicleData("hydra", "Hydra Jet", 12, new Vector3(-2430.8f, 3295.2f, 35.0f), 315f),
                    new OrgVehicleData("lazer", "P-996 LAZER", 12, new Vector3(-2445.3f, 3308.7f, 35.0f), 315f),
                    new OrgVehicleData("barracks2", "Troop Transport", 4, new Vector3(-2382.6f, 3266.9f, 92.9f), 315f),
                }
            },
        };

        #endregion

        #region Vehicle Restriction System

        /// <summary>
        /// Called when a player tries to enter a vehicle - blocks civilians from org vehicles
        /// </summary>
        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(ExtPlayer player, ExtVehicle vehicle, sbyte seatId)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                // Check if this is a restricted org vehicle
                int owningFraction = GetVehicleOwningFraction(vehicle);
                if (owningFraction == 0) return; // Not an org vehicle, allow

                // Player must be in the owning org
                if (characterData.FractionID != owningFraction)
                {
                    // Kick them out
                    player.WarpOutOfVehicle();
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        "ACCESS DENIED! This vehicle belongs to a government organization. You are not authorized.", 4000);
                    return;
                }

                // Check minimum rank requirement
                var orgVehicles = OrgVehicles[owningFraction];
                string vehicleModel = NAPI.Vehicle.GetVehicleDisplayName(vehicle.Model);
                var vehData = orgVehicles.FirstOrDefault(v => IsMatchingVehicle(v, vehicle));
                
                if (vehData != null && characterData.FractionLVL < vehData.MinRank)
                {
                    player.WarpOutOfVehicle();
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        $"You need Rank {vehData.MinRank}+ to use this vehicle. Your rank: {characterData.FractionLVL}", 4000);
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Write($"OnPlayerEnterVehicle Exception: {e.ToString()}");
            }
        }

        /// <summary>
        /// Determines which fraction owns a vehicle based on spawn data
        /// </summary>
        private static int GetVehicleOwningFraction(ExtVehicle vehicle)
        {
            foreach (var kvp in OrgVehicles)
            {
                foreach (var vehData in kvp.Value)
                {
                    if (IsMatchingVehicle(vehData, vehicle))
                        return kvp.Key;
                }
            }
            return 0; // Not an org vehicle
        }

        private static bool IsMatchingVehicle(OrgVehicleData data, ExtVehicle vehicle)
        {
            // Check by spawn position proximity (within 5 units)
            return vehicle.Position.DistanceTo(data.SpawnPosition) < 5f;
        }

        #endregion

        #region Vehicle Spawning

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                int totalVehicles = 0;
                foreach (var kvp in OrgVehicles)
                {
                    foreach (var vehData in kvp.Value)
                    {
                        // Spawn the vehicles
                        var vehicle = NAPI.Vehicle.CreateVehicle(
                            NAPI.Util.GetHashKey(vehData.ModelName),
                            vehData.SpawnPosition,
                            vehData.SpawnHeading,
                            0, 0, // colors
                            $"GOV-{kvp.Key}", 255, false, true, 0
                        );

                        if (vehicle != null)
                        {
                            vehicle.Locked = true; // Locked by default
                            totalVehicles++;
                        }
                    }
                }

                Log.Write($"Special Vehicles system initialized. Spawned {totalVehicles} restricted org vehicles.");
            }
            catch (Exception e)
            {
                Log.Write($"onResourceStart Exception: {e.ToString()}");
            }
        }

        #endregion

        #region Org Members - Vehicle Actions

        /// <summary>
        /// Unlock vehicle for org members when they approach
        /// </summary>
        public static void UnlockOrgVehicle(ExtPlayer player, ExtVehicle vehicle)
        {
            var characterData = player.GetCharacterData();
            if (characterData == null) return;

            int owningFraction = GetVehicleOwningFraction(vehicle);
            if (owningFraction == 0 || characterData.FractionID != owningFraction) return;

            vehicle.Locked = false;
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Vehicle unlocked.", 2000);
        }

        #endregion
    }

    #region Vehicle Data Model

    public class OrgVehicleData
    {
        public string ModelName { get; set; }
        public string DisplayName { get; set; }
        public int MinRank { get; set; }
        public Vector3 SpawnPosition { get; set; }
        public float SpawnHeading { get; set; }

        public OrgVehicleData(string model, string display, int minRank, Vector3 position, float heading)
        {
            ModelName = model;
            DisplayName = display;
            MinRank = minRank;
            SpawnPosition = position;
            SpawnHeading = heading;
        }
    }

    #endregion
}
