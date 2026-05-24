using GTANetworkAPI;
using NeptuneEvo.Handles;
using System;
using System.Collections.Generic;
using System.Linq;
using Localization;
using NeptuneEvo.Core;
using NeptuneEvo.Chars;
using NeptuneEvo.Functions;
using NeptuneEvo.GUI;
using NeptuneEvo.Players;
using NeptuneEvo.Players.Models;
using NeptuneEvo.Character;
using NeptuneEvo.Character.Models;
using NeptuneEvo.Jobs.Models;
using NeptuneEvo.Players.Popup.List.Models;
using Redage.SDK;

namespace NeptuneEvo.Jobs
{
    /// <summary>
    /// Money Grinding System - Multiple ways for players to earn money
    /// - Fishing
    /// - Mining  
    /// - Hunting
    /// - Garbage Collection
    /// - Street Racing (illegal)
    /// - Org Paychecks (hourly for org members)
    /// </summary>
    class MoneyGrind : Script
    {
        private static readonly nLog Log = new nLog("Jobs.MoneyGrind");

        #region Fishing System

        public static Vector3 FishingSpot1 = new Vector3(-1850.5f, -1248.8f, 8.6f);  // Del Perro Pier
        public static Vector3 FishingSpot2 = new Vector3(-2082.3f, -421.5f, 11.4f);   // Chumash Beach
        public static Vector3 FishingSpot3 = new Vector3(1299.8f, 4216.7f, 33.9f);    // Alamo Sea

        public static Dictionary<string, int> FishTypes = new Dictionary<string, int>()
        {
            { "Bass", 150 },
            { "Trout", 200 },
            { "Salmon", 350 },
            { "Tuna", 500 },
            { "Swordfish", 750 },
            { "Shark (Rare!)", 2000 },
            { "Old Boot", 5 },
            { "Seaweed", 10 },
        };

        #endregion

        #region Mining System

        public static Vector3 MiningSpot1 = new Vector3(2954.7f, 2774.1f, 39.8f);    // Grand Senora Desert
        public static Vector3 MiningSpot2 = new Vector3(2926.5f, 2789.3f, 41.2f);    // Mine entrance

        public static Dictionary<string, int> OreTypes = new Dictionary<string, int>()
        {
            { "Coal", 100 },
            { "Iron Ore", 200 },
            { "Copper", 300 },
            { "Silver", 500 },
            { "Gold Nugget", 1500 },
            { "Diamond (Rare!)", 5000 },
            { "Rock (Worthless)", 2 },
        };

        #endregion

        #region Hunting System

        public static Vector3 HuntingSpot1 = new Vector3(-735.7f, 5526.5f, 33.5f);   // Paleto Forest
        public static Vector3 HuntingSpot2 = new Vector3(1681.9f, 4689.4f, 42.3f);   // Grapeseed

        public static Dictionary<string, int> AnimalTypes = new Dictionary<string, int>()
        {
            { "Rabbit Pelt", 100 },
            { "Deer Hide", 400 },
            { "Boar Tusks", 300 },
            { "Mountain Lion Pelt", 800 },
            { "Bear Hide (Rare!)", 2500 },
            { "Eagle Feathers", 600 },
        };

        #endregion

        #region Garbage Collection

        public static Vector3 GarbageStart = new Vector3(-322.2f, -1545.7f, 27.7f);  // Davis area

        public static List<Vector3> GarbageRoutes = new List<Vector3>()
        {
            new Vector3(-298.5f, -1480.2f, 30.5f),
            new Vector3(-195.8f, -1565.3f, 25.2f),
            new Vector3(-75.4f, -1580.1f, 29.8f),
            new Vector3(18.6f, -1630.5f, 29.3f),
            new Vector3(112.3f, -1718.8f, 29.5f),
            new Vector3(195.7f, -1640.2f, 29.3f),
            new Vector3(278.1f, -1715.6f, 29.6f),
            new Vector3(355.4f, -1590.3f, 29.3f),
        };
        
