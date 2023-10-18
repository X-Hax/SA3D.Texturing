using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System;

namespace SA3D.Texturing
{
	/// <summary>
	/// Extension methods involving the Image Sharp API
	/// </summary>
	public static class ImageSharpExtensions
	{
		/// <summary>
		/// Converts a textures color representation to an image sharp image.
		/// </summary>
		/// <param name="texture">The texture to convert.</param>
		/// <returns>The converted image.</returns>
		public static Image<Rgba32> ToImageSharp(this Texture texture)
		{
			return Image.LoadPixelData<Rgba32>(texture.GetColorPixels(), texture.Width, texture.Height);
		}

		/// <summary>
		/// Converts a texture palette to an image. The palette gets divided into rows and "stacked" from top to bottom.
		/// </summary>
		/// <param name="palette">The palette to convert.</param>
		/// <param name="rowWidth">Number of pixels a single row should occupy.</param>
		/// <returns>The converted image.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static Image<Rgba32> ToImageSharp(this TexturePalette palette, int? rowWidth = null)
		{
			if(rowWidth == null)
			{
				rowWidth = palette.Width;
			}
			else if(palette.Width % rowWidth != 0)
			{
				throw new ArgumentException($"Palette Width ({palette.Width}) is not a multiple of specified row width ({rowWidth})!");
			}

			return Image.LoadPixelData<Rgba32>(palette.ColorData, rowWidth.Value, palette.Width / rowWidth.Value);
		}

		/// <summary>
		/// Converts an index texture to an image sharp image.
		/// </summary>
		/// <param name="texture">The texture to convert.</param>
		/// <returns>The converted image.</returns>
		public static Image<A8> ToIndexedImageSharp(this IndexTexture texture)
		{
			return Image.LoadPixelData<A8>(texture.Data, texture.Width, texture.Height);
		}

		/// <summary>
		/// Converts an image sharp image to a colored texture.
		/// </summary>
		/// <param name="image">The image to convert.</param>
		/// <returns>The converted texture.</returns>
		public static ColorTexture ToTexture(this Image<Rgba32> image)
		{
			byte[] data = new byte[image.Width * image.Height * 4];
			image.CopyPixelDataTo(new Span<byte>(data));
			return new ColorTexture(image.Width, image.Height, data);
		}

		/// <summary>
		/// Converts an image sharp image to a texture palette. The rows of the image get appened from top to bottom.
		/// </summary>
		/// <param name="image">The image to convert.</param>
		/// <returns>The converted palette.</returns>
		public static TexturePalette ToPalette(this Image<Rgba32> image)
		{
			byte[] data = new byte[image.Width * image.Height * 4];
			image.CopyPixelDataTo(new Span<byte>(data));
			return new TexturePalette(data);
		}

		/// <summary>
		/// Creates a palette quantizer that can be used to convert a color image to an indexed image.
		/// </summary>
		/// <param name="palette">The palette to match the colors against.</param>
		/// <param name="width">The number of colors from the palette to use.</param>
		/// <param name="offset">The offset at which to start using colors from the palette.</param>
		/// <param name="dither">Whether to allow dithering when quantizing.</param>
		/// <returns>The quantizer.</returns>
		public static PaletteQuantizer CreatePaletteQuantizer(this TexturePalette palette, int width, int offset, bool dither)
		{
			Color[] paletteColors = new Color[width];
			ReadOnlySpan<byte> colorData = palette.ColorData;

			for(int i = 0; i < width; i++)
			{
				ReadOnlySpan<byte> color = colorData.Slice((offset + i) * 4, 4);
				paletteColors[i] = new Rgba32(color[0], color[1], color[2], color[3]);
			}

			return new PaletteQuantizer(
				new(paletteColors),
				new QuantizerOptions()
				{
					MaxColors = width,
					Dither = dither ? QuantizerConstants.DefaultDither : null,
				});
		}
	}
}
