using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FamilyTools.Data.Models.EasyCompta;

[Table("AccountEnters")]
public class AccountEnter : BaseModel
{
    public AccountEnter()
    {
        if (this.Lines?.Count > 0) this.TotalValue = this.Lines.Sum(line => line.Value);
    }

    public AccountEnter(string name, OperationType operationType, DateOnly date, float totalValue,
        List<AccountLine> lines, AccountTag tag)
    {
        this.Name = name;
        this.OperationType = operationType;
        this.TotalValue = totalValue;
        this.Date = date;
        this.Lines = lines;
        this.Tag = tag;
    }

    public ICollection<AccountLine> Lines { get; set; } = new List<AccountLine>();
    public AccountTag Tag { get; set; } = new();
    public OperationType OperationType { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public float TotalValue { get; set; }
    public DateOnly Date { get; set; }
    public int PageId { get; set; }
    public bool IsDisabled { get; set; }

    [JsonIgnore] public AccountPage Page { get; set; } = null!;
}