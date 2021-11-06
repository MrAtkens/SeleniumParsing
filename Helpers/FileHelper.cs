namespace Helpers
{
    public static class FileHelper
    {
        public static string GetExtension(string contentType)
        {
            switch (contentType)
            {
                case "image/png":
                    return ".png";
                case "image/jpg":
                    return ".jpg";
                case "video/mp4":
                    return ".mp4";
                case "application/msword":
                    return ".doc";
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    return ".docx";
                case "application/pdf":
                    return ".pdf";
                case "application/vnd.ms-powerpoint":
                    return ".ppt";
                case "application/vnd.ms-excel":
                    return ".xls";
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    return ".xlsx";
                case "application/json":
                    return ".json";
                case "text/plain":
                    return ".txt";
                default:
                    return ".png";
            }
        }
    }
}