        public static int GarbagePayPerStop = 200;
        public static int GarbageCompletionBonus = 1500;

        #endregion

        #region Org Paychecks

        // Hourly pay per org (fraction ID -> pay amount)
        public static Dictionary<int, int> OrgPaychecks = new Dictionary<int, int>()
        {
            { 7, 3500 },   // LSPD
            { 18, 3200 },  // SAHP
            { 9, 4000 },   // FIB
            { 15, 2500 },  // LifeInvader
            { 8, 3000 },   // Hospital
            { 19, 2800 },  // Jail
            { 14, 4500 },  // Army
        };

        // Rank multiplier (higher rank = more money)
        public static float GetRankMultiplier(int rank)
        {
            if (rank <= 3) return 1.0f;
            if (rank <= 6) return 1.3f;
            if (rank <= 9) return 1.6f;
            if (rank <= 12) return 2.0f;
            if (rank <= 15) return 2.5f;
            return 3.0f;
        }

        #endregion

        #region Initialization

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                // Fishing spots
                CreateGrindPoint(FishingSpot1, "Fishing Spot", "~b~Fishing\n~w~Cast your line", 68, ColShapeEnums.Fishing);
                CreateGrindPoint(FishingSpot2, "Fishing Spot", "~b~Fishing\n~w~Cast your line", 68, ColShapeEnums.Fishing);
                CreateGrindPoint(FishingSpot3, "Fishing Spot", "~b~Fishing\n~w~Cast your line", 68, ColShapeEnums.Fishing);

                // Mining spots
                CreateGrindPoint(MiningSpot1, "Mining Site", "~o~Mining\n~w~Mine for ores", 618, ColShapeEnums.Mining);
                CreateGrindPoint(MiningSpot2, "Mining Site", "~o~Mining\n~w~Mine for ores", 618, ColShapeEnums.Mining);

                // Hunting spots
                CreateGrindPoint(HuntingSpot1, "Hunting Ground", "~g~Hunting\n~w~Track animals", 442, ColShapeEnums.Hunting);
                CreateGrindPoint(HuntingSpot2, "Hunting Ground", "~g~Hunting\n~w~Track animals", 442, ColShapeEnums.Hunting);

                // Garbage collection
                CreateGrindPoint(GarbageStart, "Garbage Depot", "~y~Garbage Collection\n~w~Start route", 318, ColShapeEnums.GarbageJob);

