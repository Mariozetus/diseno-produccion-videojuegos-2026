using UnityEditor;
using System;
using System.IO;

public class PackageExporter
{
    public static void Export()
    {
        try 
        {
            string[] args = Environment.GetCommandLineArgs();
            string scenePath = "";
            string outputPath = "";

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-scenePath") scenePath = args[i + 1];
                if (args[i] == "-outputName") outputPath = args[i + 1];
            }

            if (string.IsNullOrEmpty(scenePath) || string.IsNullOrEmpty(outputPath))
            {
                Console.WriteLine("ERROR: Argumentos insuficientes.");
                EditorApplication.Exit(1);
            }

            string dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            Console.WriteLine($"Iniciando exportación de: {scenePath}");
            
            AssetDatabase.ExportPackage(scenePath, outputPath, 
                ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Recurse);

            Console.WriteLine("Exportación finalizada con éxito.");
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Console.WriteLine($"ERROR FATAL: {e.Message}");
            EditorApplication.Exit(1);
        }
    }
}