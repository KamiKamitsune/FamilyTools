using FamilyTools.Data.Models.EasyCompta;

namespace FamilyTools.EasyCompta.IBusiness
{
    public interface IPaymentDoneBusiness
    {
        Task<List<PaymentDone>> CreateListFromPage(AccountPage page);
        List<PaymentDone> getPaymentDonesFromPages(AccountPage page);
        Task<List<PaymentDone>> UpdateListFromPage(AccountPage page);
    }
}