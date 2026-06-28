using FamilyTools.Data.Context;
using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.IBusiness;
using Microsoft.EntityFrameworkCore;

namespace FamilyTools.EasyCompta.Business;

public class OperationTypeBusiness(EasyComptaContext context) : BaseBusiness<OperationType>(context), IOperationTypeBusiness
{
    public async Task<OperationType> FindByName(string name)
    {
        if (name == "") return default;
        return await this._context.OperationTypes.Where(x => x.Name == name).FirstOrDefaultAsync();
    }

    public async Task<List<OperationType>> OperationTypeList()
    {
        return await this._context.OperationTypes.ToListAsync() ?? [];
    }
}