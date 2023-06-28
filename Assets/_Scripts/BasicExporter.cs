using UnityEngine;
using Autodesk.Fbx;
using UnityEditor;
using System.IO;
using UnityEditor.Formats.Fbx.Exporter;

public class BasicExporter : MonoBehaviour
{
    protected void ExportScene(string fileName)
    {
        using (FbxManager fbxManager = FbxManager.Create())
        {
            // configure IO settings.
            fbxManager.SetIOSettings(FbxIOSettings.Create(fbxManager, Globals.IOSROOT));

            // Export the scene
            using (FbxExporter exporter = FbxExporter.Create(fbxManager, "myExporter"))
            {

                // Initialize the exporter.
                bool status = exporter.Initialize(fileName, -1, fbxManager.GetIOSettings());

                FbxObject obj = FbxObject.Create(fbxManager, "obj");

                // Create a new scene to export
                FbxScene scene = FbxScene.Create(fbxManager, "myScene");

                // Export the scene to the file.
                exporter.Export(scene);
            }
        }
    }

    public void ExportObject(Object obj)
    {
        string filePath = Path.Combine(Application.dataPath, "obj.fbx");

        ModelExporter.ExportObject(filePath, obj);
    }
}
