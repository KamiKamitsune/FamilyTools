
namespace FamilyTools.EasyCompta.IBusiness
{
    public interface IImportCSVBusiness
    {
        Task CSVToAccountPages(byte[] csvFile);
        Task<bool> ImportCSVFile(IFormFile csvFile);
    }
}
