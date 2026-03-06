# GitHub Actions ile Otomatik Android Build

## 🎯 Hızlı Başlangıç

Bu rehber, GitHub Actions kullanarak Unity projenizi otomatik olarak Android AAB formatında build etmeyi açıklar.

## ✅ Adım Adım Kurulum

### 1. GitHub Hesabı Oluşturun
- [github.com](https://github.com) adresine gidin
- Ücretsiz hesap oluşturun

### 2. Proje Deposu (Repository) Oluşturun
1. GitHub'da `New Repository` butonuna tıklayın
2. İsim: `ProjectAegis-DroneDominion`
3. `Private` seçeneğini işaretleyin (gizli olsun)
4. `Create repository` butonuna tıklayın

### 3. Unity Dosyalarınızı Yükleyin

#### Seçenek A: GitHub Desktop (Kolay)
1. [GitHub Desktop](https://desktop.github.com/) indirin ve kurun
2. `File → Clone repository` seçin
3. Oluşturduğunuz repoyu seçin
4. Unity proje dosyalarınızı bu klasöre kopyalayın:
   ```
   ProjectAegis-DroneDominion/
   ├── Assets/
   ├── Packages/
   ├── ProjectSettings/
   ├── Scripts/
   └── ... (diğer dosyalar)
   ```
5. GitHub Desktop'ta commit mesajı yazın (örn: "Initial commit")
6. `Commit to main` ve `Push origin` butonlarına tıklayın

#### Seçenek B: Komut Satırı
```bash
cd /path/to/your/ProjectAegis

git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/KULLANICI_ADI/ProjectAegis-DroneDominion.git
git push -u origin main
```

### 4. Unity Lisansı Ayarlayın

GitHub Actions'ta Unity build almak için lisans gerekir:

#### Personal License (Ücretsiz)
1. [Unity ID](https://id.unity.com/) adresine gidin
2. Giriş yapın
3. `Licenses` bölümüne gidin
4. `Activate New License` → `Get a free personal license`
5. Lisansı indirin

#### GitHub Secrets Ekleme
1. GitHub repo'nuzda `Settings` sekmesine tıklayın
2. Sol menüden `Secrets and variables` → `Actions` seçin
3. `New repository secret` butonuna tıklayın
4. Aşağıdaki secret'ları ekleyin:

| Secret Adı | Değer |
|------------|-------|
| `UNITY_EMAIL` | Unity hesabınızın e-postası |
| `UNITY_PASSWORD` | Unity hesabınızın şifresi |
| `UNITY_LICENSE` | Unity lisans dosyanızın içeriği |

**UNITY_LICENSE nasıl alınır:**
```bash
# Windows PowerShell
cat "C:\ProgramData\Unity\Unity_lic.ulf" | base64

# veya lisans dosyasını açıp içeriği kopyalayın
```

### 5. Keystore Oluşturun ve Yükleyyin

#### Keystore Oluşturma
```bash
# JDK'nın olduğu dizine gidin
cd "C:\Program Files\Unity\Hub\Editor\2022.3.x\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK\bin"

# Keystore oluşturun
keytool -genkey -v \
  -keystore projectaegis.keystore \
  -alias projectaegis \
  -keyalg RSA \
  -keysize 2048 \
  -validity 9125 \
  -storepass SIFRENIZ \
  -keypass SIFRENIZ \
  -dname "CN=Project Aegis, OU=Dev, O=Project Aegis Studios, L=Istanbul, ST=Istanbul, C=TR"
```

#### Keystore'u Base64'e Dönüştürün
```bash
# Windows PowerShell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("projectaegis.keystore"))

# veya online base64 encoder kullanın
```

#### GitHub Secrets'e Ekleyin
| Secret Adı | Değer |
|------------|-------|
| `KEYSTORE_BASE64` | Base64'e dönüştürülmüş keystore |
| `KEYSTORE_PASSWORD` | Keystore şifreniz |
| `KEY_PASSWORD` | Key şifreniz (genellikle keystore şifresi ile aynı) |

### 6. GitHub Actions Workflow'unu Ekleyin

Projenizde `.github/workflows/android-build.yml` dosyasının olduğundan emin olun (ben oluşturdum).

### 7. İlk Build'i Başlatın

#### Otomatik Build (Her Push'ta)
1. Dosyalarınızı GitHub'a push edin
2. GitHub otomatik olarak build alacaktır
3. `Actions` sekmesinden build durumunu izleyin

#### Manuel Build
1. GitHub repo'nuzda `Actions` sekmesine tıklayın
2. `Android Build - Project Aegis` workflow'unu seçin
3. `Run workflow` butonuna tıklayın
4. Build tipini seçin (release/debug)
5. `Run workflow` butonuna tekrar tıklayın

### 8. Build'i İndirin

Build tamamlandığında:
1. `Actions` sekmesine gidin
2. Tamamlanan workflow'a tıklayın
3. En altta `Artifacts` bölümünde AAB dosyasını bulun
4. İndirin

---

## 📱 Telefona Kurulum

### Seçenek 1: Google Play Console (Önerilen)
1. [Google Play Console](https://play.google.com/console) adresine gidin
2. Geliştirici hesabı oluşturun (Tek seferlik $25 ücret)
3. Uygulama oluşturun
4. AAB dosyasını yükleyin
5. Yayınlayın

### Seçenek 2: Doğrudan Telefona Kurulum (Test)
AAB dosyasını APK'ya dönüştürmeniz gerekir:

#### Bundletool Kullanımı
```bash
# Bundletool'u indirin
wget https://github.com/google/bundletool/releases/download/1.15.6/bundletool-all-1.15.6.jar

# AAB'den APK seti oluşturun
java -jar bundletool-all-1.15.6.jar build-apks \
  --bundle=ProjectAegisDroneDominion_1.0.0.aab \
  --output=ProjectAegisDroneDominion.apks \
  --ks=projectaegis.keystore \
  --ks-key-alias=projectaegis \
  --ks-pass=pass:SIFRENIZ \
  --key-pass=pass:SIFRENIZ

# APK'ları cihaza yükleyin
java -jar bundletool-all-1.15.6.jar install-apks \
  --apks=ProjectAegisDroneDominion.apks \
  --device-id=CihazIDniz
```

#### Android Studio ile (Kolay)
1. Android Studio'yu açın
2. `Build → Analyze APK` seçin
3. AAB dosyanızı seçin
4. `Build → Build Bundle(s) / APK(s)`
5. Oluşan APK'yı telefona yükleyin

---

## 🔧 Build Yapılandırması

### Versiyon Güncelleme
`.github/workflows/android-build.yml` dosyasında:
```yaml
env:
  BUILD_VERSION: 1.0.1  # Yeni versiyon
  BUILD_VERSION_CODE: 2  # Artırılmalı
```

### Build Tipi
Manuel build sırasında seçebilirsiniz:
- **Release**: İmzalı, optimize edilmiş
- **Debug**: Geliştirme için, debugging bilgileri içerir

---

## 🛠 Sorun Giderme

### "License activation failed"
- UNITY_EMAIL, UNITY_PASSWORD ve UNITY_LICENSE doğru mu kontrol edin
- Unity lisansınız aktif mi kontrol edin

### "Keystore error"
- KEYSTORE_BASE64 doğru oluşturulmuş mu?
- Şifreler doğru mu?

### "Build failed"
- Actions sekmesinde detaylı logları kontrol edin
- Unity projenizde hata var mı kontrol edin

---

## 📊 Build Maliyeti

GitHub Actions ücretsiz kotası:
- **Public repo**: Sınırsız (ücretsiz)
- **Private repo**: Aylık 2000 dakika (yeterli)

Ekstra kullanım: $0.008/dakika

---

## 🎉 Sonuç

Bu yapılandırma ile:
- ✅ Her kod değişikliğinde otomatik build
- ✅ Herhangi bir bilgisayarda build alabilme
- ✅ Versiyonlama ve release yönetimi
- ✅ Ücretsiz (GitHub ücretsiz planı ile)

Build almak için tek yapmanız gereken kodunuzu GitHub'a push etmek!

---

**Yardım mı gerekiyor?**
- GitHub Actions dokümantasyonu: [docs.github.com/actions](https://docs.github.com/actions)
- Unity CI/CD rehberi: [game.ci](https://game.ci/)
