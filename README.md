# 🎮 Unity Package - Persinus Keystore Manager  
![Persinus Keystore Manager Icon](Docs/icon.png)  
![Persinus Keystore Manager Screenshot](Docs/screenshot.png)  

A **Keystore Management Tool** for Unity.  
Easily check aliases, generate SHA1/SHA256 hashes, and quickly export for Android/iOS.  

---

## 🚀 Installation  

### 1. Via Unity Package Manager (UPM)  

1. Open **Unity** → **Window → Package Manager**  
2. Click **+** → **Add package from git URL…**  
3. Paste the following URL:  

```bash
https://github.com/<username>/<repo-name>.git
```  

👉 If the package is inside a subfolder `Packages/com.persinus.keystoremanager`, use:  

```bash
https://github.com/<username>/<repo-name>.git#subfolder=Packages/com.persinus.keystoremanager
```  

---

### 2. Download `.unitypackage` directly  

<a href="https://github.com/<username>/<repo-name>/releases/latest/download/PersinusKeystoreManager.unitypackage">
  <img src="https://img.shields.io/badge/⬇️_Download_.unitypackage-blue?style=for-the-badge&logo=unity" alt="Download Unitypackage">
</a>  

👉 After downloading, open Unity → **Assets → Import Package → Custom Package…** and select the file.  

---

## ✨ Features  

- Manage and view **keystore** details directly inside Unity Editor  
- Display SHA1 / SHA256 / MD5 fingerprints  
- Support manual alias input  
- Warning for certificates using **SHA1** (deprecated / insecure)  

---

## 📋 Requirements  

- Unity **2021.3 LTS** or newer  
- .NET Standard 2.1  

---

## 📄 License  

MIT License © 2025 Persinus  
