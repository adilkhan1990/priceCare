using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/values")]
    public class ValuesController : ApiController
    {
        readonly Random random = new Random((int)DateTime.Now.Ticks);
        
        [Route("")]
        public object Get()
        {
            var months = 12*5;

            var rc = StaticData.GetRegionAndCountries();

            foreach (var regionAndCountries in rc)
            {
                foreach (var country in regionAndCountries.Countries)
                {
                    
                }
            }
            return 0;
        }

        [Route("regioncountries")]
        public object GetRegionAndCountries()
        {
            return StaticData.GetRegionAndCountries();
        }

        private double GetRandomDouble()
        {
            return random.NextDouble()* 100;
        }

        private IEnumerable<object> GetMonthsOfData(int months)
        {
            for (int i = 0; i < months; i++)
            {
                yield return new {Value = GetRandomDouble()};
            }
        }



    }

    public static class StaticData
    {
        #region Static properties

        public static List<Region> Regions = new List<Region>
        {
            new Region
            {
                Id = 0,
                Name = "Europe"
            },
            new Region
            {
                Id = 1,
                Name = "Americas"
            },
            new Region
            {
                Id = 2,
                Name = "Asia"
            }
        };

        public static List<Country> Countries = new List<Country>
        {
            new Country
            {
                Id=0,
                RegionId = 0,
                Name = "Belgium"
            },
            new Country
            {
                Id=1,
                RegionId = 0,
                Name = "France"
            },
            new Country
            {
                Id=2,
                RegionId = 0,
                Name = "Switzerland"
            },
            new Country
            {
                Id=3,
                RegionId = 0,
                Name = "Italy"
            },
            new Country
            {
                Id=4,
                RegionId = 0,
                Name = "Spain"
            },


            new Country
            {
                Id=5,
                RegionId = 1,
                Name = "Brazil"
            },
            new Country
            {
                Id=6,
                RegionId = 1,
                Name = "Chile"
            },
            new Country
            {
                Id=7,
                RegionId = 1,
                Name = "Cuba"
            },
            new Country
            {
                Id=8,
                RegionId = 1,
                Name = "Honduras"
            },
            new Country
            {
                Id=9,
                RegionId = 1,
                Name = "Mexico"
            },
            new Country
            {
                Id=10,
                RegionId = 1,
                Name = "Panama"
            },
            new Country
            {
                Id=11,
                RegionId = 1,
                Name = "USA"
            },



            new Country
            {
                Id=12,
                RegionId = 2,
                Name = "China"
            },
            new Country
            {
                Id=13,
                RegionId = 2,
                Name = "India"
            },
            new Country
            {
                Id=14,
                RegionId = 2,
                Name = "Japan"
            },
            new Country
            {
                Id=15,
                RegionId = 2,
                Name = "South Korea"
            },
            new Country
            {
                Id=16,
                RegionId = 2,
                Name = "Malaysia"
            },
            new Country
            {
                Id=17,
                RegionId = 2,
                Name = "Pakistan"
            },
        };

        #endregion

        public static List<RegionAndCountries> GetRegionAndCountries()
        {
            return Regions.Select(r => new RegionAndCountries
            {
                Region = r,
                Countries = Countries.Where(c => c.RegionId == r.Id).ToList()
            }).ToList();
        }
        public static List<RegionAndCountriesSmall> GetRegionAndCountriesSmall()
        {
            return Regions.Select(r => new RegionAndCountriesSmall
            {
                RegionId = r.Id,
                CountryIds = Countries.Where(c => c.RegionId == r.Id).Select(c=>c.Id).ToList()
            }).ToList();
        }

    }

    public class Region
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Country
    {
        public int Id { get; set; }
        public int RegionId { get; set; }
        public string Name { get; set; }
    }

    public class RegionAndCountries
    {
        public Region Region { get; set; }
        public List<Country> Countries { get; set; }
    }

    public class RegionAndCountriesSmall
    {
        public int RegionId { get; set; }
        public List<int> CountryIds { get; set; } 
    }

    public class RegionData
    {
        public int Id { get; set; }
        public List<CountryData> CountryData { get; set; }

    }

    public class CountryData
    {
        public int Id { get; set; }
        public List<CellData> CellData { get; set; }
    }

    public class CellData
    {
        public double? Value { get; set; }
        public bool? IsPercent { get; set; }
    }
}
