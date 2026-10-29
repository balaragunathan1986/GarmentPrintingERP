using PdfSharp.Fonts;

namespace Erp.Services.Implementation.Production
{
    public class PdfFontResolver : IFontResolver
    {
        private const string RegularFont = "NotoSans-Regular";
        private const string BoldFont = "NotoSans-Bold";

        public string DefaultFontName =>
            RegularFont;

        public FontResolverInfo? ResolveTypeface(
            string familyName,
            bool isBold,
            bool isItalic)
        {
            // Noto Sans
            if (familyName.Equals(
                    "Noto Sans",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (isBold)
                {
                    return new FontResolverInfo(
                        BoldFont
                    );
                }

                return new FontResolverInfo(
                    RegularFont
                );
            }

            // Courier New
            // Some MigraDoc/PDFsharp internal/default
            // styles may request Courier New.
            if (familyName.Equals(
                    "Courier New",
                    StringComparison.OrdinalIgnoreCase))
            {
                return new FontResolverInfo(
                    isBold
                        ? BoldFont
                        : RegularFont
                );
            }

            // Arial
            if (familyName.Equals(
                    "Arial",
                    StringComparison.OrdinalIgnoreCase))
            {
                return new FontResolverInfo(
                    isBold
                        ? BoldFont
                        : RegularFont
                );
            }

            // Fallback
            return new FontResolverInfo(
                isBold
                    ? BoldFont
                    : RegularFont
            );
        }

        public byte[] GetFont(
            string faceName)
        {
            var fontsPath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Fonts"
                );

            var fileName = faceName switch
            {
                RegularFont =>
                    "NotoSans-Regular.ttf",

                BoldFont =>
                    "NotoSans-Bold.ttf",

                _ =>
                    "NotoSans-Regular.ttf"
            };

            var fontPath =
                Path.Combine(
                    fontsPath,
                    fileName
                );

            if (!File.Exists(fontPath))
            {
                throw new FileNotFoundException(
                    $"Font file not found: {fontPath}"
                );
            }

            return File.ReadAllBytes(fontPath);
        }
    }
}