using UnityEditor;
using UnityEngine;
using System.IO;
using Dia = System.Diagnostics;

public class PersinusKeystoreManager : EditorWindow
{
    // User settings
    [SerializeField] private string javaBinPath = "";
    [SerializeField] private string keystorePath = "";
    [SerializeField] private string aliasName = "";

    // Hash values
    private string sha1Hash = "";
    private string sha256Hash = "";
    private string validityInfo = "";

    // Pref keys
    private const string PrefJavaBin = "Persinus_JavaBinPath";
    private const string PrefKeystorePath = "Persinus_KeystorePath";
    private const string PrefAliasName = "Persinus_KeystoreAliasName";

    // Scroll view
    private Vector2 scrollPos;

    // Toast
    private double toastEndTime = 0;
    private string toastMessage = "";
    private Color toastColor = Color.white;

    [MenuItem("Tools/Persinus/Keystore Manager")]
    public static void ShowWindow()
    {
        GetWindow<PersinusKeystoreManager>("Keystore Manager");
    }

    private void OnEnable()
    {
        javaBinPath = EditorPrefs.GetString(PrefJavaBin, "");
        keystorePath = EditorPrefs.GetString(PrefKeystorePath, "");
        aliasName = EditorPrefs.GetString(PrefAliasName, "");
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawHeader();
        GUILayout.Space(10);
        DrawInputFields();
        GUILayout.Space(10);

        if (GUILayout.Button("🔑 Generate SHA-1 & SHA-256", GUILayout.Height(35)))
        {
            ComputeHashes();
        }

        GUILayout.Space(10);
        DrawValidity();
        GUILayout.Space(10);
        DrawHashes();
        GUILayout.Space(10);

        if (!string.IsNullOrEmpty(sha1Hash) || !string.IsNullOrEmpty(sha256Hash))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📋 Copy All", GUILayout.Height(25)))
            {
                EditorGUIUtility.systemCopyBuffer =
                    $"SHA-1: {sha1Hash}\nSHA-256: {sha256Hash}";
                ShowToast("Copied all hashes!", Color.cyan);
            }
            if (GUILayout.Button("📝 Export Report", GUILayout.Height(25)))
            {
                ExportReport();
            }
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(20);
        DrawUsageGuide();

        EditorGUILayout.EndScrollView();

        DrawToast();
    }

    #region UI Sections
    private void DrawHeader()
    {
        EditorGUILayout.HelpBox(
            "🎮 Persinus Keystore Manager\nEasily extract SHA-1 and SHA-256 from your Android Keystore.\nAuthor: Persinus",
            MessageType.Info
        );
    }

    private void DrawInputFields()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("🔧 Keystore Info", EditorStyles.boldLabel);

        javaBinPath = EditorGUILayout.TextField("Java Bin Path", javaBinPath);
        if (GUILayout.Button("Browse Java Bin…", GUILayout.MaxWidth(160)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Java Bin Folder", "", "");
            if (!string.IsNullOrEmpty(path)) javaBinPath = path;
        }

        GUILayout.Space(5);

        keystorePath = EditorGUILayout.TextField("Keystore Path", keystorePath);
        if (GUILayout.Button("Browse Keystore…", GUILayout.MaxWidth(160)))
        {
            string path = EditorUtility.OpenFilePanel("Select Keystore", "", "keystore");
            if (!string.IsNullOrEmpty(path)) keystorePath = path;
        }

        GUILayout.Space(5);

        aliasName = EditorGUILayout.TextField("Alias Name", aliasName);

        EditorGUILayout.EndVertical();

        if (GUI.changed)
        {
            EditorPrefs.SetString(PrefJavaBin, javaBinPath);
            EditorPrefs.SetString(PrefKeystorePath, keystorePath);
            EditorPrefs.SetString(PrefAliasName, aliasName);
        }
    }

