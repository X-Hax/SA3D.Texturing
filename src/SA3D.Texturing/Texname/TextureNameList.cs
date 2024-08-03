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
	public class TextureNameList : ILabel
	{
		/// <inheritdoc/>
		public string Label { get; set; }

		/// <summary>
		/// Texture names.
		/// </summary>
		public ILabeledArray<TextureName> TextureNames { get; set; }


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
		/// <param name="reader">The reader to read the struct from.</param>
		/// <param name="address">The address at which to read the struct.</param>
		/// <param name="labels">Labels to use.</param>
		/// <returns>The read texture name list.</returns>
		public static TextureNameList Read(EndianStackReader reader, uint address, LabelDictionary labels)
		{
			if(!labels.TryGetValue(address, out string? name))
			{
				name = "texlist_" + address.ToString("X8");
			}

			uint texnameArrayAddr = reader.ReadPointer(address);
			if(!labels.TryGetValue(texnameArrayAddr, out string? textureNameArrayLabel))
			{
				textureNameArrayLabel = "textures_" + texnameArrayAddr.ToString("X8");
			}

			TextureName[] textureNames = new TextureName[reader.ReadUInt(address + 4)];

			if(texnameArrayAddr != 0)
			{
				for(uint i = 0; i < textureNames.Length; i++)
				{
					textureNames[i] = TextureName.Read(reader, texnameArrayAddr);
					texnameArrayAddr += TextureName.StructSize;
				}
			}

			return new(name, new LabeledArray<TextureName>(textureNameArrayLabel, textureNames));
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
		/// Writes the texture name list as a struct to an endian writer.
		/// </summary>
		/// <param name="writer">The writer to write the struct to.</param>
		/// <param name="labels">The dictionary in which to store the struct labels.</param>
		/// <returns>The address at which the structure was written.</returns>
		public uint Write(EndianStackWriter writer, LabelDictionary labels)
		{
			uint start = writer.Position;
			writer.WriteEmpty((uint)(8 + (TextureNames.Length * TextureName.StructSize)));

			// write name strings
			foreach(TextureName texName in TextureNames)
			{
				if(texName.Name == null)
				{
					continue;
				}

				if(!labels.TryGetAddress(texName.Name, out _))
				{
					labels.Add(writer.PointerPosition, texName.Name);
					writer.WriteStringNullterminated(texName.Name);
					writer.Align(4);
				}
			}

			writer.Stream.Seek(start, SeekOrigin.Begin);
			uint textureNamesAddress = writer.PointerPosition;

			labels.AddSafe(textureNamesAddress, TextureNames.Label);
			foreach(TextureName texName in TextureNames)
			{
				texName.Write(writer, labels);
			}

			uint address = writer.PointerPosition;
			labels.AddSafe(address, Label);
			writer.WriteUInt(textureNamesAddress);
			writer.WriteInt(TextureNames.Length);

			writer.Stream.Seek(0, SeekOrigin.End);
			return address;
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
