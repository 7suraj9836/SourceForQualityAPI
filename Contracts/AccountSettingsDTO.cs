namespace SourceforqualityAPI.Contracts
{
    public class AccountSettingsDTO
    {
        public int UserId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Logo { get; set; }
        public bool IsSubscribed { get; set; }
        public int RoleId { get; set; }
        public int SubscriptionPlanId { get; set; }
        public string ? MobileNo { get; set; }
    }
}
