using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.Ports.Offers;
using AeroTech.Ordering.Providers.AirOffer.Options;
using AeroTech.Ordering.Providers.AirOffer.Wire;
using Microsoft.Extensions.Options;

namespace AeroTech.Ordering.Providers.AirOffer.Services
{
    public sealed class AirOfferSourceAdapter : IOfferSourcePort
    {
        public const string PayloadContentType = "application/json";

        private static readonly JsonSerializerOptions WireOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _httpClient;
        private readonly AirOfferOptions _options;
        private readonly IClock _clock;

        public AirOfferSourceAdapter(HttpClient httpClient, IOptions<AirOfferOptions> options, IClock clock)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _clock = clock;
        }

        public async Task<CandidateResolution> ResolveCandidateAsync(ResolveCandidateRequest request, CancellationToken cancellationToken = default)
        {
            if (request.RequestedSelection.Count > 0)
                return Failed(OfferResolutionOutcome.UnsupportedCapability, null, "the observed Details contract supports only the full priced candidate");

            var attempts = Math.Max(1, _options.RetryCount + 1);
            string? lastFailure = null;

            for (var attempt = 1; attempt <= attempts; attempt++)
            {
                try
                {
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, _options.RequestTimeout)));

                    using var response = await _httpClient.PostAsJsonAsync(AirOfferProfile.DetailsRoute, new { offerId = request.OfferId }, timeout.Token);
                    var payload = await response.Content.ReadAsStringAsync(timeout.Token);

                    if ((int)response.StatusCode >= 500)
                    {
                        lastFailure = $"AirOffer returned {(int)response.StatusCode}";
                        await DelayAsync(attempt, attempts, cancellationToken);
                        continue;
                    }

                    return Interpret(request, response.StatusCode, payload);
                }
                catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
                {
                    lastFailure = exception.Message;
                    await DelayAsync(attempt, attempts, cancellationToken);
                }
            }

            return Failed(OfferResolutionOutcome.Unavailable, null, lastFailure ?? "AirOffer did not answer");
        }

        public Task<BoundCandidateEvidence> ReadBoundCandidateAsync(ReadBoundCandidateRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new BoundCandidateEvidence(
                OfferResolutionOutcome.UnsupportedCapability,
                null,
                AirOfferProfile.Profile,
                _clock.GetDateTime(),
                ["AirOffer supplies no immutable accepted binding or read-back (BD-001)"]));

        private CandidateResolution Interpret(ResolveCandidateRequest request, HttpStatusCode status, string payload)
        {
            var evidence = new SourceEvidence(
                $"airoffer:details:{_clock.GetDateTime().UtcTicks}",
                Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant(),
                PayloadContentType,
                payload);

            AirOfferEnvelopeWire? envelope;

            try
            {
                envelope = JsonSerializer.Deserialize<AirOfferEnvelopeWire>(payload, WireOptions);
            }
            catch (JsonException exception)
            {
                return Failed(OfferResolutionOutcome.ContractMismatch, evidence, $"AirOffer response is not the observed envelope ({exception.Message})");
            }

            if ((int)status >= 400 || envelope?.Data is null)
            {
                var errors = envelope?.Errors?.Select(error => $"{error.Code}:{error.Title ?? error.Detail}") ?? [];
                var reason = $"AirOffer {(int)status}: {string.Join(", ", errors)}";

                return (int)status is >= 400 and < 500
                    ? Failed(OfferResolutionOutcome.NotFound, evidence, reason)
                    : Failed(OfferResolutionOutcome.ContractMismatch, evidence, reason);
            }

            try
            {
                var candidate = AirOfferCandidateMapper.Map(request.OfferId, envelope.Data, request.AuthorizedSalesContext, evidence.PayloadHash, _clock.GetDateTime());
                return CandidateResolution.Resolved(candidate, AirOfferProfile.Profile, evidence, candidate.CapturedAt);
            }
            catch (AirOfferContractMismatchException exception)
            {
                return Failed(OfferResolutionOutcome.ContractMismatch, evidence, exception.Message);
            }
            catch (AirOfferUnsupportedException exception)
            {
                return Failed(OfferResolutionOutcome.UnsupportedCapability, evidence, exception.Message);
            }
            catch (BusinessException exception)
            {
                return Failed(OfferResolutionOutcome.ContractMismatch, evidence, exception.Message);
            }
        }

        private CandidateResolution Failed(OfferResolutionOutcome outcome, SourceEvidence? evidence, string reason)
            => CandidateResolution.Failed(outcome, AirOfferProfile.Profile, evidence, _clock.GetDateTime(), reason);

        private Task DelayAsync(int attempt, int attempts, CancellationToken cancellationToken)
            => attempt < attempts
                ? Task.Delay(TimeSpan.FromMilliseconds(Math.Max(0, _options.RetryInterval)), cancellationToken)
                : Task.CompletedTask;
    }
}
