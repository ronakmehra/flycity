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
    class WeaponSmugglers : Script
    {
        private static readonly nLog Log = new nLog("Fractions.WeaponSmugglers");

        // Weapon Smugglers HQ - hidden bunker
        public static Vector3 HQPosition = new Vector3(2493.5f, -383.2f, 92.9f);
        
        // Weapons warehouse
        public static Vector3 WarehouseEntrance = new Vector3(2490.1f, -379.8f, 92.9f);
        public static Vector3 WarehouseInterior = new Vector3(2150.5f, -222.3f, -49.5f);
        
        // Arms workshop (modify/upgrade weapons)
        public static Vector3 ArmsWorkshop = new Vector3(2153.8f, -218.5f, -49.5f);

        // Smuggling pickup points (Cayo Perico & offshore)
        public static List<Vector3> SmugglePickupPoints = new List<Vector3>()
        {
            new Vector3(4840.2f, -5175.5f, 2.0f),      // Cayo Perico dock
            new Vector3(3848.5f, 4463.2f, 2.5f),        // Paleto Bay coast
            new Vector3(1299.8f, -3350.5f, 0.5f),       // LS Terminal pier
            new Vector3(-1624.5f, -1052.8f, 1.2f),      // Del Perro Pier
            new Vector3(1418.2f, 6550.3f, 1.5f),        // North coast
        };

        // Smuggling drop-off points in Los Santos
        public static List<Vector3> SmuggleDropoffLS = new List<Vector3>()
        {
            new Vector3(963.5f, -2156.8f, 30.5f),       // El Burro Heights warehouse
            new Vector3(-330.2f, -1330.5f, 31.3f),       // South LS garage
            new Vector3(1087.3f, -2003.8f, 30.6f),      // Cypress industrial
            new Vector3(77.8f, -2005.3f, 18.4f),        // Rancho storage
            new Vector3(-1100.5f, -1623.8f, 4.5f),      // Vespucci pier storage
        };

        // Smuggling routes from Cayo Perico to Los Santos
        public static List<SmugglingRoute> Routes = new List<SmugglingRoute>()
        {
            new SmugglingRoute("Cayo Express", 4840.2f, -5175.5f, 963.5f, -2156.8f, 50000, 15),
            new SmugglingRoute("Paleto Run", 3848.5f, 4463.2f, -330.2f, -1330.5f, 35000, 12),
            new SmugglingRoute("Terminal Direct", 1299.8f, -3350.5f, 1087.3f, -2003.8f, 20000, 8),
            new SmugglingRoute("Pier Passage", -1624.5f, -1052.8f, -1100.5f, -1623.8f, 15000, 6),
            new SmugglingRoute("North Coast", 1418.2f, 6550.3f, 77.8f, -2005.3f, 45000, 14),
        };

        // Weapon catalog
        public static Dictionary<string, int> WeaponCatalog = new Dictionary<string, int>()
        {
            {"Pistol", 5000},
            {"SMG", 15000},
            {"Assault Rifle", 35000},
            {"Shotgun", 20000},
            {"Sniper Rifle", 50000},
            {"RPG", 150000},
            {"Grenade", 8000},
            {"Armor", 10000},
            {"Silencer", 7500},
            {"Extended Mag", 5000},
            {"Scope", 12000},
            {"Drum Magazine", 15000},
        };

        // Smuggler ranks
        public static Dictionary<int, string> Ranks = new Dictionary<int, string>()
        {
            {1, "Mule"},
            {2, "Courier"},
            {3, "Gunsmith"},
            {4, "Arms Dealer"},
            {5, "Logistics Chief"},
            {6, "Lieutenant"},
            {7, "Commander"},
            {8, "Kingpin"},
        };

        // Risk/Jail settings
        public static int WantedLevelSmuggling = 4;
        public static int WantedLevelSelling = 3;
        public static int JailTimeSmuggling = 40;
        public static int JailTimeSelling = 25;
        public static int PoliceAlertChance = 35; // 35% chance police gets alerted per run

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                // HQ (hidden from police)
                Main.CreateBlip(new Main.BlipData(150, "Arms Dealer", HQPosition, 1, false, 0.7f));

                // Warehouse entrance
                CustomColShape.CreateCylinderColShape(WarehouseEntrance, 2, 2, 0, ColShapeEnums.WeaponSmugglersHQ);
                NAPI.Marker.CreateMarker(1, WarehouseEntrance - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 1.5f, new Color(255, 69, 0, 180));

                // Arms workshop
                CustomColShape.CreateCylinderColShape(ArmsWorkshop, 2, 2, 0, ColShapeEnums.ArmsWorkshop);
                NAPI.TextLabel.CreateTextLabel("~o~Arms Workshop\n~w~Modify and upgrade weapons",
                    ArmsWorkshop + new Vector3(0, 0, 0.5), 5F, 0.3F, 0, new Color(255, 140, 0));

                // Pickup points
                foreach (var point in SmugglePickupPoints)
                {
                    CustomColShape.CreateCylinderColShape(point, 5, 3, 0, ColShapeEnums.SmugglePickup);
                    NAPI.Marker.CreateMarker(1, point - new Vector3(0, 0, 0.5), new Vector3(), new Vector3(), 3.0f, new Color(255, 69, 0, 80));
                }

                // Drop-off points
                foreach (var point in SmuggleDropoffLS)
                {
                    CustomColShape.CreateCylinderColShape(point, 4, 3, 0, ColShapeEnums.SmuggleDropoff);
                    NAPI.Marker.CreateMarker(1, point - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 2.5f, new Color(255, 140, 0, 80));
                }

                Log.Write("WeaponSmugglers system initialized.");
            }
            catch (Exception e)
            {
                Log.Write($"Event_ResourceStart Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.WeaponSmugglersHQ)]
        public static void OnWeaponSmugglersHQ(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.WEAPONSMUGGLERS)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This place is off limits.", 3000);
                    return;
                }

                NAPI.Entity.SetEntityPosition(player, WarehouseInterior + new Vector3(0, 0, 1.12));
                Main.PlayerEnterInterior(player, WarehouseInterior + new Vector3(0, 0, 1.12));
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Welcome to the arsenal.", 3000);
            }
            catch (Exception e)
            {
                Log.Write($"OnWeaponSmugglersHQ Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.ArmsWorkshop)]
        public static void OnArmsWorkshop(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.WEAPONSMUGGLERS)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Only weapon smugglers can use this workshop.", 3000);
                    return;
                }

                var frameList = new FrameListData();
                frameList.Header = "Arms Workshop";
                frameList.Callback = callback_armsWorkshop;

                frameList.List.Add(new ListData("Buy Pistol ($5,000)", "buy_pistol"));
                frameList.List.Add(new ListData("Buy SMG ($15,000)", "buy_smg"));
                frameList.List.Add(new ListData("Buy Assault Rifle ($35,000)", "buy_rifle"));
                frameList.List.Add(new ListData("Buy Shotgun ($20,000)", "buy_shotgun"));
                frameList.List.Add(new ListData("Buy Sniper Rifle ($50,000)", "buy_sniper"));
                frameList.List.Add(new ListData("Buy RPG ($150,000)", "buy_rpg"));
                frameList.List.Add(new ListData("Buy Grenades x5 ($8,000)", "buy_grenades"));
                frameList.List.Add(new ListData("Buy Body Armor ($10,000)", "buy_armor"));
                frameList.List.Add(new ListData("Attach Silencer ($7,500)", "mod_silencer"));
                frameList.List.Add(new ListData("Extended Magazine ($5,000)", "mod_extmag"));
                frameList.List.Add(new ListData("Scope ($12,000)", "mod_scope"));

                Trigger.ClientEvent(player, "client.framelist.show", frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnArmsWorkshop Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_armsWorkshop")]
        public static void callback_armsWorkshop(ExtPlayer player, string value)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;
                var fractionData = player.GetFractionData();
                if (fractionData == null) return;

                int cost = 0;
                string itemName = "";

                switch (value)
                {
                    case "buy_pistol": cost = 5000; itemName = "Pistol"; break;
                    case "buy_smg": cost = 15000; itemName = "SMG"; break;
                    case "buy_rifle": cost = 35000; itemName = "Assault Rifle"; break;
                    case "buy_shotgun": cost = 20000; itemName = "Shotgun"; break;
                    case "buy_sniper": cost = 50000; itemName = "Sniper Rifle"; break;
                    case "buy_rpg": cost = 150000; itemName = "RPG"; break;
                    case "buy_grenades": cost = 8000; itemName = "Grenades x5"; break;
                    case "buy_armor": cost = 10000; itemName = "Body Armor"; break;
                    case "mod_silencer": cost = 7500; itemName = "Silencer"; break;
                    case "mod_extmag": cost = 5000; itemName = "Extended Magazine"; break;
                    case "mod_scope": cost = 12000; itemName = "Scope"; break;
                    default: return;
                }

                if (fractionData.Money < cost)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        $"Organization funds insufficient. Need ${cost}.", 3000);
                    return;
                }

                fractionData.Money -= cost;
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"Purchased {itemName} for ${cost} from org funds.", 3000);

                Fractions.Table.Logs.Repository.AddLogs(player, FractionLogsType.TakeMoney, 
                    $"Arms purchase: {itemName} (${cost})");
            }
            catch (Exception e)
            {
                Log.Write($"callback_armsWorkshop Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.SmugglePickup)]
        public static void OnSmugglePickup(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.WEAPONSMUGGLERS)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Nothing here for you.", 3000);
                    return;
                }

                if (!player.IsInVehicle)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        "You need a vehicle (car or boat) to pick up the weapons crate.", 3000);
                    return;
                }

                var sessionData = player.GetSessionData();
                if (sessionData == null) return;

                // Find matching route
                SmugglingRoute activeRoute = null;
                foreach (var route in Routes)
                {
                    float dist = player.Position.DistanceTo(new Vector3(route.PickupX, route.PickupY, 2.0f));
                    if (dist < 20.0f)
                    {
                        activeRoute = route;
                        break;
                    }
                }

                if (activeRoute == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "No active pickup at this location.", 3000);
                    return;
                }

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"Weapons crate loaded! Route: {activeRoute.Name}. Deliver to drop-off point. Payout: ${activeRoute.Payout}. " +
                    $"WARNING: Police and Coast Guard may intercept you!", 8000);

                Trigger.ClientEvent(player, "createWaypoint", activeRoute.DropoffX, activeRoute.DropoffY);

                // Alert police/coast guard
                Random rnd = new Random();
                if (rnd.Next(100) < PoliceAlertChance)
                {
                    AlertAllLawEnforcement(player, $"Weapons smuggling activity detected! Suspect heading from coast to Los Santos.");
                }
            }
            catch (Exception e)
            {
                Log.Write($"OnSmugglePickup Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.SmuggleDropoff)]
        public static void OnSmuggleDropoff(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.WEAPONSMUGGLERS)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This is a restricted area.", 3000);
                    return;
                }

                // Find matching route for payout
                int payout = 25000; // default payout
                foreach (var route in Routes)
                {
                    float dist = player.Position.DistanceTo(new Vector3(route.DropoffX, route.DropoffY, 30.0f));
                    if (dist < 20.0f)
                    {
                        payout = route.Payout;
                        break;
                    }
                }

                MoneySystem.Wallet.Change(player, payout);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"Weapons crate delivered successfully! You earned ${payout}!", 5000);

                // XP/reputation gain
                var fractionData = player.GetFractionData();
                if (fractionData != null)
                {
                    fractionData.Money += payout / 5; // 20% goes to org
                    Fractions.Table.Logs.Repository.AddLogs(player, FractionLogsType.AddMoney, 
                        $"Smuggling run completed: +${payout} (personal) +${payout / 5} (org)");
                }
            }
            catch (Exception e)
            {
                Log.Write($"OnSmuggleDropoff Exception: {e.ToString()}");
            }
        }

        public static void AlertAllLawEnforcement(ExtPlayer suspect, string message)
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
                        Notify.Send(extP, NotifyType.Warning, NotifyPosition.BottomCenter, $"[HIGH PRIORITY] {message}", 10000);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write($"AlertAllLawEnforcement Exception: {e.ToString()}");
            }
        }
    }

    // Smuggling route data class
    public class SmugglingRoute
    {
        public string Name { get; set; }
        public float PickupX { get; set; }
        public float PickupY { get; set; }
        public float DropoffX { get; set; }
        public float DropoffY { get; set; }
        public int Payout { get; set; }
        public int EstimatedMinutes { get; set; }

        public SmugglingRoute(string name, float pickupX, float pickupY, float dropoffX, float dropoffY, int payout, int estMinutes)
        {
            Name = name;
            PickupX = pickupX;
            PickupY = pickupY;
            DropoffX = dropoffX;
            DropoffY = dropoffY;
            Payout = payout;
            EstimatedMinutes = estMinutes;
        }
    }
}
