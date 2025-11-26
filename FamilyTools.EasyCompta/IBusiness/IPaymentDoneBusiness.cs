using FamilyTools.Data.Models.EasyCompta;

namespace FamilyTools.EasyCompta.IBusiness
{
    public interface IPaymentDoneBusiness
    {
        Task<List<PaymentDone>> CreateListFromPage(AccountPage page);
        Task<List<PaymentDone>> GetListByPageId(int pageId);
        Task<List<PaymentDone>> UpdateListFromPage(AccountPage page);
        Task<PaymentDone> UpdateStatePaymentDone(int id, bool status);
        Task<List<PaymentDone>> UpdateListFromPage(List<AccountEnter> enters);
        List<PaymentDone> CalculPaymentDonesFromEnters(IEnumerable<AccountEnter> enters);
    }
}