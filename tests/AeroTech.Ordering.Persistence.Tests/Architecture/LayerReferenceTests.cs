using System.Reflection;
using AeroTech.Ordering.Domain._Shared.Contracts;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.Architecture
{
    public sealed class LayerReferenceTests
    {
        private static readonly string[] ForbiddenForDomain =
        {
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore",
            "Microsoft.Extensions.Configuration",
            "Microsoft.Data.SqlClient",
            "System.Net.Http",
            "MassTransit",
            "AeroTech.Framework.Infrastructure",
            "AeroTech.Framework.Presentation",
            "AeroTech.Ordering.Application",
            "AeroTech.Ordering.Persistence",
            "AeroTech.Ordering.Query",
            "AeroTech.Ordering.Providers",
            "AeroTech.Ordering.ReferenceData"
        };

        private static readonly string[] ForbiddenForApplication =
        {
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore",
            "Microsoft.Data.SqlClient",
            "MassTransit",
            "AeroTech.Framework.Infrastructure",
            "AeroTech.Framework.Presentation",
            "AeroTech.Ordering.Persistence",
            "AeroTech.Ordering.Query",
            "AeroTech.Ordering.Providers",
            "AeroTech.Ordering.RestApi"
        };

        [Fact]
        public void Domain_references_no_infrastructure_assembly()
            => AssertNoForbiddenReference(typeof(ICallerContext).Assembly, ForbiddenForDomain);

        [Fact]
        public void Application_references_no_infrastructure_assembly()
            => AssertNoForbiddenReference(typeof(AeroTech.Ordering.Application.DependencyInjection).Assembly, ForbiddenForApplication);

        private static void AssertNoForbiddenReference(Assembly assembly, string[] forbidden)
        {
            var violations = assembly.GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .Where(name => forbidden.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
                .ToList();

            Assert.True(
                violations.Count == 0,
                $"{assembly.GetName().Name} references forbidden assemblies: {string.Join(", ", violations)}");
        }
    }
}
