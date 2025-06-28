using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeXpert.LexCore
{
    /// <summary>
    /// DAWG Nodes saved in files (from the 90's) had the following structure in C++.
    /// {
    ///    unsigned int	childNodeId : 22;
    ///             int letter : 8;
	///	   unsigned int isTerminal : 1;
	///	   unsigned int isLastChild : 1;
    ///	}
    ///	The above made it a nice 32 bit (4 byte) structure which made DAWG compact.
    ///	Also, all the sibling nodes were saved contiguosly.
    ///	
    /// In CSharp class, we will use the following structure and not use the bit layout.
    /// This is version 1 of the layout!
    /// </summary>
    public struct DawgNode
    {
        public uint ChildNodeIdx { get; internal set; }
        public byte Letter { get; internal set; }
        public bool IsTerminal { get; internal set; }
        public bool IsLastChild { get; internal set; }

        public DawgNode(uint childNodeIdx, byte letter, bool isTerminal, bool isLastChild)
        {
            this.ChildNodeIdx = childNodeIdx;
            this.Letter = letter;
            this.IsTerminal = isTerminal;
            this.IsLastChild = isLastChild;
        }
    }
}
