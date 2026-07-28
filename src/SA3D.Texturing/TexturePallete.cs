using System;

namespace SA3D.Texturing
{
	/// <summary>
	/// Texture palette to be used with Index textures
	/// </summary>
	public sealed class TexturePalette : ITexturePalette
	{
		/// <summary>
		/// Color data of the palette
		/// </summary>
		public byte[] ColorData { get; private set; }

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <inheritdoc/>
		public int Width => ColorData.Length / 4;


		/// <summary>
		/// Create a new palette off color data
		/// </summary>
		/// <param name="name">Name of the palette</param>
		/// <param name="colorData">Palette colors</param>
		public TexturePalette(string name, byte[] colorData)
		{
			if(colorData.Length % 4 != 0)
			{
				throw new ArgumentException("Data has an invalid length; Must be multiple of 4!", nameof(ColorData));
			}

			Name = name;
			ColorData = colorData;
		}

		/// <summary>
		/// Create a new palette off pixel data
		/// </summary>
		/// <param name="colorData">Palette Colors</param>
		public TexturePalette(byte[] colorData) : this(string.Empty, colorData) { }


		/// <inheritdoc/>
		public ReadOnlySpan<byte> GetColorData()
		{
			return ColorData;
		}


		/// <summary>
		/// Replaces the palettes color data
		/// </summary>
		/// <param name="data"></param>
		/// <exception cref="ArgumentException"></exception>
		public void SetColors(byte[] data)
		{
			if(data.Length % 4 != 0)
			{
				throw new ArgumentException("Data has an invalid length; Must be multiple of 4!", nameof(ColorData));
			}

			ColorData = data;
		}
	}
}
