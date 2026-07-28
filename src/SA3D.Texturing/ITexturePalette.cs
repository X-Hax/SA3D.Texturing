using SA3D.Texturing.ReadOnly;
using System;

namespace SA3D.Texturing
{
	/// <summary>
	/// Texture palette interface
	/// </summary>
	public interface ITexturePalette
	{
		/// <summary>
		/// The default grayscale palette for rendering 8 bit index textures.
		/// </summary>
		public static ReadOnlyTexturePalette Index8Palette { get; }

		/// <summary>
		/// The default grayscale palette for rendering 4 bit index textures.
		/// </summary>
		public static ReadOnlyTexturePalette Index4Palette { get; }


		/// <summary>
		/// Name of the palette
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Number of colors in this palette
		/// </summary>
		public int Width { get; }


		static ITexturePalette()
		{
			byte[] index4 = new byte[64];
			byte[] index8 = new byte[1024];

			for(int i = 0; i < 256; i++)
			{
				int index = i * 4;
				byte value = (byte)i;
				index8[index] = value;
				index8[index + 1] = value;
				index8[index + 2] = value;
				index8[index + 3] = 0xFF;
			}

			for(int i = 0; i < 16; i++)
			{
				int index = i * 4;
				byte value = (byte)(i | (i << 4));
				index4[index] = value;
				index4[index + 1] = value;
				index4[index + 2] = value;
				index4[index + 3] = 0xFF;
			}

			Index4Palette = new ReadOnlyTexturePalette(index4);
			Index8Palette = new ReadOnlyTexturePalette(index8);
		}


		/// <summary>
		/// Get RGBA32 color data
		/// </summary>
		/// <returns></returns>
		public ReadOnlySpan<byte> GetColorData();


		/// <summary>
		/// Returns either <see cref="Index4Palette"/> or <see cref="Index8Palette"/> based on <paramref name="index4"/>.
		/// </summary>
		/// <param name="index4">Specifies the default palette to get.</param>
		/// <returns>The default palette.</returns>
		public static ReadOnlyTexturePalette GetDefaultPalette(bool index4)
		{
			return index4 ? Index4Palette : Index8Palette;
		}
	}
}
