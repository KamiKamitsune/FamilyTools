using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyTools.Data.Models.EasyCompta;

[Table("AccountPages")]
public class AccountPage : BaseModel
{
    public AccountPage()
    {
    }

    public AccountPage(List<AccountEnter> enters, DateOnly date) : this()
    {
        this.Name = $"Page {date.Month}/{date.Year}";
        this.Enters = enters;
        this.Date = date;
        this.IsClosing = false;
    }

    public string Name { get; set; } = string.Empty;

    public ICollection<AccountEnter> Enters { get; set; } = new List<AccountEnter>();

    public ICollection<PaymentDone> PaymentDones { get; set; } = new List<PaymentDone>();

    public bool IsClosing { get; set; }

    public DateOnly Date { get; set; }

    public float Total { get; set; }
}