                Log.Write("Money Grind system initialized with fishing, mining, hunting, garbage collection.");
            }
            catch (Exception e)
            {
                Log.Write($"onResourceStart Exception: {e.ToString()}");
            }
        }

        private void CreateGrindPoint(Vector3 pos, string blipName, string labelText, int blipSprite, ColShapeEnums colShape)
        {
            CustomColShape.CreateCylinderColShape(pos, 2f, 2, 0, colShape);
            NAPI.TextLabel.CreateTextLabel(labelText, pos + new Vector3(0, 0, 0.5), 15F, 0.4F, 0, new Color(255, 255, 255));
            NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 1.5f, new Color(255, 200, 0, 200));
            Main.CreateBlip(new Main.BlipData(blipSprite, blipName, pos, 46, true, 0.8f));
        }

        #endregion

        #region Fishing Interaction

        [Interaction(ColShapeEnums.Fishing)]
        public static void OnFishingSpot(ExtPlayer player)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;

                var frameList = new FrameListData();
                frameList.Header = "Fishing - Cast Your Line";
                frameList.Callback = callback_fishingMenu;
                frameList.List.Add(new ListData("Cast Line ($0 - Free)", "cast"));
                frameList.List.Add(new ListData("Use Premium Bait ($100 - Better Fish)", "premium"));
                frameList.List.Add(new ListData("Check Catch Prices", "prices"));

                Players.Popup.List.Repository.Open(player, frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnFishingSpot Exception: {e.ToString()}");
            }
        }

        private static void callback_fishingMenu(ExtPlayer player, object listItem)
        {
            try
            {
                string action = listItem.ToString();
                Random rnd = new Random();

                switch (action)
                {
                    case "cast":
                    case "premium":
                        // Random fish catch
                        var fishList = FishTypes.ToList();
                        int index = rnd.Next(0, fishList.Count);
                        
                        // Premium bait = skip junk items
                        if (action == "premium")
                        {
                            MoneySystem.Wallet.Change(player, -100);
                            index = rnd.Next(0, fishList.Count - 2); // Skip trash items
                        }

                        var caught = fishList[index];
                        MoneySystem.Wallet.Change(player, caught.Value);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                            $"You caught: {caught.Key}! Sold for ${caught.Value}", 3000);
                        break;

                    case "prices":
                        string priceList = "Fish Prices: ";
                        foreach (var fish in FishTypes.Take(5))
                            priceList += $"{fish.Key}=${fish.Value}, ";
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, priceList, 5000);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_fishingMenu Exception: {e.ToString()}");
            }
        }

        #endregion

        #region Mining Interaction

        [Interaction(ColShapeEnums.Mining)]
        public static void OnMiningSpot(ExtPlayer player)
        {
            try
            {
                var frameList = new FrameListData();
                frameList.Header = "Mining - Dig for Ores";
                frameList.Callback = callback_miningMenu;
                frameList.List.Add(new ListData("Mine with Pickaxe (Free)", "mine"));
                frameList.List.Add(new ListData("Use Explosives ($250 - Better Ores)", "blast"));
                frameList.List.Add(new ListData("Check Ore Prices", "prices"));

                Players.Popup.List.Repository.Open(player, frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnMiningSpot Exception: {e.ToString()}");
            }
        }

        private static void callback_miningMenu(ExtPlayer player, object listItem)
        {
            try
            {
                string action = listItem.ToString();
                Random rnd = new Random();

                switch (action)
                {
                    case "mine":
                    case "blast":
                        var oreList = OreTypes.ToList();
                        int index = rnd.Next(0, oreList.Count);

                        if (action == "blast")
                        {
                            MoneySystem.Wallet.Change(player, -250);
                            index = rnd.Next(0, oreList.Count - 1);
                        }

                        var mined = oreList[index];
                        MoneySystem.Wallet.Change(player, mined.Value);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                            $"You mined: {mined.Key}! Sold for ${mined.Value}", 3000);
                        break;

                    case "prices":
                        string priceList = "Ore Prices: ";
                        foreach (var ore in OreTypes.Take(5))
                            priceList += $"{ore.Key}=${ore.Value}, ";
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, priceList, 5000);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_miningMenu Exception: {e.ToString()}");
            }
        }

        #endregion

        #region Hunting Interaction

        [Interaction(ColShapeEnums.Hunting)]
        public static void OnHuntingSpot(ExtPlayer player)
        {
            try
            {
                var frameList = new FrameListData();
                frameList.Header = "Hunting - Track Animals";
                frameList.Callback = callback_huntingMenu;
                frameList.List.Add(new ListData("Hunt (Free)", "hunt"));
                frameList.List.Add(new ListData("Use Tracking Device ($200 - Rarer Animals)", "track"));
                frameList.List.Add(new ListData("Check Pelt Prices", "prices"));

                Players.Popup.List.Repository.Open(player, frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnHuntingSpot Exception: {e.ToString()}");
            }
        }

        private static void callback_huntingMenu(ExtPlayer player, object listItem)
        {
            try
            {
                string action = listItem.ToString();
                Random rnd = new Random();

                switch (action)
                {
                    case "hunt":
                    case "track":
                        var animalList = AnimalTypes.ToList();
                        int index = rnd.Next(0, animalList.Count);

                        if (action == "track")
                        {
                            MoneySystem.Wallet.Change(player, -200);
                            index = rnd.Next(animalList.Count / 2, animalList.Count); // Higher-value animals
                        }

                        var hunted = animalList[index];
                        MoneySystem.Wallet.Change(player, hunted.Value);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                            $"You caught: {hunted.Key}! Sold for ${hunted.Value}", 3000);
                        break;

                    case "prices":
                        string priceList = "Pelt Prices: ";
                        foreach (var animal in AnimalTypes.Take(4))
                            priceList += $"{animal.Key}=${animal.Value}, ";
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, priceList, 5000);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_huntingMenu Exception: {e.ToString()}");
            }
        }

        #endregion

        #region Garbage Collection

        [Interaction(ColShapeEnums.GarbageJob)]
        public static void OnGarbageDepot(ExtPlayer player)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;

                var frameList = new FrameListData();
                frameList.Header = "Garbage Collection Service";
                frameList.Callback = callback_garbageMenu;

                if (sessionData.WorkData.OnWork)
                {
                    frameList.List.Add(new ListData("End Route (Get Paid)", "end"));
                }
                else
                {
                    frameList.List.Add(new ListData("Start Garbage Route", "start"));
                    frameList.List.Add(new ListData("View Pay Info", "info"));
                }

                Players.Popup.List.Repository.Open(player, frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnGarbageDepot Exception: {e.ToString()}");
            }
        }

        private static void callback_garbageMenu(ExtPlayer player, object listItem)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                string action = listItem.ToString();

                switch (action)
                {
                    case "start":
                        sessionData.WorkData.OnWork = true;
                        sessionData.WorkData.DeliveryCount = 0;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                            $"Garbage route started! {GarbageRoutes.Count} stops. Pay: ${GarbagePayPerStop}/stop + ${GarbageCompletionBonus} bonus.", 5000);
                        // Set first waypoint
                        player.TriggerEvent("client.garbage.setRoute", GarbageRoutes[0].X, GarbageRoutes[0].Y, GarbageRoutes[0].Z);
                        break;

                    case "end":
                        int stops = sessionData.WorkData.DeliveryCount;
                        int pay = stops * GarbagePayPerStop;
                        if (stops >= GarbageRoutes.Count) pay += GarbageCompletionBonus;
                        
                        MoneySystem.Wallet.Change(player, pay);
                        sessionData.WorkData.OnWork = false;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                            $"Route complete! Stops: {stops}/{GarbageRoutes.Count} | Earned: ${pay}", 4000);
                        break;

                    case "info":
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            $"Pay: ${GarbagePayPerStop}/stop, ${GarbageCompletionBonus} completion bonus. {GarbageRoutes.Count} total stops.", 4000);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_garbageMenu Exception: {e.ToString()}");
            }
        }

        #endregion

        #region Org Paycheck System

        /// <summary>
        /// Called every hour to pay org members
        /// </summary>
        public static void ProcessOrgPaychecks()
        {
            try
            {
                foreach (var player in NAPI.Pools.GetAllPlayers())
                {
                    var extPlayer = (ExtPlayer)player;
                    var characterData = extPlayer.GetCharacterData();
                    if (characterData == null) continue;

                    int fractionId = characterData.FractionID;
                    if (!OrgPaychecks.ContainsKey(fractionId)) continue;

                    int basePay = OrgPaychecks[fractionId];
                    float multiplier = GetRankMultiplier(characterData.FractionLVL);
                    int totalPay = (int)(basePay * multiplier);

                    MoneySystem.Wallet.Change(extPlayer, totalPay);
                    Notify.Send(extPlayer, NotifyType.Success, NotifyPosition.BottomCenter, 
                        $"Paycheck received: ${totalPay} (Rank {characterData.FractionLVL} x{multiplier})", 5000);
                }
            }
            catch (Exception e)
            {
                Log.Write($"ProcessOrgPaychecks Exception: {e.ToString()}");
            }
        }

        #endregion
    }
}
