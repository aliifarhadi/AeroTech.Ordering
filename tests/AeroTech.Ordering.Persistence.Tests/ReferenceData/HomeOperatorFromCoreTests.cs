using System.Net;
using System.Text;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Ordering.Persistence.Tests._Shared;
using AeroTech.Ordering.ReferenceData.Core;
using AeroTech.Ordering.ReferenceData.Persistence;
using AeroTech.Ordering.ReferenceData.Syncing;
using AeroTech.Ordering.Persistence._Shared.OperatorContext;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.ReferenceData
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class HomeOperatorFromCoreTests
    {
        private const string CoreBaseAddress = "https://core.test/service/";
        private const string OperatorSettingsPath = "/service/v1/OperatorSettings";

        private const string RecordedCoreResponse = """
            {
              "data": [
                {
                  "id": 1,
                  "scopeKey": "HOME_OPERATOR",
                  "homeAirlineId": 1,
                  "defaultCurrencyId": 70,
                  "defaultLanguageCode": "en",
                  "defaultTimeZoneId": "Asia/Tehran",
                  "lastUpdateTime": "2026-09-14T11:08:06.4209807+00:00"
                }
              ],
              "errors": null
            }
            """;

        private readonly OrderingDatabaseFixture _fixture;

        public HomeOperatorFromCoreTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Owner_airline_comes_from_core_operator_settings_after_sync()
        {
            await using (var clear = _fixture.NewReferenceContext())
                await clear.OperatorSettings.ExecuteDeleteAsync();

            await using (var before = _fixture.NewReferenceContext())
                await Assert.ThrowsAsync<BusinessException>(() => new ReferenceDataHomeOperatorProvider(before).GetOwnerAirlineIdAsync());

            var handler = new RecordingHandler(RecordedCoreResponse);

            await using (var sync = _fixture.NewReferenceContext())
            {
                var client = new CoreClient(new HttpClient(handler) { BaseAddress = new Uri(CoreBaseAddress) });
                await new OperatorSettingsSyncer(sync, client, TimeProvider.System).SyncAsync();
            }

            Assert.Equal(OperatorSettingsPath, handler.RequestedPath);

            await using var read = _fixture.NewReferenceContext();
            var ownerAirlineId = await new ReferenceDataHomeOperatorProvider(read).GetOwnerAirlineIdAsync();

            Assert.Equal(1L, ownerAirlineId);
        }

        private sealed class RecordingHandler : HttpMessageHandler
        {
            private readonly string _body;

            public RecordingHandler(string body) => _body = body;

            public string? RequestedPath { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestedPath = request.RequestUri!.AbsolutePath;

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_body, Encoding.UTF8, "application/json")
                });
            }
        }
    }
}
