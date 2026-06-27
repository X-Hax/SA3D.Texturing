using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SA3D.Texturing
{
	/// <summary>
	/// Read-only Texture set
	/// </summary>
	public class ReadOnlyTextureSet : ITextureSet
	{
		/// <inheritdoc/>
		public IReadOnlyList<ITexture> Textures { get; }

		/// <summary>
		/// Creates a new texture set.
		/// </summary>
		/// <param name="textures"></param>
		public ReadOnlyTextureSet(IEnumerable<ITexture> textures)
		{
			Textures = new ReadOnlyCollection<ITexture>([.. textures]);
		}
	}
}
