// ============================================================
// VEHICLE ABILITIES & SPECIAL FEATURES - FlyCity Roleplay
// ============================================================
// Features:
// - Hydraulics system (lowriders)
// - Vehicle horn customization
// - Emergency vehicle features (sirens, PA)
// - Vehicle weapons (mounted guns)
// - Smoke screen / oil slick
// - EMP blast
// - Vehicle stealth mode
// - Turbo gauge HUD display
// - Drift score counter
// - Vehicle repair kit usage
// ============================================================

// ====== HYDRAULICS SYSTEM ======
let hydraulicsActive = false;

global.binderFunctions.toggleHydraulics = () => {
    try {
        if (!global.loggedin || global.chatActive || global.editing || global.menuCheck()) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;
        
        // Only lowriders (class 4 muscle cars with hydraulics mod)
        if (!vehicle.getVariable('HYDRAULICS')) return;

        hydraulicsActive = !hydraulicsActive;
        mp.events.callRemote('server.vehicle.hydraulics', hydraulicsActive);
        
        if (hydraulicsActive) {
            mp.events.call('notify', 1, 9, translateText("Гидравлика включена! Используйте клавиши 8,4,6,2 (Numpad)"), 3000);
        } else {
            mp.events.call('notify', 1, 9, translateText("Гидравлика выключена"), 2000);
        }
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/abilities", "hydraulics", e.toString());
    }
};

// ====== SMOKE SCREEN ======
let smokeScreenReady = true;
const smokeScreenCooldown = 45000; // 45 seconds

global.binderFunctions.activateSmokeScreen = () => {
    try {
        if (!global.loggedin || global.chatActive || global.editing || global.menuCheck()) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;
        if (!vehicle.getVariable('SMOKE_SCREEN') && !vehicle.getVariable('ORG_VEHICLE_PREMIUM')) return;
        if (!smokeScreenReady) return;

        smokeScreenReady = false;
        mp.events.callRemote('server.vehicle.smokescreen');
        
        // Visual effect - spawn smoke particles
        const pos = vehicle.position;
        for (let i = 0; i < 5; i++) {
            setTimeout(() => {
                if (vehicle && mp.vehicles.exists(vehicle)) {
                    mp.game.graphics.startParticleFxNonLoopedAtCoord(
                        "exp_grd_bzgas_smoke", 
                        pos.x + (Math.random() - 0.5) * 3, 
                        pos.y + (Math.random() - 0.5) * 3, 
                        pos.z - 0.5, 
                        0, 0, 0, 3.0, false, false, false
                    );
                }
            }, i * 200);
        }
        
        mp.events.call('notify', 1, 9, translateText("💨 Дымовая завеса активирована!"), 2000);

        setTimeout(() => {
            smokeScreenReady = true;
            mp.events.call('notify', 1, 9, translateText("Дымовая завеса перезаряжена ✅"), 2000);
        }, smokeScreenCooldown);

    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/abilities", "smokeScreen", e.toString());
    }
};

// ====== OIL SLICK ======
let oilSlickReady = true;
const oilSlickCooldown = 60000; // 60 seconds

global.binderFunctions.activateOilSlick = () => {
    try {
        if (!global.loggedin || global.chatActive || global.editing || global.menuCheck()) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;
        if (!vehicle.getVariable('OIL_SLICK')) return;
        if (!oilSlickReady) return;
        if (vehicle.getSpeed() * 3.6 < 30) return; // Must be moving

        oilSlickReady = false;
        mp.events.callRemote('server.vehicle.oilslick');
        
        mp.events.call('notify', 1, 9, translateText("🛢️ Масляное пятно сброшено!"), 2000);

        setTimeout(() => {
            oilSlickReady = true;
            mp.events.call('notify', 1, 9, translateText("Масляное пятно перезаряжено ✅"), 2000);
        }, oilSlickCooldown);

    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/abilities", "oilSlick", e.toString());
    }
};

// ====== EMP BLAST ======
let empReady = true;
const empCooldown = 120000; // 2 minutes

global.binderFunctions.activateEMP = () => {
    try {
        if (!global.loggedin || global.chatActive || global.editing || global.menuCheck()) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;
        if (!vehicle.getVariable('EMP_BLAST')) return;
        if (!empReady) return;

        empReady = false;
        mp.events.callRemote('server.vehicle.emp');
        
        // EMP visual effect
        mp.game.graphics.startScreenEffect("FocusOut", 1500, false);
        mp.game.audio.playSoundFrontend(-1, "FLIGHT_MISSILE_LOCKED", "HUD_FRONTEND_DEFAULT_SOUNDSET", true);
        
        mp.events.call('notify', 1, 9, translateText("⚡ EMP взрыв! Ближайшие транспорт выключены!"), 3000);

        setTimeout(() => {
            empReady = true;
            mp.events.call('notify', 1, 9, translateText("EMP перезаряжен ✅"), 2000);
        }, empCooldown);

    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/abilities", "emp", e.toString());
    }
};

// ====== VEHICLE STEALTH MODE ======
let stealthActive = false;

global.binderFunctions.toggleStealth = () => {
    try {
        if (!global.loggedin || global.chatActive || global.editing || global.menuCheck()) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;
        if (!vehicle.getVariable('STEALTH_MODE')) return;

        stealthActive = !stealthActive;
        mp.events.callRemote('server.vehicle.stealth', stealthActive);
        
        if (stealthActive) {
            // Make vehicle semi-transparent and remove from minimap
            vehicle.setAlpha(100);
            mp.events.call('notify', 1, 9, translateText("🫥 Режим невидимости включен!"), 2000);
        } else {
            vehicle.setAlpha(255);
            mp.events.call('notify', 1, 9, translateText("Режим невидимости выключен"), 2000);
        }
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/abilities", "stealth", e.toString());
    }
};

