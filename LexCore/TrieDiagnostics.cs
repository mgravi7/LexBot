using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeXpert.LexCore
{
    /// <summary>
    /// A data class for collecting diagnostics
    /// </summary>
    internal class TrieDiagnostics
    {
        // general statistics
        public uint NumNodesBeforeCompression{ get; internal set; } = 0;
        public uint NumFirstChildrenBeforeCompression { get; internal set; } = 0;
        public uint NumWords { get; internal set; } = 0;
        public uint NumReversePartWords { get; internal set; } = 0;
        public uint NumNodesAfterCompression { get; internal set; } = 0;
        public uint NumFirstChildrenAfterCompression { get; internal set; } = 0;

        // time related statistics
        public DateTime TrieCreationTime { get; internal set; } = DateTime.UtcNow;
        public DateTime CompressionStartTime { get; internal set; } = DateTime.UtcNow;
        public DateTime CompressionEndTime { get; internal set; } = DateTime.UtcNow;
        public DateTime SaveDawgStartTime { get; internal set; } = DateTime.UtcNow;
        public DateTime SaveDawgEndTime { get; internal set; } = DateTime.UtcNow;
    }
}
