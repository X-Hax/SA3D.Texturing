using System;

namespace SA3D.Texturing
{
	/// <summary>
	/// Texture palette to be used with Index textures
	/// </summary>
	public class TexturePalette
	{
		/// <summary>
		/// The default grayscale palette for rendering 8 bit index textures.
		/// </summary>
		public static TexturePalette Index8Palette { get; }

		/// <summary>
		/// The default grayscale palette for rendering 4 bit index textures.
		/// </summary>
		public static TexturePalette Index4Palette { get; }


		/// <summary>
		/// Pixeldata in the palette
		/// </summary>
		private readonly byte[] _colorData;

		/// <summary>
		/// Name of the palette
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Number of pixels in this palette
		/// </summary>
		public int Width => _colorData.Length / 4;

		/// <summary>
		/// Pixeldata in the palette
		/// </summary>
		public ReadOnlySpan<byte> ColorData
			=> new(_colorData);

		static TexturePalette()
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

			Index4Palette = new(index4);
			Index8Palette = new(index8);
		}

		/// <summary>
		/// Create a new palette off pixel data
		/// </summary>
		/// <param name="name">Name of the palette</param>
		/// <param name="colorData">Color pixels</param>
		public TexturePalette(string name, byte[] colorData)
		{
			Name = name;
			_colorData = colorData;
		}

		/// <summary>
		/// Create a new palette off pixel data
		/// </summary>
		/// <param name="colorData">Color pixels</param>
		public TexturePalette(byte[] colorData) : this(string.Empty, colorData) { }


		/// <summary>
		/// Returns either <see cref="Index4Palette"/> or <see cref="Index8Palette"/> based on <paramref name="index4"/>.
		/// </summary>
		/// <param name="index4">Specifies the default palette to get.</param>
		/// <returns>The default palette.</returns>
		public static TexturePalette GetDefaultPalette(bool index4)
		{
			return index4 ? Index4Palette : Index8Palette;
		}
	}
}
