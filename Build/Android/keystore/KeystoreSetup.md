# Keystore Setup for Project Aegis: Drone Dominion

## Overview

This document explains how to set up the release keystore for Android builds.

## Creating a New Keystore

### Method 1: Using Unity Editor

1. Open Unity Editor
2. Go to `Edit → Project Settings → Player → Android → Publishing Settings`
3. Check `Custom Keystore`
4. Click `Create a new keystore`
5. Set keystore password (save this securely!)
6. Click `Add Key`
7. Fill in key details:
   - Alias: `projectaegis`
   - Password: (save this securely!)
   - Validity: 25 years
   - Organization: Your company name
8. Click `Create Key`

### Method 2: Using Command Line (keytool)

```bash
# Navigate to JDK bin directory
cd "C:\Program Files\Unity\Hub\Editor\2022.3.x\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK\bin"

# Create keystore
keytool -genkey -v \
  -keystore projectaegis.keystore \
  -alias projectaegis \
  -keyalg RSA \
  -keysize 2048 \
  -validity 9125 \
  -storepass YOUR_KEYSTORE_PASSWORD \
  -keypass YOUR_KEY_PASSWORD \
  -dname "CN=Project Aegis, OU=Development, O=Project Aegis Studios, L=Istanbul, ST=Istanbul, C=TR"
```

## Keystore Information

**File:** `projectaegis.keystore`
**Alias:** `projectaegis`
**Validity:** 25 years (9125 days)
**Algorithm:** RSA 2048-bit

## Important Security Notes

⚠️ **CRITICAL:** 
- Keep your keystore file secure
- Never commit keystore passwords to version control
- Backup your keystore in multiple secure locations
- Losing the keystore means you cannot update your app on Google Play

## Build Configuration

The build script expects the keystore at:
```
Build/Android/keystore/projectaegis.keystore
```

Update the passwords in `BuildScript.cs` before building:
```csharp
private const string KEYSTORE_PASSWORD = "YOUR_ACTUAL_KEYSTORE_PASSWORD";
private const string KEY_PASSWORD = "YOUR_ACTUAL_KEY_PASSWORD";
```

Or better, use environment variables:
```csharp
private static string KEYSTORE_PASSWORD => Environment.GetEnvironmentVariable("PA_KEYSTORE_PASSWORD");
private static string KEY_PASSWORD => Environment.GetEnvironmentVariable("PA_KEY_PASSWORD");
```

## Google Play Signing

For Google Play App Signing:

1. Generate upload keystore (as above)
2. Build AAB with upload keystore
3. Upload to Google Play Console
4. Google Play will re-sign with their key

## Verifying Keystore

```bash
# List keystore contents
keytool -list -v -keystore projectaegis.keystore

# Check certificate
keytool -export -rfc -keystore projectaegis.keystore -alias projectaegis -file certificate.pem
```

## Troubleshooting

### "Keystore file does not exist"
- Ensure keystore is at the correct path
- Check file permissions

### "Incorrect keystore password"
- Verify password is correct
- Check for special characters that might need escaping

### "Cannot recover key"
- Key password might be different from keystore password
- Verify key alias exists in keystore
