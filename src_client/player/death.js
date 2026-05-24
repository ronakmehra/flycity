global.deathTimerOn = false;
let deathTimer = 0;
let deathEffectActive = false;


// Disable auto screen fade
mp.game.gameplay.disableAutomaticRespawn(true);
mp.game.gameplay.ignoreNextRestart(true);
mp.game.gameplay.setFadeOutAfterDeath(false);
mp.game.gameplay.setFadeInAfterDeathArrest(false);
mp.game.gameplay.setFadeInAfterLoad(false);

gm.events.add('DeathTimer', (time) => {
	try
	{
		if (typeof time === 'number' && Math.round(time) >= 1) {
			global.deathTimerOn = true;
			global.localplayer.setInvincible(true);
			deathTimer = new Date().getTime() + Math.round(time);			
		} else if (global.deathTimerOn) {
			mp.events.call("clearCarryng");
			global.deathTimerOn = false;
			deathEffectActive = false;
			global.localplayer.setInvincible(false);
			global.binderFunctions.c_globalEscape (true);
			mp.events.call("client.phone.close");
		}
	}
	catch (e) 
	{
		mp.events.callRemote("client_trycatch", "player/death", "DeathTimer", e.toString());
	}
});

var blockcontrols = false;
var fullblockcontrols = false;

gm.events.add('blockMove', function (argument) {
	try
	{
		blockcontrols = argument;
	}
	catch (e) 
	{
		mp.events.callRemote("client_trycatch", "player/death", "blockMove", e.toString());
	}
});

gm.events.add('fullblockMove', function (argument) {
	try
	{
		fullblockcontrols = argument;
	}
	catch (e) 
	{
		mp.events.callRemote("client_trycatch", "player/death", "fullblockMove", e.toString());
	}
});

gm.events.add("playerDeath", async (player, reason, killer) =>  {
	try
	{
        if (!global.loggedin) return;
        else if (player !== global.localplayer) return;
		global.binderFunctions.c_globalEscape (true);
		mp.events.call("client.phone.close");
		if (global.localplayer.vehicle) {
			global.localplayer.clearTasks();
			global.localplayer.clearTasksImmediately();
		}
		if (!global.inAirsoftLobby || global.inAirsoftLobby == -1) {
			mp.game.audio.playSoundFrontend(-1, "Bed", "WastedSounds", true);
			mp.game.graphics.startScreenEffect("DeathFailMPIn", 0, true);
			mp.game.cam.setCamEffect(1);
			deathEffectActive = true;
			
			// Enhanced death camera - slow motion effect
			mp.game.gameplay.setTimeScale(0.3);
			setTimeout(() => {
				if (deathEffectActive) {
					mp.game.gameplay.setTimeScale(1.0);
				}
			}, 2000);
		}
	}
	catch (e) 
	{
		mp.events.callRemote("client_trycatch", "player/death", "playerDeath", e.toString());
	}
});

gm.events.add("playerSpawn", () => {
	try
	{
        if (!global.loggedin) return;
		global.deathTimerOn = false;
		deathEffectActive = false;
		global.localplayer.setInvincible(false);
		mp.game.graphics.stopScreenEffect("DeathFailMPIn");
		mp.game.graphics.stopScreenEffect("DeathFailMPDark");
		mp.game.cam.setCamEffect(0);
		mp.game.gameplay.setTimeScale(1.0);
		global.lastCheck = new Date().getTime();
		global.closeDialog();
	}
	catch (e) 
	{
		mp.events.callRemote("client_trycatch", "player/death", "playerSpawn", e.toString());
	}
});

gm.events.add("render", () => {
	if (!global.loggedin) return;
	if (global.isDeath)
	{
		mp.game.controls.disableAllControlActions(2);
		mp.game.controls.enableControlAction(2, global.Inputs.LOOK_LR, true);
		mp.game.controls.enableControlAction(2, global.Inputs.LOOK_UD, true);
		mp.game.controls.enableControlAction(2, global.Inputs.LOOK_UP_ONLY, true);
		mp.game.controls.enableControlAction(2, global.Inputs.LOOK_DOWN_ONLY, true);
		mp.game.controls.enableControlAction(2, global.Inputs.LOOK_LEFT_ONLY, true);
		mp.game.controls.enableControlAction(2, global.Inputs.LOOK_RIGHT_ONLY, true);
	}
	else if (blockcontrols || fullblockcontrols)
	{
		mp.game.controls.disableAllControlActions(2);
		if(!fullblockcontrols)
		{
			mp.game.controls.enableControlAction(2, 30, true);
			mp.game.controls.enableControlAction(2, 31, true);
			mp.game.controls.enableControlAction(2, 32, true);
			mp.game.controls.enableControlAction(2, 1, true);
			mp.game.controls.enableControlAction(2, 2, true);
		}
	}
	if (global.deathTimerOn)
	{
		const secondsLeft = Math.trunc((deathTimer - new Date().getTime()) / 1000);
		if (secondsLeft >= 0)
		{
			mp.game.cam.doScreenFadeIn(500);
			gm.discord(translateText("Ждёт медиков..."));

			const minutes = Math.trunc(secondsLeft / 60);
			const seconds = secondsLeft % 60;

			// Enhanced death timer display with pulsing effect
			const alpha = 180 + Math.floor(Math.sin(new Date().getTime() / 500) * 50);
			
			mp.game.graphics.drawText(translateText("ВЫ БЕЗ СОЗНАНИЯ"), [0.5, 0.72], {
				font: 0,
				color: [231, 29, 54, alpha],
				scale: [0.55, 0.55],
				outline: true
			});

			mp.game.graphics.drawText(translateText("До попадания в больницу: {0}:{1}", global.formatIntZero(minutes, 2), global.formatIntZero(seconds, 2)), [0.5, 0.78], {
				font: 0,
				color: [255, 255, 255, 200],
				scale: [0.35, 0.35],
				outline: true
			});

			mp.game.graphics.drawText(translateText("Ожидайте помощь медиков или используйте меню"), [0.5, 0.83], {
				font: 0,
				color: [255, 255, 255, 120],
				scale: [0.25, 0.25],
				outline: true
			});
		}
	}
});
