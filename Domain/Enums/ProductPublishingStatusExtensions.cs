namespace vitacure.Domain.Enums;

public static class ProductPublishingStatusExtensions
{
    public static bool IsPubliclyVisible(this ProductPublishingStatus status)
    {
        return status == ProductPublishingStatus.PublishedOpen;
    }

    public static bool AllowsIncompleteSave(this ProductPublishingStatus status)
    {
        return status == ProductPublishingStatus.Draft;
    }

    public static string ToAdminLabel(this ProductPublishingStatus status)
    {
        return status switch
        {
            ProductPublishingStatus.PublishedClosed => "Satisa Kapali Yayin",
            ProductPublishingStatus.PublishedOpen => "Satışa Açık Yayın",
            ProductPublishingStatus.Archived => "Arsiv",
            _ => "Taslak"
        };
    }
}
