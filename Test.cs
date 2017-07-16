using MeasuringTools.Core;
using MeasuringTools.Output;

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

            // output config
            var listenToOutputInConsole = new MeasureConsoleOutputListener();
            listenToOutputInConsole.StartListening();
            var autoBroadCastToConsole = new MeasuresBroadcaster(listenToOutputInConsole, cast);

            var listenToOutputInCVS = new MeasuresCSVOutputListener();
            listenToOutputInCVS.StartListening();
            var autoBroadCastToCSV = new MeasuresBroadcaster(listenToOutputInCVS, measure);

            // output test
            cast[ChronoStore.ChronoSlot2].Measures.AddMeasure(0.01d);
            var tmp = cast[ChronoStore.ChronoSlot2];
            tmp.Start();
            cast[MeasuresStore.MeasuresSlot1].AddMeasure(0.02d); // Why took so much time???!!
            tmp.Stop();

            measure.AddMeasure(0.03d);
            measure.AddMeasure(0.04d);

            cast[ChronoStore.ChronoSlot2].Restart();
            measure.AddMeasure(0.05d);
            cast[ChronoStore.ChronoSlot2].Stop();
        }
    }
}
