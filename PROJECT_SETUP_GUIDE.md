# Project Aegis - Unity Proje Kurulum Rehberi

## 📁 Proje Yapısı

Bu klasör Unity projeniz için gerekli tüm dosyaları içerir.

```
ProjectAegis/
├── Assets/                      # Unity Asset'leri
│   ├── Scripts/                 # C# Script'leri
│   │   ├── Core/               # Temel sistemler
│   │   ├── Systems/            # Oyun sistemleri
│   │   ├── UI/                 # Arayüz script'leri
│   │   ├── Data/               # Veri modelleri
│   │   ├── Debug/              # Hata ayıklama
│   │   ├── Mobile/             # Mobil script'ler
│   │   └── Utils/              # Yardımcılar
│   ├── ScriptableObjects/       # SO tanımları
│   ├── Prefabs/                 # Prefab'lar (boş)
│   ├── Scenes/                  # Sahneler (boş)
│   ├── Resources/               # Kaynaklar (boş)
│   └── link.xml                 # IL2CPP yapılandırması
│
├── Packages/                    # Unity Packages (manifest.json)
│
├── ProjectSettings/             # Unity proje ayarları
│   ├── ProjectVersion.txt       # Unity versiyonu
│   ├── EditorSettings.asset     # Editör ayarları
│   ├── GraphicsSettings.asset   # Grafik ayarları
│   ├── QualitySettings.asset    # Kalite ayarları
│   ├── TagManager.asset         # Tag ve layer'lar
│   ├── InputManager.asset       # Input ayarları
│   ├── TimeManager.asset        # Zaman ayarları
│   ├── AudioManager.asset       # Ses ayarları
│   ├── Physics2DSettings.asset  # 2D fizik
│   ├── PhysicsSettings.asset    # 3D fizik
│   ├── DynamicsManager.asset    # Dinamikler
│   ├── ClusterInputManager.asset
│   ├── NavMeshAreas.asset
│   ├── NetworkManager.asset
│   ├── PresetManager.asset
│   ├── VFXManager.asset
│   └── XRSettings.asset
│
├── Build/                       # Build yapılandırmaları
│   └── Android/                 # Android build dosyaları
│
├── .github/                     # GitHub Actions
│   └── workflows/               # CI/CD workflow'ları
│
├── ExampleData/                 # Örnek veriler (JSON)
├── Documentation/               # Proje dokümantasyonu
└── *.md                         # README dosyaları
```

---

## 🚀 Unity Projesini Oluşturma

### 1. Yeni Unity Projesi Oluşturun

1. Unity Hub'ı açın
2. `New Project` butonuna tıklayın
3. Şablon: `3D (URP)` veya `3D`
4. İsim: `ProjectAegis`
5. Konum: İstediğiniz klasör
6. `Create project` butonuna tıklayın

### 2. Dosyaları Kopyalayın

Unity projesi oluşturulduktan sonra:

```bash
# 1. Unity projesinin Assets klasörünü açın
# Örn: C:\Users\Kullanici\Unity Projects\ProjectAegis\Assets

# 2. Bu dosyalardaki Assets klasörünün içeriğini kopyalayın
# Kopyalanacaklar:
# - Scripts/
# - ScriptableObjects/
# - link.xml
```

### 3. ScriptableObject'leri Oluşturun

Unity Editör'de:

```
Assets → Create → Project Aegis
```

Menüsünden oluşturun:
- Research Data
- Contract Data
- Game Event Data
- Drone Data
- UI Theme

### 4. Örnek Verileri Yükleyin

`ExampleData/` klasöründeki JSON dosyalarını ScriptableObject'lere dönüştürün:

```csharp
// Unity Editör'de sağ tık menüsü
Assets → Project Aegis → Load Example Data
```

Veya manuel olarak:
1. Her JSON dosyasını açın
2. İçeriği kopyalayın
3. İlgili ScriptableObject'e yapıştırın

---

## 🎮 Sahne Kurulumu

### Ana Sahne (GameScene)

1. `File → New Scene` (Ctrl+N)
2. `Ctrl+S` ile kaydedin: `Assets/Scenes/GameScene.unity`

### Gerekli GameObject'ler

#### 1. GameInitializer
```
GameObject → Create Empty
Name: GameInitializer
```

**Component Ekle:**
```csharp
GameInitializer.cs
```

#### 2. Managers (Boş GameObject)
```
GameObject → Create Empty
Name: Managers
Parent: GameInitializer
```

#### 3. UI Canvas
```
GameObject → UI → Canvas
```

