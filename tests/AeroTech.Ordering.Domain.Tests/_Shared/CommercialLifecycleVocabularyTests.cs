using AeroTech.Messages.Ordering.Enums;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests._Shared
{
    public sealed class CommercialLifecycleVocabularyTests
    {
        [Fact]
        public void Service_commercial_status_is_exactly_the_pack_vocabulary()
        {
            Assert.Equal(
                ["Pending", "Active", "Cancelled", "Replaced", "Expired"],
                Enum.GetNames<OrderServiceCommercialStatus>());

            Assert.Equal(1, (int)OrderServiceCommercialStatus.Pending);
            Assert.Equal(2, (int)OrderServiceCommercialStatus.Active);
            Assert.Equal(3, (int)OrderServiceCommercialStatus.Cancelled);
            Assert.Equal(4, (int)OrderServiceCommercialStatus.Replaced);
            Assert.Equal(5, (int)OrderServiceCommercialStatus.Expired);
        }

        [Fact]
        public void Item_commercial_status_is_exactly_the_pack_vocabulary_and_keeps_its_numbers()
        {
            Assert.Equal(
                ["Active", "Replaced", "Cancelled", "PartiallyChanged", "Partitioned", "Expired", "Inactive"],
                Enum.GetNames<OrderItemCommercialStatus>());

            Assert.Equal(1, (int)OrderItemCommercialStatus.Active);
            Assert.Equal(2, (int)OrderItemCommercialStatus.Replaced);
            Assert.Equal(3, (int)OrderItemCommercialStatus.Cancelled);
            Assert.Equal(4, (int)OrderItemCommercialStatus.PartiallyChanged);
            Assert.Equal(5, (int)OrderItemCommercialStatus.Partitioned);
            Assert.Equal(6, (int)OrderItemCommercialStatus.Expired);
            Assert.Equal(7, (int)OrderItemCommercialStatus.Inactive);
        }

        [Fact]
        public void The_commercial_axis_carries_no_operation_outcome_and_no_delivery_facet()
        {
            Assert.DoesNotContain("Exchanged", Enum.GetNames<OrderServiceCommercialStatus>());
            Assert.DoesNotContain("Suspended", Enum.GetNames<OrderServiceCommercialStatus>());

            Assert.Empty(Enum.GetNames<OrderServiceCommercialStatus>().Intersect(Enum.GetNames<OrderServiceDeliveryStatus>()));
        }

        [Fact]
        public void A_selling_office_always_declares_the_namespace_it_belongs_to()
        {
            Assert.Equal(
                ["AirlineOffice", "TravelAgencyOffice"],
                Enum.GetNames<SellingOfficeKind>());

            Assert.DoesNotContain("NotRecorded", Enum.GetNames<SellingOfficeKind>());
            Assert.All(Enum.GetValues<SellingOfficeKind>(), kind => Assert.True((int)kind > 0));
        }
    }
}
