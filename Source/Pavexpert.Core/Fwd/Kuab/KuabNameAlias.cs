using System.Collections.Generic;

namespace Pavexpert.Core.Fwd.Kuab
{
    class KuabNameAlias
    {
        public string Name { get; set; }

        public IList<string> Alias { get; set; }

        public KuabNameAlias()
        {
            Alias = new List<string>();
        }
    }
}