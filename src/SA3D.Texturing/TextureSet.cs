using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace SA3D.Texturing
{
	/// <summary>
	/// Texture set
	/// </summary>
	public class TextureSet
	{
		/// <summary>
		/// Textures in the texture set
		/// </summary>
		public ReadOnlyCollection<Texture> Textures { get; }

		/// <summary>
		/// Creates a new texture set.
		/// </summary>
		/// <param name="textures"></param>
		public TextureSet(Texture[] textures)
		{
			Textures = new(textures);
		}

		/// <summary>
		/// Returns an index content text used by texture packs.
		/// </summary>
		/// <param name="nameSuffix">Suffix for every texture name.</param>
		/// <returns>The index contents.</returns>
		public string GetIndexContents(string nameSuffix)
		{
			StringBuilder sb = new();
			foreach(Texture texture in Textures)
			{
				sb.AppendLine($"{texture.GlobalIndex},{texture.Name}{nameSuffix},{texture.OverrideWidth}x{texture.OverrideHeight}");
			}

			return sb.ToString();
		}

		/// <summary>
		/// Exports the texture set as a texture pack useable by sonic adventure modloaders.
		/// </summary>
		/// <param name="outDirectory">The directory to which to write the files.</param>
		/// <param name="useDDS">Whether to export the texture as DDS files.</param>
		public void ExportTexturePack(string outDirectory, bool useDDS = false)
		{
			string extension = useDDS ? ".dds" : ".png";
			File.WriteAllText(outDirectory + "\\index.txt", GetIndexContents(extension));
			foreach(Texture texture in Textures)
			{
				string path = $"{outDirectory}\\{texture.Name}{extension}";
				if(texture is IndexTexture indexTex)
				{
					if(useDDS)
					{
						indexTex.IndexedToDDSFile(path);
					}
					else
					{
						indexTex.IndexedToPNGFile(path, false);
					}
				}
				else
				{
					if(useDDS)
					{
						texture.ColoredToDDSFile(path);
					}
					else
					{
						texture.ColoredToPNGFile(path);
					}
				}
			}
		}

		/// <summary>
		/// Imports texture from a texture pack useable by sonic adventure modloaders.
		/// </summary>
		/// <param name="directory">The directory from which to read the files.</param>
		/// <returns>The imported texture set.</returns>
		public static TextureSet ImportTexturePack(string directory)
		{
			List<Texture> textures = new();

			string[] index = File.ReadAllLines($"{directory}\\index.txt");
			foreach(string item in index)
			{
				string[] values = item.Split(',');
				string filename = values[1];

				Texture texture = Texture.ReadFromFile($"{directory}\\{filename}");

				texture.GlobalIndex = uint.Parse(values[0]);
				if(values.Length >= 3)
				{
					string[] overrideDimensions = values[2].Split('x');
					texture.OverrideWidth = int.Parse(overrideDimensions[0]);
					texture.OverrideHeight = int.Parse(overrideDimensions[1]);
				}

				textures.Add(texture);
			}

			return new(textures.ToArray());
		}
	}
}
