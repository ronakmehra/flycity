<script>
    import { translateText } from 'lang'
    import { executeClient } from "api/rage";
    import { addListernEvent } from 'api/functions';

    let guidesList = [];
    let selectedGuide = null;
    let isLoading = true;

    executeClient("client.support.guides.load");

    const onUpdate = (json) => {
        try {
            guidesList = JSON.parse(json);
        } catch (e) {
            guidesList = [];
        }
        isLoading = false;
    };

    addListernEvent("support.guides.init", onUpdate);

    const selectGuide = (guide) => {
        selectedGuide = guide;
    };

    const goBack = () => {
        selectedGuide = null;
    };
</script>

<div class="support__background">
    <div class="guides">
        {#if isLoading}
            <div class="guides__loading">{translateText('player1', 'Загрузка...')}</div>
        {:else if selectedGuide}
            <div class="guides__detail">
                <div class="guides__back" on:click={goBack}>&larr; {translateText('player1', 'Назад')}</div>
                <div class="guides__detail-title">{selectedGuide.title}</div>
                <div class="guides__detail-content">{@html selectedGuide.content || ''}</div>
            </div>
        {:else}
            <div class="guides__title">{translateText('player1', 'Гайды')}</div>
            <div class="guides__list">
                {#each guidesList as guide}
                    <div class="guides__item" on:click={() => selectGuide(guide)}>
                        <div class="guides__item-title">{guide.title || ''}</div>
                        <div class="guides__item-desc">{guide.description || ''}</div>
                    </div>
                {/each}
                {#if guidesList.length === 0}
                    <div class="guides__empty">{translateText('player1', 'Гайды пока недоступны')}</div>
                {/if}
            </div>
        {/if}
    </div>
</div>
