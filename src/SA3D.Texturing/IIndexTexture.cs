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
		public ITexturePalette? Palette { get; }

		/// <summary>
		/// Color-row-index of the palette to use (Row size of 16 when <see cref="IsIndex4"/>, otherwise 256).
		/// </summary>
		public int PaletteRow { get; }

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
			int paletteSize = IsIndex4 ? 16 : 256;
			return colorData.Slice(paletteSize * PaletteRow % colorData.Length, paletteSize * 4);
		}


		bool ITexture.CheckIsTransparent()
		{
			bool[] paletteUsages;
			ReadOnlySpan<byte> colors = GetUsedPaletteColors();
			ReadOnlySpan<byte> data = GetIndexPixelData();

			if(IsIndex4)
			{
				paletteUsages = new bool[16];
				for(int i = 0; i < data.Length; i++)
				{
					paletteUsages[data[i] >> 4] = true;
				}
			}
			else
			{
				paletteUsages = new bool[256];
				for(int i = 0; i < data.Length; i++)
				{
					paletteUsages[data[i]] = true;
				}
			}

			for(int i = 0; i < paletteUsages.Length; i++)
			{
				if(paletteUsages[i] && colors[(i * 4) + 3] < 255)
				{
					return true;
				}
			}

			return false;
		}

		ReadOnlySpan<byte> ITexture.GetPixelData()
		{
			byte[] result = new byte[Width * Height * 4];
			Span<byte> destination = result;
			ReadOnlySpan<byte> colors = GetUsedPaletteColors();
			ReadOnlySpan<byte> data = GetIndexPixelData();

			if(IsIndex4)
			{
				for(int i = 0; i < data.Length; i++)
				{
					colors.Slice((data[i] >> 4) * 4, 4).CopyTo(destination[(i * 4)..]);
				}
			}
			else
			{
				for(int i = 0; i < data.Length; i++)
				{
					colors.Slice(data[i] * 4, 4).CopyTo(destination[(i * 4)..]);
				}
			}

			return result;
		}
	}
}
