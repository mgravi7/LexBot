using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeXpert.LexCore
{
    /// <summary>
    /// This is similar to the standard header used in Dawg files from the 90's
    /// with a few changes.
    /// (1) Size field (value was 64) is changed to VersionNumber.
    /// (2) No size restrictions on LexiconName (used to be 32).
    /// (3) No size restrictions on LexiconDate (used to be 20).
    /// (4) Dawg Type field added
    /// (5) NumReverseWordlets field added
    /// (6) NumForwardWordlets field added
    /// </summary>
    public class DawgHeader
    {
        public uint VersionNumber { get; internal set; } = 1;
        public uint NumNodes { get; internal set; } = 0;
        public string LexiconName { get; internal set; } = "";
        public string LexiconDate { get; internal set; } = DateTime.Now.ToString("dd MMMM yyyy");
        public uint NumWords { get; internal set; } = 0;
        public DawgType DawgType { get; internal set; } = DawgType.Reverse;
        public uint NumReverseWordlets { get; internal set; } = 0;
        public uint NumForwardWordlets { get; internal set; } = 0;

        // constants
        public const uint CURRENT_HEADER_VERSION = 1;
        public const uint LEGACY_LEXICON_NAME_LENGTH = 32;
        public const uint LEGACY_LEXICON_DATE_LENGTH = 20;
        public const uint LEGACY_HEADER_VERSION = 64;
    }
}
