using Sitecore.Commerce.Core;

namespace Plugin.Commerce.AttributePromotions.Policies
{
    public class QualifiedItemPercentageDiscountPolicy : Policy
    {
        public decimal PercentageOff { get; set; }

        public int QuantityFrom { get; set; }

        public int QuantityTo { get; set; }
    }
}