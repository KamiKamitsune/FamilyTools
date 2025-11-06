using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FamilyTools.Data.Models.EasyCompta
{
    [Table("AccountLines")]
    public class AccountLine: BaseModel
    {

        public required string Name { get; set; }
        public int UserId { get; set; }
        public required User User { get; set; }
        public float Value { get; set; }
        public int EnterId { get; set; }

        [JsonIgnore]
        public AccountEnter Enter { get; set; } = null!;
    }
}
