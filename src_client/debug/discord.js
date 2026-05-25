gm.discord = (detailedStatus) => {
    let state = "на FlyCity";

    if (global.localplayer && typeof global.localplayer.remoteId !== "undefined")
        state = translateText('на FlyCity под ID {0}', global.localplayer.remoteId);

    mp.discord.update(detailedStatus, state);
}

global.discordDefault = () => {
    gm.discord(translateText('Наслаждается жизнью'))
};

discordDefault ();