// ============================================================
// GANG TERRITORY & WAR SYSTEM - FlyCity Roleplay
// ============================================================
// Features:
// - Gang territory capture and control
// - Gang reputation system
// - Drive-by shooting bonuses
// - Gang vehicle convoy system
// - Drug running routes
// - Gang safe house management
// - Turf war notifications and HUD
// - Gang rank perks & abilities
// ============================================================

let gangData = {
    territory: null,
    reputation: 0,
    warActive: false,
    warTeam: null,
    warScore: { us: 0, them: 0 },
    convoyActive: false,
    convoyVehicles: [],
};

// ====== GANG TERRITORY SYSTEM ======
const gangTerritories = {};

gm.events.add('client.gang.territory.update', (territoriesJson) => {
    try {
        const data = JSON.parse(territoriesJson);
        for (let key in data) {
            gangTerritories[key] = data[key];
        }
        updateTerritoryBlips();
    } catch (e) {
        mp.events.callRemote("client_trycatch", "gang/territory", "update", e.toString());
    }
});

const updateTerritoryBlips = () => {
    try {
        for (let key in gangTerritories) {
            const territory = gangTerritories[key];
            const blipName = `territory_${key}`;
            mp.events.call("deleteBlip", blipName);
            
            let color = 0;
            switch(territory.owner) {
                case "ballas": color = 7; break;
                case "grove": color = 2; break;
                case "vagos": color = 5; break;
                case "mafia": color = 4; break;
                case "triads": color = 3; break;
                case "russians": color = 1; break;
                case "bikers": color = 6; break;
                default: color = 0;
            }
            
            if (territory.contested) color = 1; // Red if contested
            
            mp.events.call("createBlip", blipName, territory.name, 543, 
                new mp.Vector3(territory.x, territory.y, territory.z), 1.0, color);
        }
    } catch (e) {}
};

// ====== GANG WAR SYSTEM ======
gm.events.add('client.gang.war.start', (enemyName, territoryName, duration) => {
    try {
        gangData.warActive = true;
        gangData.warScore = { us: 0, them: 0 };
        
        mp.game.audio.playSoundFrontend(-1, "TIMER_STOP", "HUD_MINI_GAME_SOUNDSET", true);
        mp.game.graphics.startScreenEffect("DeathFailMPIn", 2000, false);
        
        mp.events.call('notify', 4, 9, 
            translateText(`⚔️ ВОЙНА! Ваша банда vs ${enemyName} за территорию "${territoryName}"! Длительность: ${duration} мин.`), 10000);
        
        mp.gui.emmit(`window.hudStore.startGangWar && window.hudStore.startGangWar('${enemyName}', '${territoryName}', ${duration})`);
        
        gm.discord(translateText("Участвует в войне банд"));
    } catch (e) {
        mp.events.callRemote("client_trycatch", "gang/territory", "warStart", e.toString());
    }
});

gm.events.add('client.gang.war.score', (ourScore, theirScore) => {
    try {
        gangData.warScore = { us: ourScore, them: theirScore };
        mp.gui.emmit(`window.hudStore.updateWarScore && window.hudStore.updateWarScore(${ourScore}, ${theirScore})`);
    } catch (e) {}
});

gm.events.add('client.gang.war.end', (won, rewards) => {
    try {
        gangData.warActive = false;
        
        if (won) {
            mp.game.audio.playSoundFrontend(-1, "RACE_PLACED", "HUD_AWARDS", true);
            mp.game.graphics.startScreenEffect("SuccessMichael", 3000, false);
            mp.events.call('notify', 1, 9, 
                translateText(`🏆 ПОБЕДА! Территория захвачена! Награда: $${rewards}`), 10000);
        } else {
            mp.game.audio.playSoundFrontend(-1, "ScreenFlash", "MissionFailedSounds", true);
            mp.game.graphics.startScreenEffect("DeathFailMPDark", 3000, false);
            mp.events.call('notify', 4, 9, 
                translateText("❌ Поражение! Территория потеряна..."), 10000);
        }
        
        mp.gui.emmit(`window.hudStore.endGangWar && window.hudStore.endGangWar(${won})`);
    } catch (e) {}
});

// ====== GANG REPUTATION SYSTEM ======
gm.events.add('client.gang.reputation', (amount, reason) => {
    try {
        gangData.reputation += amount;
        const sign = amount > 0 ? "+" : "";
        mp.events.call('notify', amount > 0 ? 1 : 4, 9, 
            translateText(`${sign}${amount} репутации: ${reason}`), 4000);
        mp.gui.emmit(`window.hudStore.updateReputation && window.hudStore.updateReputation(${gangData.reputation})`);
    } catch (e) {}
});

// ====== DRIVE-BY SYSTEM ======
let driveByActive = false;
let driveByKills = 0;
let driveByTimer = null;

