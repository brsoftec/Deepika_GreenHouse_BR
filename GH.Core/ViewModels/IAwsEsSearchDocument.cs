using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public interface IAwsEsSearchDocument
    {
        int? Start { get; set; }
        int? Length { get; set; }

        string ToEsQueryString();
    }
}