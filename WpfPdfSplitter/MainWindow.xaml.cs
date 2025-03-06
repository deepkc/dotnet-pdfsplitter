using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PdfSharp.Pdf; // PdfSharp.Pdf.PdfDocument
using PdfSharp.Pdf.IO;
using System.Drawing; // System.Drawing.Bitmap
using System.Drawing.Imaging;

namespace WpfPdfSplitter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Select PDF file
        private void BtnSelectPdf_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Select PDF to Split"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                TxtPdfPath.Text = openFileDialog.FileName;
                LoadPdfPreview(openFileDialog.FileName);
            }
        }

        // Load PDF preview (render the first page as an image using System.Drawing)
        private void LoadPdfPreview(string filePath)
        {
            try
            {
                // Open the PDF using PdfSharp
                using (PdfDocument document = PdfReader.Open(filePath, PdfDocumentOpenMode.Import))
                {
                    // Render the first page as an image
                    var page = document.Pages[0];
                    var image = ConvertPdfPageToImage(page);

                    // Set the image as the source for the Image control
                    PdfPreviewImage.Source = ConvertToImageSource(image);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load preview: " + ex.Message);
            }
        }

        // Convert PdfPage to System.Drawing.Image
        private Image ConvertPdfPageToImage(PdfPage page)
        {
            var width = page.Width;
            var height = page.Height;

            // Create a bitmap image
            Bitmap bitmap = new Bitmap((int)width, (int)height);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(System.Drawing.Color.White); // Set background color

            // Render the page (you may need a library like PdfSharp.Rendering for rendering PDF content)
            // For the sake of this example, we assume we're rendering a blank page.
            // In practice, you can use additional rendering tools to extract content.

            return bitmap;
        }

        // Convert System.Drawing.Image to BitmapSource for WPF display
        private BitmapSource ConvertToImageSource(System.Drawing.Image image)
        {
            Bitmap bitmap = (Bitmap)image;
            var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                              ImageLockMode.ReadOnly,
                                              bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(bitmap.Width, bitmap.Height, 96, 96,
                                                   System.Windows.Media.PixelFormats.Bgra32,
                                                   null, bitmapData.Scan0, bitmapData.Stride * bitmap.Height,
                                                   bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        // Split PDF into separate pages using PdfSharp
        private void BtnSplitPdf_Click(object sender, RoutedEventArgs e)
        {
            string inputPdf = TxtPdfPath.Text;
            if (string.IsNullOrEmpty(inputPdf) || !File.Exists(inputPdf))
            {
                MessageBox.Show("Please select a valid PDF file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputPdf), "Split_PDFs");
                Directory.CreateDirectory(outputFolder);

                // Open the PDF document using PdfSharp
                using (PdfDocument inputDocument = PdfReader.Open(inputPdf, PdfDocumentOpenMode.Import))
                {
                    for (int i = 0; i < inputDocument.PageCount; i++)
                    {
                        PdfDocument outputDocument = new PdfDocument();
                        outputDocument.AddPage(inputDocument.Pages[i]);

                        string outputPdfPath = Path.Combine(outputFolder, $"Page_{i + 1}.pdf");
                        outputDocument.Save(outputPdfPath);
                    }
                }

                MessageBox.Show("PDF split successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error splitting PDF: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