gm.events.add('client.gang.driveby.start', () => {
    try {
        driveByActive = true;
        driveByKills = 0;
        
        mp.events.call('notify', 1, 9, translateText("🚗💨 Драйв-бай начат! Бонус за каждое попадание!"), 5000);
        
        // Timer - drive-by lasts 60 seconds
        if (driveByTimer) clearTimeout(driveByTimer);
        driveByTimer = setTimeout(() => {
            endDriveBy();
        }, 60000);
    } catch (e) {}
});

gm.events.add('client.gang.driveby.kill', () => {
    try {
        if (!driveByActive) return;
        driveByKills++;
        
        const bonus = driveByKills * 500; // $500 per kill, stacking
        mp.events.call('notify', 1, 9, translateText(`Drive-by x${driveByKills}! +$${bonus} 🎯`), 2000);
        mp.game.audio.playSoundFrontend(-1, "CHECKPOINT_PERFECT", "HUD_MINI_GAME_SOUNDSET", true);
    } catch (e) {}
});

const endDriveBy = () => {
    if (!driveByActive) return;
    driveByActive = false;
    
    const totalBonus = (driveByKills * (driveByKills + 1) / 2) * 500;
    if (driveByKills > 0) {
        mp.events.call('notify', 1, 9, 
            translateText(`Drive-by завершен! ${driveByKills} попаданий! Бонус: $${totalBonus} 💰`), 5000);
        mp.events.callRemote('server.gang.driveby.end', driveByKills);
    }
    driveByKills = 0;
};

gm.events.add("playerLeaveVehicle", () => {
    if (driveByActive) endDriveBy();
});

// ====== GANG CONVOY SYSTEM ======
gm.events.add('client.gang.convoy.start', (leaderName) => {
    try {
        gangData.convoyActive = true;
        mp.events.call('notify', 1, 9, 
            translateText(`🚗🚗🚗 Конвой начат! Лидер: ${leaderName}. Следуйте за ним!`), 8000);
        mp.gui.emmit(`window.hudStore.showConvoy && window.hudStore.showConvoy(true)`);
    } catch (e) {}
});

gm.events.add('client.gang.convoy.end', () => {
    gangData.convoyActive = false;
    mp.gui.emmit(`window.hudStore.showConvoy && window.hudStore.showConvoy(false)`);
});

gm.events.add('client.gang.convoy.bonus', (bonus) => {
    mp.events.call('notify', 1, 9, translateText(`Бонус конвоя: +$${bonus} 🚗`), 3000);
});

// ====== DRUG RUNNING ROUTES ======
let drugRouteActive = false;
let drugRouteBlip = null;
let drugRouteMarker = null;

gm.events.add('client.gang.drugrun.start', (destX, destY, destZ, reward, riskLevel) => {
    try {
        drugRouteActive = true;
        
        const riskText = riskLevel === 1 ? "Низкий" : riskLevel === 2 ? "Средний" : "Высокий";
        mp.events.call('notify', 3, 9, 
            translateText(`💊 Перевозка начата! Награда: $${reward} | Риск: ${riskText}`), 8000);
        
        drugRouteBlip = mp.blips.new(1, new mp.Vector3(destX, destY, destZ), { 
            alpha: 255, color: 5, name: translateText("Точка сброса") 
        });
        drugRouteBlip.setRoute(true);
        drugRouteBlip.setRouteColour(5);
        
        mp.gui.emmit(`window.hudStore.showDrugRoute && window.hudStore.showDrugRoute(true, ${reward}, ${riskLevel})`);
        gm.discord(translateText("Перевозит груз"));
    } catch (e) {
        mp.events.callRemote("client_trycatch", "gang/territory", "drugrunStart", e.toString());
    }
});

gm.events.add('client.gang.drugrun.end', (success, money) => {
    try {
        drugRouteActive = false;
        
        if (drugRouteBlip) {
            drugRouteBlip.setRoute(false);
            drugRouteBlip.destroy();
            drugRouteBlip = null;
        }
        
        if (success) {
            mp.events.call('notify', 1, 9, translateText(`✅ Доставка завершена! +$${money}`), 5000);
            mp.game.audio.playSoundFrontend(-1, "RACE_PLACED", "HUD_AWARDS", true);
        } else {
            mp.events.call('notify', 4, 9, translateText("❌ Перевозка провалена!"), 5000);
        }
        
        mp.gui.emmit(`window.hudStore.showDrugRoute && window.hudStore.showDrugRoute(false)`);
    } catch (e) {}
});

// ====== GANG RANK PERKS ======
const gangRankPerks = {
    1: { name: "Новичок", perks: ["Доступ к базовому оружию"] },
    2: { name: "Солдат", perks: ["Участие в войнах", "Базовые транспорт"] },
    3: { name: "Авторитет", perks: ["Drive-by бонусы", "Средний транспорт"] },
    4: { name: "Лейтенант", perks: ["Конвой лидер", "Бронированный транспорт"] },
    5: { name: "Капитан", perks: ["Вызов подкрепления", "Суперкары"] },
    6: { name: "Босс района", perks: ["Управление территорией", "Воздушный транспорт"] },
    7: { name: "Правая рука", perks: ["Все привилегии", "Премиум транспорт"] },
    8: { name: "Заместитель", perks: ["Полное управление", "Максимальный бонус"] },
    9: { name: "Со-лидер", perks: ["Управление организацией"] },
    10: { name: "Лидер", perks: ["Полный контроль над всем"] },
};

