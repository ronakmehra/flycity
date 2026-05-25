<script>
    import { translateText } from 'lang'
    import Header from './header.svelte'
    import HomeButton from './homebutton.svelte'
    import { executeClient } from "api/rage";
    import { addListernEvent } from 'api/functions';
    import { currentPage } from '../stores';

    let darknetList = [];
    let isLoading = true;

    const loadDarknet = () => {
        executeClient("client.phone.darknet.load");
    };

    loadDarknet();

    const onUpdate = (json) => {
        try {
            darknetList = JSON.parse(json);
        } catch (e) {
            darknetList = [];
        }
        isLoading = false;
    };

    addListernEvent("phone.darknet.init", onUpdate);

    const onBuy = (item) => {
        executeClient("client.phone.darknet.buy", JSON.stringify(item));
    };
</script>

<Header title={translateText('player1', 'Даркнет')} />

<div class="darknet">
    {#if isLoading}
        <div class="darknet__loading">{translateText('player1', 'Загрузка...')}</div>
    {:else if darknetList.length === 0}
        <div class="darknet__empty">{translateText('player1', 'Нет доступных предложений')}</div>
    {:else}
        <div class="darknet__list">
            {#each darknetList as item, index}
                <div class="darknet__item" on:click={() => onBuy(item)}>
                    <div class="darknet__item-name">{item.name || translateText('player1', 'Предмет')}</div>
                    <div class="darknet__item-price">${item.price || 0}</div>
                    {#if item.description}
                        <div class="darknet__item-desc">{item.description}</div>
                    {/if}
                </div>
            {/each}
        </div>
    {/if}
</div>

<HomeButton />

<style lang="sass">
    .darknet
        padding: 10px
        height: 100%
        overflow-y: auto

        &__loading, &__empty
            display: flex
            justify-content: center
            align-items: center
            height: 200px
            color: rgba(255, 255, 255, 0.5)
            font-size: 14px

        &__list
            display: flex
            flex-direction: column
            gap: 8px

        &__item
            background: rgba(255, 255, 255, 0.05)
            border-radius: 8px
            padding: 12px
            cursor: pointer
            transition: background 0.2s

            &:hover
                background: rgba(255, 255, 255, 0.1)

            &-name
                font-size: 14px
                font-weight: 600
                color: #fff

            &-price
                font-size: 12px
                color: #4caf50
                margin-top: 4px

            &-desc
                font-size: 11px
                color: rgba(255, 255, 255, 0.5)
                margin-top: 4px
</style>
