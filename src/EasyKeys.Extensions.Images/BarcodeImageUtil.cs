using System.Drawing;
using System.Drawing.Imaging;

namespace EasyKeys.Extensions.Images
{
    /// <summary>
    /// https://www.codeproject.com/Articles/6512/Reading-Barcodes-from-an-Image.
    /// </summary>
    public sealed class BarcodeImageUtil
    {
        public static string ReadCode39(Bitmap bmp, bool showDiagnostics, Color diagnosticsColor)
        {
            // To find a horizontal barcode, find the vertical histogram to find individual barcodes,
            // then get the vertical histogram to decode each
            var vertHist = VerticalHistogram(bmp, showDiagnostics, diagnosticsColor);

            // Set the threshold for determining dark/light bars to half way between the histograms min/max
            var threshold = vertHist.Min + ((vertHist.Max - vertHist.Min) / 2);

            // Variables needed to check for
            string? patternString = null;
            var nBarStart = -1;
            var nNarrowBarWidth = -1;
            var bDarkBar = false;

            // Find the narrow and wide bars
            for (var i = 0; i < vertHist.Histogram.Length; ++i)
            {
                // First find the narrow bar width
                if (nNarrowBarWidth < 0)
                {
                    if (nBarStart < 0)
                    {
                        // The code doesn't start until we see a dark bar
                        if (vertHist.Histogram[i] <= threshold)
                        {
                            // We detected a dark bar, save it's start position
                            nBarStart = i;
                        }
                    }
                    else
                    {
                        if (vertHist.Histogram[i] > threshold)
                        {
                            // We detected the end of first the dark bar, save the narrow bar width and
                            // start the rest of the barcode
                            nNarrowBarWidth = i - nBarStart + 1;
                            patternString += "n";
                            nBarStart = i;
                            bDarkBar = false;
                        }
                    }
                }
                else
                {
                    if (bDarkBar)
                    {
                        // We're on a dark bar, detect when the bar becomes light again
                        if (vertHist.Histogram[i] > threshold)
                        {
                            if ((i - nBarStart) > nNarrowBarWidth)
                            {
                                // The light bar was wider than the narrow bar width, it's a wide bar
                                patternString += "w";
                                nBarStart = i;
                            }
                            else
                            {
                                // The light bar is a narrow bar
                                patternString += "n";
                                nBarStart = i;
                            }

                            bDarkBar = false;
                        }
                    }
                    else
                    {
                        // We're on a light bar, detect when the bar becomes dark
                        if (vertHist.Histogram[i] <= threshold)
                        {
                            if ((i - nBarStart) > nNarrowBarWidth)
                            {
                                // The dark bar was wider than the narrow bar width, it's a wide bar
                                patternString += "w";
                                nBarStart = i;
                            }
                            else
                            {
                                // The dark bar is a narrow bar
                                patternString += "n";
                                nBarStart = i;
                            }

                            bDarkBar = true;
                        }
                    }
                }
            }

            // We now have a barcode in terms of narrow & wide bars... Parse it!
            var dataString = string.Empty;

            // Each pattern within code 39 is nine bars with one white bar between each pattern
            for (var i = 0; i < patternString.Length - 1; i += 10)
            {
                // Create an array of characters to hold the pattern to be tested
                var pattern = new char[9];

                // Stuff the pattern with data from the pattern string
                patternString.CopyTo(i, pattern, 0, 9);

                dataString += ParsePattern(new string(pattern));
            }

            return dataString;
        }

