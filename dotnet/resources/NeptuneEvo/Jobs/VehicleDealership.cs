using GTANetworkAPI;
using NeptuneEvo.Handles;
using System;
using System.Collections.Generic;
using System.Linq;
using NeptuneEvo.Core;
using NeptuneEvo.Chars;
using NeptuneEvo.Functions;
using NeptuneEvo.GUI;
using NeptuneEvo.Players;
using NeptuneEvo.Players.Models;
using NeptuneEvo.Character;
using NeptuneEvo.Character.Models;
using NeptuneEvo.Players.Popup.List.Models;
using Newtonsoft.Json;
using Redage.SDK;

namespace NeptuneEvo.Jobs
{
    /// <summary>
    /// Vehicle Dealership System for Civilians
    /// Players grind money and buy personal vehicles
    /// Multiple dealership tiers from economy to luxury
    /// </summary>
    class VehicleDealership : Script
    {
        private static readonly nLog Log = new nLog("Jobs.VehicleDealership");

        #region Dealership Locations

        public static Vector3 EconomyDealership = new Vector3(-56.5f, -1097.3f, 26.4f);     // Economy cars
        public static Vector3 MidRangeDealership = new Vector3(-790.5f, -222.7f, 37.1f);    // Mid-range
        public static Vector3 LuxuryDealership = new Vector3(-1260.8f, -354.2f, 36.9f);     // Luxury
        public static Vector3 MotorcycleDealership = new Vector3(-205.3f, -1310.5f, 31.3f); // Motorcycles
        public static Vector3 TruckDealership = new Vector3(1220.5f, -3213.8f, 5.8f);       // Trucks & SUVs

        #endregion

        #region Vehicle Catalog

        public static Dictionary<string, List<DealershipVehicle>> VehicleCatalog = new Dictionary<string, List<DealershipVehicle>>()
        {
            { "Economy", new List<DealershipVehicle>()
                {
                    new DealershipVehicle("blista", "Dinka Blista", 15000),
                    new DealershipVehicle("prairie", "Bollokan Prairie", 18000),
                    new DealershipVehicle("issi2", "Weeny Issi", 12000),
                    new DealershipVehicle("panto", "Benefactor Panto", 8000),
                    new DealershipVehicle("dilettante", "Karin Dilettante", 22000),
                    new DealershipVehicle("asea", "Declasse Asea", 10000),
                    new DealershipVehicle("emperor", "Albany Emperor", 14000),
                    new DealershipVehicle("fugitive", "Cheval Fugitive", 25000),
                    new DealershipVehicle("stanier", "Vapid Stanier", 16000),
                    new DealershipVehicle("stratum", "Zirconium Stratum", 20000),
                }
            },
            { "Mid-Range", new List<DealershipVehicle>()
                {
                    new DealershipVehicle("sultan", "Karin Sultan", 45000),
                    new DealershipVehicle("kuruma", "Karin Kuruma", 65000),
                    new DealershipVehicle("elegy2", "Annis Elegy RH8", 80000),
                    new DealershipVehicle("jester", "Dinka Jester", 90000),
                    new DealershipVehicle("massacro", "Dewbauchee Massacro", 95000),
                    new DealershipVehicle("schafter2", "Benefactor Schafter", 75000),
                    new DealershipVehicle("sentinel", "Ubermacht Sentinel", 55000),
                    new DealershipVehicle("fusilade", "Schyster Fusilade", 50000),
                    new DealershipVehicle("buffalo2", "Bravado Buffalo S", 70000),
                    new DealershipVehicle("comet2", "Pfister Comet", 100000),
                }
            },
            { "Luxury", new List<DealershipVehicle>()
                {
                    new DealershipVehicle("t20", "Progen T20", 350000),
                    new DealershipVehicle("zentorno", "Pegassi Zentorno", 275000),
                    new DealershipVehicle("adder", "Truffade Adder", 500000),
                    new DealershipVehicle("entityxf", "Overflod Entity XF", 450000),
                    new DealershipVehicle("osiris", "Pegassi Osiris", 400000),
                    new DealershipVehicle("turismor", "Grotti Turismo R", 320000),
                    new DealershipVehicle("reaper", "Pegassi Reaper", 280000),
                    new DealershipVehicle("fmj", "Vapid FMJ", 380000),
                    new DealershipVehicle("nero", "Truffade Nero", 420000),
                    new DealershipVehicle("vagner", "Dewbauchee Vagner", 550000),
                }
            },
            { "Motorcycles", new List<DealershipVehicle>()
                {
                    new DealershipVehicle("bati", "Pegassi Bati 801", 25000),
                    new DealershipVehicle("akuma", "Dinka Akuma", 30000),
                    new DealershipVehicle("hakuchou", "Shitzu Hakuchou", 45000),
                    new DealershipVehicle("nemesis", "Principe Nemesis", 20000),
                    new DealershipVehicle("shotaro", "Nagasaki Shotaro", 150000),
                    new DealershipVehicle("carbonrs", "Nagasaki Carbon RS", 35000),
                    new DealershipVehicle("double", "Dinka Double-T", 28000),
                    new DealershipVehicle("sanchez", "Maibatsu Sanchez", 15000),
                }
            },
            { "Trucks & SUVs", new List<DealershipVehicle>()
                {
                    new DealershipVehicle("baller2", "Gallivanter Baller", 85000),
                    new DealershipVehicle("granger", "Declasse Granger", 60000),
                    new DealershipVehicle("huntley", "Enus Huntley S", 120000),
                    new DealershipVehicle("dubsta2", "Benefactor Dubsta", 90000),
                    new DealershipVehicle("sandking", "Vapid Sandking", 55000),
                    new DealershipVehicle("bison", "Bravado Bison", 40000),
                    new DealershipVehicle("bobcatxl", "Vapid Bobcat XL", 35000),
                    new DealershipVehicle("contender", "Vapid Contender", 75000),
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
                // Economy
                CreateDealershipPoint(EconomyDealership, "Economy Cars", "~g~Economy Dealership\n~w~Budget-friendly vehicles", 225);
                // Mid-Range
                CreateDealershipPoint(MidRangeDealership, "Premium Autos", "~b~Premium Dealership\n~w~Performance vehicles", 225);
                // Luxury
                CreateDealershipPoint(LuxuryDealership, "Luxury Motors", "~p~Luxury Dealership\n~w~Exotic supercars", 225);
                // Motorcycles
                CreateDealershipPoint(MotorcycleDealership, "Bike Shop", "~o~Motorcycle Dealer\n~w~Two-wheelers", 226);
                // Trucks
                CreateDealershipPoint(TruckDealership, "Truck & SUV Lot", "~y~Truck Dealership\n~w~Heavy duty vehicles", 477);

                Log.Write("Vehicle Dealership system initialized with 5 locations.");
            }
            catch (Exception e)
            {
                Log.Write($"onResourceStart Exception: {e.ToString()}");
            }
        }

        private void CreateDealershipPoint(Vector3 pos, string blipName, string labelText, int blipSprite)
        {
            CustomColShape.CreateCylinderColShape(pos, 2f, 2, 0, ColShapeEnums.VehicleDealership);
            NAPI.TextLabel.CreateTextLabel(labelText, pos + new Vector3(0, 0, 0.5), 15F, 0.5F, 0, new Color(255, 255, 255));
            NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 2f, new Color(0, 200, 255, 200));
            Main.CreateBlip(new Main.BlipData(blipSprite, blipName, pos, 2, true, 1.0f));
        }

