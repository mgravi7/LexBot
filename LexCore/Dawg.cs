using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// (4) NumReversePartWords field added
    /// </summary>
    public struct DawgHeader
    {
        public uint     VersionNumber;
        public uint     NumNodes;
        public string   LexiconName; 
        public string   LexiconDate; 
        public uint     NumWords;
        public uint     NumReversePartWords;

        // constants
        public const uint CURRENT_HEADER_VERSION = 1;
        public const uint LEGACY_LEXICON_NAME_LENGTH = 32;
        public const uint LEGACY_LEXICON_DATE_LENGTH = 20;
        public const uint LEGACY_HEADER_VERSION = 64;
    }

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
        public uint ChildNodeIdx;
        public byte Letter;
        public bool IsTerminal;
        public bool IsLastChild;

        public DawgNode(uint childNodeIdx, byte letter, bool isTerminal, bool isLastChild)
        {
            ChildNodeIdx = childNodeIdx;
            Letter = letter;
            IsTerminal = isTerminal;
            IsLastChild = isLastChild;
        }
    }

    public class DawgCreator
    {
        // *** PROPERTIES ***

        /// <summary>
        /// Count of nodes that have been added so far
        /// </summary>
        public uint NumAddedNodes
        {
            get
            {
                return numAddedNodes;
            }
        }

        // *** PUBLIC ***

        // CONSTRUCTOR
        /// <summary>
        /// Initializes DawgCreator, the only way of initializing
        /// </summary>
        /// <param name="lexiconName">Name of the lexicon</param>
        /// <param name="numNodes">Number of nodes (after compression typically)</param>
        /// <param name="numWords">Number of full words in the Dawg</param>
        public DawgCreator(string lexiconName, uint numNodes, uint numWords, uint numReversePartWords)
        {
            // initialization
            CreateHeader(lexiconName, numNodes, numWords, numReversePartWords);
            nodes = new DawgNode[numNodes];
            numAddedNodes = 0;
        }

        // ADD NODE
        /// <summary>
        /// Adds the given node to DAWG. Please note the nodes need to be added sequentially!
        /// </summary>
        /// <param name="dawgNode">Node to be added</param>
        public void AddNode(DawgNode dawgNode)
        {
            // validations
            Debug.Assert(numAddedNodes < header.NumNodes);

            nodes[numAddedNodes++] = dawgNode;
        }

        // SAVE DAWG
        /// <summary>
        /// Saves the Dawg to the given file. This needs to be called after all the
        /// nodes have been added sequentially. The format does not follow legacy
        /// layout.
        /// </summary>
        /// <param name="fileName">Use ".lxdg" as the extension</param>
        public void SaveDawg(string fileName)
        {
            // validation
            if (numAddedNodes != header.NumNodes)
            {
                string exceptionMessage = string.Format(
                    "Expected number of nodes: {0}. However, only {1} nodes have been added so far!",
                    header.NumNodes, numAddedNodes);
                throw new InvalidOperationException(exceptionMessage);
            }

            // call the version specific save function
            Debug.Assert(header.VersionNumber == 1U);   // that is all is supported now!
            SaveDawgInV1Format(fileName);
        }

        // *** PRIVATE ***

        // CREATE HEADER
        private void CreateHeader(string lexiconName, uint numNodes, uint numWords, uint numReversePartWords)
        {
            // fill all the fields
            header.VersionNumber = DawgHeader.CURRENT_HEADER_VERSION;
            header.LexiconDate = DateTime.Now.ToString("dd MMMM yyyy");
            header.LexiconName = lexiconName;
            header.NumNodes = numNodes;
            header.NumWords = numWords;
            header.NumReversePartWords = numReversePartWords;
        }

        // SAVE DAWG IN V1 FORMAT
        private void SaveDawgInV1Format(string fileName)
        {
            Debug.Assert(header.VersionNumber == 1U);

            // open the file
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                // write the header
                writer.Write(header.VersionNumber);
                writer.Write(header.NumNodes);
                writer.Write(header.LexiconName);
                writer.Write(header.LexiconDate);
                writer.Write(header.NumWords);
                writer.Write(header.NumReversePartWords);

                // write the nodes
                for (uint idx = 0; idx < numAddedNodes; idx++)
                {
                    writer.Write(nodes[idx].ChildNodeIdx);
                    writer.Write(nodes[idx].Letter);
                    writer.Write(nodes[idx].IsTerminal);
                    writer.Write(nodes[idx].IsLastChild);
                }
            }
        }

        // DATA
        private DawgHeader  header;
        private DawgNode[]  nodes;
        private uint        numAddedNodes;
    }

    public class Dawg
    {
        // *** PUBLIC DECLARATIONS ***
        public const byte FORWARD_WORD_DAWG_SYMBOL = 42;        // '*'
        public const byte REVERSE_PARTWORD_DAWG_SYMBOL = 60;    // '<'

        public const byte START_LETTER = 65;    // 'A'
        public const byte END_LETTER = 90;      // 'Z'
        public const byte DEFAULT_LETTER = 32;  // ' '

        // *** PRIVATE DECLARATIONS ***
        private const uint ROOT_NODE_ID = 0;
        private const uint FORWARD_WORD_NODE_ID = 1;
        private const uint REVERSE_PARTWORD_NODE_ID = 2;
        private const uint MINIMUM_NUMBER_OF_NODES = 3; // Root, forward and reverse

        // *** PUBLIC ***
        // CONSTRUCTOR
        public Dawg(string fileName)
        {
            // open the file
            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                // read the header
                header.VersionNumber = reader.ReadUInt32();
                header.NumNodes = reader.ReadUInt32();
                header.LexiconName = reader.ReadString();
                header.LexiconDate = reader.ReadString();
                header.NumWords = reader.ReadUInt32();
                header.NumReversePartWords = reader.ReadUInt32();

                // validations
                if (header.VersionNumber != 1U)
                {
                    string exceptionMessage = string.Format(
                        "Unxpected Dawg header version: {0}. Only Version 1 is supported!",
                        header.VersionNumber);
                    throw new InvalidDataException(exceptionMessage);
                }

                if (header.NumNodes < Dawg.MINIMUM_NUMBER_OF_NODES)
                {
                    string exceptionMessage = string.Format(
                        "Number of nodes specified in Dawg header: {0} is less than the minimum number of nodes: {1}!",
                        header.NumNodes, Dawg.MINIMUM_NUMBER_OF_NODES);
                    throw new InvalidDataException(exceptionMessage);
                }

                // read the nodes
                nodes = new DawgNode[header.NumNodes];
                for (uint idx = 0; idx < header.NumNodes; idx++)
                {
                    nodes[idx].ChildNodeIdx = reader.ReadUInt32();
                    nodes[idx].Letter = reader.ReadByte();
                    nodes[idx].IsTerminal = reader.ReadBoolean();
                    nodes[idx].IsLastChild = reader.ReadBoolean();
                }
            }

            // validate the counts
            uint numCountedWords = CountNumWords();
            if (header.NumWords != numCountedWords)
            {
                string exceptionMessage = string.Format(
                        "Number of words specified in Dawg header: {0} is less than the actual number of words: {1}!",
                        header.NumWords, numCountedWords);
                throw new InvalidDataException(exceptionMessage);
            }

            uint numCountedReversePartWords = CountNumReversePartWords();
            if (header.NumReversePartWords != numCountedReversePartWords)
            {
                string exceptionMessage = string.Format(
                        "Number of words specified in Dawg header: {0} is less than the actual number of words: {1}!",
                        header.NumReversePartWords, numCountedReversePartWords);
                throw new InvalidDataException(exceptionMessage);
            }
        }

        // *** PRIVATE ***
        // COUNT NUM REVERSE PART WORDS
        private uint CountNumReversePartWords()
        {
            return CountNumWordFragmentsForTree(nodes[Dawg.REVERSE_PARTWORD_NODE_ID].ChildNodeIdx);
        }

        // COUNT NUM WORDS
        private uint CountNumWords()
        {
            return CountNumWordFragmentsForTree(nodes[Dawg.FORWARD_WORD_NODE_ID].ChildNodeIdx);
        }

        // COUNT NUM WORD FRAGMENTS FOR TREE
        private uint CountNumWordFragmentsForTree(uint nodeIdx)
        {
            uint numWordFragments = 0;

            // recursion stop condition
            if (nodeIdx == 0 || nodeIdx > header.NumNodes)
                return numWordFragments;

            // is the current node terminal? If so, count!
            if (nodes[nodeIdx].IsTerminal)
                numWordFragments++;

            // traverse the first child tree
            numWordFragments += CountNumWordFragmentsForTree(nodes[nodeIdx].ChildNodeIdx);

            // traverse the next sibling if available
            if (nodes[nodeIdx].IsLastChild == false)
                numWordFragments += CountNumWordFragmentsForTree(nodeIdx + 1U);

            return numWordFragments;
        }

        // DATA
        private DawgNode[] nodes;
        private DawgHeader header;
    }
}
