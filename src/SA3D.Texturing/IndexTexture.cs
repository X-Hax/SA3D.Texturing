using SA3D.Texturing.MipMapping;
using System;

namespace SA3D.Texturing
{
	/// <summary>
	/// Texture consisting of single byte pixels that refer to a palette
	/// </summary>
	public sealed class IndexTexture : IIndexTexture, IMipMapped
	{
		/// <summary>
		/// Texture data
		/// </summary>
		public MipMapSet TextureData
		{
			get;
			set
			{
				if(value.TextureType is not TextureType.Index4 and not TextureType.Index8)
				{
					throw new ArgumentException("The texture-datas type is not an Index type!", nameof(value));
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
		public ITexturePalette? Palette { get; set; }

		/// <inheritdoc/>
		public int PaletteRow { get; set; }

		/// <inheritdoc/>
		public bool IsIndex4 => TextureData.TextureType == TextureType.Index4;

		/// <inheritdoc/>
		public bool HasMipMaps => TextureData.LevelCount > 1;

		IMipMapSet IMipMapped.MipMaps => TextureData;


		/// <summary>
		/// Creates a new index texture off a mip map set
		/// </summary>
		/// <param name="textureData">Texture data to use</param>
		public IndexTexture(MipMapSet textureData)
		{
			Name = string.Empty;
			TextureData = textureData;
		}

		/// <summary>
		/// Creates a new texture off raw index data
		/// </summary>
		/// <param name="data">The index data to use</param>
		/// <param name="width">Width of the texture</param>
		/// <param name="height">Height of the texture</param>
		/// <param name="isIndex4">Whether the data uses 4 bit- instead of 8 bit indices</param>
		/// <param name="generateMipMaps">Whether to generate mip maps</param>
		public IndexTexture(ReadOnlySpan<byte> data, int width, int height, bool isIndex4, bool generateMipMaps = false)
			: this(MipMapSet.GenerateMipMaps(data, width, height, isIndex4 ? TextureType.Index4 : TextureType.Index8, isIndex4 ? TextureType.Index4 : TextureType.Index8, out _, !generateMipMaps)) { }

		/// <summary>
		/// Creates a new texture off another texture
		/// </summary>
		/// <param name="texture">Texture to use</param>
		/// <param name="generateMipMaps">Whether to generate mip maps</param>
		/// <returns></returns>
		public IndexTexture(IIndexTexture texture, bool generateMipMaps = false)
			: this(texture.GetIndexPixelData(), texture.Width, texture.Height, texture.IsIndex4, generateMipMaps)
		{
			OverrideWidth = texture.OverrideWidth;
			OverrideHeight = texture.OverrideHeight;
		}


		/// <inheritdoc/>
		public bool CheckIsTransparent()
		{
			return TextureUtilities.CheckIndexTextureUsesTransparency(this.GetUsedPaletteColors(), GetIndexPixelData(), IsIndex4);
		}


		/// <inheritdoc/>
		public ReadOnlySpan<byte> GetIndexPixelData(int mipMapLevel = 0)
		{
			return TextureData[mipMapLevel].Data;
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
