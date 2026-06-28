using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyTools.Data.Models.EasyCompta;

[Table("OperationTypes")]
public class OperationType : BaseModel
{
    [Required]
    public string Name { get; set; } = string.Empty;


}