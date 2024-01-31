const gluino = window.gluino;
const { closeWindow } = gluino.bindings.childWindow;

async function closeWnd() {
  await closeWindow();
  console.log('Called closeWindow from childWindow');
}
