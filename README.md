# KriPoint

AES-256-CBC encrypted HTTP payloads for Axios + ASP.NET Core 8.
Sensitive values are never visible in DevTools, network tab, or proxy logs.

## Packages

| Platform | Name                               |
| -------- | ---------------------------------- |
| NPM      | `kripoint`                         |
| NuGet    | `KriPoint` / `KriPoint.AspNetCore` |

## Step 1 - Generate a shared key (run once)

```js
import { generateBase64Key } from 'kripoint'
console.log(await generateBase64Key())
```

Copy the output to:

- Front-end : .env -> VITE_KRIPOINT_KEY=<key>
- Back-end : appsettings.json -> KriPoint.AesKey: <same key>

## Step 2 - NPM

```bash
npm install kripoint axios
```

```js
import axios from 'axios'
import { attachKriPointInterceptor } from 'kripoint'

export const api = axios.create({ baseURL: 'https://api.yourapp.com' })

attachKriPointInterceptor(api, {
  key: import.meta.env.VITE_KRIPOINT_KEY,
})

await api.post('/api/users', { email: 'joever@corp.com', salary: 95000 })
```

## Step 3 - .NET 8

appsettings.json:

```json
{
  "KriPoint": {
    "AesKey": "YOUR_BASE64_32_BYTE_KEY",
    "RejectOnDecryptFailure": true,
    "ExcludePaths": ["/health", "/swagger"]
  }
}
```

Program.cs:

```csharp
builder.Services.AddKriPoint(builder.Configuration);
app.UseKriPoint();
app.MapControllers();
```

Controller (no changes needed):

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateUserRequest request)
{
    return Ok(new { request.Email, request.Salary });
}
```

## Wire format

```json
{ "payload": "<Base64 AES-256-CBC ciphertext>", "iv": "<Base64 16-byte IV>" }
```

| Property  | Value                                  |
| --------- | -------------------------------------- |
| Algorithm | AES-256-CBC                            |
| Key size  | 256 bit (32 bytes)                     |
| IV size   | 128 bit (16 bytes), random per request |
| Padding   | PKCS#7                                 |
