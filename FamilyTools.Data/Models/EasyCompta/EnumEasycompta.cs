using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyTools.Data.Models.EasyCompta
{
    public class EnumEasycompta
    {
        public enum OperationType
        {
            Unknown = 0,
            Prelevement = 1,
            PaiementCarte = 2,
            VirementRecu = 3,
            VirementEmis = 4,
            RemboursementPret = 5,
            Reglement = 6,
            Avoir = 7,
            Cotisation = 8,
            ChequeEmis = 9
        }
    }
}