    private void DrawValidity()
    {
        if (!string.IsNullOrEmpty(validityInfo))
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("📅 Validity", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(validityInfo, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawHashes()
    {
        if (!string.IsNullOrEmpty(sha1Hash))
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("🔒 Hashes", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("SHA-1:", sha1Hash);
            if (GUILayout.Button("Copy", GUILayout.MaxWidth(60)))
            {
                EditorGUIUtility.systemCopyBuffer = sha1Hash;
                ShowToast("Copied SHA-1!", Color.cyan);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("SHA-256:", sha256Hash);
            if (GUILayout.Button("Copy", GUILayout.MaxWidth(60)))
            {
                EditorGUIUtility.systemCopyBuffer = sha256Hash;
                ShowToast("Copied SHA-256!", Color.cyan);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }

 private void DrawUsageGuide()
{
    EditorGUILayout.HelpBox(
        "📖 Persinus Keystore Manager\n\n" +
        "👉 Purpose:\n" +
        "- Retrieve SHA-1 and SHA-256 from Android Keystore.\n" +
        "- Required for configuring Firebase, Google Sign-In, Google Play Services, etc.\n\n" +

        "👉 Guide:\n" +
        "1. If you don’t have a Keystore yet:\n" +
        "   - In Unity: Edit > Project Settings > Player > Publishing Settings > Keystore.\n" +
        "   - Or create one using keytool command:\n" +
        "     keytool -genkey -v -keystore mykey.keystore -alias myalias -keyalg RSA -keysize 2048 -validity 10000\n\n" +
        "2. Select the correct Java JDK (the bin folder containing keytool.exe).\n" +
        "3. Choose your Keystore file.\n" +
        "4. Enter the Alias Name (must match the Alias in Player Settings).\n" +
        "5. Click Generate to get SHA-1 and SHA-256.\n" +
        "6. Copy the SHA values to configure Firebase, Google Play, etc.",
        MessageType.None
    );
}
    #endregion

    #region Hash Logic
    private void ComputeHashes()
    {
        if (string.IsNullOrEmpty(javaBinPath) || !Directory.Exists(javaBinPath))
        {
            ShowToast("❌ Invalid Java Bin path!", Color.red);
            return;
        }

        if (!File.Exists(keystorePath))
        {
            ShowToast("❌ Keystore file does not exist!", Color.red);
            return;
        }

        string keytoolExe = Path.Combine(javaBinPath, "keytool.exe");
        if (!File.Exists(keytoolExe))
        {
            ShowToast("❌ keytool.exe not found!", Color.red);
            return;
        }

        string keystorePass = PlayerSettings.Android.keystorePass;
        string aliasPass = PlayerSettings.Android.keyaliasPass;

        string args =
            $"-list -v -keystore \"{keystorePath}\" -alias \"{aliasName}\" -storepass \"{keystorePass}\"";

        if (!string.IsNullOrEmpty(aliasPass))
        {
            args += $" -keypass \"{aliasPass}\"";
        }

        try
        {
            var psi = new Dia.ProcessStartInfo(keytoolExe, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Dia.Process.Start(psi);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(output)) Debug.Log(output);
            if (!string.IsNullOrEmpty(error)) Debug.LogWarning(error);

            sha1Hash = ParseHash(output, "SHA1:");
            sha256Hash = ParseHash(output, "SHA256:");
            validityInfo = ParseValidity(output);

            if (!string.IsNullOrEmpty(sha1Hash) && !string.IsNullOrEmpty(sha256Hash))
            {
                ShowToast("✅ Hashes generated successfully!", Color.green);
            }
            else
            {
                ShowToast("⚠️ Could not parse hashes!", Color.yellow);
            }
        }
        catch (System.Exception e)
        {
            ShowToast("❌ Error: " + e.Message, Color.red);
        }
    }

    private string ParseHash(string output, string key)
    {
        using (StringReader reader = new StringReader(output))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Trim().StartsWith(key))
                {
                    string hash = line.Substring(key.Length).Trim();
                    if (char.IsDigit(hash[0]) && hash.Contains(":"))
                    {
                        int idx = hash.IndexOf(':');
                        if (idx >= 0 && idx < hash.Length - 1)
                            hash = hash.Substring(idx + 1).Trim();
                    }
                    return hash;
                }
            }
        }
        return "";
    }

    private string ParseValidity(string output)
    {
        using (StringReader reader = new StringReader(output))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("Valid from:"))
                {
                    return line.Trim();
                }
            }
        }
        return "";
    }
    #endregion

    #region Export
    private void ExportReport()
    {
        string path = EditorUtility.SaveFilePanel("Export Keystore Report", "", "keystore_report.txt", "txt");
        if (string.IsNullOrEmpty(path)) return;

        string report =
            "=== Persinus Keystore Report ===\n" +
            $"Keystore Path: {keystorePath}\n" +
            $"Alias: {aliasName}\n" +
            $"Validity: {validityInfo}\n" +
            $"SHA-1: {sha1Hash}\n" +
            $"SHA-256: {sha256Hash}\n";

        File.WriteAllText(path, report);
        ShowToast("Report exported!", Color.green);
    }
    #endregion

    #region Toast
    private void ShowToast(string message, Color color, float duration = 2f)
    {
        toastMessage = message;
        toastColor = color;
        toastEndTime = EditorApplication.timeSinceStartup + duration;
        Repaint();
    }

    private void DrawToast()
    {
        if (EditorApplication.timeSinceStartup < toastEndTime && !string.IsNullOrEmpty(toastMessage))
        {
            Rect rect = new Rect(10, position.height - 40, position.width - 20, 30);
            EditorGUI.DrawRect(rect, toastColor * new Color(1, 1, 1, 0.2f));
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = toastColor }
            };
            GUI.Label(rect, toastMessage, style);
        }
    }
    #endregion
}
