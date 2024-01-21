<script lang="ts">
  import { Button, NumberBox } from '$components';
  import logo from '$assets/svelte.png';

  const { randomNumber } = window.gluino.bindings.mainWindow;

  let min = 1;
  let max = 20;
  let random = 1;

  async function generateRandomNumber() {
    random = await randomNumber(min, max);
  }
</script>

<main>
  <div class="logo">
    <img src={logo} alt="Svelte Logo" />
    <h1>Random Number Generator</h1>
  </div>

  <div class="container">
    <div class="row input">
      <NumberBox label="Between" bind:value={min} width="70px" />
      <NumberBox label="and" bind:value={max} width="70px" />
    </div>

    <div class="row">
      <Button on:click={generateRandomNumber}>Generate Random Number</Button>
    </div>

    <div class="row result">
      <p>{random}</p>
    </div>
  </div>
</main>

<style lang="scss">
  main {
    display: flex;
    flex: 1;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 16px;

    .logo {
      display: flex;
      align-items: center;
      flex-direction: column;

      > img {
        height: 80px;
      }
    }

    .container {
      display: flex;
      flex-direction: column;
      gap: 4px;
      background-color: var(--card-bg-2);
      border: 1px solid var(--card-stroke);
      border-radius: var(--control-corner-radius);
      padding: 16px;

      .row {
        display: flex;
        gap: 5px;
        justify-content: center;

        &.input {
          gap: 3px;
        }

        &.result p {
          font-size: 30px;
        }
      }
    }
  }
</style>
