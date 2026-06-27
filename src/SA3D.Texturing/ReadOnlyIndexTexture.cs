using System;
using System.IO;

namespace SA3D.Texturing
{
	/// <summary>
	/// Read-only index texture
	/// </summary>
	public class ReadOnlyIndexTexture : IIndexTexture
	{
		private readonly byte[] _data;

		/// <inheritdoc/>
		public string Name { get; init; }

		/// <inheritdoc/>
		public uint GlobalIndex { get; init; }

		/// <inheritdoc/>
		public int Width { get; }

		/// <inheritdoc/>
		public int Height { get; }

		/// <inheritdoc/>
		public int OverrideWidth { get; init; }

		/// <inheritdoc/>
		public int OverrideHeight { get; init; }


		/// <inheritdoc/>
		public ITexturePalette? Palette { get; init; }

		/// <inheritdoc/>
		public int PaletteRow { get; init; }

		/// <inheritdoc/>
		public bool IsIndex4 { get; init; }


		/// <summary>
		/// Creates a new index texture from preexisting data.
		/// </summary>
		/// <param name="width">Width of the texture in pixels.</param>
		/// <param name="height">Height of the texture in pixels.</param>
		/// <param name="data">Raw pixel data to use.</param>
		/// <exception cref="InvalidDataException"></exception>
		public ReadOnlyIndexTexture(int width, int height, byte[] data)
		{
			if(width < 1 || height < 1)
			{
				throw new ArgumentException("Dimensions invalid! Width and height have to be at least 1!");
			}

			int expectedDataLength = width * height;
			if(data.Length != expectedDataLength)
			{
				throw new InvalidDataException($"Data length does not match expectations! Is: {data.Length}, should be: {expectedDataLength}");
			}

			_data = data;
			Width = width;
			Height = height;
			Name = string.Empty;
		}

		/// <summary>
		/// Creates a new index texture from another index texture
		/// </summary>
		/// <param name="texture"></param>
		public ReadOnlyIndexTexture(IIndexTexture texture) : this(texture.Width, texture.Height, texture.GetIndexPixelData().ToArray())
		{
			Name = texture.Name;
			GlobalIndex = texture.GlobalIndex;
			OverrideWidth = texture.OverrideWidth;
			OverrideHeight = texture.OverrideHeight;

			Palette = texture.Palette;
			PaletteRow = texture.PaletteRow;
			IsIndex4 = texture.IsIndex4;
		}


		/// <inheritdoc/>
		public ReadOnlySpan<byte> GetIndexPixelData()
		{
			return _data;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"[RO|ID] \"{Name}\": {Width}x{Height}, ({((ITexture)this).RealWidth}x{((ITexture)this).RealWidth}) {GlobalIndex}";
		}
	}
}
