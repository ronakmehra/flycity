using GTANetworkAPI;
using NeptuneEvo.Handles;
using System.Collections.Generic;
using System;
using System.Linq;
using Localization;
using NeptuneEvo.Core;
using Redage.SDK;
using NeptuneEvo.Functions;
using NeptuneEvo.Accounts;
using NeptuneEvo.Players;
using NeptuneEvo.Players.Models;
using NeptuneEvo.Character;
using NeptuneEvo.Character.Models;
using NeptuneEvo.Jobs.Models;
using NeptuneEvo.Chars;
using NeptuneEvo.Players.Popup.List.Models;
using NeptuneEvo.Quests;

namespace NeptuneEvo.Jobs
{
    class Delivery : Script
    {
        private static readonly nLog Log = new nLog("Jobs.Delivery");

        // Delivery job start location (Los Santos - near warehouse district)
        public static Vector3 JobStartPosition = new Vector3(1006.5f, -2458.5f, 28.3f);
        
        // Vehicle spawn for delivery van
        public static Vector3 VehicleSpawnPosition = new Vector3(1010.2f, -2462.1f, 28.3f);
        public static float VehicleSpawnHeading = 90.0f;

        // Delivery drop-off points around Los Santos
        public static List<Vector3> DeliveryPoints = new List<Vector3>()
        {
            new Vector3(25.7f, -1347.3f, 29.5f),       // Strawberry
            new Vector3(-706.2f, -904.7f, 19.2f),      // Vespucci
            new Vector3(373.0f, 326.6f, 103.6f),       // Downtown Vinewood
            new Vector3(-1222.4f, -906.5f, 12.3f),     // Del Perro
            new Vector3(1159.0f, -314.8f, 69.2f),      // Mirror Park
            new Vector3(-47.8f, -1757.8f, 29.4f),      // Davis
            new Vector3(1128.4f, -982.5f, 46.4f),      // El Burro Heights
            new Vector3(-1487.5f, -379.1f, 40.2f),     // Morningwood
            new Vector3(2557.1f, 382.1f, 108.6f),      // Tataviam Mountains
            new Vector3(-329.4f, -1569.3f, 25.2f),     // South LS
            new Vector3(811.6f, -1025.8f, 26.2f),      // La Mesa
            new Vector3(-1820.3f, 793.3f, 138.1f),     // Pacific Bluffs
            new Vector3(145.8f, -1035.8f, 29.3f),      // Pillbox Hill
            new Vector3(-551.2f, -195.8f, 38.2f),      // Burton
            new Vector3(2432.1f, 4966.1f, 46.8f),      // Grapeseed
        };

        // Payment per delivery (scales with distance)
        public static int BasePayPerDelivery = 350;
        public static int BonusPerKm = 75;
        public static int MaxDeliveriesPerShift = 15;

        // Express delivery bonuses
        public static int ExpressTimeLimit = 180; // seconds
        public static int ExpressBonus = 500;

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                CustomColShape.CreateCylinderColShape(JobStartPosition, 1.5f, 2, 0, ColShapeEnums.JobDelivery);
                NAPI.TextLabel.CreateTextLabel("~g~Delivery Service\n~w~Approach to start", 
                    JobStartPosition + new Vector3(0, 0, 0.5), 15F, 0.5F, 0, new Color(0, 255, 0));
                NAPI.Marker.CreateMarker(1, JobStartPosition - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 1.5f, new Color(0, 255, 100, 220));
                
                Main.CreateBlip(new Main.BlipData(478, "Delivery Service", JobStartPosition, 69, true, 1.0f));

                Log.Write("Delivery Job initialized successfully.");
            }
            catch (Exception e)
            {
                Log.Write($"onResourceStart Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.JobDelivery)]
        public static void OnJobDelivery(ExtPlayer player)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                if (characterData.WorkID != (int)JobsId.Delivery)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You don't work as a delivery driver. Get hired first!", 3000);
                    return;
                }

                var frameList = new FrameListData();
                frameList.Header = "Delivery Service";
                frameList.Callback = callback_deliveryMenu;

                if (sessionData.WorkData.OnWork)
                {
                    frameList.List.Add(new ListData("End Shift", "finish"));
                    frameList.List.Add(new ListData("Get Next Package", "next"));
                    frameList.List.Add(new ListData("Express Delivery (Bonus $500)", "express"));
                }
                else
                {
                    frameList.List.Add(new ListData("Start Shift", "start"));
                    frameList.List.Add(new ListData("View Earnings", "earnings"));
                }

                Players.Popup.List.Repository.Open(player, frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnJobDelivery Exception: {e.ToString()}");
            }
        }

