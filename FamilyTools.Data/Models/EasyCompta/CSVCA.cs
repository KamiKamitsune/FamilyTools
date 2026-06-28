namespace FamilyTools.Data.Models.EasyCompta;

public class CSVCA
{
    public DateTime Date { get; set; }
    public OperationType OperationType { get; set; } = new();
    public string Libelle { get; set; } = "";
    public float Debit { get; set; } = 0;
    public float Credit { get; set; } = 0;
}