// ============================================================
// ORGANIZATION MANAGEMENT & UPGRADES - FlyCity Roleplay
// ============================================================
// Features:
// - Org HQ building upgrades
// - Org bank/treasury system
// - Org missions and jobs
// - Org territory income
// - Org war declarations
// - Org alliance system
// - Org shop (weapons, armor, vehicles)
// - Org skill tree
// ============================================================

let orgInfo = {
    id: 0,
    name: "",
    level: 0,
    balance: 0,
    members: 0,
    maxMembers: 10,
    reputation: 0,
    territories: 0,
    upgrades: {},
};

// ====== ORG UPGRADE TREE ======
const orgUpgradeTree = {
    // HQ Upgrades
    HQ_ARMORY: {
        name: translateText("Оружейная"),
        description: translateText("Доступ к покупке оружия в штабе"),
        levels: [
            { level: 1, cost: 100000, effect: "Пистолеты" },
            { level: 2, cost: 250000, effect: "SMG + Дробовики" },
            { level: 3, cost: 500000, effect: "Винтовки + Снайперки" },
            { level: 4, cost: 1000000, effect: "Тяжелое оружие" },
        ]
    },
    HQ_GARAGE: {
        name: translateText("Гараж штаба"),
        description: translateText("Больше слотов для транспорта организации"),
        levels: [
            { level: 1, cost: 75000, effect: "5 слотов" },
            { level: 2, cost: 150000, effect: "10 слотов" },
            { level: 3, cost: 300000, effect: "20 слотов" },
            { level: 4, cost: 600000, effect: "30 слотов" },
            { level: 5, cost: 1200000, effect: "50 слотов" },
        ]
    },
    HQ_MEDBAY: {
        name: translateText("Мед. блок"),
        description: translateText("Лечение и броня для членов орг"),
        levels: [
            { level: 1, cost: 50000, effect: "Аптечки" },
            { level: 2, cost: 150000, effect: "Бронежилеты" },
            { level: 3, cost: 400000, effect: "Полное восстановление" },
        ]
    },
    HQ_WORKSHOP: {
        name: translateText("Мастерская"),
        description: translateText("Модификация транспорта организации"),
        levels: [
            { level: 1, cost: 200000, effect: "Базовый тюнинг" },
            { level: 2, cost: 500000, effect: "Продвинутый тюнинг" },
            { level: 3, cost: 1000000, effect: "NOS + Броня" },
            { level: 4, cost: 2000000, effect: "Полный тюнинг" },
        ]
    },
    HQ_SECURITY: {
        name: translateText("Безопасность"),
        description: translateText("Защита штаба и территорий"),
        levels: [
            { level: 1, cost: 100000, effect: "Камеры наблюдения" },
            { level: 2, cost: 250000, effect: "Охранники NPC" },
            { level: 3, cost: 500000, effect: "Турели" },
            { level: 4, cost: 1000000, effect: "Дрон-защита" },
        ]
    },
    HQ_HELIPAD: {
        name: translateText("Вертолётная площадка"),
        description: translateText("Доступ к воздушному транспорту"),
        levels: [
            { level: 1, cost: 500000, effect: "1 вертолёт" },
            { level: 2, cost: 1000000, effect: "3 вертолёта" },
            { level: 3, cost: 2500000, effect: "5 вертолётов + боевые" },
        ]
    },
    HQ_DOCK: {
        name: translateText("Док/Причал"),
        description: translateText("Доступ к водному транспорту"),
        levels: [
            { level: 1, cost: 200000, effect: "Лодки" },
            { level: 2, cost: 500000, effect: "Катера" },
            { level: 3, cost: 1000000, effect: "Яхта" },
        ]
    },
    ORG_INCOME: {
        name: translateText("Бизнес-доход"),
        description: translateText("Пассивный доход организации"),
        levels: [
            { level: 1, cost: 150000, effect: "+$5000/час" },
            { level: 2, cost: 350000, effect: "+$12000/час" },
            { level: 3, cost: 750000, effect: "+$25000/час" },
            { level: 4, cost: 1500000, effect: "+$50000/час" },
            { level: 5, cost: 3000000, effect: "+$100000/час" },
        ]
    },
    ORG_RECRUITMENT: {
        name: translateText("Рекрутинг"),
        description: translateText("Увеличение максимума участников"),
        levels: [
            { level: 1, cost: 50000, effect: "15 участников" },
            { level: 2, cost: 100000, effect: "20 участников" },
            { level: 3, cost: 200000, effect: "30 участников" },
            { level: 4, cost: 500000, effect: "50 участников" },
            { level: 5, cost: 1000000, effect: "100 участников" },
        ]
    },
    ORG_HACKING: {
        name: translateText("Хакерский центр"),
        description: translateText("Взлом и кибер-возможности"),
        levels: [
            { level: 1, cost: 300000, effect: "Отслеживание игроков" },
            { level: 2, cost: 600000, effect: "Взлом камер" },
            { level: 3, cost: 1200000, effect: "Глушение связи" },
        ]
    },
};

