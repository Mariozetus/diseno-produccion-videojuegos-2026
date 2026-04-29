using UnityEditor;
using System;
using System.Linq;

public class PackageExporter
{
    public static void Export()
    {
        string[] args = Environment.GetCommandLineArgs();
        
        int sceneIdx = Array.IndexOf(args, "-scenePath") + 1; // Nombre escena
        int outputIdx = Array.IndexOf(args, "-outputName") + 1; // Nombre paquete

        if (sceneIdx > 0 && outputIdx > 0)
        {
            string assetPath = args[sceneIdx];
            string fileName = args[outputIdx];

            UnityEngine.Debug.Log($"Exportando: {assetPath} hacia {fileName}...");

            AssetDatabase.ExportPackage(assetPath, fileName, 
                ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Recurse);
            
            UnityEngine.Debug.Log("¡Exportación completada con éxito!");
        }
    }
}