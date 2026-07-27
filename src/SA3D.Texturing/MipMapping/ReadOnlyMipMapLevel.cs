using System;

namespace SA3D.Texturing.MipMapping
{
	/// <summary>
	/// Read-only texture mip-map level
	/// </summary>
	public readonly struct ReadOnlyMipMapLevel : IMipMapLevel
	{
		private readonly byte[] _data;

		/// <inheritdoc/>
		public ReadOnlySpan<byte> Data => _data;

		/// <inheritdoc/>
		public int Width { get; }

		/// <inheritdoc/>
		public int Height { get; }

		/// <inheritdoc/>
		public TextureType TextureType { get; }

		/// <inheritdoc/>
		public int Level { get; }


		internal ReadOnlyMipMapLevel(byte[] data, int width, int height, TextureType textureType, int level)
		{
			_data = data;
			Width = width;
			Height = height;
			TextureType = textureType;
			Level = level;
		}
	}
}
