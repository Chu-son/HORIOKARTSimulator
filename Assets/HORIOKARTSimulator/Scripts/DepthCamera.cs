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

        public RenderTexture depthTexture;

        [HideInInspector] public Texture2D texture;

        [HideInInspector] public byte[] depthData;

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
            // this.depthTexture = new RenderTexture(this.width, this.height, 16, RenderTextureFormat.Depth);
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

                // テクスチャデータからDepthImageの配列を作成
                byte[] depthData = new byte[width * height * 2]; // 16UC1フォーマットなので2バイト

                for (int i = 0; i < pixels.Length; i++)
                {
                    // Unityの深度値を16ビットに変換して配列に格納
                    // ushort depthValue = (ushort)(pixels[i].r * 65535); // 0.0-1.0の範囲を0-65535の範囲にスケーリング
                    // 0.0-1.0の範囲をnearClipPlane-farClipPlaneの範囲にスケーリング
                    // ushort depthValue = (ushort)((pixels[i].r * (this._camera.farClipPlane - this._camera.nearClipPlane) + this._camera.nearClipPlane) * 1000);

                    ushort depthValue = (ushort)(pixels[i].r * this._camera.farClipPlane * 1000);
                    // ushort depthValue = (ushort)(pixels[i].grayscale * this._camera.farClipPlane * 1000);

                    byte[] bytes = System.BitConverter.GetBytes(depthValue);
                    depthData[i * 2] = bytes[0];
                    depthData[i * 2 + 1] = bytes[1];
                }

                this.texture.Resize(this.width, this.height);
                this.texture.SetPixels(pixels);
                this.texture.Apply();

                this.depthData = depthData;
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

    }
}
