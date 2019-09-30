using System;
using System.Threading;
namespace Liquid.Runtime.Telemetry
{
    /// <summary>
    /// Aggregates metric values for a single time period.
    /// Responsible for doing all logic aggregation, summarization, weighted average given an event defined by the developer.
    /// For start this aggregation, its necessary call BeginComputeMetric and then call EndMetricComputation.
    /// </summary>
    internal class LightMetricAggregator
    {
        private readonly SpinLock _trackLock = new SpinLock();

        #region Properties for the aggreation 
        public DateTimeOffset StartTimestamp { get; }
        public int Count { get; private set; }
        public double Sum { get; private set; }
        public double SumOfSquares { get; private set; }
        public double Min { get; private set; }
        public double Max { get; private set; }
        public double Average { get { return (Count == 0) ? 0 : (Sum / Count); } }
        public double Variance
        {
            get
            {
                return (Count == 0) ? 0 : (SumOfSquares / Count) - (Average * Average);
            }
        }
        public double StandardDeviation { get { return Math.Sqrt(Variance); } }

        #endregion

        ///Constructor necessary for take the timestamp that the matric will be tracked
        public LightMetricAggregator(DateTimeOffset startTimestamp)
        {
            this.StartTimestamp = startTimestamp;
        }

        ///Trace values ​​to be aggregated defining minimum, maximum average, time and sum of squares.
        public void TrackValue(double value)
        {
            bool lockAcquired = false;

            try
            {
                _trackLock.Enter(ref lockAcquired);

                if ((Count == 0) || (value < Min)) { Min = value; }
                if ((Count == 0) || (value > Max)) { Max = value; }
                Count++;
                Sum += value;
                SumOfSquares += value * value;
            }
            finally
            {
                if (lockAcquired)
                {
                    _trackLock.Exit();
                }
            }
        }
    }
}