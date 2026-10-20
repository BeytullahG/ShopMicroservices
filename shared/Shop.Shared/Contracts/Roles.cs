namespace Shop.Shared.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";
    public static readonly IReadOnlyList<string> All = [Admin, Customer];
}