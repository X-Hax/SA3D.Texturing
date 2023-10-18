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
		/// Sorts the colors in a palette by luminance into a new palette.
		/// </summary>
		/// <param name="palette">The palette to sort the colors of.</param>
		/// <returns>A new palette with the sorted colors.</returns>
		public static TexturePalette SortByLuminance(this TexturePalette palette)
		{
			(int, byte)[] luminanceLUT = new (int, byte)[palette.Width];
			ReadOnlySpan<byte> data = palette.ColorData;

			for(int i = 0; i < luminanceLUT.Length; i++)
			{
				ReadOnlySpan<byte> color = data[(i * 4)..];
				luminanceLUT[i] = (i, GetLuminance(color));
			}

			Array.Sort(luminanceLUT, (a, b) => a.Item2.CompareTo(b.Item2));

			byte[] newPalette = new byte[data.Length];
			Span<byte> destination = newPalette;
			for(int i = 0; i < luminanceLUT.Length; i++)
			{
				int dstIndex = luminanceLUT[i].Item1;
				data.Slice(luminanceLUT[i].Item1 * 4, 4).CopyTo(destination[(i * 4)..]);
			}

			return new(newPalette);
		}

		/// <summary>
		/// Attempts to generate a palette that contains every colors used in a texture.
		/// </summary>
		/// <param name="texture">The color texture to generate a palette for.</param>
		/// <param name="index4">Whether to use 4 bit indices instead of 8.</param>
		/// <param name="palette">The generated palette.</param>
		/// <returns>Whether the palette was successfully generated. If false, the texture has more colors than the palette can hold.</returns>
		public static bool TryGenerateExactPalette(this ColorTexture texture, bool index4, [MaybeNullWhen(false)] out TexturePalette palette)
		{
			palette = null;

			ReadOnlySpan<byte> pixels = texture.GetColorPixels();

			byte[] palleteColors = new byte[index4 ? 64 : 1024];
			Span<byte> destination = palleteColors;

			int writtenBytes = 0;

			for(int pixelAddr = 0; pixelAddr < pixels.Length; pixelAddr += 4)
			{
				ReadOnlySpan<byte> pixel = pixels.Slice(pixelAddr, 4);
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

			palette = new TexturePalette(palleteColors).SortByLuminance();

			return true;
		}

		/// <summary>
		/// Converts a color texture to an index texture with a palette.
		/// </summary>
		/// <param name="texture">The texture to convert.</param>
		/// <param name="index4">Whether to use 4 bit indices instead of 8.</param>
		/// <param name="dither">Whether to utilize dithering.</param>
		/// <returns>The index texture with the palette.</returns>
		public static IndexTexture Palettize(this ColorTexture texture, bool index4, bool dither)
		{
			Image<Rgba32> image = texture.ToImageSharp();
			IndexedImageFrame<Rgba32> frame;
			if(TryGenerateExactPalette(texture, index4, out TexturePalette? palette))
			{
				IQuantizer<Rgba32> quantizer = palette.CreatePaletteQuantizer(index4 ? 16 : 256, 0, dither)
													  .CreatePixelSpecificQuantizer<Rgba32>(Configuration.Default);

				frame = quantizer.QuantizeFrame(image.Frames.RootFrame, new Rectangle(0, 0, image.Width, image.Height));
			}
			else
			{
				QuantizerOptions quantizerOptions = new()
				{
					MaxColors = index4 ? 16 : 256,
					Dither = dither ? QuantizerConstants.DefaultDither : null,
				};

				IQuantizer<Rgba32> wuQuantizer = new WuQuantizer(quantizerOptions).CreatePixelSpecificQuantizer<Rgba32>(Configuration.Default);
				frame = wuQuantizer.BuildPaletteAndQuantizeFrame(image.Frames.RootFrame, new Rectangle(0, 0, image.Width, image.Height));
				byte[] generatedPalette = MemoryMarshal.Cast<Rgba32, byte>(wuQuantizer.Palette.Span).ToArray();
				palette = new TexturePalette(generatedPalette).SortByLuminance();
			}

			byte[] indexData = new byte[frame.Width * frame.Height];
			Span<byte> destination = indexData;

			for(int y = 0; y < frame.Height; y++)
			{
				frame.DangerousGetRowSpan(y).CopyTo(destination[(y * frame.Width)..]);
			}

			return new(texture.Width, texture.Height, indexData, texture.Name, texture.GlobalIndex)
			{
				IsIndex4 = index4,
				Palette = palette
			};
		}

	}
}
