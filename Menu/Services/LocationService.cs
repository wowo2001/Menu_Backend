using Menu.Data;
using Menu.Models;
using System.Xml.Linq;

namespace Menu.Services
{
    public interface ILocationService
    {
        Task<string> GetLocation(string name);

        Task<string> EditLocation(NameLocation namelocation);

        Task<string> DeleteLocation(string name);
    }
    public class LocationService : ILocationService
    {
        private readonly ILocationData _locationData;

        public LocationService(ILocationData locationData)
        {
            _locationData = locationData;
        }
        public async Task<string> GetLocation(string name)
        {
            return await _locationData.GetLocationData(name);
        }

        public async Task<string> EditLocation(NameLocation namelocation)
        {
            if (await _locationData.GetLocationData(namelocation.Name) == null)
            {
                return await _locationData.AddLocationData(namelocation);
            }
            else
            {
                return await _locationData.UpdateLocationData(namelocation);
            }

        }

        public async Task<string> DeleteLocation(string name)
        {
            return await _locationData.DeleteLocationData(name);
        }
    }
}
