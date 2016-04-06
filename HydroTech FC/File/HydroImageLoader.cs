using UnityEngine;

namespace HydroTech_FC
{
    public class HydroImageLoader
    {
        private FileName filename;

        public Texture2D image;

        public Vector2 Size
        {
            get { return this.image.texelSize; }
        }

        public int Width
        {
            get { return this.image.width; }
        }

        public int Height
        {
            get { return this.image.height; }
        }

        public HydroImageLoader(FileName fileName, int width, int height, TextureFormat format = TextureFormat.ARGB32)
        {
            this.filename = new FileName(fileName);
            this.image = new Texture2D(width, height, format, false);
        }

        public HydroImageLoader(FileName fileName, Vector2 size, TextureFormat format = TextureFormat.ARGB32)
        {
            this.filename = new FileName(fileName);
            this.image = new Texture2D((int)size.x, (int)size.y, format, false);
        }

        public void Load()
        {
            new WWW(this.filename.WwwForm).LoadImageIntoTexture(this.image);
        }

        public static implicit operator Texture2D(HydroImageLoader loader)
        {
            return loader.image;
        }
    }
}