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

		TextureType ITexture.TextureType => IsIndex4 ? TextureType.Index4 : TextureType.Index8;


		/// <summary>
		/// Returns the image index pixel data, where each pixel is 1 byte
		/// </summary>+
		/// <param name="mipMapLevel">Mip map level of which to get the data</param>
		public ReadOnlySpan<byte> GetIndexPixelData(int mipMapLevel = 0);

		ReadOnlySpan<byte> ITexture.GetRGBA32Data(int mipmaplevel)
		{
			return TextureUtilities.ApplyPaletteToIndexTexture(this.GetUsedPaletteColors(), GetIndexPixelData(mipmaplevel), IsIndex4);
		}
	}

	/// <summary>
	/// Index texture extensions
	/// </summary>
	public static class IndexTextureExtensions
	{
		/// <summary>
		/// Gets palette color data that will actually be used by the texture
		/// </summary>
		/// <returns></returns>
		public static ReadOnlySpan<byte> GetUsedPaletteColors(this IIndexTexture texture)
		{
			ReadOnlySpan<byte> colorData = (texture.Palette ?? ITexturePalette.GetDefaultPalette(texture.IsIndex4)).GetColorData();
			int paletteSize = (texture.IsIndex4 ? 16 : 256) * 4;
			ReadOnlySpan<byte> result = colorData[(paletteSize * texture.PaletteRow % colorData.Length)..];

			if(result.Length > paletteSize)
			{
				result = result[..paletteSize];
			}

			return result;
		}
	}
}
