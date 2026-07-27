using System.Collections.Generic;

namespace SA3D.Texturing.MipMapping
{
	/// <summary>
	/// Mip map utility methods
	/// </summary>
	public static class MipMapUtils
	{
		/// <summary>
		/// Calculates sizes for each level of a mip map texture (includes level 0)
		/// </summary>
		/// <param name="width">Width of the level 0 mip map</param>
		/// <param name="height">Height of the level 0 mip map</param>
		/// <returns></returns>
		public static (int width, int height)[] GetMipMapSizes(int width, int height)
		{
			List<(int, int)> mipmapSizes = [(width, height)];

			int mmWidth = width;
			int mmHeight = height;

			do
			{
				if(mmWidth > 1)
				{
					mmWidth >>= 1;
				}

				if(mmHeight > 1)
				{
					mmHeight >>= 1;
				}

				mipmapSizes.Add((mmWidth, mmHeight));
			}
			while(mmWidth > 1 || mmHeight > 1);

			return [.. mipmapSizes];
		}
	}
}
