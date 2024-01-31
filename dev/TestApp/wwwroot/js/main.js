const counter = document.getElementById('counter');

const { sendMessage, addListener } = window.gluino;
const { test, testAsync, openChildWindow } = window.gluino.bindings.testWindow;

function updateCounter() {
  const count = parseInt(counter.innerText);
  counter.innerText = count + 1;

  sendMessage(`Counter: ${count + 1}`);

  console.log(window.gluino);
}

async function testBind() {
  const result = await test('this is arg 1', 'this is arg 2');
  console.log(result);
}

async function testBindAsync() {
  const result = await testAsync('this is arg 1', 'this is arg 2');
  console.log(result);
}

async function openChild() {
  await openChildWindow();
}
