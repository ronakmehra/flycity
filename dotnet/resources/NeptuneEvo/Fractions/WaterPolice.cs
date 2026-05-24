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
using NeptuneEvo.Table.Tasks.Models;
using NeptuneEvo.Table.Tasks.Player;
using NeptuneEvo.Quests;
using NeptuneEvo.Quests.Models;
using Localization;

namespace NeptuneEvo.Fractions
{
    class WaterPolice : Script
    {
        private static readonly nLog Log = new nLog("Fractions.WaterPolice");

        // Water Police HQ - Vespucci Beach pier station
        public static Vector3 HQPosition = new Vector3(-1601.5f, -1078.2f, 13.0f);
        
        // Cloakroom (change uniform)
        public static Vector3 CloakroomPosition = new Vector3(-1598.2f, -1074.5f, 13.0f);
        
        // Gun armory
        public static Vector3 GunsPosition = new Vector3(-1605.8f, -1072.1f, 13.0f);

        // Boat spawn docks
        public static List<Vector3> BoatSpawns = new List<Vector3>()
        {
            new Vector3(-1617.5f, -1098.2f, 0.5f),      // Main dock
            new Vector3(-1632.8f, -1106.5f, 0.5f),      // Secondary dock
            new Vector3(-1642.1f, -1088.3f, 0.5f),      // Third dock
        };

        // Arrest position (water police jail processing)
        public static Vector3 ArrestPosition = new Vector3(-1595.8f, -1068.5f, 13.0f);
        
        // Prison holding cell
        public static Vector3 PrisonPosition = new Vector3(-1593.2f, -1064.8f, 13.0f);
        
        // Patrol routes
        public static List<PatrolRoute> PatrolRoutes = new List<PatrolRoute>()
        {
            new PatrolRoute("Vespucci Beach Patrol", new List<Vector3>()
            {
                new Vector3(-1664.5f, -1116.8f, 0.5f),
                new Vector3(-1750.2f, -1200.5f, 0.5f),
                new Vector3(-1850.8f, -1300.3f, 0.5f),
                new Vector3(-1664.5f, -1116.8f, 0.5f),
            }),
            new PatrolRoute("Terminal Docks Patrol", new List<Vector3>()
            {
                new Vector3(1299.8f, -3264.5f, 0.5f),
                new Vector3(1380.5f, -3350.2f, 0.5f),
                new Vector3(1450.8f, -3280.1f, 0.5f),
                new Vector3(1299.8f, -3264.5f, 0.5f),
            }),
            new PatrolRoute("Paleto Bay Patrol", new List<Vector3>()
            {
                new Vector3(3895.8f, 4508.2f, 0.5f),
                new Vector3(3800.5f, 4600.3f, 0.5f),
                new Vector3(3950.2f, 4400.8f, 0.5f),
                new Vector3(3895.8f, 4508.2f, 0.5f),
            }),
            new PatrolRoute("East Coast Patrol", new List<Vector3>()
            {
                new Vector3(1467.8f, 6567.5f, 0.5f),
                new Vector3(1550.2f, 6480.3f, 0.5f),
                new Vector3(1600.5f, 6350.8f, 0.5f),
                new Vector3(1467.8f, 6567.5f, 0.5f),
            }),
        };

        // NPC contacts
        private static int PedCaptainId = 0;

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                // Main blip
                Main.CreateBlip(new Main.BlipData(571, "Water Police", HQPosition, 38, true, 1.2f));

                // Arrest area
                CustomColShape.CreateCylinderColShape(ArrestPosition, 6, 3, 0, ColShapeEnums.FractionWaterPoliceArrest, 0);

                // Cloakroom
                CustomColShape.CreateCylinderColShape(CloakroomPosition, 1, 2, 0, ColShapeEnums.FractionWaterPolice, 2);
                NAPI.Marker.CreateMarker(30, CloakroomPosition, new Vector3(), new Vector3(), 1, new Color(0, 100, 255, 220));
                NAPI.TextLabel.CreateTextLabel("~b~Water Police Cloakroom\n~w~Change uniform",
                    CloakroomPosition + new Vector3(0, 0, 0.5), 5F, 0.3F, 0, new Color(0, 100, 255));

