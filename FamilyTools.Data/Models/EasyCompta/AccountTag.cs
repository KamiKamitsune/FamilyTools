using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyTools.Data.Models.EasyCompta;

[Table("AccountTags")]
public class AccountTag : BaseModel
{
    public string Name { get; set; } = "default";
    public string Color { get; set; } = "#FFFFFF";
}