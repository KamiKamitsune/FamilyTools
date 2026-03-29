using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyTools.Data.Models.EasyCompta;

[Table("Templates")]
public class Template : BaseModel
{
    public string Name { get; set; }

    public ICollection<AccountEnter> Enters { get; set; } = new List<AccountEnter>();

    public DateTime Date { get; set; }
}