                // Gun armory
                CustomColShape.CreateCylinderColShape(GunsPosition, 1, 2, 0, ColShapeEnums.FractionWaterPolice, 3);
                NAPI.Marker.CreateMarker(20, GunsPosition, new Vector3(), new Vector3(), 1, new Color(0, 100, 255, 220));
                NAPI.TextLabel.CreateTextLabel("~b~Armory\n~w~Get your equipment",
                    GunsPosition + new Vector3(0, 0, 0.5), 5F, 0.3F, 0, new Color(0, 100, 255));

                // Boat dock markers
                foreach (var dock in BoatSpawns)
                {
                    CustomColShape.CreateCylinderColShape(dock, 3, 3, 0, ColShapeEnums.WaterPoliceBoatSpawn);
                    NAPI.Marker.CreateMarker(1, dock - new Vector3(0, 0, 0.5), new Vector3(), new Vector3(), 2.0f, new Color(0, 100, 255, 150));
                }

                // Captain NPC
                Ped ped = PedSystem.Repository.CreateQuest("s_m_y_cop_01", new Vector3(-1599.5f, -1076.2f, 13.0f), 180.0f, 
                    title: "~y~NPC~w~ Captain Roberts\nWater Police Chief", colShapeEnums: ColShapeEnums.FracWaterPolice);
                PedCaptainId = ped.Value;

