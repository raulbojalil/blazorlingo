using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazorlingo.Client.Shared
{
    public class FlagComponent : ComponentBase
    {
        [Parameter]
        public string CountryCode { get; set; }

        protected string FlagPos
        {
            get
            {
                var flags = new string[]
                {
                "en",
                "es",
                "fr",
                "de",
                "ja",
                "it",
                "ko",
                "zh",
                "ru",
                "pt",
                "tr",
                "nl",
                "sv",
                "ga",
                "el",
                "he",
                "pl",
                "no",
                "vi",
                "da",
                "hv",
                "ro",
                "sw",
                "eo",
                "hu",
                "cy",
                "uk",
                "kl",
                "cs",
                "hi",
                "id",
                "ha",
                "nv",
                "ar",
                "ca",
                "th",
                "gn",
                "ambassador",
                "duolingo",
                "troubleshooting",
                "teachers",
                "la"
                };

                if (flags.Contains(CountryCode))
                {
                    if (CountryCode == "ar") return "-1063px";
                    else if (CountryCode == "eo") return "-739px";
                    else if (CountryCode == "la") return "-1320px";
                    else if (CountryCode == "tr") return "-321px";

                    var index = Array.FindIndex(flags, x => x == CountryCode);
                    return $"-{(index * 32)}px";
                }

                throw new Exception("Unknown country code " + CountryCode);
            }

        }
    }
}
