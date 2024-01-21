export default function uuidv4() {
  if (crypto.randomUUID) return crypto.randomUUID();

  const data = new Uint8Array(16);
  crypto.getRandomValues(data);
  data[6] = (data[6] & 0x0f) | 0x40;
  data[8] = (data[8] & 0x3f) | 0x80;
  const hex = Array.from(data, (byte) => byte.toString(16).padStart(2, '0')).join('');
  return `${hex.substring(0, 8)}-${hex.substring(8, 12)}-4${hex.substring(13, 16)}-${((data[15] & 0x3f) | 0x80)
    .toString(16)
    .padStart(2, '0')}${hex.substring(17, 20)}-${hex.substring(20, 32)}`;
}
