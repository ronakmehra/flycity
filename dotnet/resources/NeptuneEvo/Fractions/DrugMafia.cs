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

namespace NeptuneEvo.Fractions
{
    class DrugMafia : Script
    {
        private static readonly nLog Log = new nLog("Fractions.DrugMafia");

        // Drug Mafia HQ - abandoned warehouse in docks
        public static Vector3 HQPosition = new Vector3(1242.5f, -3200.3f, 5.9f);
        
        // Drug Mafia safe house interior
        public static Vector3 SafeHouseEntrance = new Vector3(1238.8f, -3196.1f, 5.9f);
        public static Vector3 SafeHouseInterior = new Vector3(1121.3f, -3195.2f, -49.5f);
        
        // Drug distribution warehouse
        public static Vector3 DistributionCenter = new Vector3(1124.5f, -3190.8f, -49.5f);
        
        // Import dock - where drug shipments arrive by boat
        public static Vector3 ImportDock = new Vector3(1299.8f, -3264.5f, 5.5f);
        
        // Money laundering businesses
        public static List<Vector3> LaunderingLocations = new List<Vector3>()
        {
            new Vector3(-1479.8f, -377.5f, 40.2f),     // Morningwood Laundromat
            new Vector3(-730.5f, -909.2f, 19.2f),       // Vespucci Car Wash
            new Vector3(178.3f, -1747.8f, 29.4f),       // South LS Pawn Shop
            new Vector3(-1036.2f, -2740.1f, 13.8f),     // Airport Strip Club
        };

        // Territory control zones
        public static List<Vector3> TerritoryZones = new List<Vector3>()
        {
            new Vector3(-47.8f, -1757.8f, 29.4f),      // Davis
            new Vector3(486.3f, -1528.3f, 30.3f),       // Rancho
            new Vector3(1435.6f, -1491.6f, 63.6f),      // Cypress Flats
            new Vector3(-1368.5f, -742.8f, 24.2f),      // Del Perro Pier
            new Vector3(1259.8f, -3245.5f, 5.5f),       // Terminal
        };

        // Drug Mafia ranks
        public static Dictionary<int, string> Ranks = new Dictionary<int, string>()
        {
            {1, "Associate"},
            {2, "Soldier"},
            {3, "Distributor"},
            {4, "Captain"},
            {5, "Consigliere"},
            {6, "Underboss"},
            {7, "Boss"},
            {8, "Don"},
        };

        // Shipment values
        public static int SmallShipmentValue = 25000;
        public static int MediumShipmentValue = 75000;
        public static int LargeShipmentValue = 200000;

        // Jail times
        public static int JailTimeDrugTrafficking = 35;
        public static int JailTimeMoneyLaundering = 25;
        public static int JailTimeTerritoryWar = 20;

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                // HQ
                Main.CreateBlip(new Main.BlipData(484, "Drug Mafia", HQPosition, 1, false, 0.8f));

                // Safe house entrance
                CustomColShape.CreateCylinderColShape(SafeHouseEntrance, 2, 2, 0, ColShapeEnums.DrugMafiaHQ);
                NAPI.Marker.CreateMarker(1, SafeHouseEntrance - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 1.5f, new Color(75, 0, 130, 180));

                // Import dock
                CustomColShape.CreateCylinderColShape(ImportDock, 5, 3, 0, ColShapeEnums.DrugMafiaImport);
                NAPI.Marker.CreateMarker(1, ImportDock - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 3.0f, new Color(75, 0, 130, 100));
                NAPI.TextLabel.CreateTextLabel("~p~Import Dock\n~w~Receive drug shipments",
                    ImportDock + new Vector3(0, 0, 0.5), 10F, 0.4F, 0, new Color(128, 0, 128));

                // Distribution center
                CustomColShape.CreateCylinderColShape(DistributionCenter, 2, 2, 0, ColShapeEnums.DrugMafiaDistribution);
                NAPI.TextLabel.CreateTextLabel("~p~Distribution Center\n~w~Prepare shipments",
                    DistributionCenter + new Vector3(0, 0, 0.5), 5F, 0.3F, 0, new Color(128, 0, 128));

