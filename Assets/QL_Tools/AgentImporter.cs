#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class AgentImporter : OdinEditorWindow
{
    private const string DEFAULT_REPO_URL = "https://github.com/linhquanggit/AGENTS";
    private const string TEMP_DIR_NAME = "GitImport";

    private const string PREF_REPO_URL = "AgentImporter.RepoUrl";

    private static readonly string[] MD_FILES = { "AGENTS.md", "CLAUDE.md", "GEMINI.md" };

    private bool isRunning;

    [Title("Skills Importer", "Import / update the AI Runtime from a Git repository", TitleAlignments.Centered)]
    [BoxGroup("Source", showLabel: false)]
    [LabelText("Git Repo URL"), LabelWidth(90)]
    [OnValueChanged(nameof(SaveRepoUrl))]
    [SerializeField]
    private string repoUrl;

    [BoxGroup("Source")]
    [InfoBox("$StatusMessage", InfoMessageType.Info)]
    [ShowInInspector, HideLabel, DisplayAsString, PropertyOrder(1)]
    private string Status => isRunning ? "Working..." : (IsInstalled ? "Installed" : "Not installed");

    private static string TempPath => Path.Combine(Path.GetFullPath("Temp"), TEMP_DIR_NAME);
    private static string ProjectRoot => Path.GetFullPath(".");

    private bool IsInstalled =>
        Directory.Exists(Path.Combine(ProjectRoot, "AI"))
        || MD_FILES.Any(f => File.Exists(Path.Combine(ProjectRoot, f)));

    private string StatusMessage =>
        IsInstalled ? "AI Runtime is installed. Use Update to refresh or Remove to delete it." : "AI Runtime is not installed yet.";

    private bool InstallDisabled => isRunning || string.IsNullOrWhiteSpace(repoUrl);
    private bool CanRemove => !isRunning && IsInstalled;
    private string InstallButtonLabel => isRunning ? "Working..." : (IsInstalled ? "Update" : "Install");

    [MenuItem("AI/Agent")]
    private static void ShowWindow()
    {
        GetWindow<AgentImporter>("AI").minSize = new Vector2(440, 220);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        repoUrl = EditorPrefs.GetString(PREF_REPO_URL, DEFAULT_REPO_URL);
    }

    private void SaveRepoUrl() => EditorPrefs.SetString(PREF_REPO_URL, repoUrl);

    [HorizontalGroup("Actions")]
    [Button("$InstallButtonLabel", ButtonSizes.Large)]
    [DisableIf(nameof(InstallDisabled))]
    private void Install() => ExecuteImport();

    [HorizontalGroup("Actions")]
    [Button("Remove", ButtonSizes.Large)]
    [GUIColor(1f, 0.55f, 0.55f)]
    [EnableIf(nameof(CanRemove))]
    private void Remove() => ExecuteRemove();

    private void ExecuteImport()
    {
        if (!ConfirmOverwrite())
            return;

        isRunning = true;
        try
        {
            EnsureGitAvailable();

            EditorUtility.DisplayProgressBar("AI Importer", "Cleaning temp...", 0.1f);
            CleanTemp();

            EditorUtility.DisplayProgressBar("AI Importer", "Cloning from Git...", 0.3f);
            CloneRepo();

            EditorUtility.DisplayProgressBar("AI Importer", "Copying AI folder...", 0.6f);
            CopyAIFolder();

            EditorUtility.DisplayProgressBar("AI Importer", "Copying markdown files...", 0.8f);
            CopyMdFiles();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"<color=#ff3838>[AgentImporter]</color> error: {ex.Message}");
            EditorUtility.DisplayDialog("Failed", ex.Message, "OK");
            return;
        }
        finally
        {
            CleanTemp();
            EditorUtility.ClearProgressBar();
            isRunning = false;
        }

        AssetDatabase.Refresh();
        Debug.Log("<color=#4aff21>[AgentImporter]</color> completed! New files imported and .meta will be generated.");
        EditorUtility.DisplayDialog(
            "Success",
            "Import completed successfully.",
            "OK"
        );
    }

    private void ExecuteRemove()
    {
        var targets = new System.Collections.Generic.List<string>();
        if (Directory.Exists(Path.Combine(ProjectRoot, "AI")))
            targets.Add("AI/");
        targets.AddRange(MD_FILES.Where(f => File.Exists(Path.Combine(ProjectRoot, f))));

        if (targets.Count == 0)
            return;

        bool confirmed = EditorUtility.DisplayDialog(
            "Remove AI Runtime?",
            "The following will be deleted:\n\n" + string.Join("\n", targets),
            "Remove",
            "Cancel"
        );
        if (!confirmed)
            return;

        isRunning = true;
        try
        {
            string aiDir = Path.Combine(ProjectRoot, "AI");
            if (Directory.Exists(aiDir))
            {
                Directory.Delete(aiDir, true);
                File.Delete(aiDir + ".meta");
                Debug.Log($"<color=#4aff21>[AgentImporter]</color> removed {aiDir}");
            }

            foreach (string fileName in MD_FILES)
            {
                string path = Path.Combine(ProjectRoot, fileName);
                if (!File.Exists(path))
                    continue;
                File.Delete(path);
                File.Delete(path + ".meta");
                Debug.Log($"<color=#4aff21>[AgentImporter]</color> removed {fileName}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"<color=#ff3838>[AgentImporter]</color> error: {ex.Message}");
            EditorUtility.DisplayDialog("Failed", ex.Message, "OK");
            return;
        }
        finally
        {
            isRunning = false;
        }

        AssetDatabase.Refresh();
        Debug.Log("<color=#4aff21>[AgentImporter]</color> removed AI Runtime.");
        EditorUtility.DisplayDialog("Success", "AI Runtime removed.", "OK");
    }

    private bool ConfirmOverwrite()
    {
        var targets = new System.Collections.Generic.List<string>();
        if (Directory.Exists(Path.Combine(ProjectRoot, "AI")))
            targets.Add("AI/");
        targets.AddRange(MD_FILES.Where(f => File.Exists(Path.Combine(ProjectRoot, f))));

        if (targets.Count == 0)
            return true;

        return EditorUtility.DisplayDialog(
            "Overwrite existing files?",
            "The following will be replaced:\n\n" + string.Join("\n", targets),
            "Overwrite",
            "Cancel"
        );
    }

    private static void EnsureGitAvailable()
    {
        try
        {
            RunCommand("git", "--version", ProjectRoot);
        }
        catch (System.Exception)
        {
            throw new System.Exception("Git is not installed or not in PATH.");
        }
    }

    private static void CleanTemp()
    {
        if (!Directory.Exists(TempPath)) return;

        foreach (string file in Directory.GetFiles(TempPath, "*", SearchOption.AllDirectories))
            File.SetAttributes(file, FileAttributes.Normal);

        const int maxRetries = 5;
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                Directory.Delete(TempPath, true);
                return;
            }
            catch (IOException) when (i < maxRetries - 1)
            {
                System.Threading.Thread.Sleep(300);
            }
        }
    }

    private void CloneRepo()
    {
        Debug.Log("<color=#4aff21>[AgentImporter]</color> cloning from Git...");

        string error = RunCommand("git", $"clone --depth 1 {repoUrl} \"{TempPath}\"", ProjectRoot);
        if (error != null)
            throw new System.Exception($"Clone failed:\n{error}");

        if (!Directory.Exists(TempPath))
            throw new System.Exception("Clone did not create the target directory.");
    }

    private static void CopyAIFolder()
    {
        string src = Path.Combine(TempPath, "AI");
        string dest = Path.Combine(ProjectRoot, "AI");

        if (!Directory.Exists(src))
        {
            Debug.LogWarning("<color=#ffd900>[AgentImporter]</color> Assets/AI not found in repo — skipping.");
            return;
        }

        if (Directory.Exists(dest))
            Directory.Delete(dest, true);

        CopyCleanDirectory(src, dest);
        Debug.Log($"<color=#4aff21>[AgentImporter]</color> copied Assets/AI → {dest}");
    }

    private static void CopyMdFiles()
    {
        foreach (string fileName in MD_FILES)
        {
            string src = Path.Combine(TempPath, fileName);
            string dest = Path.Combine(ProjectRoot, fileName);

            if (!File.Exists(src))
            {
                Debug.LogWarning($"<color=#ffd900>[AgentImporter]</color> {fileName} not found in repo — skipping.");
                continue;
            }

            File.Copy(src, dest, overwrite: true);
            Debug.Log($"<color=#4aff21>[AgentImporter]</color> copied {fileName} → {dest}");
        }
    }

    private static void CopyCleanDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        var files = Directory.GetFiles(sourceDir)
            .Where(f =>
            {
                string name = Path.GetFileName(f).ToLower();
                return !name.EndsWith(".meta")
                    && name != ".gitignore"
                    && name != ".gitattributes";
            });

        foreach (string file in files)
            File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), overwrite: true);

        foreach (string dir in Directory.GetDirectories(sourceDir))
        {
            if (Path.GetFileName(dir) == ".git") continue;
            CopyCleanDirectory(dir, Path.Combine(destDir, Path.GetFileName(dir)));
        }
    }

    private static string RunCommand(string fileName, string args, string workingDir)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            WorkingDirectory = workingDir,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };

        using (var process = System.Diagnostics.Process.Start(startInfo))
        {
            var stdout = process.StandardOutput.ReadToEndAsync();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();
            stdout.Wait();
            return process.ExitCode == 0 ? null : stderr;
        }
    }
}
#endif
