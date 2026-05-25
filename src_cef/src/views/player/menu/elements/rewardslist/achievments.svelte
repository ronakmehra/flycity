<script>
    import { translateText } from 'lang'
    export let onSetLoad;
    import { executeClient } from "api/rage";
    import { addListernEvent } from 'api/functions';

    executeClient("client.rewardslist.achievments.load");

    let achievmentsList = [];

    onSetLoad(true);
    const updateLoad = (json) => {
        onSetLoad(false);
        achievmentsList = JSON.parse(json);
    };

    addListernEvent("rewardList.achievments.init", updateLoad);

    const onClaim = (item) => {
        executeClient("client.rewardslist.achievments.claim", item.id);
    };

    const getProgress = (item) => {
        if (!item.max || item.max === 0) return 0;
        return Math.min(100, Math.round((item.current / item.max) * 100));
    };
</script>

<div class="rewards">
    <div class="rewards__title">{translateText('player1', 'Достижения')}</div>
    <div class="rewards__subtitle">{translateText('player1', 'Выполняйте задания и получайте награды за достижения.')}</div>
    <div class="rewards__elements">
        {#each achievmentsList as item}
            <div class="achievment" class:completed={item.completed}>
                <div class="achievment__info">
                    <div class="achievment__name">{item.name || translateText('player1', 'Достижение')}</div>
                    <div class="achievment__desc">{item.description || ''}</div>
                </div>
                <div class="achievment__progress">
                    <div class="achievment__bar">
                        <div class="achievment__fill" style="width: {getProgress(item)}%"></div>
                    </div>
                    <div class="achievment__count">{item.current || 0}/{item.max || 0}</div>
                </div>
                {#if item.completed && !item.claimed}
                    <div class="achievment__claim" on:click={() => onClaim(item)}>
                        {translateText('player1', 'Забрать')}
                    </div>
                {:else if item.claimed}
                    <div class="achievment__claimed">{translateText('player1', 'Получено')}</div>
                {/if}
            </div>
        {/each}
    </div>
</div>
