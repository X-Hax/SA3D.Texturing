using SA3D.Texturing.MipMapping;
using SA3D.Texturing.ReadOnly;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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
		public static Image<Rgba32> ToImageSharp(this ITexture texture)
		{
			return Image.LoadPixelData<Rgba32>(texture.GetRGBA32Data(), texture.Width, texture.Height);
		}

		/// <summary>
		/// Converts a texture palette to an image. The palette gets divided into rows and "stacked" from top to bottom.
		/// </summary>
		/// <param name="palette">The palette to convert.</param>
		/// <param name="rowWidth">Number of pixels a single row should occupy.</param>
		/// <returns>The converted image.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static Image<Rgba32> ToImageSharp(this ITexturePalette palette, int? rowWidth = null)
		{
			if(rowWidth == null)
			{
				rowWidth = palette.Width;
			}
			else if(palette.Width % rowWidth != 0)
			{
				throw new ArgumentException($"Palette Width ({palette.Width}) is not a multiple of specified row width ({rowWidth})!");
			}

			return Image.LoadPixelData<Rgba32>(palette.GetColorData(), rowWidth.Value, palette.Width / rowWidth.Value);
		}

		/// <summary>
		/// Converts an index texture to an image sharp image.
		/// </summary>
		/// <param name="texture">The texture to convert.</param>
		/// <returns>The converted image.</returns>
		public static Image<L8> ToIndexedImageSharp(this IIndexTexture texture)
		{
			return Image.LoadPixelData<L8>(texture.GetIndexPixelData(), texture.Width, texture.Height);
		}

		/// <summary>
		/// Converts a single mip map from a mip map set to an RGBA32 ImageSharp image
		/// </summary>
		/// <param name="mipMap">The mip map to convert</param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static Image<Rgba32> ToImageSharp(this IMipMapLevel mipMap)
		{
			ReadOnlySpan<byte> data = mipMap.Data;

			if(mipMap.TextureType != TextureType.RGBA32)
			{
				bool index4 = mipMap.TextureType == TextureType.Index4;
				data = TextureUtilities.ApplyPaletteToIndexTexture(ITexturePalette.GetDefaultPalette(index4).GetColorData(), data, index4);
			}

			return Image.LoadPixelData<Rgba32>(data, mipMap.Width, mipMap.Height);
		}

		/// <summary>
		/// Converts a single mip map from a mip map set to an Indexed ImageSharp image
		/// </summary>
		/// <param name="mipMap">The mip map to convert</param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static Image<L8> ToIndexedImageSharp(this IMipMapLevel mipMap)
		{
			if(mipMap.TextureType == TextureType.RGBA32)
			{
				throw new InvalidOperationException("Mip map set does not have an index texture type!");
			}

			return Image.LoadPixelData<L8>(mipMap.Data, mipMap.Width, mipMap.Height);
		}

		/// <summary>
		/// Converts an image sharp image to a colored texture.
		/// </summary>
		/// <param name="image">The image to convert.</param>
		/// <returns>The converted texture.</returns>
		public static ITexture ToTexture(this Image<Rgba32> image)
		{
			byte[] data = new byte[image.Width * image.Height * 4];
			image.CopyPixelDataTo(new Span<byte>(data));
			return new ReadOnlyTexture(data, image.Width, image.Height);
		}

		/// <summary>
		/// Converts an image sharp image to a texture palette. The rows of the image get appened from top to bottom.
		/// </summary>
		/// <param name="image">The image to convert.</param>
		/// <returns>The converted palette.</returns>
		public static ITexturePalette ToPalette(this Image<Rgba32> image)
		{
			byte[] data = new byte[image.Width * image.Height * 4];
			image.CopyPixelDataTo(new Span<byte>(data));
			return new ReadOnlyTexturePalette(data);
		}
	}
}