        /// <summary>
        /// Vertical histogram of an image.
        /// </summary>
        /// <param name="bmp">the bitmap to be processed.</param>
        /// <param name="showDiagnostics">if true, draws an overlay on the image.</param>
        /// <param name="diagnosticsColor">the color of the overlay.</param>
        /// <returns>a histogramResult representing the vertical histogram.</returns>
        private static HistogramResult VerticalHistogram(Bitmap bmp, bool showDiagnostics, Color diagnosticsColor)
        {
            // Create the return value
            var histResult = new float[bmp.Width];

            var vertSum = new float[bmp.Width];

            // Start the max value at zero
            float maxValue = 0;

            // Start the min value at the absolute maximum
            float minValue = 255;

            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            var bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            var stride = bmData.Stride;
            var scan0 = bmData.Scan0;

            unsafe
            {
                var p = (byte*)(void*)scan0;

                var nOffset = stride - (bmp.Width * 3);
                var nWidth = bmp.Width * 3;

                for (var y = 0; y < bmp.Height; ++y)
                {
                    for (var x = 0; x < bmp.Width; ++x)
                    {
                        // Add up all the pixel values vertically (average the R,G,B channels)
                        vertSum[x] += (p[0] + (p + 1)[0] + (p + 2)[0]) / 3;

                        p += 3;
                    }

                    p += nOffset;
                }
            }

            bmp.UnlockBits(bmData);

            // Now get the average of the row by dividing the pixel by num pixels
            for (var i = 0; i < bmp.Width; i++)
            {
                histResult[i] = vertSum[i] / bmp.Height;

                // Save the max value for later
                if (histResult[i] > maxValue)
                {
                    maxValue = histResult[i];
                }

                // Save the min value for later
                if (histResult[i] < minValue)
                {
                    minValue = histResult[i];
                }
            }

            // Use GDI+ to draw a lines up the image showing each lines average
            if (showDiagnostics)
            {
                // Set the alpha of the overlay to half transparency
                var alpha = (int)255 / 2;
                var p = new Pen(Color.FromArgb(alpha, diagnosticsColor));

                // Get the graphics to draw on
                Graphics g;
                g = Graphics.FromImage(bmp);
                for (var i = 0; i < bmp.Width; ++i)
                {
                    // Normalize the drawn histogram to the height of the image
                    var drawLength = (int)(histResult[i] * (bmp.Height / maxValue));
                    g.DrawLine(p, i, bmp.Height, i, bmp.Height - (int)drawLength);
                }

                p.Dispose();
                g.Dispose();
            }

            var retVal = new HistogramResult
            {
                Histogram = histResult,
                Max = maxValue,
                Min = minValue
            };
            return retVal;
        }

        private static string? ParsePattern(string pattern)
        {
            switch (pattern)
            {
                case "wnnwnnnnw":
                    return "1";
                case "nnwwnnnnw":
                    return "2";
                case "wnwwnnnnn":
                    return "3";
                case "nnnwwnnnw":
                    return "4";
                case "wnnwwnnnn":
                    return "5";
                case "nnwwwnnnn":
                    return "6";
                case "nnnwnnwnw":
                    return "7";
                case "wnnwnnwnn":
                    return "8";
                case "nnwwnnwnn":
                    return "9";
                case "nnnwwnwnn":
                    return "0";
                case "wnnnnwnnw":
                    return "A";
                case "nnwnnwnnw":
                    return "B";
                case "wnwnnwnnn":
                    return "C";
                case "nnnnwwnnw":
                    return "D";
                case "wnnnwwnnn":
                    return "E";
                case "nnwnwwnnn":
                    return "F";
                case "nnnnnwwnw":
                    return "G";
                case "wnnnnwwnn":
                    return "H";
                case "nnwnnwwnn":
                    return "I";
                case "nnnnwwwnn":
                    return "J";
                case "wnnnnnnww":
                    return "K";
                case "nnwnnnnww":
                    return "L";
                case "wnwnnnnwn":
                    return "M";
                case "nnnnwnnww":
                    return "N";
                case "wnnnwnnwn":
                    return "O";
                case "nnwnwnnwn":
                    return "P";
                case "nnnnnnwww":
                    return "Q";
                case "wnnnnnwwn":
                    return "R";
                case "nnwnnnwwn":
                    return "S";
                case "nnnnwnwwn":
                    return "T";
                case "wwnnnnnnw":
                    return "U";
                case "nwwnnnnnw":
                    return "V";
                case "wwwnnnnnn":
                    return "W";
                case "nwnnwnnnw":
                    return "X";
                case "wwnnwnnnn":
                    return "Y";
                case "nwwnwnnnn":
                    return "Z";
                case "nwnnnnwnw":
                    return "-";
                case "wwnnnnwnn":
                    return ".";
                case "nwwnnnwnn":
                    return " ";
                case "nwnnwnwnn":
                    return "*";
                case "nwnwnwnnn":
                    return "$";
                case "nwnwnnnwn":
                    return "/";
                case "nwnnnwnwn":
                    return "+";
                case "nnnwnwnwn":
                    return "%";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Structure used to return the processed data from an image's histogram.
        /// </summary>
        private struct HistogramResult
        {
            public float[] Histogram;
            public float Min;
            public float Max;
        }
    }
}