        #endregion

        #region Dealership Interaction

        [Interaction(ColShapeEnums.VehicleDealership)]
        public static void OnDealership(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                string dealerType = GetDealerType(player.Position);
                if (!VehicleCatalog.ContainsKey(dealerType)) return;

                var vehicles = VehicleCatalog[dealerType];
                var frameList = new FrameListData();
                frameList.Header = $"{dealerType} Dealership - Browse Vehicles";
                frameList.Callback = callback_dealerMenu;

                foreach (var veh in vehicles)
                {
                    frameList.List.Add(new ListData($"{veh.DisplayName} - ${veh.Price:N0}", veh.ModelName));
                }

                Players.Popup.List.Repository.Open(player, frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnDealership Exception: {e.ToString()}");
            }
        }

        private static void callback_dealerMenu(ExtPlayer player, object listItem)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                string modelName = listItem.ToString();

                // Find vehicle info
                DealershipVehicle vehicle = null;
                foreach (var category in VehicleCatalog.Values)
                {
                    vehicle = category.FirstOrDefault(v => v.ModelName == modelName);
                    if (vehicle != null) break;
                }

                if (vehicle == null) return;

                // Check if player can afford it
                int playerMoney = MoneySystem.Wallet.GetBalance(player);
                if (playerMoney < vehicle.Price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        $"Not enough money! Need ${vehicle.Price:N0}, you have ${playerMoney:N0}. Grind more jobs!", 4000);
                    return;
                }

                // Purchase the vehicle
                MoneySystem.Wallet.Change(player, -vehicle.Price);

                // Spawn personal vehicle near player
                Vector3 spawnPos = player.Position + new Vector3(3, 0, 0);
                var newVehicle = NAPI.Vehicle.CreateVehicle(
                    NAPI.Util.GetHashKey(vehicle.ModelName),
                    spawnPos, player.Heading, 0, 0,
                    $"P-{new Random().Next(1000, 9999)}", 255, false, true, 0
                );

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"Congratulations! You purchased {vehicle.DisplayName} for ${vehicle.Price:N0}!", 5000);
            }
            catch (Exception e)
            {
                Log.Write($"callback_dealerMenu Exception: {e.ToString()}");
            }
        }

        private static string GetDealerType(Vector3 position)
        {
            if (position.DistanceTo(EconomyDealership) < 5) return "Economy";
            if (position.DistanceTo(MidRangeDealership) < 5) return "Mid-Range";
            if (position.DistanceTo(LuxuryDealership) < 5) return "Luxury";
            if (position.DistanceTo(MotorcycleDealership) < 5) return "Motorcycles";
            if (position.DistanceTo(TruckDealership) < 5) return "Trucks & SUVs";
            return "Economy";
        }

        #endregion
    }

    #region Dealership Vehicle Model

    public class DealershipVehicle
    {
        public string ModelName { get; set; }
        public string DisplayName { get; set; }
        public int Price { get; set; }

        public DealershipVehicle(string model, string display, int price)
        {
            ModelName = model;
            DisplayName = display;
            Price = price;
        }
    }

    #endregion
}
