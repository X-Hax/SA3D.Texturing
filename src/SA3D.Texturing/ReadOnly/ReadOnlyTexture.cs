using SA3D.Texturing.MipMapping;
using System;

namespace SA3D.Texturing.ReadOnly
{
	/// <summary>
	/// Readonly RGBA32 color texture
	/// </summary>
	public sealed class ReadOnlyTexture : ITexture, IMipMapped
	{
		/// <summary>
		/// Texture data
		/// </summary>
		public ReadOnlyMipMapSet TextureData { get; }

		/// <inheritdoc/>
		public string Name { get; init; }

		/// <inheritdoc/>
		public uint GlobalIndex { get; init; }

		/// <inheritdoc/>
		public int Width => TextureData.BaseWidth;

		/// <inheritdoc/>
		public int Height => TextureData.BaseHeight;

		/// <inheritdoc/>
		public int OverrideWidth { get; init; }

		/// <inheritdoc/>
		public int OverrideHeight { get; init; }

		/// <inheritdoc/>
		public bool HasMipMaps => TextureData.LevelCount > 1;

		IMipMapSet IMipMapped.MipMaps => TextureData;


		/// <summary>
		/// Creates a new texture off a mip map set
		/// </summary>
		/// <param name="textureData">Texture data to use</param>
		public ReadOnlyTexture(ReadOnlyMipMapSet textureData)
		{
			if(textureData.TextureType != TextureType.RGBA32)
			{
				throw new ArgumentException("The texture-datas type is not RGBA32!", nameof(textureData));
			}

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
		public ReadOnlyTexture(ReadOnlySpan<byte> data, int width, int height, bool generateMipMaps = false)
			: this(ReadOnlyMipMapSet.GenerateMipMaps(data, width, height, TextureType.RGBA32, TextureType.RGBA32, out _, !generateMipMaps)) { }

		/// <summary>
		/// Creates a new texture off another texture
		/// </summary>
		/// <param name="texture">Texture to use</param>
		/// <param name="generateMipMaps">Whether to generate mip maps</param>
		/// <returns></returns>
		public ReadOnlyTexture(ITexture texture, bool generateMipMaps = false)
			: this(texture.GetRGBA32Data(), texture.Width, texture.Height, generateMipMaps)
		{
			OverrideWidth = texture.OverrideWidth;
			OverrideHeight = texture.OverrideHeight;
		}


		/// <inheritdoc/>
		public ReadOnlySpan<byte> GetRGBA32Data(int mipMapLevel = 0)
		{
			return TextureData[mipMapLevel].Data;
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
