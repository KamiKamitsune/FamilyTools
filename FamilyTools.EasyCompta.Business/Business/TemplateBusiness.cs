using FamilyTools.Data.Context;
using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.EasyCompta.IBusiness;

namespace FamilyTools.EasyCompta.Business;

public class TemplateBusiness(EasyComptaContext context) : BaseBusiness<Template>(context), ITemplateBusiness
{
}