using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using AeroTech.Ordering.Domain._Shared.Contracts;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.Architecture
{
    public sealed class DomainMessagesAllowlistTests
    {
        private const string MessagesAssembly = "AeroTech.Messages";
        private const string OrderingEnumsNamespace = "AeroTech.Messages.Ordering.Enums";

        private static readonly HashSet<string> AllowedCallerContextTypes = new(StringComparer.Ordinal)
        {
            "AeroTech.Messages.Aegis.Enums.BusinessContextType",
            "AeroTech.Messages.Aegis.Enums.PrincipalType",
            "AeroTech.Messages.Shared.Enums.AuthorizationSurface",
            "AeroTech.Messages.Shared.Enums.SalesChannel"
        };

        [Fact]
        public void Domain_uses_only_allowlisted_Messages_types()
        {
            var violations = MessagesTypeReferences(typeof(ICallerContext).Assembly.Location)
                .Where(type => !IsAllowed(type))
                .ToList();

            Assert.True(violations.Count == 0, $"Domain references non-allowlisted Messages types: {string.Join(", ", violations)}");
        }

        [Fact]
        public void Allowlist_check_detects_a_forbidden_Messages_type()
        {
            Assert.False(IsAllowed("AeroTech.Messages.BaseIntegrationEvent"));
            Assert.False(IsAllowed("AeroTech.Messages.FlightFlow.IntegrationEvents.FlightReservationExpiredEvent"));
            Assert.False(IsAllowed("AeroTech.Messages.Aegis.Enums.SomethingElse"));
            Assert.True(IsAllowed($"{OrderingEnumsNamespace}.AnyOrderingEnum"));
        }

        [Fact]
        public void Metadata_reader_sees_the_caller_context_Messages_references()
        {
            var references = MessagesTypeReferences(typeof(ICallerContext).Assembly.Location);

            Assert.Contains("AeroTech.Messages.Aegis.Enums.PrincipalType", references);
        }

        [Fact]
        public void Caller_context_exposes_no_owner_airline_identity()
        {
            var members = typeof(ICallerContext).GetMembers().Select(member => member.Name).ToList();

            Assert.DoesNotContain(members, name => name.Contains("OwnerAirline", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(members, name => name.Contains("HomeOperator", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsAllowed(string fullName)
            => fullName.StartsWith($"{OrderingEnumsNamespace}.", StringComparison.Ordinal)
               || AllowedCallerContextTypes.Contains(fullName);

        private static List<string> MessagesTypeReferences(string assemblyPath)
        {
            using var stream = File.OpenRead(assemblyPath);
            using var peReader = new PEReader(stream);
            var reader = peReader.GetMetadataReader();
            var result = new List<string>();

            foreach (var handle in reader.TypeReferences)
            {
                var reference = reader.GetTypeReference(handle);
                if (reference.ResolutionScope.Kind != HandleKind.AssemblyReference)
                    continue;

                var assembly = reader.GetAssemblyReference((AssemblyReferenceHandle)reference.ResolutionScope);
                if (reader.GetString(assembly.Name) != MessagesAssembly)
                    continue;

                result.Add($"{reader.GetString(reference.Namespace)}.{reader.GetString(reference.Name)}");
            }

            return result;
        }
    }
}
