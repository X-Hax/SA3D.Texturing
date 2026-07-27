using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SA3D.Texturing.MipMapping
{
	/// <summary>
	/// Set of mip maps
	/// </summary>
	public sealed class MipMapSet : IMipMapSet, IEnumerable<MipMapLevel>
	{
		private readonly MipMapLevel[] _mipMaps;

		/// <inheritdoc/>
		public TextureType TextureType { get; }

		/// <inheritdoc/>
		public int LevelCount => _mipMaps.Length;


		/// <summary>
		/// Get mip map texture for the specific level
		/// </summary>
		/// <param name="level">Level for which to retrieve the mip map</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public MipMapLevel this[int level]
		{
			get
			{
				if(level > 0 && LevelCount == 1)
				{
					throw new ArgumentOutOfRangeException(nameof(level), $"Tried accessing mip map level {level}, but texture has no (additional) mip maps!");
				}
				else if(LevelCount <= level)
				{
					throw new ArgumentOutOfRangeException(nameof(level), $"Tried accessing mip map level {level}, but texture only has mip maps available up to level {LevelCount - 1}!");
				}

				return _mipMaps[level];
			}
		}

		IMipMapLevel IMipMapSet.this[int level] => this[level];


		private MipMapSet(MipMapLevel[] mipMaps, TextureType textureType)
		{
			_mipMaps = mipMaps;
			TextureType = textureType;
		}

		/// <summary>
		/// Creates a new, blank mip map set.
		/// </summary>
		/// <param name="width">Width of the texture at level 0</param>
		/// <param name="height">Height of the texture at level 0</param>
		/// <param name="textureType">Texture type of the mip maps</param>
		/// <param name="level0Only">Do not generate higher levels (effectively "don't use mip maps)</param>
		public MipMapSet(int width, int height, TextureType textureType, bool level0Only = false)
		{
			TextureType = textureType;

			int bpp = textureType.GetBytesPerPixel();
			if(level0Only)
			{
				_mipMaps = [new(new byte[width * height * bpp], width, height, 0)];
			}
			else
			{
				(int, int)[] sizes = MipMapUtils.GetMipMapSizes(width, height);
				_mipMaps = new MipMapLevel[sizes.Length];

				for(int i = 0; i < sizes.Length; i++)
				{
					(int mmWidth, int mmHeight) = sizes[i];
					_mipMaps[i] = new(new byte[mmWidth * mmHeight * bpp], mmWidth, mmHeight, i);
				}
			}
		}

		/// <summary>
		/// Creates a copy from another mip map set
		/// </summary>
		/// <param name="set"></param>
		/// <returns></returns>
		public static MipMapSet Copy(IMipMapSet set)
		{
			return new([.. set.Select(x => new MipMapLevel(x.Data.ToArray(), x.Width, x.Height, x.Level))], set.TextureType);
		}


		/// <inheritdoc/>
		public IEnumerator<MipMapLevel> GetEnumerator()
		{
			return (IEnumerator<MipMapLevel>)_mipMaps.GetEnumerator();
		}

		IEnumerator<IMipMapLevel> IEnumerable<IMipMapLevel>.GetEnumerator()
		{
			return _mipMaps.Cast<IMipMapLevel>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		private static void QuantizeIndexImage<C>(Image<C> image, IQuantizer<C> quantizer, bool index4, Span<byte> output) where C : unmanaged, IPixel<C>
		{
			IndexedImageFrame<C> mipMapFrame = quantizer.QuantizeFrame(image.Frames[0], new(0, 0, image.Width, image.Height));

			for(int y = 0; y < image.Height; y++)
			{
				mipMapFrame.DangerousGetRowSpan(y).CopyTo(output[(y * image.Height)..]);
			}

			if(index4)
			{
				for(int j = 0; j < output.Length; j++)
				{
					output[j] |= (byte)(output[j] << 4);
				}
			}
		}

		/// <summary>
		/// Generates mip maps for texture data (includes level 0)
		/// </summary>
		/// <typeparam name="C"></typeparam>
		/// <param name="textureData">The texture data to generate mip maps for</param>
		/// <param name="width">Width of the texture</param>
		/// <param name="height">Height of the texture</param>
		/// <param name="inputTextureDataType">Type of <paramref name="textureData"/></param>
		/// <param name="outputTextureDataType">Type of the output data</param>
		/// <param name="indexQuantizer">Quantizer to use for when generating index mipmaps off an rgba texture</param>
		/// <param name="resampler">Resampler to use when downscaling to higher mipmap levels</param>
		/// <param name="level0Only">Do not generate higher levels (effectively "don't use mip maps)</param>
		/// <returns></returns>
		public static MipMapSet GenerateMipMaps<C>(
			ReadOnlySpan<byte> textureData,
			int width,
			int height,
			TextureType inputTextureDataType,
			TextureType outputTextureDataType,
			IQuantizer<C>? indexQuantizer,
			IResampler resampler,
			bool level0Only = false) where C : unmanaged, IPixel<C>
		{
			MipMapSet result = new(width, height, outputTextureDataType, level0Only);

			if(inputTextureDataType != TextureType.RGBA32 || outputTextureDataType == TextureType.RGBA32)
			{
				indexQuantizer = null;
			}

			Image<C> image = Image.LoadPixelData<C>(textureData, width, height);
			foreach(MipMapLevel mipmap in result)
			{
				image.Mutate(x => x.Resize(mipmap.Width, mipmap.Height, resampler));

				if(indexQuantizer != null)
				{
					QuantizeIndexImage(image, indexQuantizer, outputTextureDataType == TextureType.Index4, mipmap.Data);
				}
				else
				{
					image.CopyPixelDataTo(mipmap.Data);
				}
			}

			return result;
		}

		/// <summary>
		/// Generates mip maps for texture data (includes level 0)
		/// </summary>
		/// <param name="textureData">Texture data to generate mip maps for</param>
		/// <param name="width">Width of the texture</param>
		/// <param name="height">Height of the texture</param>
		/// <param name="inputTextureDataType">Type of <paramref name="textureData"/></param>
		/// <param name="outputTextureDataType">Type of the output data</param>
		/// <param name="paletteColors">Generated palette colors</param>
		/// <param name="level0Only">Do not generate higher levels (effectively "don't use mip maps)</param>
		/// <param name="dither">Apply dithering where applicable</param>
		/// <returns></returns>
		public static MipMapSet GenerateMipMaps(
			ReadOnlySpan<byte> textureData,
			int width,
			int height,
			TextureType inputTextureDataType,
			TextureType outputTextureDataType,
			out byte[]? paletteColors,
			bool level0Only = false,
			bool dither = true)
		{
			if(outputTextureDataType == TextureType.RGBA32)
			{
				paletteColors = null;
				if(inputTextureDataType != TextureType.RGBA32)
				{
					bool index4 = inputTextureDataType == TextureType.Index4;
					textureData = TextureUtilities.ApplyPaletteToIndexTexture(ITexturePalette.GetDefaultPalette(index4).GetColorData(), textureData, index4);
				}

				return GenerateMipMaps<Rgba32>(
					textureData,
					width,
					height,
					inputTextureDataType,
					outputTextureDataType,
					null,
					KnownResamplers.Bicubic,
					level0Only
				);
			}

			if(inputTextureDataType != TextureType.RGBA32)
			{
				paletteColors = null;

				if(inputTextureDataType != outputTextureDataType)
				{
					byte[] newTextureData = textureData.ToArray();

					if(inputTextureDataType == TextureType.Index4)
					{
						TextureUtilities.ConvertIndex4To8(newTextureData);
					}
					else
					{
						TextureUtilities.ConvertIndex8To4(newTextureData);
					}

					textureData = newTextureData;
				}

				return GenerateMipMaps<L8>(
					textureData,
					width,
					height,
					inputTextureDataType,
					outputTextureDataType,
					null,
					KnownResamplers.NearestNeighbor,
					level0Only
				);
			}

			paletteColors = TextureUtilities.GeneratePaletteForTexture(
				textureData,
				width,
				height,
				outputTextureDataType == TextureType.Index4,
				dither
			);

			IQuantizer<Rgba32> quantizer = TextureUtilities
				.CreatePaletteQuantizer(paletteColors, dither)
				.CreatePixelSpecificQuantizer<Rgba32>(Configuration.Default);

			return GenerateMipMaps(
				textureData,
				width,
				height,
				inputTextureDataType,
				outputTextureDataType,
				quantizer,
				KnownResamplers.Bicubic,
				level0Only
			);
		}
	}
}
