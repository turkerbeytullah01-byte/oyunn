# Project Aegis: Drone Dominion - Nasıl Kullanılır?

## 🎯 Bu Paket İçinde Ne Var?

Bu paket, **Project Aegis: Drone Dominion** oyununun tam kaynak kodunu ve yapılandırmasını içerir.

**Toplam:**
- 📄 **162+ dosya**
- 💾 **2.1 MB**
- 🎮 **Tam oyun altyapısı**

---

## 📦 İçindekiler

### 1. Oyun Script'leri (117 C# dosyası)
- ✅ **Core Systems**: GameManager, EventManager, SaveManager, TimeManager
- ✅ **Research System**: Araştırma sistemi, teknoloji ağacı
- ✅ **Idle System**: Offline ilerleme, üretim sistemi
- ✅ **Event System**: Dinamik olaylar (15-20 dk aralık)
- ✅ **Risk System**: Risk hesaplama, başarısızlık sonuçları
- ✅ **Contract System**: Kontratlar, teklif sistemi, AI rakipler
- ✅ **Save System**: JSON + PlayerPrefs kaydetme
- ✅ **UI System**: Arayüz yönetimi, popup'lar
- ✅ **Mobile System**: Touch input, optimizasyon, safe area
- ✅ **Debug Tools**: 15+ hata ayıklama aracı

### 2. Örnek Veriler (17 JSON)
- 5 Araştırma (Energy Systems)
- 3 Kontrat
- 6 Olay
- 3 Drone

### 3. Build Yapılandırmaları
- ✅ Android AAB build script'leri
- ✅ IL2CPP yapılandırması
- ✅ ARM64 optimizasyonu
- ✅ GitHub Actions CI/CD

### 4. Dokümantasyon (15+ MD dosya)
- Proje özeti
- Build rehberleri
- Kurulum talimatları
- API dokümantasyonu

---

## 🚀 Hızlı Başlangıç (3 Adım)

### ADIM 1: Unity Projesi Oluşturun

1. **Unity Hub**'ı açın
2. **New Project** → **3D** şablonu seçin
3. İsim: `ProjectAegis`
4. **Create**

### ADIM 2: Dosyaları Kopyalayın

Bu dosyalardan Unity projenize kopyalayın:

```
📁 KOPYALANACAKLAR:

1. Assets/Scripts/         → Unity/Assets/Scripts/
2. Assets/ScriptableObjects/ → Unity/Assets/ScriptableObjects/
3. Assets/link.xml         → Unity/Assets/link.xml
4. Build/                  → Unity/Build/
5. .github/                → Unity/.github/
```

**Nasıl kopyalanır:**
- Windows: Klasörleri sürükleyip bırakın
- VEYA kopyalayıp yapıştırın (Ctrl+C, Ctrl+V)

### ADIM 3: Build Alın

Unity Editör'de:

```
Project Aegis → Build → Android → Build AAB Release
```

**VEYA**

```
File → Build Settings → Android → Build
```

---

## 📱 Telefona Kurulum

### APK Olarak Kurulum (Test)

Build aldıktan sonra:

1. **APK dosyasını** telefonunuza kopyalayın
2. Telefonda **"Bilinmeyen kaynaklar"** iznini açın:
   - Ayarlar → Güvenlik → Bilinmeyen kaynaklar
3. APK dosyasına dokunun ve kurun

### Google Play'den Yayınlama

