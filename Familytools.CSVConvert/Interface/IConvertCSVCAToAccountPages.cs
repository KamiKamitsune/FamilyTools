using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace FamilyTools.CSVConvert.Interface
{
    public interface IConvertCSVCAToAccountPages
    {
        Task ConvertCSVFile(IFormFile csvFile, CancellationToken cancellationToken);
    }
}
