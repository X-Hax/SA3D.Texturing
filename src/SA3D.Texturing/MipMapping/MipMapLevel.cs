using System;

namespace SA3D.Texturing.MipMapping
{
	/// <summary>
	/// Texture mip-map level
	/// </summary>
	public readonly struct MipMapLevel : IMipMapLevel
	{
		/// <summary>
		/// Texture data
		/// </summary>
		public byte[] Data { get; }

		/// <inheritdoc/>
		public int Width { get; }

		/// <inheritdoc/>
		public int Height { get; }

		/// <inheritdoc/>
		public int Level { get; }

		ReadOnlySpan<byte> IMipMapLevel.Data => Data;


		internal MipMapLevel(byte[] data, int width, int height, int level)
		{
			Data = data;
			Width = width;
			Height = height;
			Level = level;
		}
	}
}
