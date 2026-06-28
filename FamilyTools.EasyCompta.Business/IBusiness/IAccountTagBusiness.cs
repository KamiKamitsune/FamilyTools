using FamilyTools.Data.Models.EasyCompta;

namespace FamilyTools.EasyCompta.IBusiness;

public interface IAccountTagBusiness : IBaseBusiness<AccountTag>
{
    Task<AccountTag> DefaultTag();
    Task<List<AccountTag>> TagList();
}