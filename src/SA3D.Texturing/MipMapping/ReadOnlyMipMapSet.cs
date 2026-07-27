using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SA3D.Texturing.MipMapping
{
	/// <summary>
	/// Read only mip map set
	/// </summary>
	public sealed class ReadOnlyMipMapSet : IMipMapSet<ReadOnlyMipMapLevel>
	{
		private readonly ReadOnlyMipMapLevel[] _mipMaps;

		/// <inheritdoc/>
		public TextureType TextureType { get; }

		/// <inheritdoc/>
		public int LevelCount => _mipMaps.Length;

		/// <inheritdoc/>
		public ReadOnlyMipMapLevel this[int level]
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

		private ReadOnlyMipMapSet(ReadOnlyMipMapLevel[] mipMaps, TextureType textureType)
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
		public ReadOnlyMipMapSet(int width, int height, TextureType textureType, bool level0Only = false)
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
				_mipMaps = new ReadOnlyMipMapLevel[sizes.Length];

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
		/// <typeparam name="T"></typeparam>
		/// <param name="set"></param>
		/// <returns></returns>
		public static ReadOnlyMipMapSet Copy<T>(IMipMapSet<T> set) where T : IMipMapLevel
		{
			return new([.. set.Select(x => new ReadOnlyMipMapLevel(x.Data.ToArray(), x.Width, x.Height, x.Level))], set.TextureType);
		}

		/// <inheritdoc/>
		public IEnumerator<ReadOnlyMipMapLevel> GetEnumerator()
		{
			return (IEnumerator<ReadOnlyMipMapLevel>)_mipMaps.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
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
		public static ReadOnlyMipMapSet GenerateMipMaps<C>(
			ReadOnlySpan<byte> textureData,
			int width,
			int height,
			TextureType inputTextureDataType,
			TextureType outputTextureDataType,
			IQuantizer<C>? indexQuantizer,
			IResampler resampler,
			bool level0Only = false) where C : unmanaged, IPixel<C>
		{
			MipMapSet mipmaps = MipMapSet.GenerateMipMaps(textureData, width, height, inputTextureDataType, outputTextureDataType, indexQuantizer, resampler, level0Only);
			return new([.. mipmaps.Select(x => new ReadOnlyMipMapLevel(x.Data, x.Width, x.Height, x.Level))], mipmaps.TextureType);
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
		public static ReadOnlyMipMapSet GenerateMipMaps(
			ReadOnlySpan<byte> textureData,
			int width,
			int height,
			TextureType inputTextureDataType,
			TextureType outputTextureDataType,
			out byte[]? paletteColors,
			bool level0Only = false,
			bool dither = true)
		{
			MipMapSet mipmaps = MipMapSet.GenerateMipMaps(textureData, width, height, inputTextureDataType, outputTextureDataType, out paletteColors, level0Only, dither);
			return new([.. mipmaps.Select(x => new ReadOnlyMipMapLevel(x.Data, x.Width, x.Height, x.Level))], mipmaps.TextureType);
		}
	}
}
