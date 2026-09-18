using AeroTech.Ordering.ReferenceData.ReadModels;
using AeroTech.Ordering.ReferenceData.Syncing;

namespace AeroTech.Ordering.ReferenceData.Core.Wire
{
    public sealed class CoreEnvelope<T>
    {
        public T? Data { get; set; }

        public CoreError[]? Errors { get; set; }
    }

    public sealed class CoreError
    {
        public int? Code { get; set; }

        public string? Title { get; set; }

        public string? Detail { get; set; }
    }

    public sealed class CustomerDto : ISyncSourceDto<long>
    {
        public long Id { get; set; }
        public string CustomerNumber { get; set; } = default!;
        public CustomerType CustomerType { get; set; }
        public long? TravelAgencyId { get; set; }
        public long SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public CustomerStatus Status { get; set; }
        public int? PreferredCurrencyId { get; set; }
        public DateTimeOffset LastUpdateTime { get; set; }
    }

    public sealed class OperatorSettingsDto : ISyncSourceDto<long>
    {
        public long Id { get; set; }
        public string ScopeKey { get; set; } = default!;
        public long HomeAirlineId { get; set; }
        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
