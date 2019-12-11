using System;
using iTextSharp.text.pdf;
using System.Text;
using iTextSharp.text.pdf.parser;
using System.Collections.Generic;
using System.Drawing;
using IronOcr;

namespace pdf
{
	class Program
	{
		/// <summary>
		/// Parse a Pdf file in two part.
		/// Every part will be written in an existing text file (which in my case is on the desktop)
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			const string filePath = @"c:\Users\sofiane\Desktop\25_1.pdf";
			const string outPath = @"c:\Users\sofiane\Desktop\test.txt";

			//The following line is to prevent a warning when reading more than one pdf page. On the second page you'll get an encoding error otherwise

			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
			PdfReader reader = new PdfReader(filePath);
			string strText = string.Empty;

			HashSet<String> names = new HashSet<string>();
			var pdfDictionary = new PdfDictionary();

			//ITextExtractionStrategy its = new iTextSharp.text.pdf.parser.LocationTextExtractionStrategy() if you want to extract info everywhere;

			for (int page = 1; page <= reader.NumberOfPages; page++)
			{
				PdfDictionary dic = reader.GetPageN(page);
				PdfDictionary resources = dic.GetAsDict(PdfName.RESOURCES);
				if (resources != null)
				{
					//get fonts dico
					PdfDictionary fonts = resources.GetAsDict(PdfName.FONT);
					if (fonts != null)
					{
						PdfDictionary font;
						foreach (PdfName key in fonts.Keys)
						{
							font = fonts.GetAsDict(key);
							string name = font.GetAsName(PdfName.BASEFONT).ToString();
							Console.WriteLine(name);
							names.Add(name);
						}
					}
				}
				//ITextExtractionStrategy its = new FilteredTextRenderListener(new LocationTextExtractionStrategy(), filter);
				RenderFilter[] filter = get_render(page, 0);
				ITextExtractionStrategy its = new FilteredTextRenderListener(new LocationTextExtractionStrategy(), filter);
				WriteInfile(reader, page, its, outPath);

				leaRenderFilter[] filter2 = get_render(page, 1);
				ITextExtractionStrategy itsL = new FilteredTextRenderListener(new LocationTextExtractionStrategy(), filter2);
				WriteInfile(reader, page, itsL, outPath);
			}

			reader.Close();
		}

		/// <summary>
		/// Get the render format for the data extraction, you'll get the left and right side. And for the first page you'll get a shorter format of the rectangles so we can skip unnecessary info
		/// Premiere page sans en tête.
		/// Rectangle (lower left x, lower left y, upper right x, upper right y)
		/// </summary>
		/// <param name="page"> Which page are we extracting the date from</param>
		/// <param name="right">That's to know which side of the page we should extract the information left or right side </param>
		/// <returns></returns>
		private static RenderFilter[] get_render(int page, int right)
		{
			if (page == 1)
			{
				if (right == 0)
				{
					System.util.RectangleJ rect = new System.util.RectangleJ(0, 0, 536 / 2, 500);
					RenderFilter[] filter = { new RegionTextRenderFilter(rect) };
					return (filter);
				}
				else
				{
					System.util.RectangleJ rectL = new System.util.RectangleJ(300, 0, 536 / 2, 500);
					RenderFilter[] filterL = { new RegionTextRenderFilter(rectL) };
					return (filterL);
				}
			}
			else 
			{
				if (right == 0)
				{
					System.util.RectangleJ rect = new System.util.RectangleJ(0, 0, 536 / 2, 830);
					RenderFilter[] filter = { new RegionTextRenderFilter(rect) };
					return (filter);
				}
				else
				{
					System.util.RectangleJ rectL = new System.util.RectangleJ(300, 0, 536 / 2, 830);
					RenderFilter[] filterL = { new RegionTextRenderFilter(rectL) };
					return (filterL);
				}
			}
		}

		/// <summary>
		/// Writing the extracted date into textfile.
		/// </summary>
		/// <param name="reader"> Open reader to the to read pdf file </param>
		/// <param name="page"> which page are we going to extract the information from the pdf file </param>
		/// <param name="its"> Which extraction strategy do we use when extracting our data </param>
		/// <param name="outPath"> Where is the textfile located in my computer </param>
		/// 
		private static void WriteInfile(PdfReader reader, int page, ITextExtractionStrategy its, string outPath)
		{
			string strText = string.Empty;
			strText = PdfTextExtractor.GetTextFromPage(reader, page, its);
			strText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(strText)));
			string[] lines = strText.Split('\n');
			foreach (string line in lines)
			{
				using (System.IO.StreamWriter file = new System.IO.StreamWriter(outPath, true))
				{
					string test = line + "\0";
					int index = test.Length;
					if (index > 0 && index < 55 && !char.IsPunctuation(test[index - 2]) && !char.IsDigit(test[0]))
					{
						Console.WriteLine("TITLE = " + line + "  " + index);
						file.Write("Title - - - - - ");
						file.WriteLine(line + "\n");
					}
					else
						file.WriteLine(line);
				}
			}
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(outPath, true))
				file.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - ");
		}
	}
}
