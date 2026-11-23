using EstateFlow.src.Models.Properties;

namespace EstateFlow.src.Services.Interfaces
{
    public interface IPropertyRepository
    {
        bool SaveAll(List<Property> properties, string path);
        List<Property> LoadAll(string path);
    }
}
