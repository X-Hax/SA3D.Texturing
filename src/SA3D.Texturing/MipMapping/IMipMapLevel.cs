using System;

namespace SA3D.Texturing.MipMapping
{
	/// <summary>
	/// Mip map level interface
	/// </summary>
	public interface IMipMapLevel
	{
		/// <summary>
		/// Texture data
		/// </summary>
		public ReadOnlySpan<byte> Data { get; }

		/// <summary>
		/// Width of the mip map
		/// </summary>
		public int Width { get; }

		/// <summary>
		/// Height of the mip map
		/// </summary>
		public int Height { get; }

		/// <summary>
		/// The mip map data texture type
		/// </summary>
		public TextureType TextureType { get; }

		/// <summary>
		/// The mip map level
		/// </summary>
		public int Level { get; }
	}
}
