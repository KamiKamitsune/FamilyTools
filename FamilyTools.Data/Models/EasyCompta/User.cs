using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyTools.Data.Models.EasyCompta;

[Table("Users")]
public class User : BaseModel
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string UserName { get; set; }
}