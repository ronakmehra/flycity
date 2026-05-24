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
using NeptuneEvo.VehicleData.LocalData;

namespace NeptuneEvo.Fractions
{
    class DrugLord : Script
    {
        private static readonly nLog Log = new nLog("Fractions.DrugLord");

        // Drug Lord HQ - hidden underground lab in the desert
        public static Vector3 HQPosition = new Vector3(1391.5f, 3606.2f, 38.9f);
        
        // Drug Lab entrance (hidden in Sandy Shores)
        public static Vector3 LabEntrance = new Vector3(1388.2f, 3601.8f, 38.9f);
        public static Vector3 LabInterior = new Vector3(1009.5f, -3200.0f, -38.9f);
        
        // Drug processing stations
        public static Vector3 ProcessingStation = new Vector3(1012.3f, -3195.5f, -38.9f);
        
        // Drug stash houses across Los Santos
        public static List<Vector3> StashHouses = new List<Vector3>()
        {
            new Vector3(-47.8f, -1757.8f, 29.4f),      // Davis
            new Vector3(486.3f, -1528.3f, 30.3f),       // Rancho
            new Vector3(971.1f, -1734.5f, 30.5f),       // El Burro Heights
            new Vector3(1398.2f, -2058.3f, 52.1f),      // Palmer-Taylor Power
            new Vector3(-1165.8f, -1576.2f, 4.4f),      // Vespucci Beach
        };
        
        // Drug sale points (street corners)
        public static List<Vector3> DrugSalePoints = new List<Vector3>()
        {
            new Vector3(113.3f, -1961.4f, 20.7f),       // South LS
            new Vector3(-222.5f, -1617.4f, 35.9f),      // Davis
            new Vector3(486.3f, -1528.3f, 30.3f),       // Rancho
            new Vector3(971.1f, -1734.5f, 30.5f),       // El Burro Heights
            new Vector3(1435.6f, -1491.6f, 63.6f),      // Cypress Flats
            new Vector3(-1368.5f, -742.8f, 24.2f),      // West Vinewood
            new Vector3(378.2f, -2051.1f, 22.3f),       // Strawberry
        };

        // Drug production types
        public static Dictionary<string, int> DrugTypes = new Dictionary<string, int>()
        {
            {"Marijuana", 250},
            {"Cocaine", 800},
            {"Methamphetamine", 1200},
            {"Heroin", 1500},
            {"Ecstasy", 600},
        };

        // Drug Lord ranks
        public static Dictionary<int, string> Ranks = new Dictionary<int, string>()
        {
            {1, "Street Dealer"},
            {2, "Runner"},
            {3, "Cook"},
            {4, "Lieutenant"},
            {5, "Enforcer"},
            {6, "Underboss"},
            {7, "Right Hand"},
            {8, "Drug Lord"},
        };

        // Wanted level for drug operations
        public static int WantedLevelForDealing = 2;
        public static int WantedLevelForProduction = 4;
        public static int WantedLevelForSmuggling = 5;
        
        // Jail time (minutes)
        public static int JailTimeDealing = 15;
        public static int JailTimeProduction = 30;
        public static int JailTimeSmuggling = 45;

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                // HQ blip (hidden from non-members)
                Main.CreateBlip(new Main.BlipData(140, "Drug Lord HQ", HQPosition, 1, false, 0.8f));

                // Lab entrance
                CustomColShape.CreateCylinderColShape(LabEntrance, 2, 2, 0, ColShapeEnums.DrugLordLab);
                NAPI.Marker.CreateMarker(1, LabEntrance - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 1.5f, new Color(128, 0, 128, 180));

                // Drug sale points
                foreach (var point in DrugSalePoints)
                {
                    CustomColShape.CreateCylinderColShape(point, 3, 2, 0, ColShapeEnums.DrugSalePoint);
                    NAPI.Marker.CreateMarker(1, point - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 2.0f, new Color(128, 0, 128, 100));
                }

                // Stash houses
                foreach (var stash in StashHouses)
                {
                    CustomColShape.CreateCylinderColShape(stash, 2, 2, 0, ColShapeEnums.DrugStashHouse);
                }

