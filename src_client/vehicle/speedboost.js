// ============================================================
// SPEED BOOST / NOS / TURBO SYSTEM - FlyCity Roleplay
// ============================================================
// Features:
// - Nitro Boost (NOS) with visual effects and sound
// - Turbo Launch (burnout start)
// - Speed Limiter toggle
// - Drift Boost (gain boost from drifting)
// - Jump Boost (launch vehicle in air)
// - Wheelie Mode (bikes)
// - Slipstream (drafting behind vehicles)
// ============================================================

let boostData = {
    nitroActive: false,
    nitroAmount: 100.0, // NOS tank percentage
    nitroRechargeRate: 0.15, // recharge per tick
    nitroDrainRate: 1.2, // drain per tick while active
    nitroBoostPower: 25.0, // speed added
    nitroCooldown: false,
    
    turboLaunchReady: true,
    turboLaunchCooldown: 15000, // 15 seconds
    turboLaunchPower: 35.0,
    
    driftBoostAmount: 0,
    driftBoostMax: 50,
    driftBoostMultiplier: 1.5,
    isDrifting: false,
    
    jumpBoostReady: true,
    jumpBoostCooldown: 30000, // 30 seconds
    jumpBoostPower: 12.0,
    
    speedLimiterActive: false,
    speedLimiterMax: 80, // km/h
    
    slipstreamActive: false,
    slipstreamBonus: 5.0,
    
    wheelieMode: false,
};

