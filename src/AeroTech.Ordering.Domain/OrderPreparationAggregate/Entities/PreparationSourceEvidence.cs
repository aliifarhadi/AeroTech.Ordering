using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.Entities
{
    public sealed class PreparationSourceEvidence : Entity<long>
    {
        private PreparationSourceEvidence()
        {
        }

        internal PreparationSourceEvidence(long id, long preparationId, string evidenceRef, string payloadHash, string contentType, string payload, DateTimeOffset capturedAt)
        {
            Id = id;
            PreparationId = preparationId;
            EvidenceRef = evidenceRef;
            PayloadHash = payloadHash;
            ContentType = contentType;
            Payload = payload;
            CapturedAt = capturedAt;
        }

        public long PreparationId { get; private set; }

        public string EvidenceRef { get; private set; } = null!;

        public string PayloadHash { get; private set; } = null!;

        public string ContentType { get; private set; } = null!;

        public string Payload { get; private set; } = null!;

        public DateTimeOffset CapturedAt { get; private set; }
    }
}
