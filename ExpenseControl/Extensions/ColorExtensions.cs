namespace ExpenseControl.Extensions
{
    public static class ColorExtensions
    {
        public static string GetContrastColor(this string hexColor)
        {
            // Domyślny kolor jeśli null lub pusty
            if (string.IsNullOrEmpty(hexColor)) return "black";

            // Jeśli nie ma # na początku, dodaj go (dla bezpieczeństwa)
            if (!hexColor.StartsWith("#")) hexColor = "#" + hexColor;

            try
            {
                // Wyciągamy R, G, B z Hexa
                // Zakładamy format #RRGGBB
                if (hexColor.Length == 7)
                {
                    var r = Convert.ToInt32(hexColor.Substring(1, 2), 16);
                    var g = Convert.ToInt32(hexColor.Substring(3, 2), 16);
                    var b = Convert.ToInt32(hexColor.Substring(5, 2), 16);

                    // Obliczamy luminancję (wzór na jasność postrzeganą przez ludzkie oko)
                    var yiq = ((r * 299) + (g * 587) + (b * 114)) / 1000;

                    // Jeśli jasne tło (>128), daj czarny tekst. Jeśli ciemne, daj biały.
                    return yiq >= 128 ? "black" : "white";
                }
                return "black"; // Fallback
            }
            catch
            {
                return "black";
            }
        }
    }
}