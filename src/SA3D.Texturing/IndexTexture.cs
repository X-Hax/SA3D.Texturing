using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace SA3D.Texturing
{
	/// <summary>
	/// Texture consisting of single byte pixels that refer to a palette
	/// </summary>
	public sealed class IndexTexture : Texture
	{
		/// <summary>
		/// Palette used to generate colored image data. If none is passed, grayscale values will be used
		/// </summary>
		public TexturePalette? Palette { get; set; }

		/// <summary>
		/// Row of the palette to use (calculates offset based on <see cref="IsIndex4"/>).
		/// </summary>
		public int PaletteRow { get; set; }

		/// <summary>
		/// Whether the indices are actually 4 bits big, instead of 8 bits.
		/// <br/> When setting to <see langword="true"/>, the lower 4 bits of every byte will be ignored.
		/// </summary>
		public bool IsIndex4 { get; set; }


		/// <summary>
		/// Creates a new index texture from preexisting data.
		/// </summary>
		/// <param name="width">Width of the texture in pixels.</param>
		/// <param name="height">Height of the texture in pixels.</param>
		/// <param name="data">Raw pixel data to use.</param>
		/// <param name="name">Name of the texture.</param>
		/// <param name="globalIndex">Global index of the texture.</param>
		/// <exception cref="InvalidDataException"></exception>
		public IndexTexture(int width, int height, byte[] data, string name, uint globalIndex)
			: base(width, height, data, name, globalIndex) { }

		/// <summary>
		/// Creates a new index texture from preexisting data.
		/// </summary>
		/// <param name="width">Width of the texture in pixels.</param>
		/// <param name="height">Height of the texture in pixels.</param>
		/// <param name="data">Raw pixel data to use.</param>
		/// <param name="globalIndex">Global index of the texture.</param>
		/// <exception cref="InvalidDataException"></exception>
		public IndexTexture(int width, int height, byte[] data, uint globalIndex)
			: this(width, height, data, string.Empty, globalIndex) { }

		/// <summary>
		/// Creates a new index texture from preexisting data.
		/// </summary>
		/// <param name="width">Width of the texture in pixels.</param>
		/// <param name="height">Height of the texture in pixels.</param>
		/// <param name="data">Raw pixel data to use.</param>
		/// <param name="name">Name of the texture.</param>
		/// <exception cref="InvalidDataException"></exception>
		public IndexTexture(int width, int height, byte[] data, string name)
			: this(width, height, data, name, 0) { }

		/// <summary>
		/// Creates a new index texture from preexisting data.
		/// </summary>
		/// <param name="width">Width of the texture in pixels.</param>
		/// <param name="height">Height of the texture in pixels.</param>
		/// <param name="data">Raw pixel data to use.</param>
		/// <exception cref="InvalidDataException"></exception>
		public IndexTexture(int width, int height, byte[] data)
			: this(width, height, data, string.Empty, 0) { }


		/// <inheritdoc/>
		protected override int CalculateExpectedDataLength(int width, int height)
		{
			return width * height;
		}

		/// <inheritdoc/>
		public override bool CheckIsTransparent()
		{
			return false;
		}

		/// <inheritdoc/>
		public override ReadOnlySpan<byte> GetColorPixels()
		{
			byte[] result = new byte[Width * Height * 4];
			Span<byte> destination = result;

			if(IsIndex4)
			{
				TexturePalette palette = Palette ?? TexturePalette.Index4Palette;
				ReadOnlySpan<byte> colors = palette.ColorData;

				int paletteOffset = 16 * PaletteRow % palette.Width * 4;
				int index = 0;
				for(int y = 0; y < Height; y++)
				{
					for(int x = 0; x < Width; x++)
					{
						colors.Slice(paletteOffset + ((Data[index] >> 4) * 4), 4).CopyTo(destination[(index * 4)..]);
						index++;
					}
				}
			}
			else
			{
				TexturePalette palette = Palette ?? TexturePalette.Index8Palette;
				ReadOnlySpan<byte> colors = palette.ColorData;

				int paletteOffset = 256 * PaletteRow % palette.Width * 4;
				int index = 0;
				for(int y = 0; y < Height; y++)
				{
					for(int x = 0; x < Width; x++)
					{
						colors.Slice(paletteOffset + (Data[index] * 4), 4).CopyTo(destination[(index * 4)..]);
						index++;
					}
				}
			}


			return result;
		}


		/// <summary>
		/// Encode the indexed texture as a PNG/DDS file.
		/// </summary>
		/// <param name="stream">The file data stream to write to.</param>
		/// <param name="storeInAlpha">Whether the index should be stored in the alpha channel, instead of outputing a grayscale image.</param>
		public void WriteIndexedToPNGFileStream(Stream stream, bool storeInAlpha)
		{
			PngEncoder encoder = new()
			{
				BitDepth = IsIndex4 ? PngBitDepth.Bit4 : PngBitDepth.Bit8,
				ColorType = storeInAlpha ? PngColorType.GrayscaleWithAlpha : PngColorType.Grayscale
			};

			if(storeInAlpha)
			{
				Image.LoadPixelData<A8>(Data, Width, Height).SaveAsPng(stream, encoder);
			}
			else
			{
				Image.LoadPixelData<L8>(Data, Width, Height).SaveAsPng(stream, encoder);
			}
		}

		/// <summary>
		/// Write the indexed texture to a PNG file.
		/// </summary>
		/// <param name="filepath">The path to the file to write to.</param>
		/// <param name="storeInAlpha">Whether the index should be stored in the alpha channel, instead of outputing a grayscale image.</param>
		public void WriteIndexedToPNGFile(string filepath, bool storeInAlpha)
		{
			using(FileStream stream = File.Create(filepath))
			{
				WriteIndexedToPNGFileStream(stream, storeInAlpha);
			}
		}

		/// <summary>
		/// Encode the indexed texture as a PNG file.
		/// </summary>
		/// <param name="storeInAlpha">Whether the index should be stored in the alpha channel, instead of outputing a grayscale image.</param>
		public byte[] WriteIndexedToPNGFileData(bool storeInAlpha)
		{
			using(MemoryStream stream = new())
			{
				WriteIndexedToPNGFileStream(stream, storeInAlpha);
				return stream.ToArray();
			}
		}


		/// <summary>
		/// Encode the indexed texture as a DDS file.
		/// </summary>
		/// <param name="stream">The file data stream to write to.</param>
		public void WriteIndexedToDDSFileStream(Stream stream)
		{
			int prevRow = PaletteRow;
			PaletteRow = 0;

			TexturePalette? prevPalette = Palette;
			Palette = IsIndex4 ? TexturePalette.Index4Palette : TexturePalette.Index8Palette;

			new BcEncoder(CompressionFormat.R).EncodeToDds(GetColorPixels(), Width, Height, PixelFormat.Rgba32).Write(stream);

			PaletteRow = prevRow;
			Palette = prevPalette;
		}

		/// <summary>
		/// Write the indexed texture to a DDS file.
		/// </summary>
		/// <param name="filepath">The path to the file to write to.</param>
		public void WriteIndexedToDDSFile(string filepath)
		{
			using(FileStream stream = File.Create(filepath))
			{
				WriteIndexedToDDSFileStream(stream);
			}
		}

		/// <summary>
		/// Encode the indexed texture as a DDS file.
		/// </summary>
		public byte[] WriteIndexedToDDSFileData()
		{
			using(MemoryStream stream = new())
			{
				WriteIndexedToDDSFileStream(stream);
				return stream.ToArray();
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
		public static bool CheckCanReadIndexedFromFile(Stream stream, out bool index4, out bool storedInAlpha)
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
		/// Verifies whether a file is readable as an index texture.
		/// <br/> Does not alter the stream position.
		/// </summary>
		/// <param name="filepath">Path from which the file should be read.</param>
		/// <param name="index4">Whether the file stores 4 bit indices.</param>
		/// <param name="storedInAlpha">Whether the index data is stored in alpha.</param>
		/// <returns>Whether the file can be reas as an index texture.</returns>
		public static bool CheckCanReadIndexedFromFile(string filepath, out bool index4, out bool storedInAlpha)
		{
			using(FileStream stream = File.OpenRead(filepath))
			{
				return CheckCanReadIndexedFromFile(stream, out index4, out storedInAlpha);
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
		public static bool CheckCanReadIndexedFromFile(byte[] data, out bool index4, out bool storedInAlpha)
		{
			using(MemoryStream stream = new(data))
			{
				return CheckCanReadIndexedFromFile(stream, out index4, out storedInAlpha);
			}
		}


		/// <summary>
		/// Attempts to read an indexed texture from a file data stream.
		/// </summary>
		/// <param name="stream">The stream to read the file data from.</param>
		/// <param name="filename">Filename that should be used.</param>
		/// <param name="result">The read index texture. Null if file was not an index texture</param>
		/// <returns>Whether the file was successfully read as index texture.</returns>
		public static bool TryReadIndexedFromFile(Stream stream, string filename, [MaybeNullWhen(false)] out IndexTexture result)
		{
			if(CheckCanReadIndexedFromFile(stream, out bool index4, out bool inAlpha))
			{
				result = inAlpha
					? ReadFromFile<A8>(stream, filename, index4)
					: ReadFromFile<L8>(stream, filename, index4);

				return true;
			}
			else
			{
				result = null;
				return false;
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
				return TryReadIndexedFromFile(stream, Path.GetFileNameWithoutExtension(filepath), out result);
			}
		}

		/// <summary>
		/// Attempts to read an indexed texture from file data.
		/// </summary>
		/// <param name="data">File data to read.</param>
		/// <param name="filename">Filename that should be used.</param>
		/// <param name="result">The read index texture. Null if file was not an index texture</param>
		/// <returns>Whether the file was successfully read as index texture.</returns>
		public static bool TryReadIndexedFromFile(byte[] data, string filename, [MaybeNullWhen(false)] out IndexTexture result)
		{
			using(MemoryStream stream = new(data))
			{
				return TryReadIndexedFromFile(stream, filename, out result);
			}
		}


		/// <summary>
		/// Reads an index texture from a file data stream.
		/// </summary>
		/// <param name="stream">The stream to read the file data from.</param>
		/// <param name="filename">Filename that should be used.</param>
		/// <returns>The read index texture.</returns>
		/// <exception cref="InvalidDataException"></exception>
		public static IndexTexture ReadIndexedFromFile(Stream stream, string filename)
		{
			if(TryReadIndexedFromFile(stream, filename, out IndexTexture? result))
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
		public static IndexTexture ReadIndexedFromFile(string filepath)
		{
			if(TryReadIndexedFromFile(filepath, out IndexTexture? result))
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
		public static IndexTexture ReadIndexedFromFile(byte[] data, string filename)
		{
			if(TryReadIndexedFromFile(data, filename, out IndexTexture? result))
			{
				return result;
			}

			throw new InvalidDataException("File Data was not able to be read as an index texture.");
		}


		private static IndexTexture ReadFromFile<TPixel>(Stream stream, string filename, bool isIndex4)
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

			return new IndexTexture(width, height, data, filename, 0)
			{
				IsIndex4 = isIndex4
			};
		}

	}
}
