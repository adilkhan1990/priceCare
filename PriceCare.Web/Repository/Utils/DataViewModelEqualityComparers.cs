using System.Collections.Generic;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository.Utils
{
    internal class DataIdentityEqualityComparer : IEqualityComparer<DataViewModel>
    {
        public bool Equals(DataViewModel x, DataViewModel y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.GeographyId == y.GeographyId && x.ProductId == y.ProductId && x.PriceTypeId == y.PriceTypeId && x.DataTypeId == y.DataTypeId && x.SegmentId == y.SegmentId;
        }

        public int GetHashCode(DataViewModel obj)
        {
            unchecked
            {
                var hashCode = obj.GeographyId;
                hashCode = (hashCode * 397) ^ obj.ProductId;
                hashCode = (hashCode * 397) ^ obj.PriceTypeId;
                hashCode = (hashCode * 397) ^ obj.DataTypeId;
                hashCode = (hashCode * 397) ^ obj.SegmentId;
                return hashCode;
            }
        }
    }

    internal class PriceDataEqualityComparer : DataIdentityEqualityComparer, IEqualityComparer<DataViewModel>
    {
        public new bool Equals(DataViewModel x, DataViewModel y)
        {
            return base.Equals(x, y) && x.CurrencySpotId == y.CurrencySpotId && x.UnitTypeId == y.UnitTypeId && x.UsdBudget.Equals(y.UsdBudget) && x.EurBudget.Equals(y.EurBudget);
        }

        public new int GetHashCode(DataViewModel obj)
        {
            unchecked
            {
                var hashCode = base.GetHashCode();

                hashCode = (hashCode * 397) ^ obj.CurrencySpotId;
                hashCode = (hashCode * 397) ^ obj.UnitTypeId;
                hashCode = (hashCode * 397) ^ obj.UsdBudget.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.EurBudget.GetHashCode();

                return hashCode;
            }
        }
    }

    internal class VolumeDataEqualityComparer : DataIdentityEqualityComparer, IEqualityComparer<DataViewModel>
    {
        public new bool Equals(DataViewModel x, DataViewModel y)
        {
            return base.Equals(x, y) && x.CurrencySpotId == y.CurrencySpotId && x.UnitTypeId == y.UnitTypeId;
        }

        public new int GetHashCode(DataViewModel obj)
        {
            unchecked
            {
                var hashCode = base.GetHashCode();

                hashCode = (hashCode * 397) ^ obj.CurrencySpotId;
                hashCode = (hashCode * 397) ^ obj.UnitTypeId;

                return hashCode;
            }
        }
    }

    internal class EventDataEqualityComparer : DataIdentityEqualityComparer, IEqualityComparer<DataViewModel>
    {
        public new bool Equals(DataViewModel x, DataViewModel y)
        {
            return base.Equals(x, y) && x.CurrencySpotId == y.CurrencySpotId && x.UnitTypeId == y.UnitTypeId;
        }

        public new int GetHashCode(DataViewModel obj)
        {
            unchecked
            {
                var hashCode = base.GetHashCode();

                hashCode = (hashCode * 397) ^ obj.CurrencySpotId;
                hashCode = (hashCode * 397) ^ obj.UnitTypeId;

                return hashCode;
            }
        }
    }
}