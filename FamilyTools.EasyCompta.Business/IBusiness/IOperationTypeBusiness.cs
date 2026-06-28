using FamilyTools.Data.Models.EasyCompta;

namespace FamilyTools.EasyCompta.IBusiness;

public interface IOperationTypeBusiness : IBaseBusiness<OperationType>
{
    Task<OperationType> FindByName(string name);
    Task<List<OperationType>> OperationTypeList();
}