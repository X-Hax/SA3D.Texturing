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
		/// Returns <see cref="OverrideWidth"/> if it is > 0. Otherwise returns <see cref="Width"/>
		/// </summary>
		public int RealWidth => OverrideWidth == 0 ? Width : OverrideWidth;

		/// <summary>
		/// Returns <see cref="OverrideHeight"/> if it is > 0. Otherwise returns <see cref="Height"/>
		/// </summary>
		public int RealHeight => OverrideHeight == 0 ? Height : OverrideHeight;


		/// <summary>
		/// Returns the image in RGBA32 format.
		/// </summary>
		/// <returns>RGBA32 formatted byte array.</returns>
		public ReadOnlySpan<byte> GetPixelData();

		/// <summary>
		/// Checks whether any pixel has an alpha value below 255.
		/// </summary>
		/// <returns>Whether any pixel is has an alpha value below 255</returns>
		public bool CheckIsTransparent()
		{
			return TextureUtilities.CheckIsTextureTransparent(GetPixelData());
		}

	}
}
