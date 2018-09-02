using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PrtgAPI.PowerShell.GoPrtg
{
    class GoPrtgProfile
    {
        internal const string GoPrtgHeader = "########################### Start GoPrtg Servers ###########################";
        internal const string GoPrtgFooter = "############################ End GoPrtg Servers ############################";

        public Lazy<int> HeaderLine { get; set; }

        public bool HeaderMissing => HeaderLine.Value == -1;

        public Lazy<int> FooterLine { get; set; }

        public bool FooterMissing => FooterLine.Value == -1;

        public Lazy<List<string>> Contents { get; set; }

        public Lazy<List<string>> AboveHeader { get; set; }

        public Lazy<List<string>> BelowFooter { get; set;}

        public GoPrtgProfile(string profile)
        {
            Contents = new Lazy<List<string>>(() => File.Exists(profile) ? File.ReadAllLines(profile).ToList() : new List<string>());
            HeaderLine = new Lazy<int>(GetGoPrtgHeader);
            FooterLine = new Lazy<int>(GetGoPrtgFooter);

            AboveHeader = new Lazy<List<string>>(GetContentAboveHeader);
            BelowFooter = new Lazy<List<string>>(GetContentBelowFooter);
        }

        int GetGoPrtgHeader()
        {
            for (var i = 0; i < Contents.Value.Count; i++)
            {
                if (Contents.Value[i] == GoPrtgHeader)
                    return i;
            }

            return -1;
        }

        int GetGoPrtgFooter()
        {
            var start = HeaderLine.Value != -1 ? HeaderLine.Value : 1;

            for (var i = start; i < Contents.Value.Count; i++)
            {
                if (Contents.Value[i] == GoPrtgFooter)
                    return i;
            }

            return -1;
        }

        private List<string> GetContentAboveHeader()
        {
            if (HeaderMissing)
                return Contents.Value;

            return Contents.Value.Take(HeaderLine.Value).ToList();
        }

        private List<string> GetContentBelowFooter()
        {
            if (FooterMissing)
                return Contents.Value;

            return Contents.Value.Skip(FooterLine.Value + 1).ToList();
        }
    }
}
