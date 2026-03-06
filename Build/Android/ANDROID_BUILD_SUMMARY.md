# Android Build Summary - Project Aegis: Drone Dominion

## 📱 Android Yapılandırması Tamamlandı

Bu dosya, Unity projenizin Android App Bundle (AAB) formatında build edilmesi için gerekli tüm yapılandırmaları içerir.

---

## ✅ Oluşturulan Dosyalar

### Build Script'leri (Editor)
| Dosya | Açıklama |
|-------|----------|
| `Build/Android/Editor/BuildScript.cs` | Unity Editor build script'leri |

### Android Yapılandırma Dosyaları
| Dosya | Açıklama |
|-------|----------|
| `Build/Android/AndroidManifest.xml` | Android manifest (API 26-34) |
| `Build/Android/gradleTemplate.properties` | Gradle yapılandırması |
| `Build/Android/IL2CPP_Configuration.md` | IL2CPP ayarları dokümantasyonu |
| `Build/Android/BUILD_GUIDE.md` | Detaylı build rehberi |
| `Build/Android/keystore/KeystoreSetup.md` | Keystore kurulum rehberi |

### Mobile Script'leri
| Dosya | Açıklama |
|-------|----------|
| `Scripts/Mobile/MobileInputManager.cs` | Touch input yönetimi |
| `Scripts/Mobile/MobileOptimizationManager.cs` | Performans optimizasyonu |
| `Scripts/Mobile/SafeAreaHandler.cs` | Notch ve güvenli alan |

### IL2CPP Yapılandırması
| Dosya | Açıklama |
|-------|----------|
| `Assets/link.xml` | IL2CPP stripping koruma listesi |

---

## 🔧 Build Ayarları

### Scripting Backend
```csharp
Backend: IL2CPP
C++ Compiler: Release
Code Generation: Optimize Size
```

### Mimarlık
```csharp
Target: ARM64 only (ARMv7 unchecked)
Graphics API: OpenGL ES 3.0
```

### SDK Seviyeleri
```
Minimum SDK: API 26 (Android 8.0)
Target SDK: API 34 (Android 14)
```

### Optimizasyonlar
```csharp
Strip Engine Code: Enabled
Managed Stripping: Medium
IL2CPP Optimize: Size
Architecture: ARM64
```

---

## 📦 Build Çıktısı

### Release AAB
- **Format**: Android App Bundle (.aab)
- **Konum**: `Builds/Android/ProjectAegisDroneDominion_1.0.0.aab`
- **İmza**: Release keystore ile imzalı
- **Optimizasyon**: IL2CPP + ARM64

### Debug APK (Opsiyonel)
- **Format**: Android Package (.apk)
- **Konum**: `Builds/Android/ProjectAegisDroneDominion_DEBUG_1.0.0.apk`
- **İmza**: Debug keystore
- **Özellikler**: Development build, debugging enabled

---

## 🚀 Build Adımları

### 1. Unity Editor'de Build

```
Project Aegis → Build → Android → Build AAB Release
```

Bu menü otomatik olarak:
- Android platformuna geçer
- IL2CPP'yi etkinleştirir
- ARM64 mimarisini seçer
- Keystore'u yapılandırır
- Release build oluşturur

### 2. Build Settings Penceresi

```
File → Build Settings → Android → Build
```

### 3. Komut Satırı Build

```bash
Unity.exe -quit -batchmode \
  -projectPath "C:\Path\To\ProjectAegis" \
  -executeMethod ProjectAegis.Build.BuildScript.BuildAndroidAAB
```

---

## 📋 Build Öncesi Kontrol Listesi

- [ ] Android platformuna geçildi
- [ ] Tüm sahneler Build Settings'e eklendi
- [ ] Keystore oluşturuldu/yapılandırıldı
- [ ] Versiyon kodu artırıldı
- [ ] `Build/Android/keystore/` dizini oluşturuldu
- [ ] Keystore dosyası `projectaegis.keystore` olarak kopyalandı
- [ ] BuildScript.cs'de şifreler güncellendi (veya ortam değişkenleri ayarlandı)

---

## 🔐 Keystore Yapılandırması

### Keystore Oluşturma

```bash
keytool -genkey -v \
  -keystore projectaegis.keystore \
  -alias projectaegis \
  -keyalg RSA \
  -keysize 2048 \
  -validity 9125
```

### BuildScript.cs'de Şifreleri Güncelleme

```csharp
private const string KEYSTORE_PASSWORD = "YOUR_KEYSTORE_PASSWORD";
private const string KEY_PASSWORD = "YOUR_KEY_PASSWORD";
```

### VEYA Ortam Değişkenleri Kullanma

