namespace Pavexpert.Core.Fwd.Kuab
{
    abstract class KuabKeyValuePair
    {
        public string Key { get; set; }

        public string Value { get; set; }

        protected KuabKeyValuePair()
        {
            Key = string.Empty;
            Value = string.Empty;
        }

        protected KuabKeyValuePair(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    class KuabInstallation : KuabKeyValuePair
    {
        public KuabInstallation(string key, string value)
            : base(key, value)
        { }
    }

    class KuabHeader : KuabKeyValuePair
    {
        public KuabHeader(string key, string value)
            : base(key, value)
        { }
    }

    class KuabBlock : KuabKeyValuePair
    {
        public double Station { get; set; }

        public KuabBlock(string key, string value)
            : base(key, value)
        { }
    }

    class KuabJump : KuabKeyValuePair
    {
        public KuabJump(string key, string value)
            : base(key, value)
        { }
    }
}