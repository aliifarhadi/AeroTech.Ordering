namespace AeroTech.Ordering.ReferenceData.ReadModels
{
    public sealed class CustomerReadModel : IReferenceReadModel<long>
    {
        public long Id { get; set; }
        public string CustomerNumber { get; set; } = default!;
        public CustomerType Type { get; set; }
        public long? TravelAgencyId { get; set; }
        public long SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public CustomerStatus Status { get; set; }
        public int? PreferredCurrencyId { get; set; }
        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
