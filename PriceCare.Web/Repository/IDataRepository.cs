using System.Collections.Generic;
using PriceCare.Web.Models;
using System;
namespace PriceCare.Web.Repository
{
    public interface IDataRepository
    {
        IEnumerable<DataViewModel> GetVersionData(List<int> geographyId, List<int> productId, int dataTypeId, int saveId,
            int versionId, int currencyBudgetVersionId, int startYear, int endYear, bool fillData = true, bool takeInactive = false);
        IEnumerable<DateTime> CreateDataTimes(DateTime startTime, int simulationDuration);
        IEnumerable<DateTime> CreateDataTimes(int simulationDuration);

        IEnumerable<DataViewModel> GetLatestData(int saveId, List<int> geographyId, List<int> productId,
            List<int> dataTypeId, bool fillData = true);

        IEnumerable<DataViewModel> GetVersionData(int saveId, int versionId, int currencyBudgetVersionId,
            List<int> geographyId, List<int> productId, List<int> dataTypeId, bool fillData = true);

        IEnumerable<DataViewModel> GetVersionData(int dataTypeId, int saveId, int versionId, int currencyBudgetVersionId,
            int startYear, int endYear, bool fillData = true);

        void SaveVersionData(List<DataViewModel> data);
        void SaveVersionData(List<DataViewModel> data, int saveId);

        void SaveVersionData(List<DataViewModel> data, int saveId, bool replaceAll);
        DateTime GetVersionStartTime(int versionId);
        LatestUsdViewModel GetLatestUsdRates();

        IEnumerable<DataViewModel> FillEmptyData(List<int> existingGeographyId, IReadOnlyCollection<int> geographyId0,
            IEnumerable<int> productId, int dataTypeId);

        IEnumerable<DataViewModel> GetSaveData(List<int> geographyId, List<int> productId, int dataTypeId, int saveId,
            int dataVersionId, int currencyBudgetVersionId, int assumptionsSaveId, int startYear, int endYear);

    }
}