                // Processing station
                CustomColShape.CreateCylinderColShape(ProcessingStation, 2, 2, 0, ColShapeEnums.DrugProcessing);
                NAPI.TextLabel.CreateTextLabel("~p~Drug Processing Station\n~w~Cook your product here",
                    ProcessingStation + new Vector3(0, 0, 0.5), 5F, 0.3F, 0, new Color(128, 0, 128));

                Log.Write("DrugLord system initialized.");
            }
            catch (Exception e)
            {
                Log.Write($"Event_ResourceStart Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.DrugLordLab)]
        public static void OnDrugLordLab(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.DRUGLORD)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You don't have access to this location.", 3000);
                    return;
                }

                NAPI.Entity.SetEntityPosition(player, LabInterior + new Vector3(0, 0, 1.12));
                Main.PlayerEnterInterior(player, LabInterior + new Vector3(0, 0, 1.12));
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Welcome to the lab. Be careful - if police raid this place, you're done.", 5000);
            }
            catch (Exception e)
            {
                Log.Write($"OnDrugLordLab Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.DrugSalePoint)]
        public static void OnDrugSalePoint(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.DRUGLORD && fracId != (int)Models.Fractions.DRUGMAFIA)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You're not a drug dealer.", 3000);
                    return;
                }

                var frameList = new FrameListData();
                frameList.Header = "Street Sales";
                frameList.Callback = callback_drugSale;

                frameList.List.Add(new ListData("Sell Marijuana ($250)", "sell_marijuana"));
                frameList.List.Add(new ListData("Sell Cocaine ($800)", "sell_cocaine"));
                frameList.List.Add(new ListData("Sell Meth ($1,200)", "sell_meth"));
                frameList.List.Add(new ListData("Sell Heroin ($1,500)", "sell_heroin"));
                frameList.List.Add(new ListData("Sell Ecstasy ($600)", "sell_ecstasy"));

                Trigger.ClientEvent(player, "client.framelist.show", frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnDrugSalePoint Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_drugSale")]
        public static void callback_drugSale(ExtPlayer player, string value)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;

                string drugName = "";
                int price = 0;

                switch (value)
                {
                    case "sell_marijuana": drugName = "Marijuana"; price = 250; break;
                    case "sell_cocaine": drugName = "Cocaine"; price = 800; break;
                    case "sell_meth": drugName = "Methamphetamine"; price = 1200; break;
                    case "sell_heroin": drugName = "Heroin"; price = 1500; break;
                    case "sell_ecstasy": drugName = "Ecstasy"; price = 600; break;
                    default: return;
                }

                // Check if player has drugs in inventory
                int drugCount = Chars.Repository.getCountItem(player, "inventory", ItemId.Drugs);
                if (drugCount <= 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You don't have any drugs to sell!", 3000);
                    return;
                }

                // Random chance of customer buying
                Random rnd = new Random();
                int chance = rnd.Next(100);
                
                if (chance < 70) // 70% success
                {
                    Chars.Repository.Remove(player, "inventory", ItemId.Drugs, 1);
                    MoneySystem.Wallet.Change(player, price);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                        $"You sold {drugName} for ${price}. Be careful, police might be watching!", 3000);

                    // 25% chance police gets alerted
                    if (rnd.Next(100) < 25)
                    {
                        Police.SendPoliceAlert(player, $"Suspicious drug activity reported near {player.Position.X:F0}, {player.Position.Y:F0}");
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, 
                            "Someone might have called the cops! Move quickly!", 3000);
                    }
                }
                else if (chance < 90) // 20% customer refuses
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, 
                        "The customer backed out. Try again later.", 3000);
                }
                else // 10% undercover cop
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        "UNDERCOVER COP! You've been spotted! Police are on their way!", 5000);
                    Police.SendPoliceAlert(player, $"Drug dealing in progress! Suspect spotted near {player.Position.X:F0}, {player.Position.Y:F0}");
                    // Add wanted level
                    characterData.WantedLVL = (characterData.WantedLVL ?? 0) + WantedLevelForDealing;
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_drugSale Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.DrugProcessing)]
        public static void OnDrugProcessing(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.DRUGLORD)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Only Drug Lord members can process drugs.", 3000);
                    return;
                }

                var frameList = new FrameListData();
                frameList.Header = "Drug Processing Lab";
                frameList.Callback = callback_drugProcess;

                frameList.List.Add(new ListData("Process Marijuana (x5 raw -> x3 product)", "process_weed"));
                frameList.List.Add(new ListData("Cook Cocaine (x10 raw -> x5 product)", "process_cocaine"));
                frameList.List.Add(new ListData("Cook Methamphetamine (x8 raw -> x4 product)", "process_meth"));
                frameList.List.Add(new ListData("Refine Heroin (x12 raw -> x4 product)", "process_heroin"));
                frameList.List.Add(new ListData("Press Ecstasy Pills (x6 raw -> x8 pills)", "process_ecstasy"));

                Trigger.ClientEvent(player, "client.framelist.show", frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnDrugProcessing Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_drugProcess")]
        public static void callback_drugProcess(ExtPlayer player, string value)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                int rawNeeded = 0;
                int productMade = 0;
                string productName = "";

                switch (value)
                {
                    case "process_weed": rawNeeded = 5; productMade = 3; productName = "Marijuana"; break;
                    case "process_cocaine": rawNeeded = 10; productMade = 5; productName = "Cocaine"; break;
                    case "process_meth": rawNeeded = 8; productMade = 4; productName = "Methamphetamine"; break;
                    case "process_heroin": rawNeeded = 12; productMade = 4; productName = "Heroin"; break;
                    case "process_ecstasy": rawNeeded = 6; productMade = 8; productName = "Ecstasy"; break;
                    default: return;
                }

                int rawCount = Chars.Repository.getCountItem(player, "inventory", ItemId.Drugs);
                if (rawCount < rawNeeded)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        $"You need {rawNeeded} raw materials. You only have {rawCount}.", 3000);
                    return;
                }

                Chars.Repository.Remove(player, "inventory", ItemId.Drugs, rawNeeded);
                Chars.Repository.AddNewItem(player, player, "inventory", ItemId.Drugs, productMade);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"Processed {productName}! Used {rawNeeded} raw materials, produced {productMade} units.", 5000);
            }
            catch (Exception e)
            {
                Log.Write($"callback_drugProcess Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.DrugStashHouse)]
        public static void OnDrugStashHouse(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.DRUGLORD && fracId != (int)Models.Fractions.DRUGMAFIA)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This stash house is locked.", 3000);
                    return;
                }

                var frameList = new FrameListData();
                frameList.Header = "Stash House";
                frameList.Callback = callback_stashHouse;

                frameList.List.Add(new ListData("Store Drugs", "store"));
                frameList.List.Add(new ListData("Retrieve Drugs", "retrieve"));
                frameList.List.Add(new ListData("Store Money", "storemoney"));
                frameList.List.Add(new ListData("Retrieve Money", "getmoney"));

                Trigger.ClientEvent(player, "client.framelist.show", frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnDrugStashHouse Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_stashHouse")]
        public static void callback_stashHouse(ExtPlayer player, string value)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                switch (value)
                {
                    case "store":
                        int drugCount = Chars.Repository.getCountItem(player, "inventory", ItemId.Drugs);
                        if (drugCount <= 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You have no drugs to store.", 3000);
                            return;
                        }
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                            $"Stored {drugCount} drug units in the stash house.", 3000);
                        break;

                    case "retrieve":
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                            "Retrieved drugs from stash house.", 3000);
                        break;

                    case "storemoney":
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                            "Money stored in stash house safe.", 3000);
                        break;

                    case "getmoney":
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                            "Money retrieved from stash house safe.", 3000);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_stashHouse Exception: {e.ToString()}");
            }
        }

        // Police alert helper
        public static class Police
        {
            public static void SendPoliceAlert(ExtPlayer suspect, string message)
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
                            Notify.Send(extP, NotifyType.Warning, NotifyPosition.BottomCenter, 
                                $"[DISPATCH] {message}", 8000);
                        }
                    }
                }
                catch (Exception e)
                {
                    var log = new nLog("DrugLord.Police");
                    log.Write($"SendPoliceAlert Exception: {e.ToString()}");
                }
            }
        }
    }
}
