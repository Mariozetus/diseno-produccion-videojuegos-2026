#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace UnityIntegration
{
    public class IntegrationEngine
    {
        private static string _currentBranch;
        private static string _backupRoot = ".integration_backup";
        private static List<string> _movedAssets = new List<string>();
        private static bool _backupMode = false;

        public static void ProcessScene()
        {
            var args = System.Environment.GetCommandLineArgs();
            string scenePath = GetArg("-scenePath", args);
            string userName = SanitizeName(GetArg("-userName", args));
            _currentBranch = GetArg("-branch", args) ?? "unknown";

            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError("[Integration] ERROR: No scene path provided. Use -scenePath <path>");
                return;
            }

            if (string.IsNullOrEmpty(userName))
            {
                Debug.LogError("[Integration] ERROR: No userName provided. Use -userName <name>");
                return;
            }

            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string targetFolder = $"Assets/Integracion/{userName}";
            string backupFolder = Path.Combine(_backupRoot, $"{_currentBranch}_{timestamp}");

            Log($"===============================================");
            Log($"  UNITY INTEGRATION ENGINE v2.0");
            Log($"===============================================");
            Log($"Scene: {scenePath}");
            Log($"User: {userName}");
            Log($"Branch: {_currentBranch}");
            Log($"Target: {targetFolder}");
            Log($"Backup: {backupFolder}");

            if (!AssetDatabase.IsValidFolder("Assets/Integracion"))
            {
                AssetDatabase.CreateFolder("Assets", "Integracion");
                Log("Created Assets/Integracion folder");
            }

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
                Log($"Created target folder: {targetFolder}");
            }

            string asmdefPath = Path.Combine(targetFolder, $"{userName}.asmdef");
            CreateAssemblyDefinition(asmdefPath, userName);

            StartBackup(backupFolder);

            string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);
            Log($"Found {dependencies.Length} dependencies for scene");

            int movedCount = 0;
            int skippedCount = 0;
            int errorCount = 0;

            foreach (string dep in dependencies)
            {
                if (ShouldSkipAsset(dep))
                {
                    skippedCount++;
                    continue;
                }

                if (MoveAssetSafely(dep, targetFolder, userName))
                {
                    movedCount++;
                }
                else
                {
                    errorCount++;
                }
            }

            string sceneName = Path.GetFileName(scenePath);
            string newScenePath = Path.Combine(targetFolder, sceneName);

            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), scenePath)))
            {
                LogError($"Scene file not found: {scenePath}");
                Rollback(backupFolder);
                return;
            }

            string sceneError = AssetDatabase.MoveAsset(scenePath, newScenePath);
            if (!string.IsNullOrEmpty(sceneError))
            {
                LogError($"Failed to move scene: {sceneError}");
                Rollback(backupFolder);
                return;
            }
            _movedAssets.Add(scenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Log($"===============================================");
            Log($"  INTEGRATION COMPLETE");
            Log($"===============================================");
            Log($"Assets moved: {movedCount}");
            Log($"Assets skipped: {skippedCount}");
            Log($"Errors: {errorCount}");
            Log($"Scene relocated to: {newScenePath}");

            if (_backupMode)
            {
                CleanupBackup(backupFolder);
            }

            Debug.Log("INTEGRATION_SUCCESS");
        }

        private static void CreateAssemblyDefinition(string asmdefPath, string userName)
        {
            if (File.Exists(asmdefPath))
            {
                Log($"Assembly Definition already exists: {asmdefPath}");
                return;
            }

            string asmdefContent = @"{
    ""name"": """ + userName + @""",
    ""rootNamespace"": """ + userName + @""",
    ""references"": [],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";
            File.WriteAllText(asmdefPath, asmdefContent);
            AssetDatabase.ImportAsset(asmdefPath);
            Log($"Created Assembly Definition: {asmdefPath}");
        }

        private static bool ShouldSkipAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return true;
            if (assetPath.StartsWith("Packages")) return true;
            if (assetPath.Contains("BaseProject")) return true;
            if (assetPath.EndsWith(".unity") && !assetPath.Contains("Integracion")) return true;
            if (assetPath.Contains("/Editor/") && !assetPath.Contains("Integration")) return true;
            if (assetPath.Contains("WebGLTemplates")) return true;
            if (assetPath.Contains("Packages/com.unity")) return true;

            return false;
        }

        private static bool MoveAssetSafely(string assetPath, string targetFolder, string userName)
        {
            string fileName = Path.GetFileName(assetPath);
            string newPath = Path.Combine(targetFolder, fileName);

            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), newPath)))
            {
                string uniqueSuffix = GetUniqueSuffix(assetPath, targetFolder);
                string nameWithoutExt = Path.GetFileNameWithoutExtension(assetPath);
                string extension = Path.GetExtension(assetPath);
                string dirWithoutFilename = Path.GetDirectoryName(newPath);
                newPath = Path.Combine(dirWithoutFilename, $"{nameWithoutExt}_{uniqueSuffix}{extension}").Replace("\\", "/");
            }

            string error = AssetDatabase.MoveAsset(assetPath, newPath);

            if (!string.IsNullOrEmpty(error))
            {
                LogError($"Failed to move {assetPath}: {error}");
                return false;
            }

            _movedAssets.Add(assetPath);

            if (newPath.EndsWith(".cs"))
            {
                InjectNamespace(newPath, userName);
            }

            Log($"Moved: {assetPath} -> {newPath}");
            return true;
        }

        private static string GetUniqueSuffix(string assetPath, string targetFolder)
        {
            string hashInput = assetPath + System.DateTime.Now.Ticks.ToString();
            int hash = hashInput.GetHashCode();
            return Mathf.Abs(hash).ToString("X8").Substring(0, 6);
        }

        private static void InjectNamespace(string filePath, string userName)
        {
            if (!File.Exists(filePath)) return;

            string content = File.ReadAllText(filePath);
            string safeName = SanitizeName(userName);

            string existingNamespace = ExtractExistingNamespace(content);
            if (!string.IsNullOrEmpty(existingNamespace))
            {
                if (existingNamespace == safeName)
                {
                    Log($"File already has correct namespace: {filePath}");
                    return;
                }

                content = ReplaceNamespace(content, existingNamespace, safeName);
                File.WriteAllText(filePath, content);
                Log($"Replaced namespace {existingNamespace} -> {safeName} in {Path.GetFileName(filePath)}");
                return;
            }

            if (content.Contains("namespace "))
            {
                LogWarning($"File has unnamed namespace block, skipping: {filePath}");
                return;
            }

            if (!content.Contains("namespace ") && !content.Contains("using UnityEngine"))
            {
                LogWarning($"File does not appear to be a C# script: {filePath}");
                return;
            }

            string indentedContent = IndentContent(content, 1);
            string namespacedContent = $"namespace {safeName} {{\n{indentedContent}\n}}";
            File.WriteAllText(filePath, namespacedContent);
            Log($"Injected namespace {safeName} into {Path.GetFileName(filePath)}");
        }

        private static string ExtractExistingNamespace(string content)
        {
            var match = Regex.Match(content, @"namespace\s+([a-zA-Z_][a-zA-Z0-9_]*)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        private static string ReplaceNamespace(string content, string oldNamespace, string newNamespace)
        {
            string pattern = $@"namespace\s+{Regex.Escape(oldNamespace)}\s*{{";
            string replacement = $"namespace {newNamespace} {{";
            return Regex.Replace(content, pattern, replacement);
        }

        private static string IndentContent(string content, int spaces)
        {
            string indent = new string(' ', spaces * 4);
            string[] lines = content.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    lines[i] = indent + lines[i].TrimEnd('\r');
                }
            }
            return string.Join("\n", lines);
        }

        private static void StartBackup(string backupFolder)
        {
            if (!_backupMode) return;

            if (Directory.Exists(backupFolder))
            {
                Directory.Delete(backupFolder, true);
            }
            Directory.CreateDirectory(backupFolder);
            Log($"Backup started: {backupFolder}");
        }

        private static void Rollback(string backupFolder)
        {
            if (!_backupMode || !Directory.Exists(backupFolder))
            {
                Log("Rollback skipped (no backup available)");
                return;
            }

            LogWarning("ROLLBACK INITIATED - Restoring original asset locations");

            foreach (string asset in _movedAssets)
            {
                string backedUpFile = FindInBackup(asset, backupFolder);
                if (backedUpFile != null && File.Exists(backedUpFile))
                {
                    File.Copy(backedUpFile, asset, true);
                    Log($"Restored: {asset}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            LogError("ROLLBACK COMPLETE - Please check scene integrity");
        }

        private static string FindInBackup(string originalPath, string backupFolder)
        {
            string fileName = Path.GetFileName(originalPath);
            return Path.Combine(backupFolder, fileName);
        }

        private static void CleanupBackup(string backupFolder)
        {
            if (Directory.Exists(backupFolder))
            {
                Directory.Delete(backupFolder, true);
                Log($"Cleaned up backup: {backupFolder}");
            }
        }

        private static string SanitizeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Unknown";

            string sanitized = name.Replace("-", "_").Replace(" ", "_");
            sanitized = Regex.Replace(sanitized, @"[^a-zA-Z0-9_]", "");
            sanitized = Regex.Replace(sanitized, @"^[0-9]", "_0");

            if (string.IsNullOrEmpty(sanitized) || sanitized == "_")
            {
                sanitized = "Unknown";
            }

            return sanitized;
        }

        private static string GetArg(string name, string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && i + 1 < args.Length)
                    return args[i + 1];
            }
            return null;
        }

        private static void Log(string message)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            Debug.Log($"[Integration {timestamp}] {message}");
        }

        private static void LogWarning(string message)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            Debug.LogWarning($"[Integration {timestamp}] {message}");
        }

        private static void LogError(string message)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            Debug.LogError($"[Integration {timestamp}] {message}");
        }
    }
}
#endif