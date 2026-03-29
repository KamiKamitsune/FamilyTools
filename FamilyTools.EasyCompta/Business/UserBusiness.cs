using FamilyTools.Data.Context;
using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.IBusiness;
using Microsoft.EntityFrameworkCore;

namespace FamilyTools.EasyCompta.Business;

public class UserBusiness(EasyComptaContext context) : BaseBusiness<User>(context), IUserBusiness
{
    public async Task<List<User>> UserList()
    {
        var users = await this._context.Users.ToListAsync() ?? [];

        return users;
    }
}