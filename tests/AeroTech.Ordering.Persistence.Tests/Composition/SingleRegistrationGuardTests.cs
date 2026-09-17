using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Ordering.Domain._Shared.Contracts;
using AeroTech.Ordering.ServiceHost.Composition;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.Composition
{
    public sealed class SingleRegistrationGuardTests
    {
        [Fact]
        public void Two_unit_of_work_registrations_fail_with_the_named_duplicate()
        {
            var services = new ServiceCollection();
            services.AddScoped<IUnitOfWork, FirstUnitOfWork>();
            services.AddScoped<IUnitOfWork, SecondUnitOfWork>();

            var exception = Assert.Throws<InvalidOperationException>(() => services.EnsureSingleRegistrations());

            Assert.Contains(typeof(IUnitOfWork).FullName!, exception.Message);
            Assert.Contains(nameof(FirstUnitOfWork), exception.Message);
            Assert.Contains(nameof(SecondUnitOfWork), exception.Message);
        }

        [Fact]
        public void Real_and_deterministic_binding_of_the_same_contract_fail_with_the_named_duplicate()
        {
            var services = new ServiceCollection();
            services.AddScoped<IHomeOperatorProvider, RealHomeOperatorProvider>();
            services.AddScoped<IHomeOperatorProvider, DeterministicHomeOperatorProvider>();

            var exception = Assert.Throws<InvalidOperationException>(() => services.EnsureSingleRegistrations());

            Assert.Contains(typeof(IHomeOperatorProvider).FullName!, exception.Message);
            Assert.Contains(nameof(RealHomeOperatorProvider), exception.Message);
            Assert.Contains(nameof(DeterministicHomeOperatorProvider), exception.Message);
        }

        [Fact]
        public void Single_bindings_pass()
        {
            var services = new ServiceCollection();
            services.AddScoped<IUnitOfWork, FirstUnitOfWork>();
            services.AddScoped<IHomeOperatorProvider, RealHomeOperatorProvider>();

            Assert.Same(services, services.EnsureSingleRegistrations());
        }

        private sealed class FirstUnitOfWork : IUnitOfWork
        {
            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
        }

        private sealed class SecondUnitOfWork : IUnitOfWork
        {
            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
        }

        private sealed class RealHomeOperatorProvider : IHomeOperatorProvider
        {
            public Task<long> GetOwnerAirlineIdAsync(CancellationToken cancellationToken = default) => Task.FromResult(1L);
        }

        private sealed class DeterministicHomeOperatorProvider : IHomeOperatorProvider
        {
            public Task<long> GetOwnerAirlineIdAsync(CancellationToken cancellationToken = default) => Task.FromResult(1L);
        }
    }
}
