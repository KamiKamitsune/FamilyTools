using FamilyTools.Data.Context;

namespace FamilyTools.Data.Seed.EasyCompta;

public interface IContextSeed
{
    EasyComptaContext Context { get; set; }

    Task Execute();
}