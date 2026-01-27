# Quick Setup & Connection Fix Guide

## ? Connection Issue FIXED!

The API connection issue has been resolved by correcting the port configuration.

---

## ?? What Was Fixed

### Problem
- WebInterface was trying to connect to `https://localhost:7001`
- WebService CORS was allowing `https://localhost:7000`
- **Both ports were incorrect!**

### Solution
Updated to match the actual launch settings:

| Service | Correct HTTPS Port | Correct HTTP Port |
|---------|-------------------|-------------------|
| **WebService (API)** | 7293 | 5123 |
| **WebInterface (Blazor)** | 7166 | 5132 |

---

## ?? How to Run (Correct Steps)

### 1?? Start the WebService API

```bash
cd WebService
dotnet run
```

**Expected output:**
```
Now listening on: https://localhost:7293
Now listening on: http://localhost:5123
```

### 2?? Start the WebInterface

Open a **new terminal**:

```bash
cd WebInterface
dotnet run
```

**Expected output:**
```
Now listening on: https://localhost:7166
Now listening on: http://localhost:5132
```

### 3?? Open Browser

Navigate to: **https://localhost:7166**

---

## ? Verify Connection

### Method 1: Home Page
1. Open https://localhost:7166
2. You should see the user list load
3. If you see "Error loading data", there's still a connection issue

### Method 2: API Test Page
1. Navigate to https://localhost:7166/api-test
2. Look at the "API Status" indicator
3. Should show: **? Connected** (green)
4. Click "Test Connection" to verify

### Method 3: Browser Console
1. Press F12 to open Developer Tools
2. Go to "Console" tab
3. Should see no CORS errors
4. Network tab should show successful API calls

---

## ?? Port Configuration Files

The ports are defined in these files:

### WebService Launch Settings
**File:** `WebService/Properties/launchSettings.json`
```json
"applicationUrl": "https://localhost:7293;http://localhost:5123"
```

### WebInterface Launch Settings
**File:** `WebInterface/Properties/launchSettings.json`
```json
"applicationUrl": "https://localhost:7166;http://localhost:5132"
```

### WebService CORS Policy
**File:** `WebService/Program.cs`
```csharp
.WithOrigins(
    "https://localhost:7166",  // WebInterface HTTPS
    "http://localhost:5132")   // WebInterface HTTP
```

### WebInterface API Client
**File:** `WebInterface/Program.cs`
```csharp
BaseAddress = new Uri("https://localhost:7293") // WebService API URL
```

---

## ?? Troubleshooting

### Issue: "API Status: ? Disconnected"

**Solution:**
1. Verify WebService is running: `https://localhost:7293`
2. Test API directly: Open https://localhost:7293/api/users/groups in browser
3. Check CORS configuration in WebService/Program.cs
4. Clear browser cache and reload

### Issue: CORS Error in Browser Console

**Error message:**
```
Access to XMLHttpRequest at 'https://localhost:7293/api/users' 
from origin 'https://localhost:7166' has been blocked by CORS policy
```

**Solution:**
1. Restart WebService after code changes
2. Verify CORS allows `https://localhost:7166`
3. Check that `app.UseCors("AllowBlazorWasm")` is called before `app.MapControllers()`

### Issue: Port Already in Use

**Error:**
```
Failed to bind to address https://127.0.0.1:7293: address already in use
```

**Solution:**
1. Find and stop the process using the port:
   ```powershell
   # Find process
   netstat -ano | findstr :7293
   
   # Kill process (replace PID with actual process ID)
   taskkill /PID <PID> /F
   ```

2. Or change the port in `launchSettings.json`

### Issue: Database Not Found

**Error:**
```
Microsoft.Data.Sqlite.SqliteException: SQLite Error 14: 'unable to open database file'
```

**Solution:**
1. The database is auto-created on first run
2. Make sure you're running from the correct directory
3. Check write permissions in WebService folder

---

## ?? Configuration Checklist

Before running, verify these settings:

- [ ] WebService/Program.cs CORS origins: `7166` and `5132`
- [ ] WebInterface/Program.cs HttpClient BaseAddress: `7293`
- [ ] WebService running on port `7293`
- [ ] WebInterface running on port `7166`
- [ ] No firewall blocking ports
- [ ] Browser allows HTTPS on localhost

---

## ?? Quick Test Commands

### Test API Directly
```bash
# PowerShell
Invoke-RestMethod https://localhost:7293/api/users/groups

# Or use browser
# https://localhost:7293/api/users/groups
```

### Expected Response
```json
[
  {"id":1,"name":"Admin"},
  {"id":2,"name":"Level 1"},
  {"id":3,"name":"Level 2"}
]
```

---

## ? Success Indicators

You'll know everything is working when:

1. ? WebService starts on port 7293
2. ? WebInterface starts on port 7166
3. ? No CORS errors in browser console
4. ? API Test page shows "? Connected"
5. ? User list loads on home page
6. ? Can add/edit/delete users

---

## ?? Still Having Issues?

If connection still fails after following these steps:

1. **Restart both services** (stop and start fresh)
2. **Clear browser cache** (Ctrl+Shift+Delete)
3. **Check firewall settings** (allow ports 7293 and 7166)
4. **Verify .NET 9 SDK** is installed: `dotnet --version`
5. **Run tests** to verify services work: `dotnet test`

---

**Last Updated:** 2026-01-27  
**Ports Confirmed:** WebService: 7293, WebInterface: 7166  
**Status:** ? Connection Issue Resolved
