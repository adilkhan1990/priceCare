using System;
using System.Collections.Generic;
using PriceCare.Web.Models;

namespace PriceCare.Web.Math.Utils
{
    public class AnalyzerView
    {
        public List<DataViewModel> Focus { get; set; }
        public List<DataViewModel> ImpactedOrder1 { get; set; }
        public List<DataViewModel> ImpactedOrderN { get; set; }
        public List<DataViewModel> Referenced { get; set; } 
    }
    public class NetData
    {
        public List<DataViewModel> Price { get; set; }
        public List<DataViewModel> Sales { get; set; } 
    }
    public class DataRule
    {
        public List<DataViewModel> Data { get; set; }
        public List<RuleViewModel> Rule { get; set; } 
    }
    public class PriceVolume
    {
        public double Price { get; set; }
        public double Volume { get; set; }
    }

    public class GeographyDataTimePriceTypeIdentifier : IGeographyDataTimePriceTypeIdentifier
    {
        public GeographyDataTimePriceTypeIdentifier()
        {
        }

        public GeographyDataTimePriceTypeIdentifier(int geographyId, DateTime dataTime, int priceTypeId)
        {
            GeographyId = geographyId;
            DataTime = dataTime;
            PriceTypeId = priceTypeId;
        }

        public int GeographyId { get; set; }
        public DateTime DataTime { get; set; }
        public int PriceTypeId { get; set; }
    }

    public class GeographyDataTimeIdentifier : IGeographyDataTimeIdentifier
    {
        public GeographyDataTimeIdentifier()
        {
        }

        public GeographyDataTimeIdentifier(int geographyId, DateTime dataTime)
        {
            GeographyId = geographyId;
            DataTime = dataTime;
        }

        public int GeographyId { get; set; }
        public DateTime DataTime { get; set; }
    }

    public class GeographySegmentDataTimeIdentifier : IGeographySegmentDataTimeIdentifier
    {
        public GeographySegmentDataTimeIdentifier()
        {
        }

        public GeographySegmentDataTimeIdentifier(int geographyId, int segmentId, DateTime dataTime)
        {
            GeographyId = geographyId;
            SegmentId = segmentId;
            DataTime = dataTime;
        }

        public int GeographyId { get; set; }
        public int SegmentId { get; set; }
        public DateTime DataTime { get; set; }
    }
    public class GeographyProductIdentifier : IGeographyProductIdentifier
    {
        public GeographyProductIdentifier()
        {
        }

        public GeographyProductIdentifier(int geographyId, int productId)
        {
            GeographyId = geographyId;
            ProductId = productId;
        }

        public int GeographyId { get; set; }
        public int ProductId { get; set; }
    }
    public class GeographyProductSegmentIdentifier : IGeographyProductSegmentIdentifier
    {
        public GeographyProductSegmentIdentifier()
        {
        }

        public GeographyProductSegmentIdentifier(int geographyId, int productId, int segmentId)
        {
            GeographyId = geographyId;
            ProductId = productId;
            SegmentId = segmentId;
        }

        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int SegmentId { get; set; }
    }

    public class ProductIdentifier : IProductIdentifier
    {
        public ProductIdentifier()
        {
        }

        public ProductIdentifier(int productId)
        {
            ProductId = productId;
        }

        public int ProductId { get; set; }
    }

    public interface IGeographyDataTimePriceTypeIdentifier : IGeographyDataTimeIdentifier
    {
        int PriceTypeId { get; set; }
    }

    public interface IGeographyDataTimeIdentifier
    {
        int GeographyId { get; set; }
        DateTime DataTime { get; set; }
    }

    public interface IGeographySegmentDataTimeIdentifier : IGeographyDataTimeIdentifier
    {
        int SegmentId { get; set; }
    }
    public interface IGeographyProductIdentifier : IProductIdentifier
    {
        int GeographyId { get; set; }
    }
    public interface IGeographyProductSegmentIdentifier : IGeographyProductIdentifier
    {
        int SegmentId { get; set; }
    }

    public interface IProductIdentifier
    {
        int ProductId { get; set; }
    }

    internal class GeographyDataTimePriceTypeIdentityComparer : IEqualityComparer<IGeographyDataTimePriceTypeIdentifier>
    {
        public bool Equals(IGeographyDataTimePriceTypeIdentifier x, IGeographyDataTimePriceTypeIdentifier y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.GeographyId == y.GeographyId && x.DataTime == y.DataTime && x.PriceTypeId == y.PriceTypeId;
        }

        public int GetHashCode(IGeographyDataTimePriceTypeIdentifier obj)
        {
            unchecked
            {
                var hashCode = obj.GeographyId;
                hashCode = (hashCode * 397) ^ obj.DataTime.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.PriceTypeId;
                return hashCode;
            }
        }
    }

    internal class GeographyDataTimeIdentityComparer : IEqualityComparer<IGeographyDataTimeIdentifier>
    {
        public bool Equals(IGeographyDataTimeIdentifier x, IGeographyDataTimeIdentifier y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.GeographyId == y.GeographyId && x.DataTime == y.DataTime;
        }

        public int GetHashCode(IGeographyDataTimeIdentifier obj)
        {
            unchecked
            {
                var hashCode = obj.GeographyId;
                hashCode = (hashCode * 397) ^ obj.DataTime.GetHashCode();
                return hashCode;
            }
        }
    }

    internal class GeographySegmentDataTimeIdentityComparer : IEqualityComparer<IGeographySegmentDataTimeIdentifier>
    {
        public bool Equals(IGeographySegmentDataTimeIdentifier x, IGeographySegmentDataTimeIdentifier y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.GeographyId == y.GeographyId && x.SegmentId == y.SegmentId && x.DataTime == y.DataTime;
        }

