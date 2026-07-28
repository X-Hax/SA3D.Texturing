using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace SA3D.Texturing
{
	/// <summary>
	/// Various utilities for working with textures.
	/// </summary>
	public static class TextureUtilities
	{
		/// <summary>
		/// Calculates the luminance of an RGB24 pixel.
		/// </summary>
		/// <param name="red">The red value of the pixel.</param>
		/// <param name="green">The green value of the pixel.</param>
		/// <param name="blue">The blue value of the pixel.</param>
		/// <returns>The colors luminance.</returns>
		public static byte GetLuminance(byte red, byte green, byte blue)
		{
			return (byte)((0.2126f * red) + (0.7152f * green) + (0.0722f * blue));
		}

		/// <summary>
		/// Calculates the luminance of an RGB24 pixel.
		/// </summary>
		/// <param name="color">Byte source for the pixel.</param>
		/// <returns>The luminance</returns>
		public static byte GetLuminance(ReadOnlySpan<byte> color)
		{
			return GetLuminance(color[0], color[1], color[2]);
		}

		/// <summary>
		/// Converts 8 bit indices to 4 bit indices
		/// </summary>
		/// <param name="data"></param>
		public static void ConvertIndex8To4(Span<byte> data)
		{
			for(int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)((data[i] & 0xF) | (data[i] << 4));
			}
		}

		/// <summary>
		/// Converts 4 bit indices to 8 bit indices
		/// </summary>
		/// <param name="data"></param>
		public static void ConvertIndex4To8(Span<byte> data)
		{
			for(int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(data[i] >> 4);
			}
		}

		/// <summary>
		/// Sorts the colors in a palette by luminance into a new palette.
		/// </summary>
		/// <param name="paletteColors">The palette to sort the colors of.</param>
		/// <param name="indexMap">Array mapping old to new indices</param>
		/// <returns>A new palette with the sorted colors.</returns>
		public static byte[] SortColorDataByLuminance(ReadOnlySpan<byte> paletteColors, out int[] indexMap)
		{
			(int dstIndex, byte luminance)[] luminanceLUT = new (int, byte)[paletteColors.Length / 4];

			for(int i = 0; i < luminanceLUT.Length; i++)
			{
				ReadOnlySpan<byte> color = paletteColors[(i * 4)..];
				luminanceLUT[i] = (i, GetLuminance(color));
			}

			Array.Sort(luminanceLUT, (a, b) => a.luminance.CompareTo(b.luminance));

			byte[] newPalette = new byte[paletteColors.Length];
			Span<byte> destination = newPalette;
			indexMap = new int[luminanceLUT.Length];
			for(int i = 0; i < luminanceLUT.Length; i++)
			{
				int dstIndex = luminanceLUT[i].dstIndex;
				paletteColors.Slice(dstIndex * 4, 4).CopyTo(destination[(i * 4)..]);
				indexMap[dstIndex] = i;
			}

			return newPalette;
		}

		/// <summary>
		/// Attempts to generate a palette that contains every colors used in a texture.
		/// </summary>
		/// <param name="data">RGBA32 image data to generate a palette for.</param>
		/// <param name="index4">Whether to use 4 bit indices instead of 8.</param>
		/// <param name="paletteColors">The generated palette colors.</param>
		/// <returns>Whether the palette was successfully generated. If false, the texture has more colors than the palette can hold.</returns>
		public static bool TryGenerateExactPalette(ReadOnlySpan<byte> data, bool index4, [NotNullWhen(true)] out byte[]? paletteColors)
		{
			paletteColors = null;

			byte[] palleteColors = new byte[index4 ? 64 : 1024];
			Span<byte> destination = palleteColors;

			int writtenBytes = 0;

			for(int pixelAddr = 0; pixelAddr < data.Length; pixelAddr += 4)
			{
				ReadOnlySpan<byte> pixel = data.Slice(pixelAddr, 4);
				for(int paletteIndex = 0; paletteIndex < writtenBytes; paletteIndex += 4)
				{
					if(pixel.SequenceEqual(destination.Slice(paletteIndex, 4)))
					{
						goto found;
					}
				}

				if(writtenBytes >= destination.Length)
				{
					return false;
				}
				else
				{
					pixel.CopyTo(destination[writtenBytes..]);
					writtenBytes += 4;
				}

				found:
				;
			}

			paletteColors = SortColorDataByLuminance(palleteColors, out _);
			return true;
		}

		/// <summary>
		/// Genrates
		/// </summary>
		/// <param name="data">The image data to generate a palette for</param>
		/// <param name="width">Image width</param>
		/// <param name="height">Image height</param>
		/// <param name="index4">Whether the target index texture uses 4 bit indices, instead of 8</param>
		/// <param name="dither">Whether the palette is to be used with dithering</param>
		/// <returns></returns>
		public static byte[] GeneratePaletteForTexture(ReadOnlySpan<byte> data, int width, int height, bool index4, bool dither)
		{
			ReadOnlySpan<byte> rawPalette;

			if(TryGenerateExactPalette(data, index4, out byte[]? exactPaletteColors))
			{
				rawPalette = exactPaletteColors;
			}
			else
			{
				QuantizerOptions quantizerOptions = new()
				{
					MaxColors = index4 ? 16 : 256,
					Dither = dither ? QuantizerConstants.DefaultDither : null,
				};

				IQuantizer<Rgba32> wuQuantizer = new WuQuantizer(quantizerOptions).CreatePixelSpecificQuantizer<Rgba32>(Configuration.Default);
				Image<Rgba32> image = Image.LoadPixelData<Rgba32>(data, width, height);
				wuQuantizer.BuildPalette(new ExtensivePixelSamplingStrategy(), image);
				rawPalette = MemoryMarshal.Cast<Rgba32, byte>(wuQuantizer.Palette.Span);
			}

			return SortColorDataByLuminance(rawPalette, out _);
		}

		/// <summary>
		/// Converts a color texture to an index texture with a palette.
		/// </summary>
		/// <param name="data">RGBA32 image data</param>
		/// <param name="width">Image width</param>
		/// <param name="height">Image width</param>
		/// <param name="index4">Whether to use 4 bit indices instead of 8.</param>
		/// <param name="dither">Whether to utilize dithering.</param>
		/// <param name="indexData">Resulting index image data</param>
		/// <param name="paletteColors">Resulting palette colors</param>
		/// <returns>The index texture with the palette.</returns>
		public static void PalettizeTexture(ReadOnlySpan<byte> data, int width, int height, bool index4, bool dither, out byte[] indexData, out byte[] paletteColors)
		{
			paletteColors = GeneratePaletteForTexture(data, width, height, index4, dither);

			Image<Rgba32> image = Image.LoadPixelData<Rgba32>(data, width, height);

			IQuantizer<Rgba32> quantizer =
				CreatePaletteQuantizer(paletteColors, dither)
				.CreatePixelSpecificQuantizer<Rgba32>(Configuration.Default);

			IndexedImageFrame<Rgba32> frame = quantizer.QuantizeFrame(image.Frames.RootFrame, new Rectangle(0, 0, image.Width, image.Height));

			indexData = new byte[frame.Width * frame.Height];
			Span<byte> destination = indexData;

			for(int y = 0; y < frame.Height; y++)
			{
				frame.DangerousGetRowSpan(y).CopyTo(destination[(y * frame.Width)..]);
			}

			if(index4)
			{
				for(int i = 0; i < indexData.Length; i++)
				{
					indexData[i] |= (byte)(indexData[i] << 4);
				}
			}
		}


		/// <summary>
		/// Checks whether RGBA32 pixel data has pixels that are not fully opaque
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static bool CheckIsTextureTransparent(ReadOnlySpan<byte> data)
		{
			for(int i = 3; i < data.Length; i += 4)
			{
				if(data[i] < 0xFF)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Checks whether an index texture uses transparent palette colors without putting together a color image
		/// </summary>
		/// <param name="colors">Palette colors to use</param>
		/// <param name="data">Index data</param>
		/// <param name="index4">Whether indices are 4 bits in size, rather than the full 8</param>
		/// <returns></returns>
		public static bool CheckIndexTextureUsesTransparency(ReadOnlySpan<byte> colors, ReadOnlySpan<byte> data, bool index4)
		{
			bool[] paletteUsages;

			if(index4)
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

		/// <summary>
		/// Applies palette colors to index data
		/// </summary>
		/// <param name="colors">Palette colors to use</param>
		/// <param name="data">Index data</param>
		/// <param name="index4">Whether indices are 4 bits in size, rather than the full 8</param>
		/// <returns></returns>
		public static byte[] ApplyPaletteToIndexTexture(ReadOnlySpan<byte> colors, ReadOnlySpan<byte> data, bool index4)
		{
			byte[] result = new byte[data.Length * 4];
			Span<byte> destination = result;

			if(index4)
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

		/// <summary>
		/// Creates a palette quantizer that can be used to convert a color image to an indexed image.
		/// </summary>
		/// <param name="paletteColors">Palette colors to use</param>
		/// <param name="dither">Whether to allow dithering when quantizing.</param>
		/// <returns>The quantizer.</returns>
		public static PaletteQuantizer CreatePaletteQuantizer(ReadOnlySpan<byte> paletteColors, bool dither)
		{
			Color[] resultColors = new Color[paletteColors.Length / 4];

			for(int i = 0; i < resultColors.Length; i++)
			{
				int j = i * 4;
				resultColors[i] = new Rgba32(paletteColors[j], paletteColors[j + 1], paletteColors[j + 2], paletteColors[j + 3]);
			}

			return new PaletteQuantizer(
				new(resultColors),
				new QuantizerOptions()
				{
					MaxColors = resultColors.Length,
					Dither = dither ? QuantizerConstants.DefaultDither : null,
				});
		}
	}
}