gm.events.add('client.gang.rankup', (newRank) => {
    try {
        const rankInfo = gangRankPerks[newRank];
        if (!rankInfo) return;
        
        mp.game.audio.playSoundFrontend(-1, "RANK_UP", "HUD_AWARDS", true);
        mp.game.graphics.startScreenEffect("SuccessMichael", 3000, false);
        
        let perksText = rankInfo.perks.join(", ");
        mp.events.call('notify', 1, 9, 
            translateText(`⬆️ Новый ранг: ${rankInfo.name}! Привилегии: ${perksText}`), 10000);
            
        mp.gui.emmit(`window.hudStore.gangRankUp && window.hudStore.gangRankUp(${newRank}, '${rankInfo.name}')`);
    } catch (e) {}
});

// ====== GANG SAFE HOUSE ======
gm.events.add('client.gang.safehouse.enter', (safeHouseData) => {
    try {
        const data = JSON.parse(safeHouseData);
        mp.events.call('notify', 1, 9, translateText(`Вы в безопасном доме: ${data.name} 🏠`), 3000);
        mp.gui.emmit(`window.hudStore.enterSafeHouse && window.hudStore.enterSafeHouse('${JSON.stringify(data)}')`);
    } catch (e) {}
});

gm.events.add('client.gang.safehouse.stash', (action, itemId, amount) => {
    mp.events.callRemote('server.gang.safehouse.stash', action, itemId, amount);
});

// ====== GANG VEHICLE CUSTOMIZATION ======
gm.events.add('client.gang.vehicle.customize', (vehicleId) => {
    try {
        mp.events.callRemote('server.gang.vehicle.customize', vehicleId);
    } catch (e) {}
});

// Gang vehicle colors by gang type
const gangColors = {
    "ballas": { primary: [128, 0, 128], secondary: [75, 0, 130] },
    "grove": { primary: [0, 128, 0], secondary: [0, 100, 0] },
    "vagos": { primary: [255, 255, 0], secondary: [200, 200, 0] },
    "mafia": { primary: [20, 20, 20], secondary: [40, 40, 40] },
    "triads": { primary: [255, 0, 0], secondary: [139, 0, 0] },
    "russians": { primary: [0, 0, 139], secondary: [0, 0, 100] },
    "bikers": { primary: [50, 50, 50], secondary: [25, 25, 25] },
    "cartel": { primary: [139, 69, 19], secondary: [101, 67, 33] },
    "yakuza": { primary: [255, 255, 255], secondary: [200, 200, 200] },
};

gm.events.add("vehicleStreamIn", (entity) => {
    try {
        if (!entity || !mp.vehicles.exists(entity)) return;
        
        const gangVehicle = entity.getVariable('GANG_VEHICLE');
        if (!gangVehicle) return;

        const gangType = entity.getVariable('GANG_TYPE');
        if (gangType && gangColors[gangType]) {
            const colors = gangColors[gangType];
            entity.setCustomPrimaryColour(colors.primary[0], colors.primary[1], colors.primary[2]);
            entity.setCustomSecondaryColour(colors.secondary[0], colors.secondary[1], colors.secondary[2]);
        }
    } catch (e) {}
});

// ====== HIT CONTRACT SYSTEM ======
let activeHitContract = null;

gm.events.add('client.gang.hitcontract', (targetName, reward, timeLimit) => {
    try {
        activeHitContract = { target: targetName, reward: reward, timeLimit: timeLimit };
        
        mp.events.call('notify', 4, 9, 
            translateText(`🎯 Контракт на убийство: ${targetName} | Награда: $${reward} | Время: ${timeLimit} мин.`), 10000);
        
        mp.gui.emmit(`window.hudStore.showHitContract && window.hudStore.showHitContract('${targetName}', ${reward}, ${timeLimit})`);
        
        gm.discord(translateText("Выполняет контракт"));
    } catch (e) {}
});

gm.events.add('client.gang.hitcontract.complete', (reward) => {
    activeHitContract = null;
    mp.events.call('notify', 1, 9, translateText(`🎯 Контракт выполнен! +$${reward}`), 5000);
    mp.game.audio.playSoundFrontend(-1, "CHECKPOINT_PERFECT", "HUD_MINI_GAME_SOUNDSET", true);
    mp.gui.emmit(`window.hudStore.hideHitContract && window.hudStore.hideHitContract()`);
});

gm.events.add('client.gang.hitcontract.fail', () => {
    activeHitContract = null;
    mp.events.call('notify', 4, 9, translateText("❌ Контракт провален!"), 5000);
    mp.gui.emmit(`window.hudStore.hideHitContract && window.hudStore.hideHitContract()`);
});
