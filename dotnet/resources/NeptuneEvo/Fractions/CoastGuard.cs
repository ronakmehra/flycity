using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using NeptuneEvo.Handles;
using NeptuneEvo.Core;
using Redage.SDK;
using NeptuneEvo.GUI;
using NeptuneEvo.Functions;
using NeptuneEvo.Chars;
using NeptuneEvo.Chars.Models;
using NeptuneEvo.Accounts;
using NeptuneEvo.Players.Models;
using NeptuneEvo.Players;
using NeptuneEvo.Character.Models;
using NeptuneEvo.Character;
using NeptuneEvo.Fractions.Models;
using NeptuneEvo.Fractions.Player;
using NeptuneEvo.Players.Popup.List.Models;
using NeptuneEvo.Table.Models;
using Localization;

namespace NeptuneEvo.Fractions
{
    class CoastGuard : Script
    {
        private static readonly nLog Log = new nLog("Fractions.CoastGuard");

        // Coast Guard HQ - Los Santos Port
        public static Vector3 HQPosition = new Vector3(1340.2f, -3290.5f, 5.9f);
        
        // Cloakroom
        public static Vector3 CloakroomPosition = new Vector3(1337.5f, -3286.8f, 5.9f);
        
        // Gun armory
        public static Vector3 GunsPosition = new Vector3(1343.8f, -3285.2f, 5.9f);
        
        // Helicopter pad
        public static Vector3 HeliPadPosition = new Vector3(1355.2f, -3278.5f, 5.9f);

        // Boat docks
        public static List<Vector3> BoatDocks = new List<Vector3>()
        {
            new Vector3(1325.5f, -3310.8f, 0.5f),       // Main dock
            new Vector3(1315.2f, -3325.5f, 0.5f),       // Cutter dock
            new Vector3(1345.8f, -3318.2f, 0.5f),       // Patrol dock
        };

        // Rescue stations along coast
        public static List<Vector3> RescueStations = new List<Vector3>()
        {
            new Vector3(-1730.5f, -1162.8f, 13.0f),     // Vespucci Beach station
            new Vector3(3870.2f, 4485.5f, 2.5f),        // Paleto Bay station  
            new Vector3(-3025.8f, 42.5f, 7.2f),          // Chumash station
            new Vector3(1480.5f, 6560.2f, 2.0f),        // North coast station
        };

        // Arrest/detention position
        public static Vector3 DetentionPosition = new Vector3(1348.5f, -3282.1f, 5.9f);

        // Coast Guard ranks
        public static Dictionary<int, string> Ranks = new Dictionary<int, string>()
        {
            {1, "Seaman Recruit"},
            {2, "Seaman Apprentice"},
            {3, "Seaman"},
            {4, "Petty Officer"},
            {5, "Chief Petty Officer"},
            {6, "Warrant Officer"},
            {7, "Lieutenant"},
            {8, "Commander"},
            {9, "Captain"},
            {10, "Admiral"},
        };

        // Vehicle types available
        public static Dictionary<string, VehicleHash> CoastGuardVehicles = new Dictionary<string, VehicleHash>()
        {
            {"Patrol Boat", VehicleHash.PolicePredator},
            {"Rescue Dinghy", VehicleHash.Dinghy4},
            {"Speed Boat", VehicleHash.Seashark2},
            {"Patrol Helicopter", VehicleHash.Polmav},
        };

        private static int PedCommanderId = 0;

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                // Main blip
                Main.CreateBlip(new Main.BlipData(410, "Coast Guard", HQPosition, 3, true, 1.2f));

                // Cloakroom
                CustomColShape.CreateCylinderColShape(CloakroomPosition, 1, 2, 0, ColShapeEnums.FractionCoastGuard, 2);
                NAPI.Marker.CreateMarker(30, CloakroomPosition, new Vector3(), new Vector3(), 1, new Color(0, 150, 200, 220));
                NAPI.TextLabel.CreateTextLabel("~b~Coast Guard Cloakroom\n~w~Change uniform",
                    CloakroomPosition + new Vector3(0, 0, 0.5), 5F, 0.3F, 0, new Color(0, 150, 200));

