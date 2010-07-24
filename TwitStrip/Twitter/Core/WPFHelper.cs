using System;
using System.Windows.Documents;

namespace Core
{
    static class WPFHelper
    {
        /// <summary> Convert a string into an array of inline containing plain text and hyperlinks </summary>
        public static Inline[] CreateInlineTextWithLinks(string sText, EventHandler<System.Windows.RoutedEventArgs> ClickMethod) {
            Paragraph para = new Paragraph();
            int iURLPos = 0;
            char[] EndOfURL = new char[] { ' ', ',' };
            const string HTTP = "http://";

            do {
                // Search for a url
                iURLPos = sText.IndexOf(HTTP, StringComparison.CurrentCultureIgnoreCase);

                if (iURLPos == -1)  // No url found so just add the text
                    para.Inlines.Add(sText);

                if (iURLPos > -1) {
                    // Add normal text up to the point of the url
                    para.Inlines.Add(sText.Substring(0, iURLPos));

                    // Find the length of the url
                    int iURLLength = sText.IndexOfAny(EndOfURL, iURLPos) - iURLPos;

                    // iEndOfURLPos < 0 means url was at the end of the text, so calculate based on text length
                    if (iURLLength < 0) iURLLength = sText.Length - iURLPos;

                    // Create the hyperlink
                    string sHyper = sText.Substring(iURLPos, iURLLength);

                    if (sHyper == HTTP)
                        para.Inlines.Add(sHyper);
                    else
                        para.Inlines.Add(CreateHyperLink(sHyper, sHyper, ClickMethod));

                    // Shorten text to the end of the url onwards
                    sText = sText.Substring(iURLPos + iURLLength);
                }
            } while (iURLPos != -1);

            Inline[] lines = new Inline[para.Inlines.Count];
            para.Inlines.CopyTo(lines, 0);

            return lines;
        }

		/// <summary> Create a WPF Hyperlink class </summary>
        public static Hyperlink CreateHyperLink(string sURI, string sDescription, EventHandler<System.Windows.RoutedEventArgs> ClickMethod)
        {
            Hyperlink hyper = new Hyperlink();
            hyper.Inlines.Add(sDescription);
            hyper.NavigateUri = new System.Uri(sURI);
            hyper.Click += new System.Windows.RoutedEventHandler(ClickMethod);
            return hyper;
        }
    }	
}
