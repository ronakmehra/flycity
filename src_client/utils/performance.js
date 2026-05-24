// ============================================
// FlyCity Performance Optimization Module
// Reduces CPU/GPU load and improves FPS
// ============================================

// Cache frequently accessed values to avoid repeated lookups
const perfCache = {
    lastPos: null,
    lastVehicle: null,
    lastHealth: 0,
    lastArmor: 0,
    frameCount: 0,
    lastFPSCheck: 0,
    currentFPS: 60,
    isLowFPS: false
};

// Throttled function execution helper
global.throttle = (func, limit) => {
    let lastFunc;
    let lastRan;
    return function() {
        const context = this;
        const args = arguments;
        if (!lastRan) {
            func.apply(context, args);
            lastRan = Date.now();
        } else {
            clearTimeout(lastFunc);
            lastFunc = setTimeout(function() {
                if ((Date.now() - lastRan) >= limit) {
                    func.apply(context, args);
                    lastRan = Date.now();
                }
            }, limit - (Date.now() - lastRan));
        }
    }
};

// Debounce helper for UI updates
global.debounce = (func, wait) => {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
};

// Object pool for frequently created/destroyed objects
class ObjectPool {
    constructor(createFn, resetFn, initialSize = 10) {
        this.createFn = createFn;
        this.resetFn = resetFn;
        this.pool = [];
        for (let i = 0; i < initialSize; i++) {
            this.pool.push(this.createFn());
        }
    }

    get() {
        if (this.pool.length > 0) {
            return this.pool.pop();
        }
        return this.createFn();
    }

    release(obj) {
        if (this.resetFn) this.resetFn(obj);
        if (this.pool.length < 50) {
            this.pool.push(obj);
        }
    }
}

global.ObjectPool = ObjectPool;

// Optimized distance check (avoid sqrt when not needed)
global.distSquared = (pos1, pos2) => {
    const dx = pos1.x - pos2.x;
    const dy = pos1.y - pos2.y;
    const dz = pos1.z - pos2.z;
    return dx * dx + dy * dy + dz * dz;
};

global.isInRange = (pos1, pos2, range) => {
    return global.distSquared(pos1, pos2) <= (range * range);
};

// Batch GUI updates to reduce CEF bridge calls
let guiUpdateQueue = [];
let guiUpdateScheduled = false;

global.batchGuiEmit = (code) => {
    guiUpdateQueue.push(code);
    if (!guiUpdateScheduled) {
        guiUpdateScheduled = true;
        setTimeout(() => {
            if (guiUpdateQueue.length > 0) {
                const batchCode = guiUpdateQueue.join(';');
                mp.gui.emmit(batchCode);
                guiUpdateQueue = [];
            }
            guiUpdateScheduled = false;
        }, 16); // ~60fps batch rate
    }
};

// Performance monitoring
let fpsFrames = 0;
let fpsLastTime = Date.now();

gm.events.add("render", () => {
    fpsFrames++;
    const now = Date.now();
    if (now - fpsLastTime >= 1000) {
        perfCache.currentFPS = fpsFrames;
        perfCache.isLowFPS = fpsFrames < 30;
        fpsFrames = 0;
        fpsLastTime = now;
    }
});

// Adaptive render quality based on FPS
global.getAdaptiveInterval = (baseInterval) => {
    if (perfCache.isLowFPS) {
        return baseInterval * 2; // Double interval when FPS is low
    }
    return baseInterval;
};

// Optimized player streaming - reduce processing for distant players
global.getVisiblePlayers = (range = 100) => {
    const myPos = global.localplayer.position;
    const rangeSq = range * range;
    const visible = [];
    
    mp.players.forEach((player) => {
        if (player === global.localplayer) return;
        if (!player || !mp.players.exists(player)) return;
        
        const pos = player.position;
        const distSq = (pos.x - myPos.x) ** 2 + (pos.y - myPos.y) ** 2 + (pos.z - myPos.z) ** 2;
        
        if (distSq <= rangeSq) {
            visible.push({ player, distSq });
        }
    });
    
    return visible.sort((a, b) => a.distSq - b.distSq);
};

// Memory optimization - cleanup old data
const cleanupInterval = setInterval(() => {
    if (typeof gc === 'function') {
        gc(); // Force GC if available
    }
}, 60000); // Every minute

// Optimized JSON parse with caching
const jsonCache = new Map();
const JSON_CACHE_MAX = 100;

global.cachedJsonParse = (str) => {
    if (jsonCache.has(str)) {
        return jsonCache.get(str);
    }
    
    const result = JSON.parse(str);
    
    if (jsonCache.size >= JSON_CACHE_MAX) {
        const firstKey = jsonCache.keys().next().value;
        jsonCache.delete(firstKey);
    }
    
    jsonCache.set(str, result);
    return result;
};

// Reduce texture loading stutters - preload system
global.preloadedModels = new Set();

global.preloadModel = async (modelName) => {
    if (global.preloadedModels.has(modelName)) return true;
    
    const hash = mp.game.joaat(modelName);
    if (mp.game.streaming.hasModelLoaded(hash)) {
        global.preloadedModels.add(modelName);
        return true;
    }
    
    mp.game.streaming.requestModel(hash);
    
    let attempts = 0;
    while (!mp.game.streaming.hasModelLoaded(hash) && attempts < 50) {
        await global.wait(10);
        attempts++;
    }
    
    if (mp.game.streaming.hasModelLoaded(hash)) {
        global.preloadedModels.add(modelName);
        return true;
    }
    return false;
};

// Optimize vehicle streaming - reduce render calls for distant vehicles
global.optimizeVehicleStream = () => {
    const myPos = global.localplayer.position;
    
    mp.vehicles.forEach((vehicle) => {
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        
        const pos = vehicle.position;
        const distSq = (pos.x - myPos.x) ** 2 + (pos.y - myPos.y) ** 2 + (pos.z - myPos.z) ** 2;
        
        // LOD optimization
        if (distSq > 10000) { // > 100m
            vehicle.setLodMultiplier(0.3);
        } else if (distSq > 2500) { // > 50m
            vehicle.setLodMultiplier(0.6);
        } else {
            vehicle.setLodMultiplier(1.0);
        }
    });
};

// Run vehicle optimization every 2 seconds
gm.events.add(global.renderName["2.5ms"] || "render2500", () => {
    try {
        if (!global.loggedin) return;
        global.optimizeVehicleStream();
    } catch(e) {}
});

// Disable unnecessary game features for better performance
gm.events.add("playerReady", () => {
    try {
        // Disable useless ambient events
        mp.game.gameplay.setRandomEventFlag(false);
        
        // Reduce population density for performance
        mp.game.streaming.setReducePedModelBudget(true);
        mp.game.streaming.setReduceVehicleModelBudget(true);
        
        // Disable random peds doing scenarios (reduces CPU)
        mp.game.invoke("0x7A556143A1C03898", 0.0); // SET_DENSITY_MULTIPLIER_THIS_FRAME (parked vehicles)
        
        // Optimize shadow rendering
        mp.game.graphics.cascadeShadowsSetShadowSampleType("Poisson_3");
    } catch(e) {}
});

// Optimize render tick - skip frames for non-critical updates
global.shouldUpdateThisFrame = (updateId, intervalFrames = 2) => {
    perfCache.frameCount++;
    return (perfCache.frameCount + updateId) % intervalFrames === 0;
};

console.log("[FlyCity] Performance optimization module loaded");
