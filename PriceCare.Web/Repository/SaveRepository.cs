using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public class SaveRepository
    {
        private static readonly CacheRepository CacheRepository = new CacheRepository();
        private static readonly DataRepository DataRepository = new DataRepository();
        private static readonly GprmRuleRepository GprmRuleRepository = new GprmRuleRepository();
        private static readonly IAccountRepository AccountRepository = new AccountRepository();
        public int SaveCache(SaveSimulationViewModel model)
        {
            var simulationInfo = CacheRepository.GetSimulation(model.SimulationId);
            var saveId = simulationInfo.SaveId;
            var saveInfo = model.Save;
            if (saveId == 0)
            {
                
                saveId = SetSave(simulationInfo, saveInfo);
            }
            else
            {
                if (saveInfo.UserId == simulationInfo.UserId)
                {
                    if (saveInfo.OverrideValue)
                        UpdateSave(saveInfo, saveId);
                    else
                        saveId = SetSave(simulationInfo, saveInfo);
                }    
                else
                    saveId = SetSave(simulationInfo, saveInfo);
            }
            SaveForecast(model.SimulationId, saveId);
            return saveId;
        }
        public List<TypeAndSimulationsViewModel> GetLaunchSimulations(string userId)
        {
            var model = new List<TypeAndSimulationsViewModel>
            {
                new TypeAndSimulationsViewModel
                {
                    Name = "Public",
                    Simulations = GetLaunchSimulationList(userId, true, true).ToList()
                },
                new TypeAndSimulationsViewModel
                {
                    Name = "User",
                    Simulations = GetLaunchSimulationList(userId, false, true).ToList()
                }
            };
            return model;
        }
        public List<TypeAndSimulationsViewModel> GetSimulations(string userId)
        {
            var model = new List<TypeAndSimulationsViewModel>
            {
                new TypeAndSimulationsViewModel
                {
                    Name = "Budget",                    
                    Simulations = GetBudgetSimulationList().ToList()
                },
                new TypeAndSimulationsViewModel
                {
                    Name = "Reference",
                    Simulations = GetReferenceSimulationList().ToList()
                },
                new TypeAndSimulationsViewModel
                {
                    Name = "Public",
                    Simulations = GetPublicSimulationList().ToList()
                },
                new TypeAndSimulationsViewModel
                {
                    Name = "User",
                    Simulations = GetUserSimulationList(userId).ToList()
                }
            };
            return model;
        }

        public List<SaveViewModel> GetSimulations(string userId, int simulationType, bool isActive)
        {
            var list = new List<SaveViewModel>();
            switch (simulationType)
            {
                case (int)SimulationTypes.Budget:
                    list = GetBudgetSimulationList(isActive).ToList();
                    break;

                case (int)SimulationTypes.Public:
                    list = GetPublicSimulationList(isActive).ToList();
                    break;

                case (int)SimulationTypes.Reference:
                    list = GetReferenceSimulationList(isActive).ToList();
                    break;

                case (int)SimulationTypes.User:
                    list = GetUserSimulationList(userId, isActive).ToList();
                    break;
            }
            return list;
        }
        /// <summary>
        /// Checks if a save is valid.
        ///     - Check if the name is empty
        ///     - Check if the name is unique
        /// </summary>
        /// <param name="saveInfo">The model to check</param>
        /// <returns></returns>
        public SaveValidationResponseViewModel IsValid(SaveViewModel saveInfo)
        {
            var viewModel = new SaveValidationResponseViewModel();
            viewModel.IsNameEmpty = string.IsNullOrEmpty(saveInfo.Name);

            if (!viewModel.IsNameEmpty)
            {
                viewModel.IsNameUnique = IsSaveNameUnqiue(saveInfo);
            }

            return viewModel;
        }

        private bool IsSaveNameUnqiue(SaveViewModel saveInfo)
        {
            const string query = "SELECT COUNT(*) FROM [Save] WHERE Name = @name";

            var dictionary = new Dictionary<string, object>
                {
                    {"name", saveInfo.Name}
                };

            var result = RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, query, dictionary);

            return result == 0;
        }

        public int CreateAssumptionsScenario(SaveViewModel saveInfo)
        {
            var isValidModel = IsValid(saveInfo);
            if(isValidModel.IsNameEmpty)
                throw new ArgumentException("The name is required");
            if(!isValidModel.IsNameUnique)
                throw new ArgumentException("The name is already taken, please choose another one");
            
            var userId = AccountRepository.GetUserId();
            const string query =
                @"INSERT INTO [Save] (Name,Comment,StartTime,Duration,SaveTypeId,SaveTime,
                IsPublic,IsApproved,IsReference,IsBudget,UserId,
                DataVersionId,CurrencyBudgetVersionId,AssumptionsSaveId,Active,SimulationCurrencyId,IsLaunch)
                OUTPUT INSERTED.Id VALUES 
                (@name,@comment,@startTime,@duration,@saveTypeId,@saveTime,
                @isPublic,@isApproved,@isReference,@isBudget,@userId,
                @dataVersionId,@currencyBudgetVersionId,@assumptionsSaveId,@active,@simulationCurrencyId,@isLaunch)";
            var dictionnary = new Dictionary<string, object>
            { 
                {"name", saveInfo.Name},
                {"comment", saveInfo.Comment ?? ""},
                {"startTime", DateTime.Now},
                {"duration", 0},
                {"saveTypeId", (int)SaveTypes.Assumptions},
                {"saveTime", DateTime.Now},
                {"isPublic", true},
                {"isApproved", true},
                {"isReference", true},
                {"isBudget", true},
                {"userId", userId},
                {"dataVersionId", 1},
                {"currencyBudgetVersionId", 1},
                {"assumptionsSaveId", 1},
                {"active", true},
                {"simulationCurrencyId",1},
                {"isLaunch", false}
            };
            return RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, query, dictionnary);
        }
        private static void SaveForecast(int simulationId, int saveId)
        {
            var data = CacheRepository.GetDataCache(simulationId).ToList();
            var rule = CacheRepository.GetRule(simulationId, true).ToList();
            CacheRepository.UpdateSimulationInfo(simulationId, saveId);
            DataRepository.SaveVersionData(data, saveId);
            GprmRuleRepository.SaveVersionRule(rule, saveId);
        }
        private static int SetSave(CacheViewModel cacheInfo, SaveViewModel saveInfo)
        {
            const string query =
                @"INSERT INTO [Save] (Name,Comment,StartTime,Duration,SaveTypeId,SaveTime,
                IsPublic,IsApproved,IsReference,IsBudget,UserId,
                DataVersionId,CurrencyBudgetVersionId,AssumptionsSaveId,Active,SimulationCurrencyId,IsLaunch)
                OUTPUT INSERTED.Id VALUES 
                (@name,@comment,@startTime,@duration,@saveTypeId,@saveTime,
                @isPublic,@isApproved,@isReference,@isBudget,@userId,
                @dataVersionId,@currencyBudgetVersionId,@assumptionsSaveId,@active,@simulationCurrencyId,@isLaunch)";
            var dictionnary = new Dictionary<string, object>
            { 
                {"name", saveInfo.Name},
                {"comment", saveInfo.Comment ?? ""},
                {"startTime", cacheInfo.StartTime},
                {"duration", cacheInfo.Duration},
                {"saveTypeId", (int)SaveTypes.Forecast},
                {"saveTime", DateTime.Now},
                {"isPublic", saveInfo.IsPublic},
                {"isApproved", saveInfo.IsApproved},
                {"isReference", saveInfo.IsReference},
                {"isBudget", saveInfo.IsBudget},
                {"userId", cacheInfo.UserId},
                {"dataVersionId", cacheInfo.DataVersionId},
                {"currencyBudgetVersionId", cacheInfo.CurrencyBudgetVersionId},
                {"assumptionsSaveId", cacheInfo.AssumptionsSaveId},
                {"active", true},
                {"simulationCurrencyId",cacheInfo.SimulationCurrencyId},
                {"isLaunch", cacheInfo.IsLaunch}
            };
            return RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, query, dictionnary);
        }
        private static void UpdateSave(SaveViewModel saveInfo, int saveId)
        {
            const string query =
                @"UPDATE [Save]
                SET SaveTime=@saveTime,
                IsPublic=@isPublic,
                IsApproved=@isApproved,
                IsReference=@isReference,
                IsBudget=@isBudget
                WHERE Id=@saveId";
            var dictionnary = new Dictionary<string, object>
            { 
                {"saveTime", DateTime.Now},
                {"isPublic", saveInfo.IsPublic},
                {"isApproved", saveInfo.IsApproved},
                {"isReference", saveInfo.IsReference},
                {"isBudget", saveInfo.IsBudget},
                {"saveId", saveId}
            };
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionnary);
        }
        public SaveViewModel GetSimulationInfo(int saveId)
        {
            const string query =
                "SELECT * FROM [Save] WHERE Id=@saveId";
            var result = RequestHelper.ExecuteQuery<SaveViewModel>(
                DataBaseConstants.ConnectionString,
                query,
                MapSimulationList,
                new Dictionary<string, object>
                {
                    {"saveId", saveId}
                }).FirstOrDefault();
            return result;
        }
        public IEnumerable<SaveViewModel> GetAssumptionsScenarios()
        {
            var result = GetSimulationList(null, (int)SaveTypes.Data, true, true, true, true, true, false).ToList();
            result.AddRange(GetSimulationList(null, (int)SaveTypes.Assumptions, true, true, true, true, true, false));
            return result.OrderBy(s => s.SaveTime);
        }
        public IEnumerable<SaveViewModel> GetBudgetSimulationList()
        {
            return GetSimulationList(null, (int)SaveTypes.Forecast, true, true, true, true, true, true);
        }

        public IEnumerable<SaveViewModel> GetBudgetSimulationList(bool isActive)
        {
            return GetSimulationList(null, (int)SaveTypes.Forecast, true, true, true, true, isActive, true);
        } 

        public IEnumerable<SaveViewModel> GetReferenceSimulationList()
        {
            return GetSimulationList(null, (int)SaveTypes.Forecast, true, true, true, false, true, true);
        }

        public IEnumerable<SaveViewModel> GetReferenceSimulationList(bool isActive)
        {
            return GetSimulationList(null, (int)SaveTypes.Forecast, true, true, true, false, isActive, true);
        } 

        public IEnumerable<SaveViewModel> GetPublicSimulationList()
        {
            return GetSimulationList(null, (int)SaveTypes.Forecast, true, false, false, false, true, true);
        }

        public IEnumerable<SaveViewModel> GetPublicSimulationList(bool isActive)
        {
            return GetSimulationList(null, (int)SaveTypes.Forecast, true, false, false, false, isActive, true);
        }

        public IEnumerable<SaveViewModel> GetUserSimulationList(string userId)
        {
            return GetSimulationList(userId, (int)SaveTypes.Forecast, false, false, false, false, true, true);
        }

        public IEnumerable<SaveViewModel> GetUserSimulationList(string userId, bool isActive)
        {
            return GetSimulationList(userId, (int)SaveTypes.Forecast, false, false, false, false, isActive, true);
        }

        public bool UpdateSimulation(SaveViewModel model)
        {
            const string query = "UPDATE [dbo].[Save] SET Active=@status WHERE Id=@id";

            var dictionary = new Dictionary<string, object>
            {
                {"status", model.Active},
                {"id", model.SaveId}
            };

            var count = RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);
            return count == 1;
        }

        private static IEnumerable<SaveViewModel> GetSimulationList(string userId, int saveTypeId, bool isPublic,
            bool isApproved, bool isReference, bool isBudget, bool isActive, bool includeLaunch)
        {
            const string query =
                "SELECT * FROM GetSimulationList(@userId,@saveTypeId,@isPublic,@isApproved,@isReference,@isBudget,@isActive,@includeLaunch)";

            var result = RequestHelper.ExecuteQuery<SaveViewModel>(
                DataBaseConstants.ConnectionString,
                query,
                MapSimulationList,
                new Dictionary<string, object>
                {
                    {"userId", userId ?? (object) DBNull.Value},
                    {"saveTypeId", saveTypeId},
                    {"isPublic", isPublic},
                    {"isApproved", isApproved},
                    {"isReference", isReference},
                    {"isBudget", isBudget},
                    {"isActive", isActive},
                    {"includeLaunch",includeLaunch}
                });
            return result.OrderByDescending(r => r.SaveTime);
        }
        private static IEnumerable<SaveViewModel> GetLaunchSimulationList(string userId, bool isPublic, bool isActive)
        {
            const string query =
                "SELECT * FROM GetLaunchSimulationList(@userId,@isPublic,@isActive)";

            var result = RequestHelper.ExecuteQuery<SaveViewModel>(
                DataBaseConstants.ConnectionString,
                query,
                MapSimulationList,
                new Dictionary<string, object>
                {
                    {"userId", userId ?? (object) DBNull.Value},
                    {"isPublic", isPublic},
                    {"isActive", isActive}
                });
            return result.OrderByDescending(r => r.SaveTime);
        }
        private static SaveViewModel MapSimulationList(DataRow row)
        {
            var result = new SaveViewModel
            {
                SaveId = (int)row["Id"],
                Name = row["Name"].PreventStringNull(),
                Comment = row["Comment"].PreventStringNull(),
                StartTime = (DateTime)row["StartTime"],
                Duration = (int)row["Duration"],
                SaveTypeId = (int)row["SaveTypeId"],
                SaveTime = (DateTime)row["SaveTime"],
                IsPublic = (bool)row["IsPublic"],
                IsApproved = (bool)row["IsApproved"],
                IsReference = (bool)row["IsReference"],
                UserId = row["UserId"].PreventStringNull(),
                DataVersionId = (int)row["DataVersionId"],
                CurrencyBudgetVersionId = (int)row["CurrencyBudgetVersionId"],
                AssumptionsSaveId = (int)row["AssumptionsSaveId"],
                Active = (bool)row["Active"],
                IsLaunch = (bool)row["IsLaunch"]
            };

            if (row.Table.Columns.Contains("SimulationCurrencyId"))
                result.SimulationCurrencyId = (int) row["SimulationCurrencyId"];

            return result;
        }
    }
}