# KriPoint &nbsp;<img src="https://raw.githubusercontent.com/Ethan0007/KriPoint/master/icon_kripoint.png" alt="KriPoint" width="30" valign="middle" />

AES-256-CBC encrypted HTTP payloads for Axios + ASP.NET Core 8.  
Sensitive values are never visible in DevTools, network tab, or proxy logs.


```
Browser DevTools (Network tab)          ASP.NET Core controller
────────────────────────────────        ────────────────────────────────
Request payload:                        [FromBody] CreateUserRequest req
{                                       {
  "payload": "Xk92mP3z...",               Email:  "joever@sample.com",
  "iv":      "rAnD0mIv=="                 Salary: 1000,
}                                         Role:   "admin"
                                        }
```

---

## Packages

| Platform | Name |
|---|---|
| NPM | `https://www.npmjs.com/package/kripoint` |
| NuGet | `https://www.nuget.org/packages/KriPoint/1.0.0` |

---

## Quickstart

### Step 1 — Generate a shared key

```js
import { generateBase64Key } from "kripoint";
console.log(await generateBase64Key());
```

Store the output in:
- **Front-end**: `.env` → `VITE_KRIPOINT_KEY=...`
- **Back-end**: `appsettings.json` → `KriPoint.AesKey`

---

### Step 2 — NPM

```bash
npm install kripoint axios
```

```js
import axios from "axios";
import { attachKriPointInterceptor } from "kripoint";

export const api = axios.create({ baseURL: "https://api.yourapp.com" });

attachKriPointInterceptor(api, {
  key: import.meta.env.VITE_KRIPOINT_KEY,
});
```

```js
await api.post("/api/users", { email: "alice@corp.com", salary: 95000 });
await api.put("/api/users/123", { role: "manager" });
```

#### Options

| Option | Type | Default | Description |
|---|---|---|---|
| `key` | `string` | required | Base64 AES-256 key |
| `decryptResponse` | `boolean` | `false` | Auto-decrypt `{ payload, iv }` responses |
| `excludePaths` | `string[]` | `[]` | URL substrings that bypass encryption |
| `onEncryptError` | `Function` | rethrow | Called on encryption failure |
| `onDecryptError` | `Function` | pass-through | Called on response decrypt failure |

#### Detach

```js
const handle = attachKriPointInterceptor(api, { key });
handle.detach();
```

---

### Step 3 — .NET 8

#### appsettings.json

```json
{
  "KriPoint": {
    "AesKey": "YOUR_BASE64_32_BYTE_KEY",
    "RejectOnDecryptFailure": true,
    "ExcludePaths": ["/health", "/swagger"]

    // NOTE:
    // Do not store sensitive values like AES keys directly in appsettings.json in production.
    // Recommended:
    // - Azure Key Vault
    // - Environment Variables
    // - Secret Manager (for local development only)
  }
}
```

#### Program.cs

```csharp
builder.Services.AddKriPoint(builder.Configuration);

app.UseKriPoint();
app.MapControllers();
```

#### Controllers — no changes needed

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateUserRequest request)
{
    return Ok(new { request.Email, request.Salary });
}
```

#### Advanced options

```csharp
builder.Services.AddKriPoint(opt =>
{
    opt.AesKey                 = Environment.GetEnvironmentVariable("KRIPOINT_KEY")!;
    opt.RejectOnDecryptFailure = true;
    opt.ExcludePaths           = ["/health", "/auth/refresh"];
});
```

---

## Wire format

```json
{
  "payload": "<Base64-encoded AES-256-CBC ciphertext>",
  "iv":      "<Base64-encoded 16-byte IV>"
}
```

| Property | Value |
|---|---|
| Algorithm | AES-256-CBC |
| Key size | 256 bit (32 bytes) |
| IV size | 128 bit (16 bytes), random per request |
| Padding | PKCS#7 |

---

## Project structure

```
kripoint/
├── npm-package/
│   ├── package.json
│   └── src/
│       ├── index.js
│       ├── index.d.ts
│       ├── crypto.js
│       └── axiosInterceptor.js
│
└── dotnet-library/
    ├── middleware/
    │   └── KriPointMiddleware.cs
    └── src/
        ├── Contracts.cs
        ├── KriPointEncryptionService.cs
        ├── KriPointExtensions.cs
        ├── DemoController.cs
        ├── Program.cs
        └── appsettings.json
```