                // Armory
                CustomColShape.CreateCylinderColShape(GunsPosition, 1, 2, 0, ColShapeEnums.FractionCoastGuard, 3);
                NAPI.Marker.CreateMarker(20, GunsPosition, new Vector3(), new Vector3(), 1, new Color(0, 150, 200, 220));
                NAPI.TextLabel.CreateTextLabel("~b~Armory\n~w~Get equipment",
                    GunsPosition + new Vector3(0, 0, 0.5), 5F, 0.3F, 0, new Color(0, 150, 200));

                // Helicopter pad
                CustomColShape.CreateCylinderColShape(HeliPadPosition, 5, 3, 0, ColShapeEnums.CoastGuardHeliPad);
                NAPI.Marker.CreateMarker(26, HeliPadPosition - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 3.0f, new Color(0, 150, 200, 150));
                NAPI.TextLabel.CreateTextLabel("~b~Helicopter Pad\n~w~Spawn patrol helicopter",
                    HeliPadPosition + new Vector3(0, 0, 0.5), 10F, 0.4F, 0, new Color(0, 150, 200));

                // Boat docks
                foreach (var dock in BoatDocks)
                {
                    CustomColShape.CreateCylinderColShape(dock, 3, 3, 0, ColShapeEnums.CoastGuardBoatDock);
                    NAPI.Marker.CreateMarker(1, dock - new Vector3(0, 0, 0.5), new Vector3(), new Vector3(), 2.0f, new Color(0, 150, 200, 150));
                }

                // Rescue stations
                foreach (var station in RescueStations)
                {
                    CustomColShape.CreateCylinderColShape(station, 3, 2, 0, ColShapeEnums.CoastGuardRescueStation);
                    Main.CreateBlip(new Main.BlipData(410, "CG Rescue Station", station, 3, true, 0.6f));
                }

                // Detention area
                CustomColShape.CreateCylinderColShape(DetentionPosition, 5, 3, 0, ColShapeEnums.CoastGuardDetention);

                // Commander NPC
                Ped ped = PedSystem.Repository.CreateQuest("s_m_y_uscg_01", new Vector3(1342.5f, -3288.2f, 5.9f), 270.0f,
                    title: "~y~NPC~w~ Commander Harris\nCoast Guard Station Chief", colShapeEnums: ColShapeEnums.FracCoastGuard);
                PedCommanderId = ped.Value;

                Log.Write("Coast Guard system initialized.");
            }
            catch (Exception e)
            {
                Log.Write($"onResourceStart Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.FracCoastGuard)]
        public static void Open(ExtPlayer player, int index)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;

                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.COASTGUARD)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You're not a Coast Guard member.", 3000);
                    return;
                }