        private static void callback_deliveryMenu(ExtPlayer player, object listItem)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                string action = listItem.ToString();

                switch (action)
                {
                    case "start":
                        StartDeliveryShift(player, sessionData, characterData);
                        break;
                    case "finish":
                        EndDeliveryShift(player, sessionData, characterData);
                        break;
                    case "next":
                        AssignNextDelivery(player, sessionData, false);
                        break;
                    case "express":
                        AssignNextDelivery(player, sessionData, true);
                        break;
                    case "earnings":
                        ShowEarnings(player, characterData);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_deliveryMenu Exception: {e.ToString()}");
            }
        }

        private static void StartDeliveryShift(ExtPlayer player, dynamic sessionData, dynamic characterData)
        {
            try
            {
                sessionData.WorkData.OnWork = true;
                sessionData.WorkData.DeliveryCount = 0;
                sessionData.WorkData.DeliveryEarnings = 0;

                // Spawn delivery vehicle
                var vehicle = VehicleManager.CreateVehicle("speedo", VehicleSpawnPosition, VehicleSpawnHeading, 
                    "DLV-" + new Random().Next(100, 999), player);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    "Shift started! Your delivery van is ready. Pick up packages and deliver them around LS.", 5000);
                
                // Assign first delivery
                AssignNextDelivery(player, sessionData, false);
            }
            catch (Exception e)
            {
                Log.Write($"StartDeliveryShift Exception: {e.ToString()}");
            }
        }

        private static void EndDeliveryShift(ExtPlayer player, dynamic sessionData, dynamic characterData)
        {
            try
            {
                int totalEarnings = sessionData.WorkData.DeliveryEarnings;
                int deliveries = sessionData.WorkData.DeliveryCount;

                sessionData.WorkData.OnWork = false;
                
                // Pay the player
                MoneySystem.Wallet.Change(player, totalEarnings);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"Shift complete! Deliveries: {deliveries} | Earned: ${totalEarnings}", 5000);
            }
            catch (Exception e)
            {
                Log.Write($"EndDeliveryShift Exception: {e.ToString()}");
            }
        }

        private static void AssignNextDelivery(ExtPlayer player, dynamic sessionData, bool isExpress)
        {
            try
            {
                if (sessionData.WorkData.DeliveryCount >= MaxDeliveriesPerShift)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, 
                        "Max deliveries reached for this shift. End your shift to collect payment.", 4000);
                    return;
                }

                Random rnd = new Random();
                int pointIndex = rnd.Next(0, DeliveryPoints.Count);
                Vector3 deliveryTarget = DeliveryPoints[pointIndex];

                // Set waypoint for player
                player.TriggerEvent("client.delivery.setWaypoint", deliveryTarget.X, deliveryTarget.Y, deliveryTarget.Z, isExpress);

                string deliveryType = isExpress ? "EXPRESS" : "Standard";
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                    $"[{deliveryType}] Package assigned! Follow GPS to delivery point.", 4000);
            }
            catch (Exception e)
            {
                Log.Write($"AssignNextDelivery Exception: {e.ToString()}");
            }
        }

        public static void CompleteDelivery(ExtPlayer player, bool isExpress, float distance)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;

                int payment = BasePayPerDelivery + (int)(distance * BonusPerKm);
                if (isExpress) payment += ExpressBonus;

                sessionData.WorkData.DeliveryCount++;
                sessionData.WorkData.DeliveryEarnings += payment;

                string bonus = isExpress ? " (+$500 EXPRESS BONUS!)" : "";
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"Delivery complete! +${payment}{bonus} | Total: {sessionData.WorkData.DeliveryCount}/{MaxDeliveriesPerShift}", 4000);
            }
            catch (Exception e)
            {
                Log.Write($"CompleteDelivery Exception: {e.ToString()}");
            }
        }

        private static void ShowEarnings(ExtPlayer player, dynamic characterData)
        {
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                $"Your total delivery earnings today: Check your bank statement.", 3000);
        }
    }
}