        public int GetHashCode(IGeographySegmentDataTimeIdentifier obj)
        {
            unchecked
            {
                var hashCode = obj.GeographyId;
                hashCode = (hashCode*397) ^ obj.SegmentId;
                hashCode = (hashCode * 397) ^ obj.DataTime.GetHashCode();
                return hashCode;
            }
        }
    }
    internal class GeographyProductIdentityComparer : IEqualityComparer<IGeographyProductIdentifier>
    {
        public bool Equals(IGeographyProductIdentifier x, IGeographyProductIdentifier y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.GeographyId == y.GeographyId && x.ProductId == y.ProductId;
        }

        public int GetHashCode(IGeographyProductIdentifier obj)
        {
            unchecked
            {
                var hashCode = obj.GeographyId;
                hashCode = (hashCode * 397) ^ obj.ProductId;
                return hashCode;
            }
        }
    }
    internal class GeographyProductSegmentIdentityComparer : IEqualityComparer<IGeographyProductSegmentIdentifier>
    {
        public bool Equals(IGeographyProductSegmentIdentifier x, IGeographyProductSegmentIdentifier y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.GeographyId == y.GeographyId && x.ProductId == y.ProductId && x.SegmentId == y.SegmentId;
        }

        public int GetHashCode(IGeographyProductSegmentIdentifier obj)
        {
            unchecked
            {
                var hashCode = obj.GeographyId;
                hashCode = (hashCode * 397) ^ obj.ProductId;
                hashCode = (hashCode * 397) ^ obj.SegmentId;
                return hashCode;
            }
        }
    }
    internal class ProductIdentityComparer : IEqualityComparer<IProductIdentifier>
    {
        public bool Equals(IProductIdentifier x, IProductIdentifier y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.ProductId == y.ProductId;
        }

        public int GetHashCode(IProductIdentifier obj)
        {
            unchecked
            {
                var hashCode = obj.ProductId;
                return hashCode;
            }
        }
    }

    internal class DataPermIdentityEqualityComparer : IEqualityComparer<IDataPermIdentifier>
    {
        public bool Equals(IDataPermIdentifier x, IDataPermIdentifier y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.PermId == y.PermId;
        }

        public int GetHashCode(IDataPermIdentifier obj)
        {
            unchecked
            {
                var hashCode = obj.PermId.GetHashCode();
                return hashCode;
            }
        }
    }
    public interface IDataPermIdentifier
    {
        string PermId { get; }
    }
    public class DataPermIdentifier : IDataPermIdentifier
    {
        public DataPermIdentifier(string permId)
        {
            PermId = permId;
        }

        public DataPermIdentifier()
        {
        }
        public string PermId { get; set; }
    }
    
    internal class RulegptEqualityComparer : IEqualityComparer<IRulegptIdentifier>
    {
        public bool Equals(IRulegptIdentifier x, IRulegptIdentifier y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.GeographyId == y.GeographyId && x.ProductId == y.ProductId && x.GprmRuleTypeId == y.GprmRuleTypeId;
        }

        public int GetHashCode(IRulegptIdentifier obj)
        {
            unchecked
            {
                var hashCode = obj.GeographyId;
                hashCode = (hashCode * 397) ^ obj.ProductId;
                hashCode = (hashCode * 397) ^ obj.GprmRuleTypeId;
                return hashCode;
            }
        }
    }
    internal class RulegptaEqualityComparer : IEqualityComparer<IRulegptaIdentifier>
    {
        public bool Equals(IRulegptaIdentifier x, IRulegptaIdentifier y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.GeographyId == y.GeographyId && x.ProductId == y.ProductId && x.GprmRuleTypeId == y.GprmRuleTypeId && x.ApplicableFrom == y.ApplicableFrom;
        }

        public int GetHashCode(IRulegptaIdentifier obj)
        {
            unchecked
            {
                var hashCode = obj.GeographyId;
                hashCode = (hashCode * 397) ^ obj.ProductId;
                hashCode = (hashCode * 397) ^ obj.GprmRuleTypeId;
                hashCode = (hashCode * 397) ^ obj.ApplicableFrom.GetHashCode();
                return hashCode;
            }
        }
    }

    public interface IRulegptaIdentifier : IRulegptIdentifier
    {
        DateTime ApplicableFrom { get; set; }
    }
    public interface IRulegptIdentifier
    {
        int GeographyId { get; set; }
        int ProductId { get; set; }
        int GprmRuleTypeId { get; set; }
    }
    public class RulegptIdentifier : IRulegptIdentifier
    {
        public RulegptIdentifier(int geographyId, int productId, int gprmRuleTypeId)
        {
            GeographyId = geographyId;
            ProductId = productId;
            GprmRuleTypeId = gprmRuleTypeId;
        }

        public RulegptIdentifier()
        {
            
        }

        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int GprmRuleTypeId { get; set; }

    }
    public class RulegptaIdentifier : IRulegptaIdentifier
    {
         public RulegptaIdentifier(int geographyId, int productId, int gprmRuleTypeId, DateTime applicableFrom)
        {
            GeographyId = geographyId;
            ProductId = productId;
            GprmRuleTypeId = gprmRuleTypeId;
            ApplicableFrom = applicableFrom;
        }

        public RulegptaIdentifier()
        {
            
        }

        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int GprmRuleTypeId { get; set; }
        public DateTime ApplicableFrom { get; set; }
    }
    
}