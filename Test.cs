using MeasuringTools.Core;

namespace MeasuringTools
{
    public class Test
    {
        public Test()
        {
            // power of generic !
            MeasuresAggregatorStores<MinMaxSigmaMeanMeasures> mca = new MeasuresAggregatorStores<MinMaxSigmaMeanMeasures>();
            MinMaxSigmaMeanMeasures measure = new MinMaxSigmaMeanMeasures("test0");
            var average = measure.MovingMean;
            var chrono = new ChronoMeasures<MinMaxSigmaMeanMeasures>(measure);
            var sameaverage = chrono.Measures.MovingMean;
            mca.RegisterMeasure(ChronoStore.ChronoSlot1, chrono);
            foreach (var measures in mca)
            {
                var sameAgainAverage = measures.MovingMean;
            }

            // simple one
            IMeasuresAggregator<AbstractMeasures> mcas = new MeasuresAggregatorStores<MinMaxSigmaMeanMeasures>();
            var cast = mcas as MeasuresAggregatorStores<MinMaxSigmaMeanMeasures>;
            cast.RegisterMeasure(ChronoStore.ChronoSlot2, new ChronoMeasures<MinMaxSigmaMeanMeasures>(new MinMaxSigmaMeanMeasures("test1")));
            cast.RegisterMeasure(new MinMaxSigmaMeanMeasuresCollection(1000));
            cast.RegisterMeasure(MeasuresStore.MeasuresSlot1, new MinMaxSigmaMeanMeasures("anotherOne"));
            foreach (var measures in mcas)
            {
                var measureStringRepresentation = measures.ToString();
                var sameAgainAverage = ((MinMaxSigmaMeanMeasures)(measures))?.MovingMean;
            }
        }
    }
}
