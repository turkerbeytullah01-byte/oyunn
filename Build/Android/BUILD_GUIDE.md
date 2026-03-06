# Android Build Guide - Project Aegis: Drone Dominion

## Prerequisites

### Required Software

1. **Unity 2022.3 LTS** or later
2. **Android Build Support** module
3. **Android SDK & NDK** (included with Unity Hub)
4. **JDK** (OpenJDK included with Unity)
5. **Gradle** (included with Unity)

### Unity Modules Required

- Android Build Support
- Android SDK & NDK Tools
- OpenJDK

## Build Configuration

### Step 1: Platform Switch

1. Open Unity Editor
2. Go to `File → Build Settings`
3. Select `Android` platform
4. Click `Switch Platform`

### Step 2: Player Settings

Navigate to `Edit → Project Settings → Player → Android`

#### Identification
- **Company Name**: Project Aegis Studios
- **Product Name**: Project Aegis: Drone Dominion
- **Version**: 1.0.0
- **Bundle Version Code**: 1
- **Package Name**: com.projectaegis.dronedominion

#### Configuration
- **Scripting Backend**: IL2CPP
- **API Compatibility Level**: .NET Standard 2.1
- **Target Architectures**: ARM64 ✓ (uncheck ARMv7)
- **Target SDK Version**: API Level 34 (Android 14)
- **Minimum SDK Version**: API Level 26 (Android 8.0)

#### Optimization
- **Strip Engine Code**: ✓
- **Managed Stripping Level**: Medium
- **C++ Compiler Configuration**: Release
- **IL2CPP Code Generation**: Optimize Size

#### Publishing Settings
- **Build App Bundle (Google Play)**: ✓
- **Create symbols.zip**: Debugging symbols

### Step 3: Keystore Setup

#### Option A: Create New Keystore

1. In Player Settings → Android → Publishing Settings
2. Check `Custom Keystore`
3. Click `Create a new keystore`
4. Set password and save
5. Click `Add Key`
6. Set:
   - Alias: `projectaegis`
   - Password: (secure password)
   - Validity: 25 years

#### Option B: Use Existing Keystore

1. Place keystore in `Build/Android/keystore/projectaegis.keystore`
2. In Publishing Settings:
   - Keystore path: Select file
   - Keystore password: Enter password
   - Key alias: `projectaegis`
   - Key password: Enter password

### Step 4: Scene Setup

1. Open `File → Build Settings`
2. Click `Add Open Scenes` or drag scenes
3. Ensure correct build order:
   - MainMenuScene (if exists)
   - GameScene (main game)

## Building

### Method 1: Using Build Menu (Recommended)

```
Project Aegis → Build → Android → Build AAB Release
```

This will:
1. Configure all settings automatically
2. Build Android App Bundle (.aab)
3. Output to `Builds/Android/ProjectAegisDroneDominion_1.0.0.aab`

### Method 2: Using Build Settings Window

1. `File → Build Settings`
2. Ensure `Build App Bundle` is checked
3. Click `Build`
4. Select output location
5. Wait for build to complete

### Method 3: Command Line Build

```bash
# Navigate to Unity installation
cd "C:\Program Files\Unity\Hub\Editor\2022.3.x\Editor"

# Run build
Unity.exe -quit -batchmode -nographics \
  -projectPath "C:\Path\To\ProjectAegis" \
  -executeMethod ProjectAegis.Build.BuildScript.BuildAndroidAAB \
  -logFile "Builds/build.log"
```

## Build Output

### Release AAB

**Location**: `Builds/Android/ProjectAegisDroneDominion_1.0.0.aab`

**Contents**:
- Base module (universal)
- Configuration splits (by device)
- Language splits (if localization added)

### Debug APK (Optional)

```
Project Aegis → Build → Android → Build APK Debug
```

**Location**: `Builds/Android/ProjectAegisDroneDominion_DEBUG_1.0.0.apk`

## Build Validation

Before uploading to Google Play, verify:

```
Project Aegis → Build → Android → Validate Build
```

This checks:
- ✓ Platform is Android
- ✓ IL2CPP is enabled
- ✓ ARM64 architecture
- ✓ Keystore is configured
- ✓ Scenes are added
- ✓ SDK versions are correct

## Google Play Upload

### Step 1: Sign In

1. Go to [Google Play Console](https://play.google.com/console)
2. Sign in with developer account

### Step 2: Create App

1. Click `Create app`
2. App name: `Project Aegis: Drone Dominion`
3. Default language: English
4. App or game: Game
5. Free or paid: Free (or paid)

### Step 3: Upload AAB

1. Go to `Production` track
2. Click `Create new release`
3. Upload AAB file
4. Add release notes
5. Review and rollout

### App Signing

Google Play uses App Signing by Google Play:

1. Upload your signing keystore
2. Google Play will re-sign with their key
3. Keep your upload keystore safe
4. You need it for all future updates

## Troubleshooting

### Build Failures

**"Gradle build failed"**
```
Solution: Check internet connection, try again
Or: Delete Library/Gradle folder and rebuild
```

**"IL2CPP build failed"**
```
Solution: Check code compatibility with IL2CPP
Or: Update to latest Unity version
```

**"Keystore error"**
```
Solution: Verify keystore path and passwords
Or: Create new keystore
```

**"Out of memory"**
```
Solution: Increase Gradle heap size in gradleTemplate.properties
org.gradle.jvmargs=-Xmx6144m
```

### Performance Issues

**Large build size**
- Enable managed stripping
- Use Optimize Size for IL2CPP
- Remove unused assets
- Compress textures

**Slow build times**
- Use SSD for project
- Close other applications
- Incremental builds (debug)

## Build Checklist

Before release build:

- [ ] Switched to Android platform
- [ ] IL2CPP scripting backend
- [ ] ARM64 architecture only
- [ ] Release keystore configured
- [ ] Version code incremented
- [ ] All scenes added
- [ ] Tested on device
- [ ] Performance validated
- [ ] Memory usage checked
- [ ] Battery usage optimized

## Build Automation (CI/CD)

### GitHub Actions Example

```yaml
name: Android Build

on: [push]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      
      - name: Build Android
        uses: game-ci/unity-builder@v2
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
          UNITY_EMAIL: ${{ secrets.UNITY_EMAIL }}
          UNITY_PASSWORD: ${{ secrets.UNITY_PASSWORD }}
        with:
          targetPlatform: Android
          buildMethod: ProjectAegis.Build.BuildScript.BuildAndroidAAB
          
      - name: Upload AAB
        uses: actions/upload-artifact@v2
        with:
          name: android-aab
          path: Builds/Android/*.aab
```

## Additional Resources

- [Unity Android Build](https://docs.unity3d.com/Manual/android-BuildProcess.html)
- [Google Play Console](https://support.google.com/googleplay/android-developer)
- [Android App Bundles](https://developer.android.com/guide/app-bundle)
- [IL2CPP](https://docs.unity3d.com/Manual/IL2CPP.html)
