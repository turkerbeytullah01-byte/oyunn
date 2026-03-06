# IL2CPP Configuration for Project Aegis: Drone Dominion

## Overview

IL2CPP (Intermediate Language To C++) is Unity's scripting backend that converts IL code to C++ for improved performance and platform compatibility.

## Configuration

### Unity Editor Settings

1. Open `Edit → Project Settings → Player`
2. Select Android platform
3. Under "Other Settings":
   - **Scripting Backend**: IL2CPP
   - **Target Architectures**: ARM64 (uncheck ARMv7 for release)
   - **C++ Compiler Configuration**: Release
   - **IL2CPP Code Generation**: Optimize Size

### IL2CPP Compiler Options

```csharp
// In BuildScript.cs
PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Release);
PlayerSettings.SetIl2CppCodeGeneration(BuildTargetGroup.Android, Il2CppCodeGeneration.OptimizeSize);
PlayerSettings.SetAdditionalIl2CppArgs("--emit-source-mapping");
```

## Build Size Optimization

### 1. Managed Stripping Level

```csharp
PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Medium);
```

Options:
- **Minimal**: No stripping (largest size)
- **Low**: Basic stripping
- **Medium**: Recommended (good balance)
- **High**: Aggressive stripping (smallest size, may cause issues)

### 2. Strip Engine Code

```csharp
PlayerSettings.stripEngineCode = true;
```

### 3. Link.xml (for preserving code)

Create `Assets/link.xml`:

```xml
<linker>
  <!-- Preserve entire assembly -->
  <assembly fullname="ProjectAegis.Core" preserve="all"/>
  
  <!-- Preserve specific types -->
  <assembly fullname="ProjectAegis.Systems">
    <type fullname="ProjectAegis.Systems.ResearchManager" preserve="all"/>
    <type fullname="ProjectAegis.Systems.ContractManager" preserve="all"/>
  </assembly>
  
  <!-- Preserve with methods -->
  <assembly fullname="ProjectAegis.Data">
    <type fullname="ProjectAegis.Data.PlayerData">
      <method name="Save"/>
      <method name="Load"/>
    </type>
  </assembly>
</linker>
```

## Architecture Configuration

### ARM64 Only (Recommended for Release)

```csharp
PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
```

Benefits:
- Smaller build size
- Better performance on modern devices
- Required for Google Play 64-bit requirement

### ARMv7 + ARM64 (For compatibility)

```csharp
PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
```

Use this if you need to support older devices (pre-2015).

## Performance Optimization

### 1. C++ Compiler Configuration

| Configuration | Use Case |
|--------------|----------|
| Debug | Development builds |
| Release | Production builds |
| Master | Maximum performance (longer build times) |

### 2. IL2CPP Code Generation

| Option | Description |
|--------|-------------|
| Optimize Speed | Faster runtime, larger binary |
| Optimize Size | Smaller binary, slightly slower |

For mobile games, **Optimize Size** is usually preferred.

## Troubleshooting

### Build Errors

**"IL2CPP error: failed to convert"**
- Check for incompatible plugins
- Update to latest Unity version
- Check IL2CPP compatible code

**"Out of memory during build"**
- Increase Gradle heap size: `org.gradle.jvmargs=-Xmx4096m`
- Close other applications
- Use 64-bit Unity editor

**"Stripping errors"**
- Add `link.xml` to preserve required types
- Reduce stripping level

### Runtime Errors

**"MissingMethodException"**
- Code was stripped incorrectly
- Add to `link.xml`

**"DllNotFoundException"**
- Native plugin not included
- Check plugin settings for Android

## Build Comparison

| Backend | Build Time | Runtime Speed | Binary Size |
|---------|-----------|---------------|-------------|
| Mono | Fast | Good | Medium |
| IL2CPP | Slow | Excellent | Large |

## Recommended Settings for Release

```csharp
// Scripting
PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Release);
PlayerSettings.SetIl2CppCodeGeneration(BuildTargetGroup.Android, Il2CppCodeGeneration.OptimizeSize);

// Architecture
PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

// Stripping
PlayerSettings.stripEngineCode = true;
PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Medium);
```

## Additional Resources

- [Unity IL2CPP Documentation](https://docs.unity3d.com/Manual/IL2CPP.html)
- [Managed Stripping](https://docs.unity3d.com/Manual/ManagedCodeStripping.html)
- [Android 64-bit Support](https://developer.android.com/distribute/best-practices/develop/64-bit)
