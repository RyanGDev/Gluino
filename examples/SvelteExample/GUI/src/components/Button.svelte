<button on:click>
  <slot />
</button>

<style>
  button {
    appearance: none;
    outline: none;
    border: none;
    user-select: none;
    font-family: inherit;
    color: inherit;
    line-height: 0;

    font-size: var(--control-font-size);
    height: var(--control-height);
    padding: var(--control-padding);
    border: 1px solid;
    border-radius: var(--control-corner-radius);
    border-color: var(--control-border);
    color: var(--text-input);
    background-color: var(--control-fill-1);
    transition: 86ms ease background-color;

    &:hover {
      background-color: var(--control-fill-2);
    }

    &:active {
      background-color: var(--control-fill-3);
    }
  }
</style>
