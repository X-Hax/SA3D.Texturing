using SA3D.Texturing.MipMapping;
using System;

namespace SA3D.Texturing
{
	/// <summary>
	/// RGBA32 texture.
	/// </summary>
	public sealed class Texture : ITexture, IMipMapped
	{
		/// <summary>
		/// Texture data
		/// </summary>
		public MipMapSet TextureData
		{
			get;
			set
			{
				if(value.TextureType != TextureType.RGBA32)
				{
					throw new ArgumentException("The texture-datas type is not RGBA32!", nameof(value));
				}

				field = value;
			}
		}

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <inheritdoc/>
		public uint GlobalIndex { get; set; }

		/// <inheritdoc/>
		public int Width => TextureData.BaseWidth;

		/// <inheritdoc/>
		public int Height => TextureData.BaseHeight;

		/// <inheritdoc/>
		public int OverrideWidth { get; set; }

		/// <inheritdoc/>
		public int OverrideHeight { get; set; }

		/// <inheritdoc/>
		public bool HasMipMaps => TextureData.LevelCount > 1;

		IMipMapSet IMipMapped.MipMaps => TextureData;


		/// <summary>
		/// Creates a new texture off a mip map set
		/// </summary>
		/// <param name="textureData">Texture data to use</param>
		public Texture(MipMapSet textureData)
		{
			Name = string.Empty;
			TextureData = textureData;
		}

		/// <summary>
		/// Creates a new texture off raw RGBA32 data
		/// </summary>
		/// <param name="data">The RGBA32 data to use</param>
		/// <param name="width">Width of the texture</param>
		/// <param name="height">Height of the texture</param>
		/// <param name="generateMipMaps">Whether to generate mip maps</param>
		public Texture(ReadOnlySpan<byte> data, int width, int height, bool generateMipMaps = false)
			: this(MipMapSet.GenerateMipMaps(data, width, height, TextureType.RGBA32, TextureType.RGBA32, out _, !generateMipMaps)) { }

		/// <summary>
		/// Creates a new texture off another texture
		/// </summary>
		/// <param name="texture">Texture to use</param>
		/// <param name="generateMipMaps">Whether to generate mip maps</param>
		/// <returns></returns>
		public Texture(ITexture texture, bool generateMipMaps = false)
			: this(texture.GetRGBA32Data(), texture.Width, texture.Height, generateMipMaps)
		{
			OverrideWidth = texture.OverrideWidth;
			OverrideHeight = texture.OverrideHeight;
		}


		/// <inheritdoc/>
		public ReadOnlySpan<byte> GetRGBA32Data(int mipMapLevel = 0)
		{
			if(mipMapLevel > 0 && !HasMipMaps)
			{
				throw new ArgumentOutOfRangeException(nameof(mipMapLevel), $"Tried accessing mip map level {mipMapLevel}, but texture has no mip maps!");
			}
			else if(TextureData.LevelCount <= mipMapLevel)
			{
				throw new ArgumentOutOfRangeException(nameof(mipMapLevel), $"Tried accessing mip map level {mipMapLevel}, but texture only has mip maps available up to level {TextureData.LevelCount - 1}!");
			}

			return TextureData.MipMapLevels[mipMapLevel].Data;
		}

		/// <inheritdoc/>
		public bool CheckIsTransparent()
		{
			return TextureUtilities.CheckIsTextureTransparent(GetRGBA32Data());
		}


		/// <inheritdoc/>
		public override string ToString()
		{
			if(OverrideWidth == 0 && OverrideHeight == 0)
			{
				return $"\"{Name}\": {Width}x{Height}, {GlobalIndex}";
			}
			else
			{
				return $"\"{Name}\": {Width}x{Height}, ({this.RealWidth}x{this.RealWidth}) {GlobalIndex}";
			}
		}

	}
}
