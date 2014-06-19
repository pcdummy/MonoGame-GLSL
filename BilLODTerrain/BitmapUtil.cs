using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace BilLODTerrain
{
	public class BitmapUtil
	{
		public static Texture2D GetTexture2DFromFile(GraphicsDevice device, string path)
		{
			if (!File.Exists(path)) {
//				returnResult.Problem = string.Format("File \"{0}\" does not exists.", path);
//				resultBitmap = null;
//				return returnResult;
				throw new Exception (string.Format ("File \"{0}\" does not exists.", path));
			}

            Texture2D text2D;
            using (var stream = File.Open (path, FileMode.Open))
            {
                text2D = GetTexture2FromStream (device, stream);
            }
			return text2D;
		}

        public static Texture2D GetTexture2FromStream(GraphicsDevice device, Stream stream)
        {
            return Texture2D.FromStream(device, stream);
        }           
	}
}

