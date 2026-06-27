using System;

namespace SA3D.Texturing
{
	/// <summary>
	/// Texture palette to be used with Index textures
	/// </summary>
	public class TexturePalette : ITexturePalette
	{
		private byte[] _colorData;

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <inheritdoc/>
		public int Width => _colorData.Length / 4;

		/// <inheritdoc/>
		public ReadOnlySpan<byte> ColorData
			=> _colorData;


		/// <summary>
		/// Create a new palette off color data
		/// </summary>
		/// <param name="name">Name of the palette</param>
		/// <param name="colorData">Palette colors</param>
		public TexturePalette(string name, byte[] colorData)
		{
			Name = name;
			_colorData = colorData;
		}

		/// <summary>
		/// Create a new palette off pixel data
		/// </summary>
		/// <param name="colorData">Palette Colors</param>
		public TexturePalette(byte[] colorData) : this(string.Empty, colorData) { }


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

			_colorData = data;
		}
	}
}
