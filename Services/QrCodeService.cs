using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.SkiaSharp;
using SkiaSharp;
using System.IO;
using ZXing.Common;


namespace SRQCC.Services
{
    public class QrCodeService
    {
        public ImageSource GenerateQrCodeSimple(string text, int size = 200)
        {
            try
            {

                var barcodeWriter = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new EncodingOptions
                    {
                        Width = size,
                        Height = size,
                        Margin = 1,
                        PureBarcode = true
                    }
                };

                var bitmap = barcodeWriter.Write(text);

                if (bitmap == null)
                {
                    return null;
                }


                // Conversión DIRECTA y SIMPLE
                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    var stream = new MemoryStream();
                    data.SaveTo(stream);
                    stream.Position = 0;
                    //Console.WriteLine(" Conversión exitosa");
                    return ImageSource.FromStream(() => stream);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"ERROR: {ex.Message}");
                //Console.WriteLine($" StackTrace: {ex.StackTrace}");
                return null;
            }
        }
    }
}