```csharp
private static string KEYSTORE_PASSWORD => 
    Environment.GetEnvironmentVariable("PA_KEYSTORE_PASSWORD");
```

---

## 🎯 Google Play Yükleme

### 1. Google Play Console
- [Google Play Console](https://play.google.com/console) adresine gidin
- Geliştirici hesabı ile giriş yapın

### 2. Uygulama Oluşturma
- `Create app` butonuna tıklayın
- App name: `Project Aegis: Drone Dominion`
- Default language: English
- App or game: Game
- Free or paid: Free

### 3. AAB Yükleme
1. `Production` track'e gidin
2. `Create new release` butonuna tıklayın
3. Oluşturulan `.aab` dosyasını yükleyin
4. Release notes ekleyin
5. Review and rollout

### 4. App Signing
Google Play App Signing kullanır:
- Siz upload keystore ile imzalarsınız
- Google Play kendi key'iyle yeniden imzalar
- Upload keystore'unuzu saklayın (güncellemeler için gerekli)

---

## 🛠 Sorun Giderme

### "Gradle build failed"
```
Çözüm: İnternet bağlantısını kontrol edin
Veya: Library/Gradle klasörünü silip yeniden deneyin
```

### "IL2CPP build failed"
```
Çözüm: Kodun IL2CPP uyumluluğunu kontrol edin
Veya: Unity'i en son sürüme güncelleyin
```

### "Keystore error"
```
Çözüm: Keystore yolunu ve şifreleri doğrulayın
Veya: Yeni keystore oluşturun
```

### "Out of memory"
```
Çözüm: Gradle heap boyutunu artırın
gradleTemplate.properties:
org.gradle.jvmargs=-Xmx6144m
```

---

## 📊 Build Boyutu Optimizasyonu

Mevcut yapılandırma ile beklenen boyut:
- **AAB**: ~50-100 MB (oyun içeriğine bağlı)
- **İndirilen cihaz başına**: ~30-60 MB (split APK)

Optimizasyon önerileri:
1. Texture compression kullanın (ETC2, ASTC)
2. Audio compression (Vorbis, MP3)
3. AssetBundle'lar ile dinamik içerik
4. Managed Stripping Level: Medium (veya High)

---

## 🔍 Build Doğrulama

Build sonrası kontrol:

```
Project Aegis → Build → Android → Validate Build
```

Bu komut şunları kontrol eder:
- ✓ Platform Android
- ✓ IL2CPP etkin
- ✓ ARM64 mimarisi
- ✓ Keystore yapılandırılmış
- ✓ Sahneler eklenmiş
- ✓ SDK versiyonları doğru

---

## 📱 Mobil Özellikler

### Touch Input
- Tek dokunma (tap)
- Çift dokunma (double tap)
- Kaydırma (swipe)
- Sürükleme (drag)
- İki parmak yakınlaştırma (pinch zoom)

### Optimizasyonlar
- Otomatik kalite algılama
- FPS sınırlama (30 FPS)
- Arka planda düşük FPS (5 FPS)
- Bellek yönetimi
- Pil optimizasyonu

### Güvenli Alan
- Notch desteği
- Yuvarlak köşe desteği
- Güvenli alan padding

---

## 📚 Önemli Dokümanlar

| Dosya | Konum |
|-------|-------|
| Build Rehberi | `Build/Android/BUILD_GUIDE.md` |
| IL2CPP Yapılandırması | `Build/Android/IL2CPP_Configuration.md` |
| Keystore Kurulumu | `Build/Android/keystore/KeystoreSetup.md` |
| Link.xml | `Assets/link.xml` |

---

## ⚠️ Önemli Notlar

1. **Keystore Güvenliği**: Keystore dosyanızı ve şifrelerinizi güvenli bir yerde saklayın. Kaybederseniz uygulamayı güncelleyemezsiniz.

2. **Versiyon Kodu**: Her release'de `bundleVersionCode`'u artırın.

3. **Test**: Release build'i yayınlamadan önce gerçek cihazlarda test edin.

4. **Google Play**: AAB formatı zorunludur (APK kabul edilmez).

5. **64-bit**: ARM64 desteği zorunludur (Google Play politikası).

---

## 🎉 Sonuç

Android build yapılandırmanız tamamlandı! 

Build oluşturmak için:
```
Project Aegis → Build → Android → Build AAB Release
```

**Not**: Gerçek bir Unity editörü ve Android SDK gereklidir. Bu dosyalar build yapılandırmasını sağlar ancak build işlemi Unity editöründe gerçekleştirilmelidir.

---

**Versiyon**: 1.0.0  
**Unity**: 2022.3 LTS  
**Platform**: Android  
**Build Format**: AAB (Android App Bundle)  
**Architecture**: ARM64  
**Scripting Backend**: IL2CPP
