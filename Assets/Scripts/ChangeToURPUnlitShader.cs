using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

public class ChangeToURPUnlitShader : EditorWindow
{
    [MenuItem("Tools/Change Materials to URP Unlit")]
    public static void ChangeMaterialsToURPUnlit()
    {
        // Shader name for the URP Unlit Shader
        string urpUnlitShaderName = "Universal Render Pipeline/Unlit";

        // Find all Material assets in the project
        string[] materialGUIDs = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in materialGUIDs)
        {
            string materialPath = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            // Change the material's shader to URP Unlit
            if (material.shader.name != urpUnlitShaderName)
            {
                Shader urpUnlitShader = Shader.Find(urpUnlitShaderName);
                if (urpUnlitShader != null)
                {
                    material.shader = urpUnlitShader;
                    Debug.Log($"Changed shader of {material.name} to URP Unlit", material);
                }
                else
                {
                    Debug.LogWarning($"Could not find shader: {urpUnlitShaderName}. Make sure URP is correctly set up in your project.");
                }
            }
        }

        Debug.Log("Shader change process completed.");
    }
}
