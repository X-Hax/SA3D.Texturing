using System;
using System.Collections.Generic;
using System.Linq;

namespace SA3D.Texturing.MipMapping
{
	/// <summary>
	/// Mip map set interface
	/// </summary>
	public interface IMipMapSet : IEnumerable<IMipMapLevel>
	{
		/// <summary>
		/// Number of levels in the set
		/// </summary>
		public int LevelCount { get; }

		/// <summary>
		/// Mip map texture type
		/// </summary>
		public TextureType TextureType { get; }

		/// <summary>
		/// Get mip map texture for the specific level
		/// </summary>
		/// <param name="level">Level for which to retrieve the mip map</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public IMipMapLevel this[int level] { get; }
	}

	/// <summary>
	/// Mip map set extensions
	/// </summary>
	public static class MipMapSetExtensions
	{
		extension(IMipMapSet set)
		{
			/// <summary>
			/// Texture width at mip map level 0
			/// </summary>
			public int BaseWidth => set.First().Width;

			/// <summary>
			/// Texture height at mip map level 0
			/// </summary>
			public int BaseHeight => set.First().Height;

			/// <summary>
			/// Whether the set could have further mip map levels, but has none
			/// </summary>
			public bool NoMipMaps => set.LevelCount == 1 && (set.BaseWidth > 1 || set.BaseHeight > 1);
		}
	}
}
