using UnityEngine;

public class PanaromaCapture : MonoBehaviour
{
    public Camera targetCamera;
    public RenderTexture cubeMapLeft;
    public RenderTexture equirectRT;

    // // Start is called once before the first execution of Update after the MonoBehaviour is created
    // void Update()
    // {
    //     Capture();
    // }

    // Update is called once per frame

    [ContextMenu("Capture")]
    void Capture()
    {
        targetCamera.RenderToCubemap(cubeMapLeft);
        cubeMapLeft.ConvertToEquirect(equirectRT);
        Save(equirectRT);
    }

    public void Save(RenderTexture rt)
    {
        Texture2D tex = new Texture2D(rt.width, rt.height);

        //RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0,0,rt.width,rt.height), 0, 0);
        //RenderTexture.active = null;

        byte[] bytes = tex.EncodeToJPG();

        string path = Application.dataPath + "/Panorama" + ".jpg";

        System.IO.File.WriteAllBytes(path, bytes);
    }

    public void SaveNew(RenderTexture rt)
    {
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);

        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();  // Important to apply changes
        RenderTexture.active = null;

        byte[] bytes = tex.EncodeToJPG();
        string path = Application.dataPath + "/Panorama" + ".jpg";

        System.IO.File.WriteAllBytes(path, bytes);
    }
}
