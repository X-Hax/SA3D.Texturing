using System;

namespace SA3D.Texturing
{
	/// <summary>
	/// Color texture interface
	/// </summary>
	public interface ITexture
	{
		/// <summary>
		/// Texture name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Global texture index.
		/// </summary>
		public uint GlobalIndex { get; }

		/// <summary>
		/// Width of the texture in pixels.
		/// </summary>
		public int Width { get; }

		/// <summary>
		/// Height of the texture in pixels.
		/// </summary>
		public int Height { get; }

		/// <summary>
		/// Texture width expected by the game.
		/// </summary>
		public int OverrideWidth { get; }

		/// <summary>
		/// Texture height expected by the game.
		/// </summary>
		public int OverrideHeight { get; }

		/// <summary>
		/// Wether the texture has mip maps
		/// </summary>
		public bool HasMipMaps { get; }

		/// <summary>
		/// Texture data type
		/// </summary>
		public TextureType TextureType => TextureType.RGBA32;


		/// <summary>
		/// Returns the image in RGBA32 format.
		/// </summary>
		/// <param name="mipMapLevel">Mip map level of which to get the data</param>
		public ReadOnlySpan<byte> GetRGBA32Data(int mipMapLevel = 0);

		/// <summary>
		/// Checks whether any pixel has an alpha value below 255.
		/// </summary>
		public bool CheckIsTransparent();
	}

	/// <summary>
	/// Texture extension methods
	/// </summary>
	public static class TextureExtensions
	{
		extension(ITexture tex)
		{
			/// <summary>
			/// Returns <see cref="ITexture.OverrideWidth"/> if it is > 0. Otherwise returns <see cref="ITexture.Width"/>
			/// </summary>
			public int RealWidth => tex.OverrideWidth == 0 ? tex.Width : tex.OverrideWidth;

			/// <summary>
			/// Returns <see cref="ITexture.OverrideHeight"/> if it is > 0. Otherwise returns <see cref="ITexture.Height"/>
			/// </summary>
			public int RealHeight => tex.OverrideHeight == 0 ? tex.Height : tex.OverrideHeight;

		}
	}
}
