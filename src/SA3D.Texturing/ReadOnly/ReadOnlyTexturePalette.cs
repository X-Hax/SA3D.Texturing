using System;

namespace SA3D.Texturing.ReadOnly
{
	/// <summary>
	/// Readonly texture palette
	/// </summary>
	public sealed class ReadOnlyTexturePalette : ITexturePalette
	{
		private readonly byte[] _colorData;

		/// <inheritdoc/>
		public string Name { get; }

		/// <inheritdoc/>
		public int Width => _colorData.Length / 4;


		/// <summary>
		/// Create a new palette off color data
		/// </summary>
		/// <param name="name">Name of the palette</param>
		/// <param name="colorData">Color pixels</param>
		public ReadOnlyTexturePalette(string name, byte[] colorData)
		{
			Name = name;
			_colorData = colorData;
		}

		/// <summary>
		/// Create a new palette off color data
		/// </summary>
		/// <param name="colorData">Palette colors</param>
		public ReadOnlyTexturePalette(byte[] colorData) : this(string.Empty, colorData) { }

		/// <inheritdoc/>
		public ReadOnlySpan<byte> GetColorData()
		{
			return _colorData;
		}
	}
}