                // Money laundering points
                foreach (var loc in LaunderingLocations)
                {
                    CustomColShape.CreateCylinderColShape(loc, 2, 2, 0, ColShapeEnums.MoneyLaundering);
                }

                // Territory zones
                foreach (var zone in TerritoryZones)
                {
                    CustomColShape.CreateCylinderColShape(zone, 50, 5, 0, ColShapeEnums.DrugMafiaTerritory);
                }

                Log.Write("DrugMafia system initialized.");
            }
            catch (Exception e)
            {
                Log.Write($"Event_ResourceStart Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.DrugMafiaHQ)]
        public static void OnDrugMafiaHQ(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.DRUGMAFIA)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You're not welcome here.", 3000);
                    return;
                }

                NAPI.Entity.SetEntityPosition(player, SafeHouseInterior + new Vector3(0, 0, 1.12));
                Main.PlayerEnterInterior(player, SafeHouseInterior + new Vector3(0, 0, 1.12));
            }
            catch (Exception e)
            {
                Log.Write($"OnDrugMafiaHQ Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.DrugMafiaImport)]
        public static void OnDrugMafiaImport(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.DRUGMAFIA)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This is a restricted area.", 3000);
                    return;
                }

                if (!player.IsInVehicle)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You need a vehicle to receive shipments.", 3000);
                    return;
                }

                var frameList = new FrameListData();
                frameList.Header = "Import Dock - Drug Shipments";
                frameList.Callback = callback_drugImport;

                frameList.List.Add(new ListData($"Receive Small Shipment (${SmallShipmentValue})", "small"));
                frameList.List.Add(new ListData($"Receive Medium Shipment (${MediumShipmentValue})", "medium"));
                frameList.List.Add(new ListData($"Receive Large Shipment (${LargeShipmentValue})", "large"));
                frameList.List.Add(new ListData("Check Shipment Status", "status"));

                Trigger.ClientEvent(player, "client.framelist.show", frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnDrugMafiaImport Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_drugImport")]
        public static void callback_drugImport(ExtPlayer player, string value)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;
                var fractionData = player.GetFractionData();
                if (fractionData == null) return;

                int cost = 0;
                int drugAmount = 0;
                string shipmentType = "";

                switch (value)
                {
                    case "small": cost = SmallShipmentValue; drugAmount = 50; shipmentType = "Small"; break;
                    case "medium": cost = MediumShipmentValue; drugAmount = 200; shipmentType = "Medium"; break;
                    case "large": cost = LargeShipmentValue; drugAmount = 500; shipmentType = "Large"; break;
                    case "status":
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "No active shipments inbound.", 3000);
                        return;
                    default: return;
                }

