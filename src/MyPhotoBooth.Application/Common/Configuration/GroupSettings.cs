namespace MyPhotoBooth.Application.Common.Configuration;

public class GroupSettings
{
    public const string SectionName = "GroupSettings";

    public int MaxMembersPerGroup { get; set; } = 50;
    public int DeletionDays { get; set; } = 90;
    public int MemberContentGraceDays { get; set; } = 7;
    public int[] ReminderDays { get; set; } = { 60, 30, 7, 1 };
    public string CleanupServiceInterval { get; set; } = "01:00:00";
}
