using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public class CountryRepository : ICountryRepository
    {
        private readonly LoadRepository loadRepository = new LoadRepository();
        private readonly IDimensionDictionaryRepository dimensionDictionaryRepository = new DimensionDictionaryRepository();
        public IEnumerable<RegionAndCountriesViewModel> GetRegionsAndCountries()
        {
            var regions = GetAllRegions();
            var countries = GetAllCountries();

            return regions.Select(r => new RegionAndCountriesViewModel
            {
                Region = r,
                Countries = countries.Where(c => c.RegionId == r.Id).ToList()
            });
        }

        public RegionAndCountriesSearchResponseViewModel GetPagedRegionsAndCountries(RegionAndCountriesSearchRequestViewModel regionAndCountriesSearchRequest)
        {
            var regions = GetAllRegions();
            var countries = GetCountriesByRegion(regionAndCountriesSearchRequest.RegionId).ToList();

            var totalCountries = countries.Count();

            var region = new RegionAndCountriesViewModel
            {
                Region = regions.FirstOrDefault(r => r.Id == regionAndCountriesSearchRequest.RegionId),
                Countries = countries.Where(c => c.RegionId == regionAndCountriesSearchRequest.RegionId)
                    .Skip(regionAndCountriesSearchRequest.PageNumber * regionAndCountriesSearchRequest.ItemsPerPage)
                    .Take(regionAndCountriesSearchRequest.ItemsPerPage)
                    .ToList()
            };
            
            var viewModel = new RegionAndCountriesSearchResponseViewModel
            {
                RegionAndCountries = region,
                IsLastPage = (region.Countries.Count() + (regionAndCountriesSearchRequest.PageNumber * regionAndCountriesSearchRequest.ItemsPerPage)) >= totalCountries,
                PageNumber = ++regionAndCountriesSearchRequest.PageNumber,
                TotalCountries = totalCountries
            };

            return viewModel;
        }

        #region Region

        public IEnumerable<RegionViewModel> GetAllRegions()
        {
            const string queryString = "SELECT Id, Name FROM Geography WHERE GeographyTypeId = 1 ORDER BY Name";
            var regions =
                RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, queryString, MapDataToRegion).ToList();
            return regions;
        }

        public bool AddRegion(string regionName)
        {
            if (!RegionExists(regionName))
            {
                const string queryString = "INSERT INTO Geography VALUES (@name, @shortName, @iso2, @geographyTypeId, @displayCurrencyId, @active)";

                var dictionary = new Dictionary<string, object>
                {
                    {"name", regionName},
                    {"shortName", ""},
                    {"iso2", ""},
                    {"geographyTypeId", GeographyType.Region},
                    {"displayCurrencyId", 1},
                    {"active", 1}
                };

                var result = RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, queryString, dictionary);
                return result == 1;
                }
            return false;
        }

        private static bool RegionExists(string name)
        {
            const string query = "SELECT Id FROM Geography WHERE Name=@name AND GeographyTypeId = 1";

            var dictionary = new Dictionary<string, object>
            {
                {"name", name},                
            };

            var result = RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query,
                row => new RegionViewModel
                {
                    Id = (int) row["Id"]
                }, dictionary).ToList();

            return result.Count > 0;
        }

        private static RegionViewModel MapDataToRegion(DataRow row)
        {
            var region = new RegionViewModel
            {
                Id = (int)row["Id"],
                Name = (string)row["Name"]
            };

            return region;
        }
        #endregion 

        #region Country
        public IEnumerable<CountryViewModel> GetAllCountries()
        {
            const string queryString = "Exec GetCountries @regionid = 0";
            var result = RequestHelper.ExecuteQuery<CountryViewModel>(DataBaseConstants.ConnectionString, queryString, MapDataToCountry);
            return result.OrderBy(c => c.Name);
        }

        public IEnumerable<CountryViewModel> GetRuleCountries(List<int> referencedCountriesId)
        {
            const string queryString = "Exec GetCountries @regionid = 0";
            var result = RequestHelper.ExecuteQuery<CountryViewModel>(DataBaseConstants.ConnectionString, queryString, MapDataToCountry);
            return result.Where(c => !referencedCountriesId.Contains(c.Id));
        } 

        public IEnumerable<CountryViewModel> GetCountriesByRegion(int regionId)
        {
            const string queryString = "Exec GetCountries @regionid = @regionIdentifier";
            var parametersDictionary = new Dictionary<string, object> {{"regionIdentifier", regionId}};
            var result = RequestHelper.ExecuteQuery<CountryViewModel>(DataBaseConstants.ConnectionString, queryString, MapDataToCountry, parametersDictionary);
            return result;
        }

        public CountrySearchResponseViewModel GetCountriesByRegionPaged(CountrySearchRequestViewModel countrySearch)
        {
            const string queryString = "Exec GetCountries @regionid = @regionIdentifier";
            var parametersDictionary = new Dictionary<string, object> { { "regionIdentifier", countrySearch.RegionId } };
            var countries = RequestHelper.ExecuteQuery<CountryViewModel>(DataBaseConstants.ConnectionString, queryString, MapDataToCountry, parametersDictionary);
            
            switch (countrySearch.Status)
            {
                case (int)SearchCountryStatus.All:
                    countries = countries.Where(c => c.RegionId == countrySearch.RegionId);
                    break;

                case (int)SearchCountryStatus.Active:
                    countries = countries.Where(c => c.RegionId == countrySearch.RegionId && c.Active);
                    break;

                case (int)SearchCountryStatus.Inactive:
                    countries = countries.Where(c => c.RegionId == countrySearch.RegionId && !c.Active);
                    break;
            }

            countries = countries.Skip(countrySearch.PageNumber * countrySearch.ItemsPerPage)
                                 .Take(countrySearch.ItemsPerPage)
                                 .ToList();

            var totalCountries = countries.Count();

            var viewModel = new CountrySearchResponseViewModel
            {
                Countries = countries,
                IsLastPage = (countries.Count() + (countrySearch.PageNumber * countrySearch.ItemsPerPage)) >= totalCountries,
                PageNumber = ++countrySearch.PageNumber,
                TotalCountries = totalCountries
            };

            return viewModel;
        }

        public void Save(IEnumerable<CountryViewModel> countries)
        {
            var listIds = countries.Select(p => p.Id);
            string query = "SELECT * FROM Geography WHERE Id in ( ";
            foreach (var id in listIds)
            {
                query += id + " , ";
            }
            query = query.Remove(query.Length - 3);
            query += " )";
            var originalCountries = RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query, MapCountry);
            foreach (var country in countries)
            {
                var original = originalCountries.FirstOrDefault(op => op.Id == country.Id);

                if (original == null)
                    continue;

                const string queryL2S = "Exec UpdateCountry @GeographyId = @geographyId, " +
                                        "@UpdateExportName = @updateExportName, @ExportName = @exportName, " +
                                        "@UpdateActiveStatus = @updateActiveStatus, @ActiveStatus = @activeStatus, " +
                                        "@UpdateCurrency = @currency, @CurrencyId = @currencyId," +
                                        "@UpdateIso2 = @updateIso2, @Iso2 = @iso2 ";
                RequestHelper.ExecuteScalarRequest<ProductViewModel>(
                    DataBaseConstants.ConnectionString,
                    queryL2S,
                    new Dictionary<string, object>
                    {
                        {"geographyId", country.Id},
                        {"updateExportName", original.ExportName != country.ExportName},
                        {"exportName", country.ExportName},
                        {"updateActiveStatus", original.Active != country.Active},
                        {"activeStatus", country.Active},
                        {"currency", country.DisplayCurrency != original.DisplayCurrency},
                        {"currencyId", country.CurrencyId},
                        {"UpdateIso2", country.Iso2 != original.Iso2},
                        {"Iso2", country.Iso2},
                    }
                );
            }
        }

        public void SaveLoad(SaveCountryModel saveCountryModel)
        {
            var countriesToUpdate = new List<CountryViewModel>();

            foreach (var countryToSave in saveCountryModel.Countries)
            {
                if (countryToSave.Id != 0)
                {
                    countriesToUpdate.Add(countryToSave);
                }
                else
                {
                    var id = dimensionDictionaryRepository.GetExistingDimensionId(
                        new[]
                        {
                            countryToSave.Name, 
                            countryToSave.ExportName
                        },
                        DimensionConstant.Geography);
                    if (id == null)
                    {
                        InsertCountry(countryToSave);
                    }
                    else
                    {
                        countryToSave.Id = (int)id;
                        countriesToUpdate.Add(countryToSave);
                    }
                }
            }

            if (countriesToUpdate.Any())
                 Save(countriesToUpdate);

            loadRepository.ValidateLoadItem(saveCountryModel.LoadId, LoadConstants.Country);
            loadRepository.CreateLoadItemDetailScenarioForSku(saveCountryModel.LoadId);
        }

        private void InsertCountry(CountryViewModel country)
        {
            var queryCountry = "INSERT INTO Geography (Name,ShortName,Iso2,GeographyTypeId,DisplayCurrencyId,Active) " +
                               "OUTPUT INSERTED.ID " +
                               "VALUES (@name, @shortname, @iso2, @geographyTypeId, @currencyId, @active)";

            var countryId = RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, queryCountry,
                new Dictionary<string, object>
                {
                    {"name", country.Name},
                    {"shortName", country.Iso3},
                    {"iso2", country.Iso2},
                    {"geographyTypeId", 3},
                    {"currencyId", country.CurrencyId},
                    {"active", country.Active},
                });

            var regionUndefined = GetAllRegions().FirstOrDefault(r => r.Name.ToLower() == "undefined");

            if (regionUndefined != null)
            {
                var queryHierarchy = "INSERT INTO GeographyLink (GeographyUpId, GeographyId) " +
                                 "VALUES (@geographyUpId, @geographyId)";
                RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, queryHierarchy,
                    new Dictionary<string, object>
                {
                    {"geographyUpId",regionUndefined.Id},
                    {"geographyId", countryId}
                });
            }

            var dimension = new DimensionDictionaryModel
            {
                Dimension = DimensionConstant.Geography,
                DimensionId = countryId,
                SystemId = (int) DimensionType.Excel,
                Name = country.ExportName
            };

            dimensionDictionaryRepository.Create(dimension);

            dimension.SystemId = (int)DimensionType.Gcods;
            dimension.Name = country.Name;

            dimensionDictionaryRepository.Create(dimension);
        }

        public bool DeleteRegion(int regionId)
        {            
            const string queryString = "DELETE FROM Geography WHERE Id=@geographyId AND GeographyTypeId=@geographyTypeId";
            
            MoveCountriesToUndefined(regionId);
            DeleteGeographyLinks(regionId); //delete links between region and countries

            var dictionary = new Dictionary<string, object>
            {
                {"active", false},
                {"geographyId", regionId},
                {"geographyTypeId", GeographyType.Region}
            };

            var count = RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, queryString, dictionary);

            return count == 1;
        }

        private void DeleteGeographyLinks(int regionId)
        {
            const string queryString = "DELETE FROM GeographyLink WHERE GeographyUpId=@regionId";

            var dictionary = new Dictionary<string, object>
            {
                {"regionId", regionId}
            };

            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, queryString, dictionary);
        }
        
        private void MoveCountriesToUndefined(int regionId)
        {
            string queryString = "SELECT * FROM GeographyLink WHERE GeographyUpId=@deletedRegionId"; //get links for region to delete

            var dictionary = new Dictionary<string, object>
            {
                {"deletedRegionId", regionId}
            };

            var links = RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, queryString,
                row => new GeographyLink
                {
                    RegionId = (int) row["GeographyUpId"],
                    CountryId = (int) row["GeographyId"]
                }, dictionary);

            foreach (var link in links)
            {
                queryString = "SELECT COUNT(*) FROM GeographyLink WHERE GeographyId=@geographyId";

                dictionary = new Dictionary<string, object>
                {
                    {"geographyId", link.CountryId}
                };

                var count = RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, queryString, dictionary);

                if (count == 1) // move country to 'Undefined' area if they are about to be deleted
                {
                    queryString = "UPDATE GeographyLink SET GeographyUpId=@undefinedRegionId WHERE GeographyUpId=@deletedRegionId";

                    dictionary = new Dictionary<string, object>
                    {
                        {"undefinedRegionId", ApplicationConstants.UndefinedRegionId},
                        {"deletedRegionId", regionId}
                    };

                    RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, queryString, dictionary);   
                }
            }                     
        }

        private static CountryViewModel MapDataToCountry(DataRow row)
        {
            var country = new CountryViewModel
            {
                Id = Convert.ToInt32(row["Id"].ToString()),
                Name = row["Name"].ToString(),
                Iso2 = row["Iso2"].ToString(),
                Iso3 = row["ShortName"].ToString(),
                DisplayCurrencyId = (int)row["DisplayCurrencyId"],
                DisplayCurrency = row["Iso"].ToString(),   
                RegionId = (int)row["RegionId"],
                Active = (bool) row["Active"],
                CurrencyId = (int)row["DisplayCurrencyId"]
            };

            if (row.Table.Columns.Contains("ExportName"))
                country.ExportName = row["ExportName"].ToString();
           
            return country;
        }

        private static CountryViewModel MapCountry(DataRow row)
        {
            var country = new CountryViewModel
            {
                Id = (int)row["Id"],
                Name = (string)row["Name"],
                Iso2 = (string)row["Iso2"],
                DisplayCurrencyId = (int)row["DisplayCurrencyId"],
                Active = (bool)row["Active"],
            };

            return country;
        }

        #endregion


        #region Export Country


        public IEnumerable<CountryViewModel> GetCountryExport()
        {
            var query = "SELECT * FROM GetGeographyExportName() ORDER BY Name";

            return RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query, MapCountryExport);
        }

        //public bool UpdateRegionCountries(UpdateCountryRegionsRequest model)
        public bool UpdateRegionCountries(List<UpdateCountryRegionsViewModel> model)
        {
            //foreach (var action in model.Updates)
            foreach(var action in model)
            {
                if (action.IsAssign)
                {
                    AddCountryToRegion(action.RegionId, action.CountryId);
                }
                else
                {
                    RemoveCountryFromRegion(action.RegionId, action.CountryId);
                }                
            }
            return true;
        }

        private bool AddCountryToRegion(int regionId, int countryId)
        {
            const string query = "INSERT INTO GeographyLink VALUES (@regionId, @countryId)";

            var dictionary = new Dictionary<string, object>
            {
                {"regionId", regionId},
                {"countryId", countryId}
            };

            var count = RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);
            return count > 0;
        }

        private bool RemoveCountryFromRegion(int regionId, int countryId)
        {
            const string query = "DELETE FROM GeographyLink WHERE GeographyUpId=@regionId AND GeographyId=@countryId";

            var dictionary = new Dictionary<string, object>
            {
                {"regionId", regionId},
                {"countryId", countryId}
            };

            var count = RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);
            return count > 0;
        }

        private CountryViewModel MapCountryExport(DataRow row)
        {
            var country = new CountryViewModel
            {
                Id = (int)row["GeographyId"],
                Name = row["Name"].ToString()
            };

            return country;
        }

        #endregion
    }
}