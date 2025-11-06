using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FamilyTools.Data.Models.EasyCompta
{
    [Table("AccountEnters")]
    public class AccountEnter : BaseModel
    {
        public ICollection<AccountLine> Lines { get; set; } = new List<AccountLine>();
        public AccountTag Tag { get; set; } = new AccountTag();
        public string Name { get; set; } = string.Empty;
        public float TotalValue { get; set; }
        public DateOnly Date { get; set; }
        public int PageId { get; set; }

        [JsonIgnore]
        public AccountPage Page { get; set; } = null!;

        public AccountEnter()
        {
            if (Lines?.Count > 0)
            {
                TotalValue = Lines.Sum(line => line.Value);
            }
        }

        public AccountEnter(string name, DateOnly date, float totalValue, List<AccountLine> lines, AccountTag tag)
        {
            Name = name;
            TotalValue = totalValue;
            Date = date;
            Lines = lines;
            Tag = tag;
        }
    }
}
