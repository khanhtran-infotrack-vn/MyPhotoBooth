namespace MyPhotoBooth.Application.Common;

public static class Errors
{
    public static class Auth
    {
        public const string EmailAlreadyExists = "Email already registered";
        public const string InvalidCredentials = "Invalid email or password";
        public const string UserNotFound = "User not found";
        public const string InvalidToken = "Invalid or expired token";
        public const string PasswordResetFailed = "Failed to reset password";
    }

    public static class Photos
    {
        public const string NotFound = "Photo not found";
        public const string UnauthorizedAccess = "You do not have access to this photo";
        public const string InvalidFile = "Invalid image file";
        public const string FileTooLarge = "File size exceeds limit";
        public const string NoFileUploaded = "No file uploaded";
        public const string StorageError = "Failed to store file";
    }

    public static class Albums
    {
        public const string NotFound = "Album not found";
        public const string UnauthorizedAccess = "You do not have access to this album";
        public const string PhotoNotInAlbum = "Photo is not in this album";
        public const string PhotoAlreadyInAlbum = "Photo is already in this album";
    }

    public static class Tags
    {
        public const string NotFound = "Tag not found";
        public const string UnauthorizedAccess = "You do not have access to this tag";
    }

    public static class ShareLinks
    {
        public const string NotFound = "Share link not found";
        public const string Expired = "Share link has expired";
        public const string Revoked = "Share link has been revoked";
        public const string InvalidPassword = "Incorrect password";
        public const string PasswordRequired = "Password required";
        public const string DownloadNotAllowed = "Download is not allowed";
        public const string PhotoIdRequired = "PhotoId is required for photo shares";
        public const string AlbumIdRequired = "AlbumId is required for album shares";
    }

    public static class General
    {
        public const string Unauthorized = "You are not authorized to perform this action";
        public const string NotFound = "Resource not found";
    }
}
