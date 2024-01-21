<script lang="ts">
  import uuidv4 from '$lib/uuid';

  export let label: string = '';
  export let value = 0;
  export let min: number | null = null;
  export let max: number | null = null;
  export let width: string | null = null;

  const uuid = uuidv4();
</script>

<div class="number-box">
  <label for={uuid}>{label}</label>
  <input id={uuid} type="number" bind:value {min} {max} style:width />
</div>

<style lang="scss">
  .number-box {
    font-size: var(--control-font-size);

    > label {
      user-select: none;
    }

    > input[type='number'] {
      appearance: none;
      outline: none;
      border: none;
      font-family: inherit;
      color: inherit;
      line-height: 0;

      font-size: var(--control-font-size);
      height: var(--control-height);
      padding: var(--control-padding);
      border: 1px solid var(--control-stroke-1);
      border-radius: var(--control-corner-radius);
      color: var(--text-input);
      background-color: var(--control-fill-1);
      transition: 86ms ease background-color;

      &:hover {
        background-color: var(--control-fill-2);
      }
    }
  }
</style>