                if (fractionData.Money < cost)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        $"Organization funds insufficient. Need ${cost}.", 3000);
                    return;
                }

                fractionData.Money -= cost;

                var vehicle = (ExtVehicle)player.Vehicle;
                Chars.Repository.AddNewItem(null, VehicleModel.VehicleManager.GetVehicleToInventory(vehicle.NumberPlate), 
                    "vehicle", ItemId.Drugs, drugAmount);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"{shipmentType} shipment received! {drugAmount} units loaded into vehicle. Cost: ${cost}", 5000);

                // 30% chance coast guard / water police gets alerted
                Random rnd = new Random();
                if (rnd.Next(100) < 30)
                {
                    AlertLawEnforcement(player, $"Suspicious boat activity at Terminal docks. Possible drug import.");
                }

                Fractions.Table.Logs.Repository.AddLogs(player, FractionLogsType.TakeMoney, 
                    $"Drug import: {shipmentType} shipment ({drugAmount} units, ${cost})");
            }
            catch (Exception e)
            {
                Log.Write($"callback_drugImport Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.MoneyLaundering)]
        public static void OnMoneyLaundering(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.DRUGMAFIA && fracId != (int)Models.Fractions.DRUGLORD)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This business doesn't offer special services to you.", 3000);
                    return;
                }

                var frameList = new FrameListData();
                frameList.Header = "Money Laundering";
                frameList.Callback = callback_launder;

                frameList.List.Add(new ListData("Launder $10,000 (Fee: 15%)", "10k"));
                frameList.List.Add(new ListData("Launder $50,000 (Fee: 12%)", "50k"));
                frameList.List.Add(new ListData("Launder $100,000 (Fee: 10%)", "100k"));

                Trigger.ClientEvent(player, "client.framelist.show", frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnMoneyLaundering Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_launder")]
        public static void callback_launder(ExtPlayer player, string value)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                int dirty = 0;
                int clean = 0;

                switch (value)
                {
                    case "10k": dirty = 10000; clean = 8500; break;
                    case "50k": dirty = 50000; clean = 44000; break;
                    case "100k": dirty = 100000; clean = 90000; break;
                    default: return;
                }

                // Check if player has enough dirty money
                if (characterData.Money < dirty)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        $"You don't have ${dirty} to launder.", 3000);
                    return;
                }

                MoneySystem.Wallet.Change(player, -dirty);
                MoneySystem.Wallet.Change(player, clean);
                
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"Laundered ${dirty} -> Received ${clean} clean money. Fee: ${dirty - clean}", 5000);

                // 15% chance of getting caught
                Random rnd = new Random();
                if (rnd.Next(100) < 15)
                {
                    AlertLawEnforcement(player, $"Suspicious financial transaction detected. Possible money laundering.");
                    characterData.WantedLVL = (characterData.WantedLVL ?? 0) + 3;
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        "The bank flagged your transaction! Police have been alerted!", 5000);
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_launder Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.DrugMafiaDistribution)]
        public static void OnDistribution(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.DRUGMAFIA)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Only Drug Mafia members can access distribution.", 3000);
                    return;
                }

                var frameList = new FrameListData();
                frameList.Header = "Distribution Center";
                frameList.Callback = callback_distribution;

                frameList.List.Add(new ListData("Start Distribution Run (Los Santos)", "run_ls"));
                frameList.List.Add(new ListData("Start Distribution Run (Blaine County)", "run_blaine"));
                frameList.List.Add(new ListData("Start Smuggling Run (Cayo Perico)", "run_cayo"));
                frameList.List.Add(new ListData("Check Inventory", "check"));

                Trigger.ClientEvent(player, "client.framelist.show", frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnDistribution Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_distribution")]
        public static void callback_distribution(ExtPlayer player, string value)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;

                switch (value)
                {
                    case "run_ls":
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            "Distribution run started! Deliver drugs to marked locations in Los Santos. Be careful of police checkpoints!", 5000);
                        // Set first delivery point
                        Trigger.ClientEvent(player, "createWaypoint", TerritoryZones[0].X, TerritoryZones[0].Y);
                        break;

                    case "run_blaine":
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            "Distribution run started! Head to Blaine County. Long drive but bigger payoff!", 5000);
                        Trigger.ClientEvent(player, "createWaypoint", 1962.5f, 3817.2f);
                        break;

                    case "run_cayo":
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            "Smuggling run to Cayo Perico initiated! Get a boat and head to the island. High risk, high reward!", 5000);
                        Trigger.ClientEvent(player, "createWaypoint", ImportDock.X, ImportDock.Y);
                        break;

                    case "check":
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            "Distribution center inventory: Check the warehouse terminal.", 3000);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_distribution Exception: {e.ToString()}");
            }
        }

        public static void AlertLawEnforcement(ExtPlayer suspect, string message)
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
                        fracId == (int)Models.Fractions.COASTGUARD)
                    {
                        Notify.Send(extP, NotifyType.Warning, NotifyPosition.BottomCenter, $"[DISPATCH] {message}", 8000);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write($"AlertLawEnforcement Exception: {e.ToString()}");
            }
        }
    }
}
