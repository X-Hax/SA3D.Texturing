
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using SA3D.Texturing.ReadOnly;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Qoi;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace SA3D.Texturing.IO
{
	/// <summary>
	/// Texture file utilities
	/// </summary>
	public static class TextureFileUtilities
	{
		/// <summary>
		/// Encode a texture as the given image format
		/// </summary>
		/// <param name="texture">Texture to write</param>
		/// <param name="stream">The stream to write to</param>
		/// <param name="format">The format to write as</param>
		/// <exception cref="ArgumentException"></exception>
		public static void WriteImage(this ITexture texture, Stream stream, ImageFormat format)
		{
			ReadOnlySpan<byte> colorData = texture.GetRGBA32Data();
			bool isTransparent = texture.CheckIsTransparent();
			IImageEncoder? encoder;

			switch(format)
			{
				case ImageFormat.PNG:
					encoder = new PngEncoder()
					{
						ColorType = isTransparent ? PngColorType.RgbWithAlpha : PngColorType.Rgb,
						TransparentColorMode = PngTransparentColorMode.Preserve,
						ChunkFilter = PngChunkFilter.ExcludeAll
					};
					break;
				case ImageFormat.BMP:
					encoder = new BmpEncoder()
					{
						BitsPerPixel = isTransparent ? BmpBitsPerPixel.Pixel32 : BmpBitsPerPixel.Pixel24,
						SupportTransparency = isTransparent
					};
					break;
				case ImageFormat.JPG:
					encoder = new JpegEncoder();
					break;
				case ImageFormat.PBM:
					encoder = new PbmEncoder()
					{
						ColorType = PbmColorType.Rgb,
						ComponentType = PbmComponentType.Byte
					};
					break;
				case ImageFormat.QOI:
					encoder = new QoiEncoder()
					{
						Channels = isTransparent ? QoiChannels.Rgba : QoiChannels.Rgb
					};
					break;
				case ImageFormat.TGA:
					encoder = new TgaEncoder()
					{
						BitsPerPixel = isTransparent ? TgaBitsPerPixel.Pixel32 : TgaBitsPerPixel.Pixel24
					};
					break;
				case ImageFormat.TIFF:
					encoder = new TiffEncoder()
					{
						BitsPerPixel = isTransparent ? TiffBitsPerPixel.Bit32 : TiffBitsPerPixel.Bit24
					};
					break;
				case ImageFormat.WEBP:
					encoder = new WebpEncoder()
					{
						FileFormat = WebpFileFormatType.Lossless,
						TransparentColorMode = isTransparent ? WebpTransparentColorMode.Preserve : WebpTransparentColorMode.Clear
					};
					break;
				case ImageFormat.DDS:
					BcEncoder bcEncoder = new(isTransparent ? CompressionFormat.Bc3 : CompressionFormat.Bc1);
					bcEncoder.EncodeToDds(colorData, texture.Width, texture.Height, PixelFormat.Rgba32).Write(stream);
					return;
				default:
					throw new ArgumentException("Invalid image format", nameof(format));
			}

			Image.LoadPixelData<Rgba32>(colorData, texture.Width, texture.Height).Save(stream, encoder);
		}

		/// <summary>
		/// Encode a texture as an image file.
		/// </summary>
		/// <param name="texture">Texture to write</param>
		/// <param name="format">The format to write as</param>
		public static byte[] WriteImageToBytes(this ITexture texture, ImageFormat format)
		{
			using MemoryStream stream = new();
			texture.WriteImage(stream, format);
			return stream.ToArray();
		}

		/// <summary>
		/// Write a texture to an image file.
		/// </summary>
		/// <param name="texture">Texture to write</param>
		/// <param name="filepath">Path to the file to write to.</param>
		/// <param name="format">The format to write as</param>
		public static void WriteImageToFile(this ITexture texture, string filepath, ImageFormat format)
		{
			using FileStream stream = File.Create(filepath);
			texture.WriteImage(stream, format);
		}


		/// <summary>
		/// Encode an index texture as an image.
		/// </summary>
		/// <param name="texture">Texture to write</param>
		/// <param name="stream">The stream to write to</param>
		/// <param name="format">The image format to write as</param>
		/// <param name="storeInAlpha">Whether the index should be stored in the alpha channel, instead of outputing a grayscale image.</param>
		public static void WriteIndexImage(this IIndexTexture texture, Stream stream, ImageFormat format, bool storeInAlpha)
		{
			IImageEncoder? encoder;

			switch(format)
			{
				case ImageFormat.PNG:
					encoder = new PngEncoder()
					{
						BitDepth = texture.IsIndex4 ? PngBitDepth.Bit4 : PngBitDepth.Bit8,
						ColorType = storeInAlpha ? PngColorType.GrayscaleWithAlpha : PngColorType.Grayscale
					};
					break;
				case ImageFormat.BMP:
					encoder = new BmpEncoder()
					{
						BitsPerPixel = texture.IsIndex4 ? BmpBitsPerPixel.Pixel4 : BmpBitsPerPixel.Pixel8,
						SupportTransparency = storeInAlpha
					};
					break;
				case ImageFormat.JPG:
					encoder = new JpegEncoder()
					{
						ColorType = JpegEncodingColor.Luminance
					};
					storeInAlpha = false;
					break;
				case ImageFormat.PBM:
					encoder = new PbmEncoder()
					{
						ColorType = PbmColorType.Grayscale,
						ComponentType = PbmComponentType.Byte
					};
					storeInAlpha = false;
					break;
				case ImageFormat.QOI:
					encoder = new QoiEncoder()
					{
						Channels = storeInAlpha ? QoiChannels.Rgba : QoiChannels.Rgb,
					};
					break;
				case ImageFormat.TGA:
					encoder = new TgaEncoder()
					{
						BitsPerPixel = storeInAlpha ? TgaBitsPerPixel.Pixel32 : TgaBitsPerPixel.Pixel8,
					};
					break;
				case ImageFormat.TIFF:
					encoder = new TiffEncoder()
					{
						BitsPerPixel = storeInAlpha ? TiffBitsPerPixel.Bit32 : TiffBitsPerPixel.Bit24,
					};
					break;
				case ImageFormat.WEBP:
					encoder = new WebpEncoder()
					{
						FileFormat = WebpFileFormatType.Lossless,
						TransparentColorMode = storeInAlpha ? WebpTransparentColorMode.Preserve : WebpTransparentColorMode.Clear
					};
					break;
				case ImageFormat.DDS:

					IIndexTexture grayscaleTexture = new ReadOnlyIndexTexture(texture)
					{
						Palette = ITexturePalette.GetDefaultPalette(texture.IsIndex4),
						PaletteRow = 0,
					};

					new BcEncoder(CompressionFormat.R).EncodeToDds(grayscaleTexture.GetRGBA32Data(), texture.Width, texture.Height, PixelFormat.Rgba32).Write(stream);

					return;
				default:
					throw new ArgumentException("Invalid image format", nameof(format));
			}

			Image image = storeInAlpha
				? Image.LoadPixelData<A8>(texture.GetIndexPixelData(), texture.Width, texture.Height)
				: Image.LoadPixelData<L8>(texture.GetIndexPixelData(), texture.Width, texture.Height);

			image.Save(stream, encoder);
		}

		/// <summary>
		/// Encode an index texture as an image.
		/// </summary>
		/// <param name="texture">Texture to write</param>
		/// <param name="format">The image format to write as</param>
		/// <param name="storeInAlpha">Whether the index should be stored in the alpha channel, instead of outputing a grayscale image.</param>
		public static byte[] WriteIndexImageToBytes(this IIndexTexture texture, ImageFormat format, bool storeInAlpha)
		{
			using MemoryStream stream = new();
			texture.WriteIndexImage(stream, format, storeInAlpha);
			return stream.ToArray();
		}

		/// <summary>
		/// Write an index texture to a PNG file.
		/// </summary>
		/// <param name="texture">Texture to write</param>
		/// <param name="filepath">The path to the file to write to.</param>
		/// <param name="format">The image format to write as</param>
		/// <param name="storeInAlpha">Whether the index should be stored in the alpha channel, instead of outputing a grayscale image.</param>
		public static void WriteIndexImageToFile(this IIndexTexture texture, string filepath, ImageFormat format, bool storeInAlpha)
		{
			using FileStream stream = File.Create(filepath);
			texture.WriteIndexImage(stream, format, storeInAlpha);
		}



		/// <summary>
		/// Read a texture from a file data stream.
		/// </summary>
		/// <param name="stream">Stream to read the file data from.</param>
		/// <param name="filename">Filename that should be used.</param>
		public static Texture ReadImage(Stream stream, string filename)
		{
			long dataStart = stream.Position;

			ImageFileFormat bcFormat = ImageFileFormat.Unknown;
			try
			{
				bcFormat = ImageFile.DetermineImageFormat(stream);
			}
			catch { }

			stream.Seek(dataStart, SeekOrigin.Begin);

			Image<Rgba32> image;
			if(bcFormat != ImageFileFormat.Unknown)
			{
				BcDecoder decoder = new();
				image = decoder.DecodeToImageRgba32(stream);
			}
			else
			{
				image = Image.Load<Rgba32>(stream);
			}

			byte[] data = new byte[image.Width * image.Height * 4];
			image.CopyPixelDataTo(data);

			return new Texture(data, image.Width, image.Height)
			{
				Name = filename
			};
		}

		/// <summary>
		/// Read a texture from file data.
		/// </summary>
		/// <param name="data">File data to read.</param>
		/// <param name="filename">Filename that should be used.</param>
		public static Texture ReadImageFromBytes(byte[] data, string filename)
		{
			using(MemoryStream stream = new(data))
			{
				return ReadImage(stream, filename);
			}
		}

		/// <summary>
		/// Read a texture from a file.
		/// </summary>
		/// <param name="filepath">Path to the file to read.</param>
		public static Texture ReadImageFromFile(string filepath)
		{
			using(FileStream stream = File.OpenRead(filepath))
			{
				return ReadImage(stream, Path.GetFileNameWithoutExtension(filepath));
			}
		}


		/// <summary>
		/// Verifies whether file data is readable as an index texture.
		/// <br/> Does not alter the stream position.
		/// </summary>
		/// <param name="stream">The stream to read the file data from.</param>
		/// <param name="index4">Whether the file stores 4 bit indices.</param>
		/// <param name="storedInAlpha">Whether the index data is stored in alpha.</param>
		/// <returns>Whether the file can be reas as an index texture.</returns>
		public static bool CheckCanReadIndexImage(Stream stream, out bool index4, out bool storedInAlpha)
		{
			index4 = false;
			storedInAlpha = false;
			long position = stream.Position;

			ImageFileFormat bcFormat = ImageFile.DetermineImageFormat(stream);
			stream.Seek(position, SeekOrigin.Begin);

			if(bcFormat != ImageFileFormat.Unknown)
			{
				BcDecoder decoder = new();

				CompressionFormat format = bcFormat == ImageFileFormat.Dds
					? decoder.GetFormat(DdsFile.Load(stream))
					: decoder.GetFormat(KtxFile.Load(stream));

				stream.Seek(position, SeekOrigin.Begin);

				return format is CompressionFormat.R or CompressionFormat.Bc4;
			}
			else
			{
				ImageInfo info = Image.Identify(stream);
				stream.Seek(position, SeekOrigin.Begin);

				IImageFormat format = Image.DetectFormat(stream);
				stream.Seek(position, SeekOrigin.Begin);

				switch(format.Name)
				{
					case "PNG":
						PngMetadata pngMD = info.Metadata.GetPngMetadata();
						if(pngMD.ColorType == null)
						{
							return false;
						}

						switch(pngMD.ColorType)
						{
							case PngColorType.Grayscale:
								index4 = pngMD.BitDepth < PngBitDepth.Bit8;
								return true;
							case PngColorType.GrayscaleWithAlpha:
								storedInAlpha = true;
								return true;
							case PngColorType.Rgb:
							case PngColorType.Palette:
							case PngColorType.RgbWithAlpha:
							case null:
							default:
								return false;
						}

					case "JPEG":
						JpegMetadata jpegMD = info.Metadata.GetJpegMetadata();
						return jpegMD.ColorType == JpegEncodingColor.Luminance;

					case "PBM":
						PbmMetadata pbmMD = info.Metadata.GetPbmMetadata();
						return pbmMD.ColorType == PbmColorType.Grayscale;

					default:
						return false;
				}
			}
		}

		/// <summary>
		/// Verifies whether file data is readable as an index texture.
		/// <br/> Does not alter the stream position.
		/// </summary>
		/// <param name="data">File data to read.</param>
		/// <param name="index4">Whether the file stores 4 bit indices.</param>
		/// <param name="storedInAlpha">Whether the index data is stored in alpha.</param>
		/// <returns>Whether the file can be reas as an index texture.</returns>
		public static bool CheckCanReadIndexImageFromBytes(byte[] data, out bool index4, out bool storedInAlpha)
		{
			using(MemoryStream stream = new(data))
			{
				return CheckCanReadIndexImage(stream, out index4, out storedInAlpha);
			}
		}

		/// <summary>
		/// Verifies whether a file is readable as an index texture.
		/// <br/> Does not alter the stream position.
		/// </summary>
		/// <param name="filepath">Path from which the file should be read.</param>
		/// <param name="index4">Whether the file stores 4 bit indices.</param>
		/// <param name="storedInAlpha">Whether the index data is stored in alpha.</param>
		/// <returns>Whether the file can be reas as an index texture.</returns>
		public static bool CheckCanReadIndexImageFromFile(string filepath, out bool index4, out bool storedInAlpha)
		{
			using(FileStream stream = File.OpenRead(filepath))
			{
				return CheckCanReadIndexImage(stream, out index4, out storedInAlpha);
			}
		}


		/// <summary>
		/// Attempts to read an indexed texture from a file data stream.
		/// </summary>
		/// <param name="stream">The stream to read the file data from.</param>
		/// <param name="filename">Filename that should be used.</param>
		/// <param name="result">The read index texture. Null if file was not an index texture</param>
		/// <returns>Whether the file was successfully read as index texture.</returns>
		public static bool TryReadIndexImage(Stream stream, string filename, [MaybeNullWhen(false)] out IndexTexture result)
		{
			if(CheckCanReadIndexImage(stream, out bool index4, out bool inAlpha))
			{
				result = inAlpha
					? ReadIndexImageInternal<A8>(stream, filename, index4)
					: ReadIndexImageInternal<L8>(stream, filename, index4);

				return true;
			}
			else
			{
				result = null;
				return false;
			}
		}

		/// <summary>
		/// Attempts to read an indexed texture from file data.
		/// </summary>
		/// <param name="data">File data to read.</param>
		/// <param name="filename">Filename that should be used.</param>
		/// <param name="result">The read index texture. Null if file was not an index texture</param>
		/// <returns>Whether the file was successfully read as index texture.</returns>
		public static bool TryReadIndexImageFromBytes(byte[] data, string filename, [MaybeNullWhen(false)] out IndexTexture result)
		{
			using(MemoryStream stream = new(data))
			{
				return TryReadIndexImage(stream, filename, out result);
			}
		}

		/// <summary>
		/// Attempts to read an indexed texture from a file.
		/// </summary>
		/// <param name="filepath">Path from which the file should be read.</param>
		/// <param name="result">The read index texture. Null if file was not an index texture</param>
		/// <returns>Whether the file was successfully read as index texture.</returns>
		public static bool TryReadIndexedFromFile(string filepath, [MaybeNullWhen(false)] out IndexTexture result)
		{
			using(FileStream stream = File.OpenRead(filepath))
			{
				return TryReadIndexImage(stream, Path.GetFileNameWithoutExtension(filepath), out result);
			}
		}


		/// <summary>
		/// Reads an index texture from a file data stream.
		/// </summary>
		/// <param name="stream">The stream to read the file data from.</param>
		/// <param name="filename">Filename that should be used.</param>
		/// <returns>The read index texture.</returns>
		/// <exception cref="InvalidDataException"></exception>
		public static IndexTexture ReadIndexImage(Stream stream, string filename)
		{
			if(TryReadIndexImage(stream, filename, out IndexTexture? result))
			{
				return result;
			}

			throw new InvalidDataException("File Data was not able to be read as an index texture.");
		}

		/// <summary>
		/// Reads an index texture from a file data stream.
		/// </summary>
		/// <param name="data">File data to read.</param>
		/// <param name="filename">Filename that should be used.</param>
		/// <returns>The read index texture.</returns>
		/// <exception cref="InvalidDataException"></exception>
		public static IndexTexture ReadIndexImageFromBytes(byte[] data, string filename)
		{
			if(TryReadIndexImageFromBytes(data, filename, out IndexTexture? result))
			{
				return result;
			}

			throw new InvalidDataException("File Data was not able to be read as an index texture.");
		}

		/// <summary>
		/// Reads an index texture from a file data stream.
		/// </summary>
		/// <param name="filepath">Path from which the file should be read.</param>
		/// <returns>The read index texture.</returns>
		/// <exception cref="InvalidDataException"></exception>
		public static IndexTexture ReadIndexImageFromFile(string filepath)
		{
			if(TryReadIndexedFromFile(filepath, out IndexTexture? result))
			{
				return result;
			}

			throw new InvalidDataException("File Data was not able to be read as an index texture.");
		}


		private static IndexTexture ReadIndexImageInternal<TPixel>(Stream stream, string filename, bool isIndex4)
			where TPixel : unmanaged, IPixel<TPixel>
		{
			byte[] data;
			int width;
			int height;

			ImageFileFormat bcFormat = ImageFileFormat.Unknown;
			try
			{
				bcFormat = ImageFile.DetermineImageFormat(stream);
			}
			catch { }

			if(bcFormat != ImageFileFormat.Unknown)
			{
				BcDecoder decoder = new();
				Image<Rgba32> rgba = decoder.DecodeToImageRgba32(stream);
				byte[] rgbaData = new byte[rgba.Width * rgba.Height * 4];
				rgba.CopyPixelDataTo(rgbaData);

				width = rgba.Width;
				height = rgba.Height;
				data = new byte[width * height];

				int srcIndex = 0;
				int dstIndex = 0;
				for(int y = 0; y < rgba.Height; y++)
				{
					for(int x = 0; x < rgba.Width; x++)
					{
						data[dstIndex] = rgbaData[srcIndex];
						srcIndex += 4;
						dstIndex++;
					}
				}
			}
			else
			{
				Image<TPixel> image = Image.Load<TPixel>(stream);
				width = image.Width;
				height = image.Height;
				data = new byte[width * height];
				image.CopyPixelDataTo(data);
			}

			return new IndexTexture(data, width, height, isIndex4)
			{
				Name = filename
			};
		}


		/// <summary>
		/// Writes a content index used by texture packs to a writer.
		/// </summary>
		/// <param name="textureSet">The texture set for which to write the content index</param>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="nameSuffix">Suffix for every texture name.</param>
		public static void WriteContentIndex(this ITextureSet textureSet, TextWriter writer, string nameSuffix)
		{
			foreach(ITexture texture in textureSet.Textures)
			{
				writer.WriteLine($"{texture.GlobalIndex},{texture.Name}{nameSuffix},{texture.RealWidth}x{texture.RealHeight}");
			}
		}

		/// <summary>
		/// Generates a content index used by texture packs.
		/// </summary>
		/// <param name="textureSet">The texture set for which to write the content index</param>
		/// <param name="nameSuffix">Suffix for every texture name.</param>
		/// <returns>The index contents.</returns>
		public static string WriteContentIndexToString(this ITextureSet textureSet, string nameSuffix)
		{
			using StringWriter writer = new();
			WriteContentIndex(textureSet, writer, nameSuffix);
			return writer.ToString();
		}

		/// <summary>
		/// Writes a content index used by texture packs to a file.
		/// </summary>
		/// <param name="textureSet">The texture set for which to write the content index</param>
		/// <param name="filepath">Path of the file to write to.</param>
		/// <param name="nameSuffix">Suffix for every texture name.</param>
		/// <returns>The index contents.</returns>
		public static void WriteContentIndexToFile(this ITextureSet textureSet, string filepath, string nameSuffix)
		{
			using StreamWriter writer = File.CreateText(filepath);
			WriteContentIndex(textureSet, writer, nameSuffix);
		}


		/// <summary>
		/// Exports the texture set as a texture pack useable by sonic adventure modloaders.
		/// </summary>
		/// <param name="textureSet">The texture set to export</param>
		/// <param name="outDirectory">The directory to which to write the files.</param>
		/// <param name="format">Format to write images in</param>
		public static void ExportTexturePack(this ITextureSet textureSet, string outDirectory, ImageFormat format)
		{
			string extension = "." + format.ToString().ToLowerInvariant();
			string indexPath = Path.Join(outDirectory, "index.txt");
			WriteContentIndexToFile(textureSet, indexPath, extension);

			foreach(ITexture texture in textureSet.Textures)
			{
				string path = Path.Join(outDirectory, texture.Name + extension);

				if(texture is IndexTexture indexTex)
				{

					indexTex.WriteIndexImageToFile(path, format, false);
				}
				else
				{
					texture.WriteImageToFile(path, format);
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
