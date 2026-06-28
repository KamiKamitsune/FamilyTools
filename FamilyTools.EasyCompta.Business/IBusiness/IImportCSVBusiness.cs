namespace FamilyTools.EasyCompta.IBusiness;

public interface IImportCSVBusiness
{
    Task CSVToAccountPages(byte[] csvFile);
}