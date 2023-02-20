using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HJ.Simulator
{
    [RequireComponent(typeof(Camera))]
    public class ColorCamera : MonoBehaviour
    {
        [HideInInspector] public int width;
        [HideInInspector] public int height;

        [HideInInspector]
        public Camera _camera { get; private set; }
        private Rect _rect;

        private RenderTexture colorTexture;

        [HideInInspector] public Texture2D texture;

        public void Init(int width, int height)
        {
            this.width = width;
            this.height = height;

            this.colorTexture = new RenderTexture(this.width, this.height, 24);
            this._rect = new Rect(0, 0, this.width, this.height);

            this._camera = GetComponent<Camera>();

            this.texture = new Texture2D(this.width, this.height, TextureFormat.RGB24, false);
            // this.texture = new Texture2D(this.width, this.height, TextureFormat.RGBA32, false);

        }

        public void UpdateImage()
        {
            if (this.texture != null)
            {
                this._camera.targetTexture = this.colorTexture;
                this._camera.Render();
                RenderTexture.active = this.colorTexture;

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
                texture.SetPixels(pixels);
                this.texture.Apply();
            }
        }
    }
}
