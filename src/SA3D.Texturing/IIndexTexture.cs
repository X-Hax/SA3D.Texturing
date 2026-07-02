using System;

namespace SA3D.Texturing
{
	/// <summary>
	/// Index texture interface
	/// </summary>
	public interface IIndexTexture : ITexture
	{
		/// <summary>
		/// Palette used to generate colored image data. If none is passed, grayscale values will be used
		/// </summary>
		public ITexturePalette? Palette { get; set; }

		/// <summary>
		/// Color-row-index of the palette to use (Row size of 16 when <see cref="IsIndex4"/>, otherwise 256).
		/// </summary>
		public int PaletteRow { get; set; }

		/// <summary>
		/// Whether the indices are actually 4 bits big, instead of 8 bits.
		/// <br/> When setting to <see langword="true"/>, the lower 4 bits of every byte will be ignored.
		/// </summary>
		public bool IsIndex4 { get; }

		/// <summary>
		/// Returns the image index pixel data, where each pixel is 1 byte
		/// </summary>
		/// <returns>Index pixel data</returns>
		public ReadOnlySpan<byte> GetIndexPixelData();

		/// <summary>
		/// Gets palette color data that will actually be used by the texture
		/// </summary>
		/// <returns></returns>
		public ReadOnlySpan<byte> GetUsedPaletteColors()
		{
			ReadOnlySpan<byte> colorData = (Palette ?? ITexturePalette.GetDefaultPalette(IsIndex4)).GetColorData();
			int paletteSize = (IsIndex4 ? 16 : 256) * 4;
			ReadOnlySpan<byte> result = colorData[(paletteSize * PaletteRow % colorData.Length)..];

			if(result.Length > paletteSize)
			{
				result = result[..paletteSize];
			}

			return result;
		}


		bool ITexture.CheckIsTransparent()
		{
			return TextureUtilities.CheckIndexTextureUsesTransparency(GetUsedPaletteColors(), GetIndexPixelData(), IsIndex4);
		}

		ReadOnlySpan<byte> ITexture.GetPixelData()
		{
			return TextureUtilities.ApplyPaletteToIndexTexture(GetUsedPaletteColors(), GetIndexPixelData(), IsIndex4);
		}
	}
}
