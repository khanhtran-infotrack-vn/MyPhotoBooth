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

    public static class Groups
    {
        public const string NotFound = "Group not found";
        public const string UnauthorizedAccess = "You do not have access to this group";
        public const string NotAMember = "You are not a member of this group";
        public const string AlreadyAMember = "User is already a member of this group";
        public const string NotOwner = "Only the group owner can perform this action";
        public const string LastOwnerCannotLeave = "The last owner cannot leave without transferring ownership";
        public const string ContentNotShared = "Content is not shared to this group";
        public const string MemberNotFound = "Member not found in group";
        public const string InvalidEmail = "Invalid email address";
        public const string UserNotFound = "User not found";
        public const string CannotRemoveOwner = "Cannot remove the group owner";
        public const string DeletionAlreadyScheduled = "Group deletion is already scheduled";
        public const string GroupIsDeleted = "This group has been deleted";
        public const string GroupFull = "Group has reached maximum member limit";
        public const string CannotTransferToSelf = "Cannot transfer ownership to yourself";
        public const string PhotoNotFound = "Photo not found";
        public const string AlbumNotFound = "Album not found";
        public const string InvalidContentType = "Invalid content type";
        public const string MustSpecifyContentId = "Must specify PhotoId or AlbumId based on ContentType";
    }
}
