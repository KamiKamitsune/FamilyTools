using FamilyTools.Data.Models.EasyCompta;

namespace FamilyTools.EasyCompta.IBusiness
{
    public interface IAccountPageBusiness : IBaseBusiness<AccountPage>
    {
        Task CreateListPage(List<AccountPage> pages);
        Task<List<DateOnly>> GetAllMonth();
        public Task<AccountPage> GetPageByDate(int month, int year);
    }
}