                if (PedCommanderId == index)
                {
                    var frameList = new FrameListData();
                    frameList.Header = "Coast Guard - Commander Harris";
                    frameList.Callback = callback_coastGuardMenu;

                    frameList.List.Add(new ListData("Start Patrol Duty", "patrol"));
                    frameList.List.Add(new ListData("End Patrol Duty", "endpatrol"));
                    frameList.List.Add(new ListData("Sea Rescue Mission", "rescue"));
                    frameList.List.Add(new ListData("Anti-Smuggling Operation", "antiSmuggle"));
                    frameList.List.Add(new ListData("Port Security Check", "portsecurity"));
                    frameList.List.Add(new ListData("Request Air Support", "airsupport"));
                    frameList.List.Add(new ListData("Check Smuggling Activity", "checkactivity"));

                    Trigger.ClientEvent(player, "client.framelist.show", frameList);
                }
            }
            catch (Exception e)
            {
                Log.Write($"Open Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_coastGuardMenu")]
        public static void callback_coastGuardMenu(ExtPlayer player, string value)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;

                switch (value)
                {
                    case "patrol":
                        sessionData.WorkData.OnWork = true;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                            "Coast Guard patrol duty started!\n" +
                            "Monitor the coastline for smuggling, illegal fishing, and maritime emergencies.\n" +
                            "Intercept and arrest any smugglers caught in territorial waters.", 8000);
                        break;

                    case "endpatrol":
                        sessionData.WorkData.OnWork = false;
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Patrol duty ended. Good work, sailor.", 3000);
                        break;

                    case "rescue":
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            "Sea Rescue Mission activated! Get a rescue boat and respond to the distress signal.\n" +
                            "Save civilians in danger and bring them to shore.", 5000);
                        Random rnd = new Random();
                        var station = RescueStations[rnd.Next(RescueStations.Count)];
                        Trigger.ClientEvent(player, "createWaypoint", station.X, station.Y);
                        break;

                    case "antiSmuggle":
                        int activeSmuggling = SmugglingRoutes.ActiveMissions.Count;
                        if (activeSmuggling > 0)
                        {
                            Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, 
                                $"ANTI-SMUGGLING OPERATION: {activeSmuggling} active smuggling operations detected!\n" +
                                "Intercept smugglers and confiscate their cargo. Arrest on sight!", 8000);
                            
                            // Give first smuggler's rough location
                            if (SmugglingRoutes.ActiveMissions.Count > 0)
                            {
                                var firstMission = SmugglingRoutes.ActiveMissions.First();
                                var smuggler = firstMission.Key;
                                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                                    $"Last known smuggler position near: {smuggler.Position.X:F0}, {smuggler.Position.Y:F0}", 5000);
                            }
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                                "No active smuggling operations detected. Remain vigilant.", 3000);
                        }
                        break;

                    case "portsecurity":
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            "Port Security Check activated. Inspect all vessels at the Terminal docks.\n" +
                            "Check for illegal cargo, weapons, and drugs.", 5000);
                        Trigger.ClientEvent(player, "createWaypoint", 1299.8f, -3264.5f);
                        break;

                    case "airsupport":
                        BroadcastToAllLaw($"[COAST GUARD] Air support requested at {player.Position.X:F0}, {player.Position.Y:F0}!");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Air support request broadcast to all units.", 3000);
                        break;

                    case "checkactivity":
                        int total = SmugglingRoutes.ActiveMissions.Count;
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            $"Active smuggling missions: {total}\n" +
                            "Stay on radio channel for real-time alerts.", 5000);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_coastGuardMenu Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.CoastGuardBoatDock)]
        public static void OnBoatDock(ExtPlayer player)
        {
            try
            {
                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.COASTGUARD && fracId != (int)Models.Fractions.WATERPOLICE)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Only Coast Guard/Water Police can use these docks.", 3000);
                    return;
                }

                var frameList = new FrameListData();
                frameList.Header = "Coast Guard Dock";
                frameList.Callback = callback_cgBoatMenu;

                frameList.List.Add(new ListData("Patrol Boat (Police Predator)", "patrol"));
                frameList.List.Add(new ListData("Rescue Dinghy", "rescue"));
                frameList.List.Add(new ListData("Speed Boat (Seashark)", "speed"));

                Trigger.ClientEvent(player, "client.framelist.show", frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnBoatDock Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_cgBoatMenu")]
        public static void callback_cgBoatMenu(ExtPlayer player, string value)
        {
            try
            {
                Vector3 spawnPos = BoatDocks[0];
                float minDist = float.MaxValue;
                foreach (var dock in BoatDocks)
                {
                    float dist = player.Position.DistanceTo(dock);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        spawnPos = dock;
                    }
                }

                VehicleHash vehHash;
                string vehName;

                switch (value)
                {
                    case "patrol": vehHash = VehicleHash.PolicePredator; vehName = "Police Predator"; break;
                    case "rescue": vehHash = VehicleHash.Dinghy4; vehName = "Rescue Dinghy"; break;
                    case "speed": vehHash = VehicleHash.Seashark2; vehName = "Seashark"; break;
                    default: return;
                }

                NAPI.Vehicle.CreateVehicle(vehHash, spawnPos, 0.0f, 0, 0);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"{vehName} spawned at the dock!", 3000);
            }
            catch (Exception e)
            {
                Log.Write($"callback_cgBoatMenu Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.CoastGuardHeliPad)]
        public static void OnHeliPad(ExtPlayer player)
        {
            try
            {
                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.COASTGUARD)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Only Coast Guard can use the helicopter pad.", 3000);
                    return;
                }

                NAPI.Vehicle.CreateVehicle(VehicleHash.Polmav, HeliPadPosition + new Vector3(0, 0, 1.0f), 0.0f, 0, 0);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Patrol helicopter spawned on the helipad!", 3000);
            }
            catch (Exception e)
            {
                Log.Write($"OnHeliPad Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.CoastGuardDetention)]
        public static void OnDetention(ExtPlayer player)
        {
            try
            {
                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.COASTGUARD)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Only Coast Guard can process detentions.", 3000);
                    return;
                }

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                    "Coast Guard Detention Center. Bring arrested smugglers here.\n" +
                    "Use /cgDetain [id] [reason] [minutes] to detain a suspect.", 5000);
            }
            catch (Exception e)
            {
                Log.Write($"OnDetention Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.CoastGuardRescueStation)]
        public static void OnRescueStation(ExtPlayer player)
        {
            try
            {
                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.COASTGUARD)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Coast Guard personnel only.", 3000);
                    return;
                }

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                    "CG Rescue Station operational. Use this as a resupply and staging area.\n" +
                    "Rescue boats and equipment available.", 5000);
            }
            catch (Exception e)
            {
                Log.Write($"OnRescueStation Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.FractionCoastGuard)]
        public static void OnCGStation(ExtPlayer player, int index)
        {
            try
            {
                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.COASTGUARD)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Coast Guard members only.", 3000);
                    return;
                }

                switch (index)
                {
                    case 2: // Cloakroom
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Coast Guard Cloakroom - Change uniform.", 3000);
                        break;
                    case 3: // Armory
                        var frameList = new FrameListData();
                        frameList.Header = "Coast Guard Armory";
                        frameList.Callback = callback_cgArmory;

                        frameList.List.Add(new ListData("Pistol + Ammo", "pistol"));
                        frameList.List.Add(new ListData("Carbine Rifle + Ammo", "carbine"));
                        frameList.List.Add(new ListData("Shotgun + Ammo", "shotgun"));
                        frameList.List.Add(new ListData("Flare Gun", "flare"));
                        frameList.List.Add(new ListData("Body Armor", "armor"));
                        frameList.List.Add(new ListData("Handcuffs", "cuffs"));
                        frameList.List.Add(new ListData("Night Vision", "nightvision"));
                        frameList.List.Add(new ListData("Binoculars", "binoculars"));

                        Trigger.ClientEvent(player, "client.framelist.show", frameList);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write($"OnCGStation Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_cgArmory")]
        public static void callback_cgArmory(ExtPlayer player, string value)
        {
            try
            {
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"Equipment issued: {value}. Serve with honor, sailor.", 3000);
            }
            catch (Exception e)
            {
                Log.Write($"callback_cgArmory Exception: {e.ToString()}");
            }
        }

        private static void BroadcastToAllLaw(string message)
        {
            try
            {
                var players = NAPI.Pools.GetAllPlayers();
                foreach (var p in players)
                {
                    var extP = (ExtPlayer)p;
                    if (extP == null) continue;
                    var fracId = extP.GetFractionId();
                    if (fracId == (int)Models.Fractions.POLICE ||
                        fracId == (int)Models.Fractions.SHERIFF ||
                        fracId == (int)Models.Fractions.FIB ||
                        fracId == (int)Models.Fractions.WATERPOLICE ||
                        fracId == (int)Models.Fractions.COASTGUARD ||
                        fracId == (int)Models.Fractions.ARMY)
                    {
                        Notify.Send(extP, NotifyType.Warning, NotifyPosition.BottomCenter, message, 8000);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write($"BroadcastToAllLaw Exception: {e.ToString()}");
            }
        }
    }
}
