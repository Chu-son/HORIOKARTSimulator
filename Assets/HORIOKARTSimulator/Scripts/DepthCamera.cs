using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

namespace HJ.Simulator
{
    [RequireComponent(typeof(Camera))]
    public class DepthCamera : MonoBehaviour
    {
        [HideInInspector] public int width;
        [HideInInspector] public int height;

        [Header("Shader Setup")]
        public Shader uberReplacementShader;

        private Camera _mainCamera;
        private Camera _camera;
        private Rect _rect;

        private RenderTexture depthTexture;

        [HideInInspector] public Texture2D texture;

        public void Init(int width, int height)
        {
            if (!this.uberReplacementShader)
                this.uberReplacementShader = Shader.Find("Hidden/UberReplacement");

            this._mainCamera = GetComponent<Camera>();
            this._camera = CreateHiddenCamera("depth");
            this._camera.RemoveAllCommandBuffers();
            this._camera.CopyFrom(this._mainCamera);
            this._camera.targetDisplay = 3;

            this._camera.nearClipPlane = 0.2f;
            this._camera.farClipPlane = 10f;
            this._camera.fieldOfView = 60f;

            SetupCameraWithReplacementShader(this._camera, this.uberReplacementShader, 2, Color.black);

            // this._camera.targetTexture = this.depthTexture;

            this.width = width;
            this.height = height;

            this.depthTexture = new RenderTexture(this.width, this.height, 24, RenderTextureFormat.RFloat);
            // this.depthTexture = new RenderTexture(this.width, this.height, 24, RenderTextureFormat.Default);
            this._rect = new Rect(0, 0, this.width, this.height);


            this.texture = new Texture2D(this.width, this.height, TextureFormat.RFloat, false);
            // this.texture = new Texture2D(this.width, this.height, TextureFormat.RGB24, false);
            // this.texture = new Texture2D(this.width, this.height, TextureFormat.RGBA32, false);
            // Debug.Log(SystemInfo.SupportsTextureFormat(TextureFormat.RFloat));

        }

        public void UpdateImage()
        {
            if (this.texture != null)
            {
                // this._camera.depthTextureMode = DepthTextureMode.Depth;
                RenderTexture.active = this.depthTexture;
                this._camera.targetTexture = this.depthTexture;
                this._camera.Render();

                this.texture.ReadPixels(this._rect, 0, 0);

                this._camera.targetTexture = null;
                RenderTexture.active = null;

                // 上下反転
                Color[] pixels = this.texture.GetPixels();
                for (int i = 0; i < this.height / 2; i++)
                {
                    for (int j = 0; j < this.width; j++)
                    {
                        int index1 = i * this.width + j;
                        int index2 = (this.height - i - 1) * this.width + j;
                        Color temp = pixels[index1];
                        pixels[index1] = pixels[index2];
                        pixels[index2] = temp;
                    }
                }
                this.texture.SetPixels(pixels);
                this.texture.Apply();
            }
        }

        private Camera CreateHiddenCamera(string name)
        {
            var go = new GameObject(name, typeof(Camera));
            go.hideFlags = HideFlags.HideAndDontSave;
            go.transform.parent = transform;

            var newCamera = go.GetComponent<Camera>();
            return newCamera;
        }

        static private void SetupCameraWithReplacementShader(Camera cam, Shader shader, int mode, Color clearColor)
        {
            var cb = new CommandBuffer();
            cb.SetGlobalFloat("_OutputMode", (int)mode); // @TODO: CommandBuffer is missing SetGlobalInt() method
            cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cb);
            cam.AddCommandBuffer(CameraEvent.BeforeFinalPass, cb);
            cam.SetReplacementShader(shader, "");
            cam.backgroundColor = clearColor;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.allowHDR = false;
            cam.allowMSAA = false;
        }

        // private void OnGUI() {
        //     Texture2D t = new Texture2D(this.width, this.height);
        //     // t.LoadImage(this.data);
        //     // t.LoadImage(this.texture.GetRawTextureData());
        //     GUI.Box(new Rect(Screen.width - 100, Screen.height -100, 90, 90), this.texture);
        //     // GUI.Box(new Rect(Screen.width - 100, Screen.height -100, 90, 90), t);
        // }

    }
}
