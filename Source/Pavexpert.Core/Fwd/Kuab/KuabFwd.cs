using System.Collections.Generic;

namespace Pavexpert.Core.Fwd.Kuab
{
    class KuabFwd
    {
        public IList<KuabDrop> Drops { get; private set; }

        public IList<KuabJump> Jumps { get; private set; }

        public IList<KuabBlock> Blocks { get; private set; }

        public IList<KuabHeader> Headers { get; private set; }

        public IList<KuabInstallation> Installations { get; private set; }

        public KuabFwd()
        {
            Drops = new List<KuabDrop>();
            Jumps = new List<KuabJump>();
            Blocks = new List<KuabBlock>();
            Headers = new List<KuabHeader>();
            Installations = new List<KuabInstallation>();
        }
    }
}
