using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemSO))]
public class ItemEditor : Editor {
    private float rotationX = 0f;
    private float rotationY = 0f;
    private float rotationZ = 0f;
    private Vector3 prefabPositionOffset = Vector3.zero;
    private float zoomLevel = 1.0f;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Icon Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        ItemSO item = (ItemSO)target;

        // Rotation sliders for X, Y, Z axes
        rotationX = EditorGUILayout.Slider("Rotation X", rotationX, 0f, 360f);
        rotationY = EditorGUILayout.Slider("Rotation Y", rotationY, 0f, 360f);
        rotationZ = EditorGUILayout.Slider("Rotation Z", rotationZ, 0f, 360f);

        // Prefab Position Offset
        EditorGUILayout.LabelField("Prefab Position Offset");
        prefabPositionOffset = EditorGUILayout.Vector3Field("", prefabPositionOffset);

        // Zoom Level
        EditorGUILayout.LabelField("Zoom Level");
        zoomLevel = EditorGUILayout.Slider(zoomLevel, 0.1f, 10.0f);

        // Display the generated sprite if available
        if (item.itemSprite != null) {
            GUILayout.Label(item.itemSprite.texture, GUILayout.Width(64), GUILayout.Height(64));
        }

        // Generate Icon button
        if (GUILayout.Button("Generate Icon")) {
            if (item.prefab != null) {
                Vector3 rotation = new Vector3(rotationX, rotationY, rotationZ);
                Texture2D texture = GeneratePreview(item.prefab.gameObject, 128, 128, rotation, prefabPositionOffset);
                if (texture != null) {
                    string directory = "Assets/Textures/Generated";
                    if (!Directory.Exists(directory)) {
                        Directory.CreateDirectory(directory);
                    }
                    string texturePath = Path.Combine(directory, item.name + "_Icon.png");

                    File.WriteAllBytes(texturePath, texture.EncodeToPNG());
                    AssetDatabase.Refresh();

                    // Load and set import settings for the texture
                    TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                    if (importer != null) {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spritePixelsPerUnit = 100;
                        importer.alphaIsTransparency = true;
                        importer.isReadable = true;
                        importer.SaveAndReimport();
                    }

                    item.itemSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
                    EditorUtility.SetDirty(item); // Mark the item as dirty to save changes
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) {
        ItemSO item = (ItemSO)target;

        if (item.itemSprite != null) {
            Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            EditorUtility.CopySerialized(item.itemSprite.texture, texture);
            return texture;
        }

        return base.RenderStaticPreview(assetPath, subAssets, width, height);
    }

    private Texture2D GeneratePreview(GameObject prefab, int width, int height, Vector3 rotation, Vector3 positionOffset) {
        // Create a temporary layer for preview
        int previewLayer = 31; // Make sure layer 31 is not used

        // Create a temporary camera
        GameObject tempCameraGO = new GameObject("TempCamera");
        Camera tempCamera = tempCameraGO.AddComponent<Camera>();
        tempCamera.backgroundColor = new Color(0, 0, 0, 0); // Fully transparent background
        tempCamera.clearFlags = CameraClearFlags.SolidColor;  // Ensure the background is cleared to transparent
        tempCamera.orthographic = true;
        tempCamera.cullingMask = 1 << previewLayer;
        tempCamera.allowHDR = false; // Disable HDR if enabled
        tempCamera.allowMSAA = false;

        // Instantiate the prefab temporarily at the origin
        GameObject tempPrefabInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        SetLayerRecursively(tempPrefabInstance, previewLayer);

        // Calculate bounds before applying position offset
        Bounds bounds = CalculateBounds(tempPrefabInstance);

        // Apply position offset and rotation
        tempPrefabInstance.transform.position = positionOffset;
        tempPrefabInstance.transform.rotation = Quaternion.Euler(rotation);

        // Adjust the camera orthographic size and position
        tempCamera.orthographicSize = bounds.extents.magnitude / zoomLevel;
        tempCamera.transform.position = bounds.center - Vector3.forward * (bounds.extents.magnitude + 1);
        tempCamera.transform.LookAt(bounds.center);

        // Create a render texture with alpha support
        RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        tempCamera.targetTexture = renderTexture;

        // Render the prefab
        tempCamera.Render();

        // Convert RenderTexture to Texture2D
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        // Clean up
        RenderTexture.active = null;
        tempCamera.targetTexture = null;
        DestroyImmediate(tempCameraGO);
        DestroyImmediate(tempPrefabInstance);
        renderTexture.Release();

        return texture;
    }


    private void SetLayerRecursively(GameObject obj, int newLayer) {
        if (obj == null) {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform) {
            if (child == null) {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private Bounds CalculateBounds(GameObject go) {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) {
            return new Bounds(go.transform.position, Vector3.zero);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    private Sprite TextureToSprite(Texture2D texture) {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
    }
}