**Canvas Ayarları:**
- Render Mode: Screen Space - Overlay
- Canvas Scaler:
  - UI Scale Mode: Scale With Screen Size
  - Reference Resolution: 1920 x 1080
  - Screen Match Mode: Match Width Or Height
  - Match: 0.5

#### 4. EventSystem
```
GameObject → UI → Event System
```

#### 5. Main Camera
Varsayılan kamera ayarları yeterli.

#### 6. Directional Light
Varsayılan ışık ayarları yeterli.

---

## 📱 Android Build Ayarları

### 1. Platform Değiştirme

```
File → Build Settings
Platform: Android
Switch Platform
```

### 2. Player Settings

```
Edit → Project Settings → Player → Android
```

**Identification:**
- Company Name: `Project Aegis Studios`
- Product Name: `Project Aegis: Drone Dominion`
- Version: `1.0.0`
- Bundle Version Code: `1`
- Package Name: `com.projectaegis.dronedominion`

**Configuration:**
- Scripting Backend: `IL2CPP`
- API Compatibility Level: `.NET Standard 2.1`
- Target Architectures: `ARM64` (ARMv7'i kaldırın)

**Optimization:**
- Strip Engine Code: `✓`
- Managed Stripping Level: `Medium`

**Publishing Settings:**
- Build App Bundle (Google Play): `✓`
- Create symbols.zip: `✓`

### 3. Keystore Oluşturma

```
Edit → Project Settings → Player → Android → Publishing Settings
✓ Custom Keystore
Create a new keystore
```

Bilgileri doldurun:
- Keystore password
- Confirm password
- Save

```
Add Key
```

Bilgileri doldurun:
- Alias: `projectaegis`
- Password
- Validity: `25`
- Organization: `Project Aegis Studios`
- Create Key

---

## 🧪 Test Etme

### Editör'de Test

1. `GameScene`'yi açın
2. `Play` butonuna tıklayın (Ctrl+P)
3. GameInitializer'ın düzgün çalıştığını kontrol edin

### Android'de Test

#### USB ile Test
1. Telefonu USB ile bilgisayara bağlayın
2. USB Debugging'i açın (Telefon ayarlarından)
3. Unity'de: `File → Build Settings → Build And Run`

#### APK ile Test
1. `File → Build Settings → Build`
2. APK dosyasını telefona kopyalayın
3. Telefonda kurun (Bilinmeyen kaynaklara izin verin)

---

## 📦 Build Alma

### Unity Editör'den

```
Project Aegis → Build → Android → Build AAB Release
```

VEYA

```
File → Build Settings → Build
```

### GitHub Actions ile

1. Dosyaları GitHub'a push edin
2. Otomatik build başlayacak
3. Actions sekmesinden indirin

---

## 🔄 Güncelleme Yapma

### Versiyon Artırma

1. `Edit → Project Settings → Player`
2. Version: `1.0.1` (örneğin)
3. Bundle Version Code: `2` (artırın)
4. Build alın

### Kod Değişikliği

1. Script'leri düzenleyin
2. Kaydedin (Ctrl+S)
3. Test edin
4. GitHub'a push edin (varsa)
5. Yeni build alın

---

## 🆘 Sık Karşılaşılan Sorunlar

### "Script errors"
- Script'lerin derlendiğinden emin olun
- Hata konsolunu kontrol edin (Ctrl+Shift+C)

### "Missing references"
- ScriptableObject'lerin oluşturulduğundan emin olun
- Referansların doğru atandığını kontrol edin

### "Build failed"
- Android Build Support modülü kurulu mu?
- SDK ve NDK yolları doğru mu?
- Keystore şifreleri doğru mu?

---

## 📚 Önemli Dosyalar

| Dosya | Açıklama |
|-------|----------|
| `Assets/Scripts/` | Tüm C# kodları |
| `Assets/ScriptableObjects/` | Oyun verileri |
| `Assets/link.xml` | IL2CPP ayarları |
| `ProjectSettings/` | Unity ayarları |
| `Build/Android/` | Build yapılandırmaları |
| `.github/workflows/` | CI/CD ayarları |

---

## 🎯 Sonraki Adımlar

1. ✅ Unity projesi oluşturun
2. ✅ Dosyaları kopyalayın
3. ✅ ScriptableObject'leri oluşturun
4. ✅ Sahneyi kurun
5. ✅ Android ayarlarını yapın
6. ✅ Test edin
7. ✅ Build alın
8. ✅ Telefona kurun

---

**Hazır mısınız? Unity'i açın ve başlayın! 🎮**
