using Amicitia.IO.Binary;
using SA3D.Common;
using SA3D.Common.Ini;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SA3D.Texturing.Texname
{
	/// <summary>
	/// Stores a texture name list.
	/// </summary>
	public class TextureNameList : ILabel, IBinarySerializable<LabelDictionary>
	{
		private const string _labelPrefix = "texlist_";
		private const string _texturesLabelPrefix = "textures_";

		/// <inheritdoc/>
		public string Label { get; set; }

		/// <summary>
		/// Texture names.
		/// </summary>
		public ILabeledArray<TextureName> TextureNames { get; set; }

		/// <summary>
		/// Creates a new, empty texture list
		/// </summary>
		public TextureNameList() : this(
			_labelPrefix.GenerateIdentifier(),
			new LabeledArray<TextureName>(
				_texturesLabelPrefix.GenerateIdentifier(),
				[]
			)
		)
		{ }

		/// <summary>
		/// Creates a new texture name list.
		/// </summary>
		/// <param name="label">Texture list label.</param>
		/// <param name="textureNames">Texture names.</param>
		public TextureNameList(string label, ILabeledArray<TextureName> textureNames)
		{
			Label = label;
			TextureNames = textureNames;
		}


		/// <summary>
		/// Reads a texture name list struct from an endian reader.
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <param name="labels">The labels to use</param>
		public void Read(BinaryObjectReader reader, LabelDictionary? labels)
		{
			labels.NullReferenceCheck();
			Label = labels.GetSafe((uint)reader.Position, _labelPrefix);

			long texturesOffset = reader.ReadOffsetValue();
			int texturesCount = reader.ReadInt32();

			reader.ReadAtOffset(texturesOffset, () =>
			{
				string texturesLabel = labels.GetSafe((uint)reader.Position, _texturesLabelPrefix);
				TextureName[] textureNames = reader.ReadObjectArray<TextureName>(texturesCount);
				TextureNames = new LabeledArray<TextureName>(texturesLabel, textureNames);
			});
		}

		/// <summary>
		/// Reads a texture name list from from an Ini or Satex file.
		/// </summary>
		/// <param name="filepath">The path to the file.</param>
		/// <returns>The read texture name list.</returns>
		/// <exception cref="FormatException"></exception>
		public static TextureNameList ReadFromTextFile(string filepath)
		{
			string[] lines = File.ReadAllLines(filepath);

			if(lines.Length > 0 && lines[0].Contains('='))
			{
				IniTexturenameList ini = IniSerializer.DeserializeFromFile<IniTexturenameList>(filepath)
										?? throw new FormatException("File not correctly formated as an Ini");

				TextureName[] textureNames = new TextureName[ini.NumTextures];
				for(int i = 0; i < textureNames.Length; i++)
				{
					textureNames[i] = new(ini.TextureNames[i], 0, 0);
				}

				return new(ini.Name, new LabeledArray<TextureName>(ini.TexnameArrayName, textureNames));
			}
			else
			{
				TextureName[] textureNames = new TextureName[lines.Length];

				for(int i = 0; i < lines.Length; i++)
				{
					textureNames[i] = new TextureName(Path.GetFileNameWithoutExtension(lines[i]), 0, 0);
				}

				return new TextureNameList(string.Empty, new LabeledArray<TextureName>(textureNames));
			}
		}


		/// <summary>
		/// Writes the texture name list to a <see cref="BinaryObjectWriter"/>
		/// </summary>
		/// <param name="writer">The writer to write to</param>
		/// <param name="labels">The dictionary in which to store the struct labels</param>
		public void Write(BinaryObjectWriter writer, LabelDictionary? labels)
		{
			labels.NullReferenceCheck();

			labels.AddSafe(writer.Position, Label);
			writer.WriteOffset(TextureNames, () =>
			{
				labels.AddSafe(writer.Position, TextureNames.Label);
				writer.WriteObjectArray(TextureNames);
			});
			writer.WriteInt32(TextureNames.Length);
		}

		/// <summary>
		/// Saves the texture list as a plain text document.
		/// </summary>
		/// <param name="filePath">The path to write the file to.</param>
		/// <param name="extension">The file extension to add to every texture name. without dot.</param>
		public void WriteAsListToTextFile(string filePath, string extension = "pvr")
		{
			string lines = string.Empty;
			foreach(TextureName texName in TextureNames)
			{
				lines += (texName.Name ?? "empty") + $".{extension}\n";
			}

			File.WriteAllText(filePath, lines);
		}

		/// <summary>
		/// Writes the texture name list to an Ini/Satex file.
		/// </summary>
		/// <param name="filepath">The path to write the file to.</param>
		public void WriteAsIniToTextFile(string filepath)
		{
			string[] textureNames = TextureNames.Select(x => x.Name ?? "NULL").ToArray();
			IniTexturenameList ini = new(Label, TextureNames.Label, (uint)textureNames.Length, textureNames);

			IniSerializer.SerializeToFile(ini, filepath);
		}

		/// <summary>
		/// Writes the texture list as a C compilable struct.
		/// </summary>
		/// <param name="writer">the text writer to write it to</param>
		/// <param name="labels">Used labels</param>
		public void WriteAsStruct(TextWriter writer, List<string>? labels = null)
		{
			labels ??= [];

			if(labels.Contains(TextureNames.Label))
			{
				writer.WriteLine($"NJS_TEXNAME {TextureNames.Label}[] =");
				writer.WriteLine("{");
				for(int i = 0; i < TextureNames.Length; i++)
				{
					writer.Write($"\t{{ \"{TextureNames[i].Name}\" }}");
					if(i < TextureNames.Length - 1)
					{
						writer.Write(',');
					}

					writer.WriteLine();
				}

				writer.WriteLine("};");
				labels.Add(TextureNames.Label);
			}

			if(labels.Contains(Label))
			{
				writer.WriteLine($"NjsTexList {Label}[] = {{ arrayptrandlength ({TextureNames.Label}) }};");
				labels.Add(Label);
			}
		}
	}
}
