using System;
using System.IO;

namespace SA3D.Texturing
{
	/// <summary>
	/// Readonly RGBA32 color texture
	/// </summary>
	public sealed class ReadOnlyTexture : ITexture
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


		/// <summary>
		/// Creates a new texture from pixel data
		/// </summary>
		/// <param name="width">Width of the texture in pixels.</param>
		/// <param name="height">Height of the texture in pixels.</param>
		/// <param name="data">Raw pixel data to use.</param>
		/// <exception cref="InvalidDataException"></exception>
		public ReadOnlyTexture(int width, int height, byte[] data)
		{
			if(width < 1 || height < 1)
			{
				throw new ArgumentException("Dimensions invalid! Width and height have to be at least 1!");
			}

			int expectedDataLength = width * height * 4;
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
		/// Creates a new texture from another texture
		/// </summary>
		/// <param name="texture"></param>
		public ReadOnlyTexture(ITexture texture) : this(texture.Width, texture.Height, texture.GetPixelData().ToArray())
		{
			Name = texture.Name;
			GlobalIndex = texture.GlobalIndex;
			OverrideWidth = texture.OverrideWidth;
			OverrideHeight = texture.OverrideHeight;
		}


		/// <inheritdoc/>
		public ReadOnlySpan<byte> GetPixelData()
		{
			return _data;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"[RO] \"{Name}\": {Width}x{Height}, ({((ITexture)this).RealWidth}x{((ITexture)this).RealWidth}) {GlobalIndex}";
		}
	}
}
