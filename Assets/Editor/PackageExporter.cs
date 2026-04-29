using UnityEditor;
using System;
using System.IO;
using System.Linq;

public class PackageExporter
{
    public static void Export()
    {
        try 
        {
            string[] args = Environment.GetCommandLineArgs();
            string scenePathArg = "";
            string outputPath = "";

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-scenePath") scenePathArg = args[i + 1];
                if (args[i] == "-outputName") outputPath = args[i + 1];
            }

            if (string.IsNullOrEmpty(scenePathArg) || string.IsNullOrEmpty(outputPath))
            {
                EditorApplication.Exit(1);
            }

            string[] scenes = scenePathArg.Split(',');

            string dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // Exportamos todas las escenas encontradas en un solo paquete
            AssetDatabase.ExportPackage(scenes, outputPath, 
                ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Recurse);

            EditorApplication.Exit(0);
        }
        catch (Exception)
        {
            EditorApplication.Exit(1);
        }
    }
}