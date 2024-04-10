using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Models
{
    public class Country
    {
        private const string FlagCDN = @"https://flagcdn.com/";
        public string Code { get; set; }
        public string Name { get; set; }
        public string Flag => FlagCDN + this.Code + ".svg";
        public string SmallFlag => FlagCDN + @"w40/" + this.Code + ".png";
    }

    public sealed class Countries
    {
        #region private members and methods
        public static Countries Instance { get; } = new Countries();
        private static readonly string Iso3166File = @"~/App_Data/iso-3166.txt";
        private static readonly List<Country> _countries = new List<Country>();
        private static void LoadCountries()
        {
            var httpServerUtility = new HttpServerUtilityWrapper(HttpContext.Current.Server);
            var iso3166Path = httpServerUtility.MapPath(Iso3166File);
            try
            {
                // Create a StreamReader  
                using (var reader = new StreamReader(iso3166Path))
                {
                    string line;
                    // Read line by line  
                    while ((line = reader.ReadLine()) != null)
                    {
                        var token = line.Split(new char[] { ',' });
                        _countries.Add(new Country() { Code = token[0].ToLower().Trim(), Name = token[1].Trim() });
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region public method
        public static List<Country> List
        {
            get
            {
                if (_countries.Count == 0)
                    LoadCountries();
                return _countries.OrderBy(c => c.Name).ToList();
            }
        }
        public static Country Get(string code)
        {
            Country country = List.FirstOrDefault(c => c.Code == code);
            return country;
        }
        public static string FlagUrl(string code)
        {
            var url = string.Empty;
            Country country = Get(code);
            if (country != null)
                url = country.Flag;
            return url;
        }
        #endregion
    }
}