// ====== ORG MISSIONS ======
const orgMissions = {
    SUPPLY_RUN: {
        name: translateText("Поставка ресурсов"),
        description: translateText("Доставьте ресурсы в штаб"),
        reward: { money: 15000, rep: 50 },
        cooldown: 1800000, // 30 min
    },
    TERRITORY_SCOUT: {
        name: translateText("Разведка территории"),
        description: translateText("Разведайте вражескую территорию"),
        reward: { money: 10000, rep: 30 },
        cooldown: 900000, // 15 min
    },
    VIP_ESCORT: {
        name: translateText("Эскорт VIP"),
        description: translateText("Сопроводите VIP в безопасное место"),
        reward: { money: 25000, rep: 75 },
        cooldown: 2700000, // 45 min
    },
    HEIST_PREP: {
        name: translateText("Подготовка к ограблению"),
        description: translateText("Соберите необходимое оборудование"),
        reward: { money: 30000, rep: 100 },
        cooldown: 3600000, // 60 min
    },
    ARMS_DEAL: {
        name: translateText("Сделка с оружием"),
        description: translateText("Проведите сделку по покупке оружия"),
        reward: { money: 40000, rep: 80 },
        cooldown: 2400000, // 40 min
    },
    VEHICLE_THEFT: {
        name: translateText("Угон транспорта"),
        description: translateText("Угоните целевой транспорт"),
        reward: { money: 20000, rep: 60 },
        cooldown: 1200000, // 20 min
    },
    DRUG_COOKING: {
        name: translateText("Производство"),
        description: translateText("Произведите партию товара"),
        reward: { money: 35000, rep: 90 },
        cooldown: 3600000, // 60 min
    },
    ASSASSINATION: {
        name: translateText("Устранение цели"),
        description: translateText("Устраните указанную цель"),
        reward: { money: 50000, rep: 120 },
        cooldown: 5400000, // 90 min
    },
    MONEY_LAUNDERING: {
        name: translateText("Отмыв денег"),
        description: translateText("Отмойте деньги через бизнес"),
        reward: { money: 45000, rep: 70 },
        cooldown: 3600000, // 60 min
    },
    PRISON_BREAK: {
        name: translateText("Побег из тюрьмы"),
        description: translateText("Освободите члена банды из тюрьмы"),
        reward: { money: 60000, rep: 150 },
        cooldown: 7200000, // 120 min
    },
};

// ====== CLIENT EVENTS ======
gm.events.add('client.org.info.update', (infoJson) => {
    try {
        const data = JSON.parse(infoJson);
        orgInfo = { ...orgInfo, ...data };
        mp.gui.emmit(`window.orgStore && window.orgStore.updateInfo('${JSON.stringify(orgInfo)}')`);
    } catch (e) {}
});

gm.events.add('client.org.upgrade.menu', (upgradesJson) => {
    try {
        if (global.menuCheck()) return;
        global.menuOpen();
        
        const currentUpgrades = JSON.parse(upgradesJson);
        const data = {
            upgrades: orgUpgradeTree,
            current: currentUpgrades,
            balance: orgInfo.balance,
        };
        
        mp.gui.emmit(`window.router.setView("OrgUpgrades", '${JSON.stringify(data)}');`);
        gm.discord(translateText("Улучшает организацию"));
    } catch (e) {
        mp.events.callRemote("client_trycatch", "fractions/orgmanager", "upgradeMenu", e.toString());
    }
});

gm.events.add('client.org.upgrade.buy', (upgradeId, level) => {
    try {
        mp.events.callRemote('server.org.upgrade.buy', upgradeId, level);
    } catch (e) {}
});

gm.events.add('client.org.upgrade.success', (upgradeId, newLevel) => {
    mp.game.audio.playSoundFrontend(-1, "RANK_UP", "HUD_AWARDS", true);
    mp.events.call('notify', 1, 9, translateText(`✅ Улучшение "${upgradeId}" -> Уровень ${newLevel}!`), 5000);
});

