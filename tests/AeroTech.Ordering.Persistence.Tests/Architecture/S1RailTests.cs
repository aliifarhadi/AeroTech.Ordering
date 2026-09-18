using System.Reflection;
using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;
using AeroTech.Ordering.Application.OrderAggregate.Commands.RebuildOrderProjection;
using AeroTech.Ordering.Application.OrderPreparationAggregate.Commands.PrepareOrderFromOffer;
using AeroTech.Ordering.Domain.OrderAggregate.Contracts;
using AeroTech.Ordering.Domain.Ports.Offers;
using AeroTech.Ordering.Query.OperationAggregate.Queries.GetOperation;
using AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrder;
using MediatR;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.Architecture
{
    public sealed class S1RailTests
    {
        private static readonly Assembly[] OrderingAssemblies =
        {
            typeof(IOfferSourcePort).Assembly,
            typeof(PrepareOrderFromOfferCommand).Assembly,
            typeof(OrderingDbContext).Assembly,
            typeof(GetOrderQuery).Assembly,
            typeof(Synchronizer.DependencyInjection).Assembly,
            typeof(Providers.DependencyInjection).Assembly,
            typeof(Providers.Deterministic.DependencyInjection).Assembly,
            typeof(Consumers.DependencyInjection).Assembly,
            typeof(RestApi.RestApiAssembly).Assembly,
            typeof(ServiceHost.Composition.OrderingHostComposition).Assembly
        };

        [Theory]
        [InlineData(typeof(PrepareOrderFromOfferCommand))]
        [InlineData(typeof(CreateOrderFromOfferCommand))]
        [InlineData(typeof(RebuildOrderProjectionCommand))]
        [InlineData(typeof(GetOrderQuery))]
        [InlineData(typeof(GetOperationQuery))]
        public void Each_s1_command_and_query_has_exactly_one_canonical_handler(Type request)
        {
            var handlers = OrderingAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type is { IsClass: true, IsAbstract: false })
                .Where(type => type.GetInterfaces().Any(contract => contract.IsGenericType
                                                                    && contract.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
                                                                    && contract.GetGenericArguments()[0] == request))
                .ToList();

            var handler = Assert.Single(handlers);
            Assert.Equal($"{request.Name}Handler", handler.Name);
        }

        [Fact]
        public void Every_query_type_lives_in_the_query_project()
        {
            var misplaced = OrderingAssemblies
                .Where(assembly => assembly != typeof(GetOrderQuery).Assembly)
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.Name.EndsWith("Query", StringComparison.Ordinal)
                               && type.GetInterfaces().Any(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IRequest<>)))
                .Select(type => type.FullName)
                .ToList();

            Assert.True(misplaced.Count == 0, $"Queries outside the Query project: {string.Join(", ", misplaced)}");
        }

        [Fact]
        public void Offer_providers_do_not_reference_ordering_persistence()
        {
            foreach (var assembly in new[] { typeof(Providers.DependencyInjection).Assembly, typeof(Providers.Deterministic.DependencyInjection).Assembly })
            {
                var references = assembly.GetReferencedAssemblies().Select(reference => reference.Name).ToList();

                Assert.DoesNotContain("AeroTech.Ordering.Persistence", references);
                Assert.DoesNotContain("AeroTech.Ordering.Application", references);
            }

            Assert.DoesNotContain("Microsoft.EntityFrameworkCore", typeof(Providers.DependencyInjection).Assembly.GetReferencedAssemblies().Select(reference => reference.Name));
        }

        [Fact]
        public void Exactly_one_order_projector_implements_the_query_synchronizer()
        {
            var projectors = OrderingAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IOrderQuerySynchronizer).IsAssignableFrom(type))
                .ToList();

            Assert.Equal(typeof(Synchronizer.OrderAggregate.OrderProjector), Assert.Single(projectors));
        }
    }
}
