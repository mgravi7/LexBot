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
    public class TrieDiagnostics
    {
        // general statistics
        public uint NumNodesBeforeCompression{ get; internal set; } = 0;
        public uint NumNodesAfterCompression { get; internal set; } = 0;
        public uint NumFirstChildrenBeforeCompression { get; internal set; } = 0;
        public uint NumFirstChildrenAfterCompression { get; internal set; } = 0;
        public uint NumAttemptedWords { get; internal set; } = 0;
        public uint NumWords { get; internal set; } = 0;
        public uint NumAttemptedReverseWordlets { get; internal set; } = 0;
        public uint NumReverseWordlets { get; internal set; } = 0;
        public uint NumAttemptedForwardWordlets { get; internal set; } = 0;
        public uint NumForwardWordlets { get; internal set; } = 0;

        // time related statistics
        public DateTime TrieCreationTime { get; internal set; } = DateTime.UtcNow;
        public DateTime CompressStartTime { get; internal set; } = DateTime.UtcNow;
        public DateTime CompressEndTime { get; internal set; } = DateTime.UtcNow;
        public DateTime SaveDawgStartTime { get; internal set; } = DateTime.UtcNow;
        public DateTime SaveDawgEndTime { get; internal set; } = DateTime.UtcNow;
        public DateTime ValidateDawgStartTime { get; internal set; } = DateTime.UtcNow;
        public DateTime ValidateDawgEndTime { get; internal set; } = DateTime.UtcNow;
    }
}
