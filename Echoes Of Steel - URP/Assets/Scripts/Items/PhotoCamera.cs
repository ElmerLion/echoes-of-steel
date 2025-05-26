using UnityEngine;

public class PhotoCamera : Item {
    [SerializeField] private ItemSO photoItemSO;
    [SerializeField] private Camera photoCaptureCamera;
    [SerializeField] private Material photoPreviewMat;
    [SerializeField] private LayerMask obstaclesLayerMask;
    [SerializeField] private Light photoLight;

    private RenderTexture renderTexture;

    private void Start() {
        // Initialize the RenderTexture
        renderTexture = new RenderTexture(256, 256, 24); // Adjust resolution as needed
        photoCaptureCamera.targetTexture = renderTexture;
        photoPreviewMat.mainTexture = renderTexture;

        photoLight.enabled = false;

        photoCaptureCamera.clearFlags = CameraClearFlags.SolidColor;
        photoCaptureCamera.backgroundColor = Color.black;
    }

    public override bool CanUse() {
        return !InventoryManager.Instance.IsInventoryFull();
    }

    public override void OnUse() {
        base.OnUse();
        TakePhoto();
    }

    public void TakePhoto() {
        photoCaptureCamera.enabled = true;
        photoLight.enabled = true;

        photoCaptureCamera.Render();

        RenderTexture.active = renderTexture;

        // Create a new Texture2D and read the pixels from the RenderTexture
        Texture2D photoTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        photoTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        photoTexture.Apply();

        RenderTexture.active = null;

        photoLight.enabled = false;

        photoTexture = MakeTextureOpaque(photoTexture);
         
        Texture2D baseTexture = GetTextureFromSprite(photoItemSO.itemSprite);

        Color pixel = photoTexture.GetPixel(0, 0);

        Texture2D combinedTexture = CombineTextures(baseTexture, photoTexture);

        Color combinedPixel = combinedTexture.GetPixel(0, 0);

        Sprite combinedSprite = Sprite.Create(combinedTexture, new Rect(0, 0, combinedTexture.width, combinedTexture.height), new Vector2(0.5f, 0.5f));

        InventoryManager.Instance.TryAddItem(photoItemSO, null, true, combinedSprite, CheckForAnimatronicInPhoto());
    }

    private bool CheckForAnimatronicInPhoto() {
        // Find all animatronics in the scene
        Animatronic[] animatronics = AnimatronicManager.Instance.ActiveAnimatronics.ToArray();

        foreach (Animatronic animatronic in animatronics) {
            Collider animatronicCollider = animatronic.GetComponent<Collider>();
            if (animatronicCollider == null) continue;

            // Get the bounds of the animatronic's collider
            Bounds bounds = animatronicCollider.bounds;

            // The number of points to check on each axis (more points give better accuracy)
            int numPointsX = 4;
            int numPointsY = 4;

            int visiblePoints = 0;
            int totalPoints = numPointsX * numPointsY;

            // Loop through the points in a grid pattern within the bounding box of the animatronic
            for (int x = 0; x <= numPointsX; x++) {
                for (int y = 0; y <= numPointsY; y++) {
                    // Calculate the normalized position within the bounds (0 to 1)
                    float normalizedX = (float)x / numPointsX;
                    float normalizedY = (float)y / numPointsY;

                    // Calculate the world position of the point based on the bounds
                    Vector3 point = bounds.min + new Vector3(bounds.size.x * normalizedX, bounds.size.y * normalizedY, 0);

                    // Convert the point into the viewport space of the camera
                    Vector3 viewportPos = photoCaptureCamera.WorldToViewportPoint(point);

                    // Check if the point is within the camera's viewport (and in front of the camera)
                    if (viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1) {
                        // Perform a raycast to ensure no obstacles are blocking the view
                        Ray ray = photoCaptureCamera.ViewportPointToRay(new Vector3(viewportPos.x, viewportPos.y, 0));

                        // Use a LayerMask to filter out unwanted collisions (e.g., walls)
                        if (Physics.Raycast(ray, out RaycastHit hit)) {
                            if (hit.collider.GetComponent<Animatronic>() != null) {
                                visiblePoints++; // Count this point as visible
                            }
                        }
                    }
                }
            }

            // If most points are visible (adjust the threshold as needed), consider the animatronic detected
            if (visiblePoints > totalPoints * 0.1f) { // This checks if at least 50% of the points are visible
                Debug.Log("Animatronic detected in the photo!");
                return true;
            }
        }

        return false; // No animatronic was detected
    }



    private Texture2D GetTextureFromSprite(Sprite sprite) {
        if (sprite.rect.width != sprite.texture.width) {
            Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                         (int)sprite.textureRect.y,
                                                         (int)sprite.textureRect.width,
                                                         (int)sprite.textureRect.height);
            newText.SetPixels(newColors);
            newText.Apply();
            return newText;
        } else {
            return sprite.texture;
        }
    }

    private Texture2D CombineTextures(Texture2D baseTexture, Texture2D overlayTexture) {
        RenderTexture rt = new RenderTexture(baseTexture.width, baseTexture.height, 0, RenderTextureFormat.ARGB32);
        RenderTexture.active = rt;

        // Clear the RenderTexture to opaque
        GL.Clear(true, true, Color.black);

        // Blit base texture
        Graphics.Blit(baseTexture, rt);

        // Create a material for blending
        Material mat = new Material(Shader.Find("Unlit/Texture"));
        mat.SetTexture("_MainTex", overlayTexture);

        // Render overlay texture with forced opaque alpha
        Graphics.Blit(overlayTexture, rt, mat);

        // Read the combined result
        Texture2D combinedTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.RGBA32, false);
        combinedTexture.ReadPixels(new Rect(0, 0, baseTexture.width, baseTexture.height), 0, 0);
        combinedTexture.Apply();

        // Clean up
        RenderTexture.active = null;
        rt.Release();

        return combinedTexture;
    }

    private Texture2D MakeTextureOpaque(Texture2D texture) {
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++) {
            pixels[i].a = 1f; // Set alpha to fully opaque
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

}
