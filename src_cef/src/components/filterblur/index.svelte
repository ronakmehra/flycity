<script>
    import './main.sass';

    export let active = true;
    export let intensity = 1;
</script>

<div class="background__filter" class:active style="--blur-intensity: {intensity}">
</div>