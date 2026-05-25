/**
 * Native function utilities for RAGE:MP client-side
 * Provides helper wrappers for commonly used GTA V native functions
 * 
 * Note: Primary native definitions are in configs/natives.js
 * This module provides additional utility wrappers
 */

// Re-export getNative from global scope (set by configs/natives.js)
exports.getNative = function (name) {
    if (typeof global.getNative === 'function') {
        return global.getNative(name);
    }
    return null;
};

// Helper to invoke a native by name with automatic hash resolution
exports.invokeNative = function (name, ...args) {
    const hash = exports.getNative(name);
    if (hash) {
        return mp.game.invoke(hash, ...args);
    }
    return undefined;
};
