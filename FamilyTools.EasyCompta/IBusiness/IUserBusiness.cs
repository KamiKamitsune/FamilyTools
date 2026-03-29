using FamilyTools.Data.Models.EasyCompta;

namespace FamilyTools.EasyCompta.IBusiness;

public interface IUserBusiness : IBaseBusiness<User>
{
    Task<List<User>> UserList();
}