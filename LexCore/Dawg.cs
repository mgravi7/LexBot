using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeXpert.LexCore
{
    

    public enum DawgType { ForwardWord, ForwardWordAndReverseWordlet, All };

    public class Dawg
    {
        // *** PUBLIC DECLARATIONS ***
        public const byte FORWARD_WORD_DAWG_SYMBOL = 42;        // '*'
        public const byte REVERSE_WORDLET_DAWG_SYMBOL = 60;    // '<'
        public const byte FORWARD_WORDLET_DAWG_SYMBOL = 62;    // '>'

        public const byte START_LETTER = 65;    // 'A'
        public const byte END_LETTER = 90;      // 'Z'
        public const byte DEFAULT_LETTER = 32;  // ' '

        public enum FileFormat { LegacyC, CSharp };

        // *** PRIVATE DECLARATIONS ***
        private const uint ROOT_NODE_ID = 0;
        private const uint FORWARD_WORD_NODE_ID = 1;
        private const uint REVERSE_WORDLET_NODE_ID = 2;
        private const uint FORWARD_WORDLET_NODE_ID = 3;
        private uint minNumberOfNodes = 3; // Root, forward, and reverse wordlet

        // *** PUBLIC ***
        // CONSTRUCTOR
        public Dawg(string fileName)
        {
            this.header = new DawgHeader();

            // open the file
            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                // read the header
                header.VersionNumber = reader.ReadUInt32();
                header.NumNodes = reader.ReadUInt32();
                header.LexiconName = reader.ReadString();
                header.LexiconDate = reader.ReadString();
                header.NumWords = reader.ReadUInt32();
                header.DawgType = (DawgType)Enum.Parse(typeof(DawgType), reader.ReadString());
                header.NumReverseWordlets = reader.ReadUInt32();
                header.NumForwardWordlets = reader.ReadUInt32();

                // validations
                if (header.VersionNumber != 1U)
                {
                    string exceptionMessage = string.Format(
                        "Unxpected Dawg header version: {0}. Only Version 1 is supported!",
                        header.VersionNumber);
                    throw new InvalidDataException(exceptionMessage);
                }

                if (header.NumNodes < this.minNumberOfNodes)
                {
                    string exceptionMessage = string.Format(
                        "Number of nodes specified in Dawg header: {0} is less than the minimum number of nodes: {1}!",
                        header.NumNodes, this.minNumberOfNodes);
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

            uint numCountedReverseWordlets = CountNumReverseWordlets();
            if (header.NumReverseWordlets != numCountedReverseWordlets)
            {
                string exceptionMessage = string.Format(
                        "Number of reverse wordlets specified in Dawg header: {0} is less than the actual number of reverse wordlets: {1}!",
                        header.NumReverseWordlets, numCountedReverseWordlets);
                throw new InvalidDataException(exceptionMessage);
            }
        }

        // *** PRIVATE ***
        // COUNT NUM REVERSE PART WORDS
        private uint CountNumReverseWordlets()
        {
            return CountNumWordletsForTree(nodes[Dawg.REVERSE_WORDLET_NODE_ID].ChildNodeIdx);
        }

        // COUNT NUM WORDS
        private uint CountNumWords()
        {
            return CountNumWordletsForTree(nodes[Dawg.FORWARD_WORD_NODE_ID].ChildNodeIdx);
        }

        // COUNT NUM WORDLETS FOR TREE
        private uint CountNumWordletsForTree(uint nodeIdx)
        {
            uint numWordlets = 0;

            // recursion stop condition
            if (nodeIdx == 0 || nodeIdx > header.NumNodes)
                return numWordlets;

            // is the current node terminal? If so, count!
            if (nodes[nodeIdx].IsTerminal)
                numWordlets++;

            // traverse the first child tree
            numWordlets += CountNumWordletsForTree(nodes[nodeIdx].ChildNodeIdx);

            // traverse the next sibling if available
            if (nodes[nodeIdx].IsLastChild == false)
                numWordlets += CountNumWordletsForTree(nodeIdx + 1U);

            return numWordlets;
        }

        // DATA
        private DawgNode[] nodes;
        private DawgHeader header;
    }
}
