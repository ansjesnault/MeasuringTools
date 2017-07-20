using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringTools.Core
{
    /// <summary>
    /// Max Ten chronos available per instance.\n
    /// See <see cref="MeasuresAggregatorStores{U_meas}"/>.
    /// </summary>
    public enum ChronoStore : byte
    {
        /// <summary>
        /// 1st chrono flag id.
        /// </summary>
        ChronoSlot0 = 0x00,

        /// <summary>
        /// 2nd chrono flag id.
        /// </summary>
        ChronoSlot1 = 0x01,

        /// <summary>
        /// 3rd chrono flag id.
        /// </summary>
        ChronoSlot2 = 0x02,

        /// <summary>
        /// 4th chrono flag id.
        /// </summary>
        ChronoSlot3 = 0x04,

        /// <summary>
        /// 5th chrono flag id.
        /// </summary>
        ChronoSlot4 = 0x08,

        /// <summary>
        /// 6th chrono flag id.
        /// </summary>
        ChronoSlot5 = 0x10,

        /// <summary>
        /// 7th chrono flag id.
        /// </summary>
        ChronoSlot6 = 0x20,

        /// <summary>
        /// 8th chrono flag id.
        /// </summary>
        ChronoSlot7 = 0x40,

        /// <summary>
        /// 9th chrono flag id.
        /// </summary>
        ChronoSlot8 = 0x80,

        /// <summary>
        /// 10th chrono flag id.
        /// </summary>
        ChronoSlot9 = 0x0A,
    }
}