// ====== ORG MISSIONS MENU ======
gm.events.add('client.org.missions.menu', (availableMissions, cooldowns) => {
    try {
        if (global.menuCheck()) return;
        global.menuOpen();
        
        const missions = JSON.parse(availableMissions);
        const cds = JSON.parse(cooldowns);
        
        const data = {
            missions: orgMissions,
            available: missions,
            cooldowns: cds,
        };
        
        mp.gui.emmit(`window.router.setView("OrgMissions", '${JSON.stringify(data)}');`);
    } catch (e) {}
});

gm.events.add('client.org.mission.start', (missionId, objectives) => {
    try {
        const mission = orgMissions[missionId];
        if (!mission) return;
        
        mp.events.call('notify', 1, 9, 
            translateText(`🎯 Миссия: ${mission.name} | Награда: $${mission.reward.money}`), 8000);
        
        mp.gui.emmit(`window.hudStore.startOrgMission && window.hudStore.startOrgMission('${mission.name}', '${objectives}')`);
        mp.gui.emmit(`window.router.setHud();`);
        global.menuClose();
    } catch (e) {}
});

gm.events.add('client.org.mission.complete', (missionId, money, rep) => {
    try {
        mp.game.audio.playSoundFrontend(-1, "RACE_PLACED", "HUD_AWARDS", true);
        mp.game.graphics.startScreenEffect("SuccessMichael", 2000, false);
        mp.events.call('notify', 1, 9, 
            translateText(`✅ Миссия завершена! +$${money} | +${rep} реп.`), 8000);
        mp.gui.emmit(`window.hudStore.endOrgMission && window.hudStore.endOrgMission(true)`);
    } catch (e) {}
});

gm.events.add('client.org.mission.fail', (reason) => {
    mp.events.call('notify', 4, 9, translateText(`❌ Миссия провалена: ${reason}`), 5000);
    mp.gui.emmit(`window.hudStore.endOrgMission && window.hudStore.endOrgMission(false)`);
});

// ====== ORG ALLIANCE SYSTEM ======
gm.events.add('client.org.alliance.request', (orgName) => {
    mp.events.call('notify', 1, 9, 
        translateText(`🤝 Запрос на альянс от "${orgName}". Откройте планшет для ответа.`), 10000);
});

gm.events.add('client.org.alliance.accept', (orgName) => {
    mp.events.callRemote('server.org.alliance.accept', orgName);
});

gm.events.add('client.org.alliance.decline', (orgName) => {
    mp.events.callRemote('server.org.alliance.decline', orgName);
});

gm.events.add('client.org.alliance.formed', (orgName) => {
    mp.game.audio.playSoundFrontend(-1, "RANK_UP", "HUD_AWARDS", true);
    mp.events.call('notify', 1, 9, translateText(`🤝 Альянс с "${orgName}" установлен!`), 5000);
});

// ====== ORG WAR DECLARATION ======
gm.events.add('client.org.war.declare', (targetOrg) => {
    mp.events.callRemote('server.org.war.declare', targetOrg);
});

gm.events.add('client.org.war.declared', (attackerName) => {
    mp.game.audio.playSoundFrontend(-1, "TIMER_STOP", "HUD_MINI_GAME_SOUNDSET", true);
    mp.game.graphics.startScreenEffect("DeathFailMPIn", 3000, false);
    mp.events.call('notify', 4, 9, 
        translateText(`⚔️ Организация "${attackerName}" объявила войну!`), 10000);
});

// ====== ORG SHOP ======
gm.events.add('client.org.shop.open', (shopData) => {
    try {
        if (global.menuCheck()) return;
        global.menuOpen();
        mp.gui.emmit(`window.router.setView("OrgShop", '${shopData}');`);
    } catch (e) {}
});

gm.events.add('client.org.shop.buy', (itemId, amount) => {
    mp.events.callRemote('server.org.shop.buy', itemId, amount);
});

gm.events.add('client.org.shop.close', () => {
    mp.gui.emmit(`window.router.setHud();`);
    global.menuClose();
});

// ====== ORG INCOME NOTIFICATION ======
gm.events.add('client.org.income', (amount) => {
    mp.events.call('notify', 1, 9, translateText(`💰 Доход организации: +$${amount}`), 3000);
});

// ====== CLOSE MENUS ======
gm.events.add('client.org.close', () => {
    mp.gui.emmit(`window.router.setHud();`);
    global.menuClose();
});
