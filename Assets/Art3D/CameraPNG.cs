using System.IO;
using UnityEngine;

public class CameraCapture : MonoBehaviour {
    public int fileCounter;
    public KeyCode screenshotKey;


    private void LateUpdate() {
        if (Input.GetKeyDown(screenshotKey)) {
            ScreenCapture.CaptureScreenshot("rendu_scene_presentation.png");
            print("allo?");
        }
    }

    public void Capture() {
        Camera cam = Camera.main;
        RenderTexture activeRenderTexture = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;

        cam.Render();

        Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = activeRenderTexture;

        byte[] bytes = image.EncodeToPNG();
        Destroy(image);

        File.WriteAllBytes("C:/Users/benja/Documents/Unity/Concours-Ubisoft-2025/Assets/Art3D/" + fileCounter + ".png", bytes);
        fileCounter++;
    }
}