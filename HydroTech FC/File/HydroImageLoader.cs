using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    using UnityEngine;

    public class HydroImageLoader
    {
        public HydroImageLoader(
            FileName fileName,
            int width,
            int height,
            TextureFormat format = TextureFormat.ARGB32
            )
        {
            filename = new FileName(fileName);
            image = new Texture2D(width, height, format, false);
        }
        public HydroImageLoader(
            FileName fileName,
            Vector2 size,
            TextureFormat format = TextureFormat.ARGB32
            )
        {
            filename = new FileName(fileName);
            image = new Texture2D((int)size.x, (int)size.y, format, false);
        }

        public Texture2D image = null;
        private FileName filename = null;

        public Vector2 Size { get { return image.texelSize; } }
        public int Width { get { return image.width; } }
        public int Height { get { return image.height; } }

        public void Load()
        {
            new WWW(filename.WWWForm).LoadImageIntoTexture(image);
        }

        public static implicit operator Texture2D(HydroImageLoader loader) { return loader.image; }
    }
}