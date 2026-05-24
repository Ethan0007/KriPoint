const ALGO = "AES-CBC";

function toBase64(buffer) {
  return btoa(String.fromCharCode(...new Uint8Array(buffer)));
}

function fromBase64(b64) {
  return Uint8Array.from(atob(b64), (c) => c.charCodeAt(0));
}

async function importKey(base64Key) {
  return crypto.subtle.importKey(
    "raw",
    fromBase64(base64Key),
    { name: ALGO },
    false,
    ["encrypt", "decrypt"]
  );
}

export async function encrypt(value, base64Key) {
  const key = await importKey(base64Key);
  const iv = crypto.getRandomValues(new Uint8Array(16));
  const plain = new TextEncoder().encode(JSON.stringify(value));
  const cipher = await crypto.subtle.encrypt({ name: ALGO, iv }, key, plain);
  return { payload: toBase64(cipher), iv: toBase64(iv) };
}

export async function decrypt(payload, iv, base64Key) {
  const key = await importKey(base64Key);
  const plain = await crypto.subtle.decrypt(
    { name: ALGO, iv: fromBase64(iv) },
    key,
    fromBase64(payload)
  );
  return JSON.parse(new TextDecoder().decode(plain));
}

export async function generateBase64Key() {
  const key = await crypto.subtle.generateKey(
    { name: ALGO, length: 256 },
    true,
    ["encrypt", "decrypt"]
  );
  const raw = await crypto.subtle.exportKey("raw", key);
  return toBase64(raw);
}

export function isValidKey(base64Key) {
  try {
    return fromBase64(base64Key).length === 32;
  } catch {
    return false;
  }
}