// ====== VEHICLE REPAIR KIT ======
global.binderFunctions.useRepairKit = () => {
    try {
        if (!global.loggedin || global.chatActive || global.editing || global.menuCheck()) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;
        if (vehicle.getSpeed() * 3.6 > 5) {
            mp.events.call('notify', 4, 9, translateText("Остановитесь для ремонта!"), 2000);
            return;
        }

        mp.events.callRemote('server.vehicle.repairkit');
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/abilities", "repairKit", e.toString());
    }
};

gm.events.add('client.vehicle.repaired', () => {
    try {
        const vehicle = global.localplayer.vehicle;
        if (vehicle && mp.vehicles.exists(vehicle)) {
            vehicle.setFixed();
            vehicle.setDeformationFixed();
            vehicle.setDirtLevel(0);
        }
        mp.events.call('notify', 1, 9, translateText("🔧 Транспорт отремонтирован!"), 3000);
        mp.game.audio.playSoundFrontend(-1, "CHECKPOINT_PERFECT", "HUD_MINI_GAME_SOUNDSET", true);
    } catch (e) {}
});

// ====== TURBO GAUGE HUD ======
gm.events.add("render", () => {
    try {
        if (!global.loggedin) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;

        // Display speedometer info
        const speed = Math.round(vehicle.getSpeed() * 3.6);
        const rpm = vehicle.getCurrentRpm ? vehicle.getCurrentRpm() : 0;
        const gear = vehicle.getCurrentGear ? vehicle.getCurrentGear() : 0;
        
        mp.gui.emmit(`window.vehicleState.updateSpeedometer && window.vehicleState.updateSpeedometer(${speed}, ${rpm.toFixed(2)}, ${gear})`);
        
    } catch (e) {}
});

// ====== DRIFT SCORE DISPLAY ======
let driftScore = 0;
let driftCombo = 0;
let lastDriftTime = 0;

gm.events.add(global.renderName["250ms"], () => {
    try {
        if (!global.loggedin) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;

        const speed = vehicle.getSpeed() * 3.6;
        if (speed < 40) {
            if (driftScore > 0 && Date.now() - lastDriftTime > 2000) {
                // End drift - show final score
                if (driftScore > 100) {
                    mp.events.callRemote('server.vehicle.driftscore', driftScore, driftCombo);
                }
                driftScore = 0;
                driftCombo = 0;
                mp.gui.emmit(`window.vehicleState.hideDriftScore && window.vehicleState.hideDriftScore()`);
            }
            return;
        }

        // Calculate drift angle
        const velocity = vehicle.getVelocity();
        const heading = vehicle.getHeading();
        const moveAngle = Math.atan2(velocity.y, velocity.x) * (180 / Math.PI);
        let driftAngle = Math.abs(heading - moveAngle) % 360;
        if (driftAngle > 180) driftAngle = 360 - driftAngle;

        if (driftAngle > 15 && driftAngle < 120 && speed > 50) {
            const points = Math.round(driftAngle * (speed / 50));
            driftScore += points;
            driftCombo++;
            lastDriftTime = Date.now();
            
            mp.gui.emmit(`window.vehicleState.updateDriftScore && window.vehicleState.updateDriftScore(${driftScore}, ${driftCombo}, ${driftAngle.toFixed(0)})`);
        }
    } catch (e) {}
});

// ====== SIREN PA SYSTEM (Emergency Vehicles) ======
global.binderFunctions.usePASystem = () => {
    try {
        if (!global.loggedin || global.chatActive || global.editing || global.menuCheck()) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;
        if (vehicle.getClass() !== 18) return; // Emergency class only

        mp.events.callRemote('server.vehicle.pa');
    } catch (e) {}
};

// ====== VEHICLE HORN CUSTOMIZATION ======
gm.events.add('client.vehicle.sethorn', (hornId) => {
    try {
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        vehicle.setMod(14, Number(hornId));
    } catch (e) {}
});

// ====== VEHICLE PROXIMITY MINE ======
let mineReady = true;
const mineCooldown = 90000; // 90 seconds

global.binderFunctions.deployMine = () => {
    try {
        if (!global.loggedin || global.chatActive || global.editing || global.menuCheck()) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;
        if (!vehicle.getVariable('PROX_MINE')) return;
        if (!mineReady) return;
        if (vehicle.getSpeed() * 3.6 < 20) return;

        mineReady = false;
        const pos = vehicle.position;
        mp.events.callRemote('server.vehicle.mine', pos.x, pos.y, pos.z);
        
        mp.events.call('notify', 1, 9, translateText("💣 Мина установлена!"), 2000);
        mp.game.audio.playSoundFrontend(-1, "Placing_Prop", "DLC_Dmod_Prop_Editor_Sounds", true);

        setTimeout(() => {
            mineReady = true;
            mp.events.call('notify', 1, 9, translateText("Мина перезаряжена ✅"), 2000);
        }, mineCooldown);

    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/abilities", "mine", e.toString());
    }
};

// ====== RESET ON LEAVE ======
gm.events.add("playerLeaveVehicle", () => {
    hydraulicsActive = false;
    stealthActive = false;
    driftScore = 0;
    driftCombo = 0;
});
