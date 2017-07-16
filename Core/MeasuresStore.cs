using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringTools.Core
{
    /// <summary>
    /// Max Ten Measures available per instance.
    /// See <see cref="MeasuresAggregatorStores{U_meas}"/>.
    /// </summary>
    public enum MeasuresStore : byte
    {
        /// <summary>
        /// 1st Measures flag id.
        /// </summary>
        MeasuresSlot0 = 0x00,

        /// <summary>
        /// 2nd Measures flag id.
        /// </summary>
        MeasuresSlot1 = 0x01,

        /// <summary>
        /// 3rd Measures flag id.
        /// </summary>
        MeasuresSlot2 = 0x02,

        /// <summary>
        /// 4th Measures flag id.
        /// </summary>
        MeasuresSlot3 = 0x04,

        /// <summary>
        /// 5th Measures flag id.
        /// </summary>
        MeasuresSlot4 = 0x08,

        /// <summary>
        /// 6th Measures flag id.
        /// </summary>
        MeasuresSlot5 = 0x10,

        /// <summary>
        /// 7th Measures flag id.
        /// </summary>
        MeasuresSlot6 = 0x20,

        /// <summary>
        /// 8th Measures flag id.
        /// </summary>
        MeasuresSlot7 = 0x40,

        /// <summary>
        /// 9th Measures flag id.
        /// </summary>
        MeasuresSlot8 = 0x80,

        /// <summary>
        /// 10th Measures flag id.
        /// </summary>
        MeasuresSlot9 = 0x0A,
    }
}
