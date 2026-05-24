<script>
    import './main.sass'
    import { executeClient } from 'api/rage'
    import { fly, fade } from 'svelte/transition';
    import { translateText } from 'lang'
    export let popupData;

    let title = popupData.title,
        text = popupData.text;

    let timeLeft = 300;
    let timerInterval;

    const formatTime = (seconds) => {
        const m = Math.floor(seconds / 60);
        const s = seconds % 60;
        return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
    }

    timerInterval = setInterval(() => {
        if (timeLeft > 0) timeLeft--;
    }, 1000);

    import { onDestroy } from 'svelte';
    onDestroy(() => {
        if (timerInterval) clearInterval(timerInterval);
    });

    const HandleKeyDown = (event) => {
        const { keyCode } = event;
        if (keyCode == 27)
            executeClient ('client:OnHospitalDialogCallback', 1)
    }
</script>

<div id="popup__death" in:fade={{ duration: 500 }}>
    <div class="popup__death_timer">{formatTime(timeLeft)}</div>
    <div class="popup__death_title">{translateText('popups', 'Вы без сознания')}</div>
    <div class="popup__death_subtitle">{@html text}</div>
    <div class="popup__death_stats">
        <div class="popup__death_stat">
            <span class="stat_value">{formatTime(timeLeft)}</span>
            <span class="stat_label">{translateText('popups', 'До респауна')}</span>
        </div>
    </div>
    <div class="popup__death__buttons">
        <div class="popup__death_button" in:fly={{ y: 30, duration: 400, delay: 200 }} on:click={() => executeClient ('client:OnHospitalDialogCallback', 1)}>{translateText('popups', 'Подождать (5 мин)')}</div>
        <div class="popup__death_button" in:fly={{ y: 30, duration: 400, delay: 400 }} on:click={() => executeClient ('client:OnHospitalDialogCallback', 2)}>{translateText('popups', 'Вызвать EMS (10 мин)')}</div>
        <div class="popup__death_button" in:fly={{ y: 30, duration: 400, delay: 600 }} on:click={() => executeClient ('client:OnHospitalDialogCallback', 3)}>{translateText('popups', 'В больницу (1 мин)')}</div>
    </div>
    <span class="hud__icon-skull popup__death_icon"/>
</div>
