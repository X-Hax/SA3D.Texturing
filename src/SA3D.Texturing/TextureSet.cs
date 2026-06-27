using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

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
		public ReadOnlyCollection<ITexture> Textures { get; }

		/// <summary>
		/// Creates a new texture set.
		/// </summary>
		/// <param name="textures"></param>
		public TextureSet(ITexture[] textures)
		{
			Textures = new(textures);
		}


		/// <summary>
		/// Writes a content index used by texture packs to a writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="nameSuffix">Suffix for every texture name.</param>
		public void WriteContentIndex(TextWriter writer, string nameSuffix)
		{
			foreach(ITexture texture in Textures)
			{
				writer.WriteLine($"{texture.GlobalIndex},{texture.Name}{nameSuffix},{texture.OverrideWidth}x{texture.OverrideHeight}");
			}
		}

		/// <summary>
		/// Generates a content index used by texture packs.
		/// </summary>
		/// <param name="nameSuffix">Suffix for every texture name.</param>
		/// <returns>The index contents.</returns>
		public string WriteContentIndexToString(string nameSuffix)
		{
			using(StringWriter writer = new())
			{
				WriteContentIndex(writer, nameSuffix);
				return writer.ToString();
			}
		}

		/// <summary>
		/// Writes a content index used by texture packs to a file.
		/// </summary>
		/// <param name="filepath">Path of the file to write to.</param>
		/// <param name="nameSuffix">Suffix for every texture name.</param>
		/// <returns>The index contents.</returns>
		public void WriteContentIndexToFile(string filepath, string nameSuffix)
		{
			using(StreamWriter writer = File.CreateText(filepath))
			{
				WriteContentIndex(writer, nameSuffix);
			}
		}


		/// <summary>
		/// Exports the texture set as a texture pack useable by sonic adventure modloaders.
		/// </summary>
		/// <param name="outDirectory">The directory to which to write the files.</param>
		/// <param name="format">Format to write images in</param>
		public void ExportTexturePack(string outDirectory, ImageFormat format)
		{
			string extension = "." + format.ToString().ToLowerInvariant();
			string indexPath = Path.Join(outDirectory, "index.txt");
			WriteContentIndexToFile(indexPath, extension);

			foreach(ITexture texture in Textures)
			{
				string path = Path.Join(outDirectory, texture.Name + extension);

				if(texture is IndexTexture indexTex)
				{

					indexTex.WriteIndexImageToFile(path, format, false);
				}
				else
				{
					texture.WriteColorImageToFile(path, format);
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
			List<ITexture> textures = [];

			string indexPath = Path.Join(directory, "index.txt");
			string[] index = File.ReadAllLines(indexPath);

			foreach(string item in index)
			{
				string[] values = item.Split(',');
				string filename = values[1];

				string texturePath = Path.Join(directory, filename);
				Texture texture = TextureFileUtilities.ReadImageFromFile(texturePath);

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