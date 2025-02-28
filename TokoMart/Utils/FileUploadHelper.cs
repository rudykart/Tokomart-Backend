namespace TokoMart.Utils
{
    public static class FileUploadHelper
    {
        private static readonly List<string> _allowedFormats = new List<string>
        {
            "image/jpeg",
            "image/png",
            "image/jpg"
        };

        private const long MaxFileSize = 2 * 1024 * 1024; // 5 MB

        // Validasi format file
        public static bool IsValidFormat(IFormFile file)
        {
            return _allowedFormats.Contains(file.ContentType);
        }

        // Validasi ukuran file
        public static bool IsValidSize(IFormFile file)
        {
            return file.Length <= MaxFileSize;
        }

        // Validasi gabungan dengan pesan kesalahan
        public static string ValidateFile(IFormFile file)
        {
            if (!IsValidFormat(file))
            {
                return "Invalid file format. Only JPG, PNG, and GIF are allowed.";
            }

            if (!IsValidSize(file))
            {
                return "File size exceeds the maximum limit of 5 MB.";
            }

            return null; 
        }
    }
}
