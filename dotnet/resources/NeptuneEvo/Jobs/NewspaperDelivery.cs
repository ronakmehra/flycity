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
    class NewspaperDelivery : Script
    {
        private static readonly nLog Log = new nLog("Jobs.NewspaperDelivery");

        // Newspaper Delivery job start - near LifeInvader office
        public static Vector3 JobStartPosition = new Vector3(-1076.5f, -247.2f, 37.8f);

        // Vehicle spawn for delivery bicycle/scooter
        public static Vector3 VehicleSpawnPosition = new Vector3(-1080.1f, -250.5f, 37.8f);
        public static float VehicleSpawnHeading = 120.0f;

        // Newspaper pickup depot
        public static Vector3 NewspaperDepotPosition = new Vector3(-1082.3f, -244.8f, 37.8f);

        // Delivery addresses around Los Santos (house doors, mailboxes)
        public static List<Vector3> DeliveryAddresses = new List<Vector3>()
        {
            // Vinewood Hills
            new Vector3(-795.0f, 178.3f, 72.8f),
            new Vector3(-682.5f, 261.7f, 81.5f),
            new Vector3(-572.3f, 336.1f, 85.2f),
            // Rockford Hills
            new Vector3(-864.2f, -103.5f, 37.6f),
            new Vector3(-927.8f, -52.1f, 39.6f),
            new Vector3(-1012.4f, -89.7f, 40.1f),
            // Del Perro
            new Vector3(-1371.2f, -478.5f, 33.2f),
            new Vector3(-1440.8f, -542.3f, 30.5f),
            // Mirror Park
            new Vector3(1020.3f, -426.8f, 65.1f),
            new Vector3(1085.7f, -379.2f, 67.8f),
            new Vector3(1152.1f, -328.6f, 69.5f),
            // Vespucci Canals
            new Vector3(-1141.5f, -1518.3f, 4.4f),
            new Vector3(-1068.2f, -1616.7f, 4.5f),
            // Downtown
            new Vector3(98.5f, -1068.3f, 29.4f),
            new Vector3(-31.8f, -1120.6f, 26.4f),
            // Pacific Bluffs
            new Vector3(-1847.3f, -624.5f, 11.2f),
            new Vector3(-1928.1f, -564.8f, 11.5f),
            // Chumash
            new Vector3(-3012.5f, 27.8f, 10.1f),
            new Vector3(-3148.2f, 1085.3f, 20.8f),
            // Sandy Shores
            new Vector3(1962.5f, 3817.2f, 32.2f),
            new Vector3(1879.3f, 3908.5f, 33.5f),
            // Paleto Bay
            new Vector3(-370.8f, 6032.5f, 31.5f),
            new Vector3(-280.2f, 6146.8f, 31.2f),
            // Grapeseed
            new Vector3(1688.5f, 4820.3f, 42.1f),
            new Vector3(1726.8f, 4918.7f, 42.5f),
        };

        // Morning edition delivery routes (suburban)
        public static List<Vector3> MorningRoutes = new List<Vector3>()
        {
            new Vector3(-795.0f, 178.3f, 72.8f),
            new Vector3(-682.5f, 261.7f, 81.5f),
            new Vector3(-864.2f, -103.5f, 37.6f),
            new Vector3(-927.8f, -52.1f, 39.6f),
            new Vector3(1020.3f, -426.8f, 65.1f),
            new Vector3(1085.7f, -379.2f, 67.8f),
        };

        // Evening edition delivery routes (city center)
        public static List<Vector3> EveningRoutes = new List<Vector3>()
        {
            new Vector3(98.5f, -1068.3f, 29.4f),
            new Vector3(-31.8f, -1120.6f, 26.4f),
            new Vector3(-1371.2f, -478.5f, 33.2f),
            new Vector3(-1141.5f, -1518.3f, 4.4f),
            new Vector3(-1068.2f, -1616.7f, 4.5f),
            new Vector3(-1440.8f, -542.3f, 30.5f),
        };

        // Payment settings
        public static int BasePayPerPaper = 120;
        public static int BonusPerExtraStop = 25;
        public static int MaxPapersPerRun = 25;
        public static int SpeedBonusTime = 120; // seconds per delivery for speed bonus
        public static int SpeedBonus = 200;
        public static int TipChance = 30; // 30% chance of getting a tip
        public static int TipMin = 50;
        public static int TipMax = 250;

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                // Job start point
                CustomColShape.CreateCylinderColShape(JobStartPosition, 1.5f, 2, 0, ColShapeEnums.JobNewspaperDelivery);
                NAPI.TextLabel.CreateTextLabel("~g~Newspaper Delivery\n~w~Daily Gazette - Hiring Paperboys/Papergirls",
                    JobStartPosition + new Vector3(0, 0, 0.5), 15F, 0.5F, 0, new Color(0, 255, 0));
                NAPI.Marker.CreateMarker(1, JobStartPosition - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 1.5f, new Color(255, 200, 0, 220));

                Main.CreateBlip(new Main.BlipData(267, "Newspaper Delivery", JobStartPosition, 46, true, 1.0f));

                // Newspaper depot pickup
                CustomColShape.CreateCylinderColShape(NewspaperDepotPosition, 2.0f, 2, 0, ColShapeEnums.JobNewspaperDepot);
                NAPI.TextLabel.CreateTextLabel("~y~Newspaper Depot\n~w~Pick up today's papers here",
                    NewspaperDepotPosition + new Vector3(0, 0, 0.5), 10F, 0.4F, 0, new Color(255, 200, 0));
                NAPI.Marker.CreateMarker(1, NewspaperDepotPosition - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 1.5f, new Color(255, 200, 0, 220));

                Log.Write("Newspaper Delivery Job initialized successfully.");
            }
            catch (Exception e)
            {
                Log.Write($"onResourceStart Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.JobNewspaperDelivery)]
        public static void OnJobNewspaperDelivery(ExtPlayer player)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                if (characterData.WorkID != (int)JobsId.NewspaperDelivery)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You don't work as a newspaper deliverer. Get hired at City Hall first!", 3000);
                    return;
                }

                var frameList = new FrameListData();
                frameList.Header = "Daily Gazette - Newspaper Delivery";
                frameList.Callback = callback_newspaperMenu;

                if (sessionData.WorkData.OnWork)
                {
                    frameList.List.Add(new ListData("End Shift", "finish"));
                    frameList.List.Add(new ListData("Get Delivery Bicycle", "getveh"));
                    frameList.List.Add(new ListData("Morning Route (Suburbs)", "morning"));
                    frameList.List.Add(new ListData("Evening Route (City)", "evening"));
                    frameList.List.Add(new ListData("Check Earnings", "earnings"));
                }
                else
                {
                    frameList.List.Add(new ListData("Start Shift", "start"));
                }

                Trigger.ClientEvent(player, "client.framelist.show", frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnJobNewspaperDelivery Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_newspaperMenu")]
        public static void callback_newspaperMenu(ExtPlayer player, string value)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                switch (value)
                {
                    case "start":
                        sessionData.WorkData.OnWork = true;
                        sessionData.WorkData.ProgressCount = 0;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                            "You started your newspaper delivery shift! Pick up papers at the depot, then deliver to houses.", 5000);
                        break;

                    case "finish":
                        int totalEarnings = sessionData.WorkData.ProgressCount * BasePayPerPaper;
                        if (totalEarnings > 0)
                        {
                            MoneySystem.Wallet.Change(player, totalEarnings);
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                                $"Shift complete! You delivered {sessionData.WorkData.ProgressCount} newspapers and earned ${totalEarnings}.", 5000);
                        }
                        sessionData.WorkData.OnWork = false;
                        sessionData.WorkData.ProgressCount = 0;
                        break;

                    case "getveh":
                        if (!sessionData.WorkData.OnWork) return;
                        var veh = NAPI.Vehicle.CreateVehicle(VehicleHash.Faggio, VehicleSpawnPosition, VehicleSpawnHeading, 46, 46);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            "Your delivery scooter is ready! Don't forget to pick up newspapers at the depot.", 3000);
                        break;

                    case "morning":
                        if (!sessionData.WorkData.OnWork) return;
                        AssignDeliveryRoute(player, MorningRoutes, "Morning Edition");
                        break;

                    case "evening":
                        if (!sessionData.WorkData.OnWork) return;
                        AssignDeliveryRoute(player, EveningRoutes, "Evening Edition");
                        break;

                    case "earnings":
                        int earned = sessionData.WorkData.ProgressCount * BasePayPerPaper;
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            $"Papers delivered: {sessionData.WorkData.ProgressCount} | Earnings so far: ${earned}", 5000);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_newspaperMenu Exception: {e.ToString()}");
            }
        }

        private static void AssignDeliveryRoute(ExtPlayer player, List<Vector3> route, string routeName)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;

                Random rnd = new Random();
                int routeIndex = rnd.Next(0, route.Count);
                Vector3 target = route[routeIndex];

                Trigger.ClientEvent(player, "createWaypoint", target.X, target.Y);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                    $"{routeName} Route - Deliver newspaper to the marked address. GPS has been set.", 5000);
            }
            catch (Exception e)
            {
                Log.Write($"AssignDeliveryRoute Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.JobNewspaperDepot)]
        public static void OnNewspaperDepot(ExtPlayer player)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                if (characterData.WorkID != (int)JobsId.NewspaperDelivery)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You don't work here.", 3000);
                    return;
                }

                if (!sessionData.WorkData.OnWork)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Start your shift first at the main office!", 3000);
                    return;
                }

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"You picked up {MaxPapersPerRun} newspapers. Deliver them to the marked addresses!", 3000);
                
                // Set first delivery point
                Random rnd = new Random();
                int idx = rnd.Next(0, DeliveryAddresses.Count);
                Trigger.ClientEvent(player, "createWaypoint", DeliveryAddresses[idx].X, DeliveryAddresses[idx].Y);
            }
            catch (Exception e)
            {
                Log.Write($"OnNewspaperDepot Exception: {e.ToString()}");
            }
        }

        public static void CompleteDelivery(ExtPlayer player)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                sessionData.WorkData.ProgressCount++;
                int pay = BasePayPerPaper;

                // Random tip chance
                Random rnd = new Random();
                if (rnd.Next(100) < TipChance)
                {
                    int tip = rnd.Next(TipMin, TipMax);
                    pay += tip;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                        $"Paper delivered! Customer gave you a ${tip} tip! Total: ${pay}", 3000);
                }
                else
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                        $"Paper delivered! +${pay}. ({sessionData.WorkData.ProgressCount}/{MaxPapersPerRun})", 3000);
                }

                MoneySystem.Wallet.Change(player, pay);

                // Assign next delivery or end run
                if (sessionData.WorkData.ProgressCount >= MaxPapersPerRun)
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                        "All newspapers delivered! Return to the depot for more or end your shift.", 5000);
                    Trigger.ClientEvent(player, "createWaypoint", NewspaperDepotPosition.X, NewspaperDepotPosition.Y);
                    sessionData.WorkData.ProgressCount = 0;
                }
                else
                {
                    int idx = rnd.Next(0, DeliveryAddresses.Count);
                    Trigger.ClientEvent(player, "createWaypoint", DeliveryAddresses[idx].X, DeliveryAddresses[idx].Y);
                }
            }
            catch (Exception e)
            {
                Log.Write($"CompleteDelivery Exception: {e.ToString()}");
            }
        }
    }
}