1. [Google Play Console](https://play.google.com/console) hesabı oluşturun
2. **AAB dosyasını** yükleyin
3. Uygulama bilgilerini doldurun
4. Yayınlayın

---

## 🔧 Geliştirme ve Güncelleme

### Kod Değiştirme

1. Unity'de `Assets/Scripts/` klasörünü açın
2. İstediğiniz script'i düzenleyin
3. Kaydedin (Ctrl+S)
4. Test edin (Play butonu)

### Yeni Özellik Ekleme

Örnek: Yeni araştırma ekleme

1. `Assets/ScriptableObjects/` sağ tık
2. `Create → Project Aegis → Research Data`
3. Bilgileri doldurun
4. Tech Tree'e ekleyin

### Versiyon Güncelleme

1. `Edit → Project Settings → Player`
2. **Version**: `1.0.1` (artırın)
3. **Bundle Version Code**: `2` (artırın)
4. Yeni build alın

---

## ☁️ GitHub ile Otomatik Build (Önerilen)

### Neden?
- ✅ Her kod değişikliğinde otomatik build
- ✅ Ücretsiz
- ✅ Versiyonlama
- ✅ Başka bilgisayar gerekmez

### Nasıl?

1. **GitHub hesabı** oluşturun (github.com)
2. **Yeni repo** oluşturun (ProjectAegis-DroneDominion)
3. **Dosyaları yükleyin** (GitHub Desktop veya web)
4. **GitHub Actions** otomatik build alacak
5. **Actions** sekmesinden indirin

Detaylı rehber: `GITHUB_BUILD_GUIDE.md`

---

## 📚 Önemli Dosyalar

| Dosya | Ne İşe Yarar? |
|-------|---------------|
| `PROJECT_SETUP_GUIDE.md` | Unity kurulum rehberi |
| `GITHUB_BUILD_GUIDE.md` | GitHub build rehberi |
| `Build/Android/BUILD_GUIDE.md` | Android build rehberi |
| `Build/Android/ANDROID_BUILD_SUMMARY.md` | Build özeti |
| `PROJECT_SUMMARY.md` | Proje özeti (Türkçe) |
| `FINAL_DELIVERY.md` | Teslimat dokümanı |

---

## 🆘 Yardım

### Sık Sorulan Sorular

**Q: Unity bilmiyorum, ne yapmalıyım?**
A: Unity Learn (learn.unity.com) ücretsiz eğitimler sunar. Temel bilgiler 1-2 günde öğrenilir.

**Q: Build alırken hata alıyorum**
A: `Build/Android/BUILD_GUIDE.md` dosyasındaki sorun giderme bölümüne bakın.

**Q: Telefonda çalışmıyor**
A: Minimum Android 8.0 (API 26) gerekir. Daha eski telefonlarda çalışmayabilir.

**Q: Kodları nasıl değiştiririm?**
A: Unity Editör'de `Assets/Scripts/` klasöründen istediğiniz script'i açıp düzenleyin.

---

## 🎮 Oyun Özellikleri

### MVP (Minimum Viable Product)

✅ **5 Araştırma** (Energy Systems)  
✅ **3 Kontrat** (Farklı zorluklar)  
✅ **6 Olay** (Rastgele tetiklenen)  
✅ **3 Drone** (Farklı sınıflar)  
✅ **Offline İlerleme** (4 saate kadar)  
✅ **Risk Sistemi** (Stratejik kararlar)  
✅ **Teklif Sistemi** (AI rakipler)  

### Gelecek Güncellemeler İçin

- Yeni teknoloji kategorileri (AI, Materials, vb.)
- Daha fazla kontrat ve drone
- Çok oyunculu özellikler
- Gelişmiş grafikler
- Ses efektleri ve müzik

---

## 📞 İletişim ve Destek

Sorularınız veya önerileriniz varsa:
- Dokümantasyon dosyalarını okuyun
- Unity Forums (forum.unity.com)
- Stack Overflow (stackoverflow.com)

---

## 📝 Lisans

Bu proje özel kullanım içindir. Tüm hakları saklıdır.

---

## 🎉 Tebrikler!

Artık **Project Aegis: Drone Dominion** oyununu:
- ✅ Geliştirebilirsiniz
- ✅ Build alabilirsiniz
- ✅ Telefona kurabilirsiniz
- ✅ Google Play'de yayınlayabilirsiniz

**İyi eğlenceler! 🚀**

---

**Son Güncelleme**: 2024  
**Versiyon**: 1.0.0  
**Unity**: 2022.3 LTS  
**Platform**: Android
