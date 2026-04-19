namespace FEMaster.Core
{
    public class Range
    {
        public double Max { get; set; } = double.MinValue;

        public double Min { get; set; } = double.MaxValue;

        public void AddValue(double value)
        {
            if (value > Max)
                Max = value;

            if (value < Min)
                Min = value;
        }

        public void AddValue(double[] values)
        {
            for (var i = 0; i <= values.Length - 1; i++)
            {
                if (values[i] > Max)
                    Max = values[i];

                if (values[i] < Min)
                    Min = values[i];
            }
        }

        public double Span()
        {
            return Max - Min;
        }
    }
}
