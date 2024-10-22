using System.Collections.Generic;
using System.Net.Http;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface IVolumeRepository
    {
        List<DataViewModel> GetDataCache(DataSearchRequestViewModel model);
        object GetVolumes(VolumeRequest model, bool fillData = true);
        object GetForecastVolumes(VolumeRequest model);
        object GetVersions(VolumeRequest model);
    }
}