                Log.Write("Water Police system initialized.");
            }
            catch (Exception e)
            {
                Log.Write($"onResourceStart Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.FracWaterPolice)]
        public static void Open(ExtPlayer player, int index)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                if (sessionData.CuffedData.Cuffed)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You are handcuffed!", 3000);
                    return;
                }

                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.WATERPOLICE)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You're not a member of the Water Police.", 3000);
                    return;
                }

                if (PedCaptainId == index)
                {
                    var frameList = new FrameListData();
                    frameList.Header = "Water Police - Captain Roberts";
                    frameList.Callback = callback_waterPoliceMenu;

                    frameList.List.Add(new ListData("Start Patrol Duty", "patrol"));
                    frameList.List.Add(new ListData("End Patrol Duty", "endpatrol"));
                    frameList.List.Add(new ListData("Get Patrol Boat", "getboat"));
                    frameList.List.Add(new ListData("Check Active Smuggling Reports", "reports"));
                    frameList.List.Add(new ListData("Search & Seize Protocol", "search"));
                    frameList.List.Add(new ListData("Request Backup", "backup"));

                    Trigger.ClientEvent(player, "client.framelist.show", frameList);
                }
            }
            catch (Exception e)
            {
                Log.Write($"Open Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_waterPoliceMenu")]
        public static void callback_waterPoliceMenu(ExtPlayer player, string value)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                switch (value)
                {
                    case "patrol":
                        sessionData.WorkData.OnWork = true;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                            "Patrol duty started! Monitor the waters for illegal smuggling activity.\n" +
                            "Arrest smugglers on sight. Check dispatch for alerts.", 5000);
                        
                        // Assign patrol route
                        Random rnd = new Random();
                        int routeIdx = rnd.Next(0, PatrolRoutes.Count);
                        var route = PatrolRoutes[routeIdx];
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            $"Assigned patrol route: {route.Name}. Head to your boat.", 5000);
                        Trigger.ClientEvent(player, "createWaypoint", route.Waypoints[0].X, route.Waypoints[0].Y);
                        break;

                    case "endpatrol":
                        sessionData.WorkData.OnWork = false;
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Patrol duty ended.", 3000);
                        break;

                    case "getboat":
                        SpawnPatrolBoat(player);
                        break;

                    case "reports":
                        int activeSmuggling = SmugglingRoutes.ActiveMissions.Count;
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            $"Active smuggling operations detected: {activeSmuggling}\n" +
                            "Monitor dispatch channel for specific alerts.", 5000);
                        break;

                    case "search":
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            "Search & Seize Protocol active. Approach suspect vehicles in water.\n" +
                            "Use /searchvessel [id] to inspect a nearby boat for contraband.", 5000);
                        break;

                    case "backup":
                        BroadcastToWaterForces($"[BACKUP REQUEST] Officer {player.Name} requesting backup at " +
                            $"{player.Position.X:F0}, {player.Position.Y:F0}!");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Backup request sent to all water units.", 3000);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_waterPoliceMenu Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.WaterPoliceBoatSpawn)]
        public static void OnBoatSpawn(ExtPlayer player)
        {
            try
            {
                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.WATERPOLICE && fracId != (int)Models.Fractions.COASTGUARD)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Only Water Police and Coast Guard can use these docks.", 3000);
                    return;
                }

                SpawnPatrolBoat(player);
            }
            catch (Exception e)
            {
                Log.Write($"OnBoatSpawn Exception: {e.ToString()}");
            }
        }

        private static void SpawnPatrolBoat(ExtPlayer player)
        {
            try
            {
                Vector3 spawnPos = BoatSpawns[0];
                float minDist = float.MaxValue;
                foreach (var dock in BoatSpawns)
                {
                    float dist = player.Position.DistanceTo(dock);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        spawnPos = dock;
                    }
                }

                var boat = NAPI.Vehicle.CreateVehicle(VehicleHash.PolicePredator, spawnPos, 0.0f, 0, 0);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    "Police patrol boat spawned at the dock. Board it and begin your patrol!", 3000);
            }
            catch (Exception e)
            {
                Log.Write($"SpawnPatrolBoat Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.FractionWaterPoliceArrest)]
        public static void OnArrestArea(ExtPlayer player)
        {
            try
            {
                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.WATERPOLICE)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Only Water Police can process arrests here.", 3000);
                    return;
                }

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                    "Arrest Processing Area. Bring suspects here to process them.\n" +
                    "Use /wpArrest [id] [reason] [minutes] to arrest a suspect.", 5000);
            }
            catch (Exception e)
            {
                Log.Write($"OnArrestArea Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.FractionWaterPolice)]
        public static void OnWaterPoliceStation(ExtPlayer player, int index)
        {
            try
            {
                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.WATERPOLICE)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You're not Water Police.", 3000);
                    return;
                }

                switch (index)
                {
                    case 2: // Cloakroom
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            "Water Police Cloakroom - Change into your uniform.", 3000);
                        break;
                    case 3: // Armory
                        var frameList = new FrameListData();
                        frameList.Header = "Water Police Armory";
                        frameList.Callback = callback_wpArmory;

                        frameList.List.Add(new ListData("Pistol + Ammo", "pistol"));
                        frameList.List.Add(new ListData("Shotgun + Ammo", "shotgun"));
                        frameList.List.Add(new ListData("SMG + Ammo", "smg"));
                        frameList.List.Add(new ListData("Flashlight", "flashlight"));
                        frameList.List.Add(new ListData("Body Armor", "armor"));
                        frameList.List.Add(new ListData("Handcuffs", "cuffs"));
                        frameList.List.Add(new ListData("Flare Gun", "flare"));

                        Trigger.ClientEvent(player, "client.framelist.show", frameList);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write($"OnWaterPoliceStation Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_wpArmory")]
        public static void callback_wpArmory(ExtPlayer player, string value)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"Equipment issued: {value}. Stay safe out there, officer.", 3000);
            }
            catch (Exception e)
            {
                Log.Write($"callback_wpArmory Exception: {e.ToString()}");
            }
        }

        private static void BroadcastToWaterForces(string message)
        {
            try
            {
                var players = NAPI.Pools.GetAllPlayers();
                foreach (var p in players)
                {
                    var extP = (ExtPlayer)p;
                    if (extP == null) continue;
                    var fracId = extP.GetFractionId();
                    if (fracId == (int)Models.Fractions.WATERPOLICE ||
                        fracId == (int)Models.Fractions.COASTGUARD)
                    {
                        Notify.Send(extP, NotifyType.Warning, NotifyPosition.BottomCenter, message, 8000);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write($"BroadcastToWaterForces Exception: {e.ToString()}");
            }
        }
    }

    public class PatrolRoute
    {
        public string Name { get; set; }
        public List<Vector3> Waypoints { get; set; }

        public PatrolRoute(string name, List<Vector3> waypoints)
        {
            Name = name;
            Waypoints = waypoints;
        }
    }
}
