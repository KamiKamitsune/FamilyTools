using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

using static FamilyTools.Data.Models.EasyCompta.EnumEasycompta;

namespace FamilyTools.Data.Models.EasyCompta
{
    [Table("AccountEnters")]
    public class AccountEnter : BaseModel
    {
        public ICollection<AccountLine> Lines { get; set; } = new List<AccountLine>();
        public AccountTag Tag { get; set; } = new AccountTag();
        public OperationType OperationType { get; set; }
        public string Name { get; set; } = string.Empty;
        public float TotalValue { get; set; }
        public DateOnly Date { get; set; }
        public int PageId { get; set; }
        public bool IsDisabled { get; set; }

        [JsonIgnore]
        public AccountPage Page { get; set; } = null!;

        public AccountEnter()
        {
            if (Lines?.Count > 0)
            {
                TotalValue = Lines.Sum(line => line.Value);
            }
        }

        public AccountEnter(string name, OperationType operationType, DateOnly date, float totalValue, List<AccountLine> lines, AccountTag tag)
        {
            Name = name;
            OperationType = operationType;
            TotalValue = totalValue;
            Date = date;
            Lines = lines;
            Tag = tag;
        }
    }
}
