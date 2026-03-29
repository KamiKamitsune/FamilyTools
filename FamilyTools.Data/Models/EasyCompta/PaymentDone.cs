using System.Text.Json.Serialization;

namespace FamilyTools.Data.Models.EasyCompta;

public class PaymentDone : BaseModel
{
    public required User User { get; set; } = null!;
    public int UserId { get; set; }
    public bool PaymentIsDone { get; set; }
    public float Total { get; set; }
    public int PageId { get; set; }

    [JsonIgnore] public AccountPage Page { get; set; } = null!;
}