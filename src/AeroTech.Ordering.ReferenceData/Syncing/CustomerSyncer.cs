using AeroTech.Ordering.ReferenceData.Core;
using AeroTech.Ordering.ReferenceData.Core.Wire;
using AeroTech.Ordering.ReferenceData.Persistence;
using AeroTech.Ordering.ReferenceData.ReadModels;

namespace AeroTech.Ordering.ReferenceData.Syncing
{
    public sealed class CustomerSyncer : ReferenceSyncerBase<CustomerReadModel, CustomerDto, long>
    {
        private readonly ICoreClient _client;

        public CustomerSyncer(ReferenceDbContext db, ICoreClient client, TimeProvider timeProvider)
            : base(db, timeProvider) => _client = client;

        protected override string Resource => "Customers";

        protected override Task<List<CustomerDto>> FetchAsync(DateTimeOffset? modifiedAfter, CancellationToken cancellationToken)
            => _client.GetCustomersAsync(modifiedAfter, cancellationToken);

        protected override CustomerReadModel CreateNew(CustomerDto dto) => new()
        {
            Id = dto.Id,
            CustomerNumber = dto.CustomerNumber,
            Type = dto.CustomerType,
            TravelAgencyId = dto.TravelAgencyId,
            SubjectId = dto.SubjectId,
            SubjectName = dto.SubjectName,
            Status = dto.Status,
            PreferredCurrencyId = dto.PreferredCurrencyId,
            LastUpdateTime = dto.LastUpdateTime
        };

        protected override void ApplyChanges(CustomerDto dto, CustomerReadModel model)
        {
            model.CustomerNumber = dto.CustomerNumber;
            model.Type = dto.CustomerType;
            model.TravelAgencyId = dto.TravelAgencyId;
            model.SubjectId = dto.SubjectId;
            model.SubjectName = dto.SubjectName;
            model.Status = dto.Status;
            model.PreferredCurrencyId = dto.PreferredCurrencyId;
            model.LastUpdateTime = dto.LastUpdateTime;
        }
    }
}
