namespace SA3D.Texturing
{
	/// <summary>
	/// Determines a textures pixel data type and usage
	/// </summary>
	public enum TextureType
	{
		/// <summary>
		/// RGBA32
		/// </summary>
		RGBA32,

		/// <summary>
		/// 8 bit indices
		/// </summary>
		Index8,

		/// <summary>
		/// 4 bit indices
		/// </summary>
		Index4
	}

	/// <summary>
	/// Extension methods for Texture type
	/// </summary>
	public static class TextureTypeExtensions
	{
		/// <summary>
		/// Returns the bytes per pixel for the type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static int GetBytesPerPixel(this TextureType type)
		{
			return type == TextureType.RGBA32 ? 4 : 1;
		}

		/// <summary>
		/// Returns the palette size requirement for the type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static int GetPaletteWidth(this TextureType type)
		{
			return type switch
			{
				TextureType.Index8 => 256,
				TextureType.Index4 => 16,
				_ => 0,
			};
		}
	}
}
