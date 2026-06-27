using System.Collections.Generic;

namespace SA3D.Texturing
{
	/// <summary>
	/// Texture set
	/// </summary>
	public class TextureSet : ITextureSet
	{
		/// <summary>
		/// Textures of the texture set
		/// </summary>
		public List<ITexture> Textures { get; }

		IReadOnlyList<ITexture> ITextureSet.Textures => Textures;

		/// <summary>
		/// Creates a new texture set.
		/// </summary>
		/// <param name="textures"></param>
		public TextureSet(IEnumerable<ITexture> textures)
		{
			Textures = [.. textures];
		}
	}
}