// ====== NITRO BOOST (NOS) ======
global.binderFunctions.activateNitro = () => {
    try {
        if (!global.loggedin || global.chatActive || global.editing || global.menuCheck()) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        if (global.localplayer.vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;
        if (boostData.nitroCooldown) return;
        if (boostData.nitroAmount <= 0) return;

        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;

        // Check if vehicle has NOS upgrade (BOOST variable or org vehicle)
        if (!vehicle.getVariable('BOOST') && !vehicle.getVariable('ORG_VEHICLE') && !global.hasNitroKit) return;

        if (!boostData.nitroActive) {
            boostData.nitroActive = true;
            mp.events.call('notify', 1, 9, translateText("NOS активирован! 🚀"), 2000);
            
            // Visual effects
            mp.game.graphics.startScreenEffect("RaceTurbo", 0, true);
            mp.game.audio.playSoundFrontend(-1, "CHECKPOINT_PERFECT", "HUD_MINI_GAME_SOUNDSET", true);
            
            // Apply boost
            applyNitroBoost(vehicle);
        } else {
            deactivateNitro();
        }
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/speedboost", "activateNitro", e.toString());
    }
};

const applyNitroBoost = (vehicle) => {
    try {
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        
        const currentSpeed = vehicle.getSpeed();
        const boostSpeed = currentSpeed + boostData.nitroBoostPower;
        vehicle.setForwardSpeed(boostSpeed);
        
        // Exhaust flames effect
        mp.game.graphics.startParticleFxLoopedOnEntity("veh_exhaust_rocket", vehicle.handle, 0, -2.5, 0.5, 0, 0, 0, 1.5, false, false, false);
        
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/speedboost", "applyNitroBoost", e.toString());
    }
};

const deactivateNitro = () => {
    boostData.nitroActive = false;
    mp.game.graphics.stopScreenEffect("RaceTurbo");
};

// ====== TURBO LAUNCH ======
global.binderFunctions.turboLaunch = () => {
    try {
        if (!global.loggedin || global.chatActive || global.editing || global.menuCheck()) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        if (global.localplayer.vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;
        if (!boostData.turboLaunchReady) return;

        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getSpeed() > 5) return; // Must be nearly stopped

        boostData.turboLaunchReady = false;
        
        // Burnout effect
        vehicle.setForwardSpeed(boostData.turboLaunchPower);
        vehicle.setBurnout(true);
        
        mp.game.graphics.startScreenEffect("RaceTurbo", 2000, false);
        mp.game.audio.playSoundFrontend(-1, "RACE_PLACED", "HUD_AWARDS", true);
        mp.events.call('notify', 1, 9, translateText("Турбо-старт! 🏁"), 2000);

        setTimeout(() => {
            if (vehicle && mp.vehicles.exists(vehicle))
                vehicle.setBurnout(false);
        }, 2000);

        setTimeout(() => {
            boostData.turboLaunchReady = true;
            mp.events.call('notify', 1, 9, translateText("Турбо-старт перезаряжен ✅"), 2000);
        }, boostData.turboLaunchCooldown);

    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/speedboost", "turboLaunch", e.toString());
    }
};

// ====== JUMP BOOST ======
global.binderFunctions.jumpBoost = () => {
    try {
        if (!global.loggedin || global.chatActive || global.editing || global.menuCheck()) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        if (global.localplayer.vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;
        if (!boostData.jumpBoostReady) return;

        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        
        // Only works with special vehicles or org upgraded vehicles
        if (!vehicle.getVariable('JUMP_BOOST') && !vehicle.getVariable('ORG_VEHICLE_PREMIUM')) return;

        boostData.jumpBoostReady = false;

        // Apply upward velocity
        const velocity = vehicle.getVelocity();
        vehicle.setVelocity(velocity.x, velocity.y, boostData.jumpBoostPower);
        
        mp.game.audio.playSoundFrontend(-1, "FLIGHT_MISSILE_LOCKED", "HUD_FRONTEND_DEFAULT_SOUNDSET", true);
        mp.game.graphics.startScreenEffect("FocusOut", 500, false);
        mp.events.call('notify', 1, 9, translateText("Прыжок! 🦘"), 1500);

        setTimeout(() => {
            boostData.jumpBoostReady = true;
        }, boostData.jumpBoostCooldown);

    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/speedboost", "jumpBoost", e.toString());
    }
};

// ====== SPEED LIMITER ======
global.binderFunctions.toggleSpeedLimiter = () => {
    try {
        if (!global.loggedin || global.chatActive || global.editing || global.menuCheck()) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        if (global.localplayer.vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;

        boostData.speedLimiterActive = !boostData.speedLimiterActive;
        
        if (boostData.speedLimiterActive) {
            boostData.speedLimiterMax = Math.round(global.localplayer.vehicle.getSpeed() * 3.6);
            mp.events.call('notify', 1, 9, translateText(`Круиз-контроль: ${boostData.speedLimiterMax} км/ч`), 3000);
        } else {
            mp.events.call('notify', 1, 9, translateText("Круиз-контроль выключен"), 2000);
        }
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/speedboost", "toggleSpeedLimiter", e.toString());
    }
};

// ====== DRIFT BOOST SYSTEM ======
let lastDriftAngle = 0;
let driftTimer = null;

const checkDrift = () => {
    try {
        if (!global.loggedin) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;

        const speed = vehicle.getSpeed() * 3.6;
        if (speed < 40) {
            if (boostData.isDrifting) {
                boostData.isDrifting = false;
                if (boostData.driftBoostAmount > 10) {
                    // Apply drift boost reward
                    const boostAmount = Math.min(boostData.driftBoostAmount, boostData.driftBoostMax);
                    vehicle.setForwardSpeed(vehicle.getSpeed() + boostAmount * boostData.driftBoostMultiplier);
                    mp.game.graphics.startScreenEffect("RaceTurbo", 1000, false);
                    mp.events.call('notify', 1, 9, translateText(`Дрифт-буст! +${boostAmount.toFixed(0)} 🏎️`), 2000);
                }
                boostData.driftBoostAmount = 0;
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
            boostData.isDrifting = true;
            boostData.driftBoostAmount += driftAngle * 0.02;
            if (boostData.driftBoostAmount > boostData.driftBoostMax) {
                boostData.driftBoostAmount = boostData.driftBoostMax;
            }
        } else if (boostData.isDrifting && driftAngle < 5) {
            boostData.isDrifting = false;
            if (boostData.driftBoostAmount > 10) {
                const boostAmount = Math.min(boostData.driftBoostAmount, boostData.driftBoostMax);
                vehicle.setForwardSpeed(vehicle.getSpeed() + boostAmount * boostData.driftBoostMultiplier);
                mp.game.graphics.startScreenEffect("RaceTurbo", 1000, false);
                mp.events.call('notify', 1, 9, translateText(`Дрифт-буст! +${boostAmount.toFixed(0)} 🏎️`), 2000);
            }
            boostData.driftBoostAmount = 0;
        }
    } catch (e) {}
};

// ====== SLIPSTREAM SYSTEM ======
const checkSlipstream = () => {
    try {
        if (!global.loggedin) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;

        const speed = vehicle.getSpeed() * 3.6;
        if (speed < 80) {
            boostData.slipstreamActive = false;
            return;
        }

        // Check if there's a vehicle ahead within slipstream range
        const pos = vehicle.position;
        const heading = vehicle.getHeading();
        const rad = heading * (Math.PI / 180);
        const checkPos = new mp.Vector3(
            pos.x + Math.sin(rad) * 15,
            pos.y + Math.cos(rad) * 15,
            pos.z
        );

        let hasSlipstream = false;
        mp.vehicles.forEachInStreamRange((v) => {
            if (v.handle === vehicle.handle) return;
            const dist = mp.game.system.vdist(checkPos.x, checkPos.y, checkPos.z, v.position.x, v.position.y, v.position.z);
            if (dist < 8) {
                hasSlipstream = true;
            }
        });

        if (hasSlipstream && !boostData.slipstreamActive) {
            boostData.slipstreamActive = true;
            mp.gui.emmit(`window.hudStore.showSlipstream(true)`);
        } else if (!hasSlipstream && boostData.slipstreamActive) {
            boostData.slipstreamActive = false;
            mp.gui.emmit(`window.hudStore.showSlipstream(false)`);
        }

        if (boostData.slipstreamActive) {
            vehicle.setForwardSpeed(vehicle.getSpeed() + 0.1);
        }

    } catch (e) {}
};

// ====== WHEELIE MODE (BIKES) ======
global.binderFunctions.wheelieMode = () => {
    try {
        if (!global.loggedin || global.chatActive || global.editing || global.menuCheck()) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;
        
        // Only bikes (class 8)
        if (vehicle.getClass() !== 8) return;

        boostData.wheelieMode = !boostData.wheelieMode;
        if (boostData.wheelieMode) {
            mp.events.call('notify', 1, 9, translateText("Wheelie режим включен 🏍️"), 2000);
        } else {
            mp.events.call('notify', 1, 9, translateText("Wheelie режим выключен"), 2000);
        }
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/speedboost", "wheelieMode", e.toString());
    }
};

// ====== NOS REGENERATION & SPEED LIMITER TICK ======
gm.events.add(global.renderName["250ms"], () => {
    try {
        if (!global.loggedin) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;

        // NOS regen
        if (!boostData.nitroActive && boostData.nitroAmount < 100) {
            boostData.nitroAmount += boostData.nitroRechargeRate;
            if (boostData.nitroAmount > 100) boostData.nitroAmount = 100;
        }

        // NOS drain
        if (boostData.nitroActive) {
            boostData.nitroAmount -= boostData.nitroDrainRate;
            if (boostData.nitroAmount <= 0) {
                boostData.nitroAmount = 0;
                deactivateNitro();
                boostData.nitroCooldown = true;
                mp.events.call('notify', 4, 9, translateText("NOS опустошен! Перезарядка..."), 3000);
                setTimeout(() => { boostData.nitroCooldown = false; }, 5000);
            } else {
                // Continuous boost while active
                const speed = vehicle.getSpeed();
                if (speed < 70) { // Max speed cap with NOS
                    vehicle.setForwardSpeed(speed + 0.5);
                }
            }
        }

        // Speed limiter
        if (boostData.speedLimiterActive) {
            const currentSpeed = vehicle.getSpeed() * 3.6;
            if (currentSpeed > boostData.speedLimiterMax + 5) {
                vehicle.setForwardSpeed(boostData.speedLimiterMax / 3.6);
            }
        }

        // Update HUD with NOS level
        mp.gui.emmit(`window.vehicleState.updateNOS && window.vehicleState.updateNOS(${boostData.nitroAmount.toFixed(1)}, ${boostData.nitroActive})`);

    } catch (e) {}
});

// Drift & slipstream check on 500ms interval
gm.events.add(global.renderName["500ms"], () => {
    checkDrift();
    checkSlipstream();
});

// Wheelie tick
gm.events.add("render", () => {
    try {
        if (!global.loggedin) return;
        if (!boostData.wheelieMode) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getClass() !== 8) return;
        if (vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;

        const speed = vehicle.getSpeed() * 3.6;
        if (speed > 30) {
            // Apply slight upward rotation for wheelie effect
            mp.game.gameplay.applyForceToEntity(
                vehicle.handle, 1,
                0, 0, 0.3,
                0, -1.5, 0,
                0, true, true, true, false, true
            );
        }
    } catch (e) {}
});

// ====== REMOTE EVENTS FROM SERVER ======
gm.events.add('client.boost.setNitro', (amount) => {
    boostData.nitroAmount = Number(amount);
});

gm.events.add('client.boost.upgrade', (type, value) => {
    switch(type) {
        case 'power':
            boostData.nitroBoostPower = Number(value);
            break;
        case 'capacity':
            boostData.nitroAmount = Number(value);
            break;
        case 'recharge':
            boostData.nitroRechargeRate = Number(value);
            break;
        case 'turbo':
            boostData.turboLaunchPower = Number(value);
            break;
        case 'jump':
            boostData.jumpBoostPower = Number(value);
            break;
    }
    mp.events.call('notify', 1, 9, translateText("Улучшение буста применено! ⬆️"), 3000);
});

// Reset on vehicle leave
gm.events.add("playerLeaveVehicle", () => {
    deactivateNitro();
    boostData.driftBoostAmount = 0;
    boostData.isDrifting = false;
    boostData.slipstreamActive = false;
    boostData.wheelieMode = false;
    boostData.speedLimiterActive = false;
});

// Notify server of boost usage for anti-cheat
gm.events.add(global.renderName["5s"], () => {
    if (boostData.nitroActive) {
        mp.events.callRemote('server.boost.sync', boostData.nitroAmount.toFixed(1));
    }
});
