using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeXpert.LexCore
{
    public class DawgFileMaker
    {
        // *** PROPERTIES ***

        /// <summary>
        /// Count of nodes that have been added so far
        /// </summary>
        public uint NumAddedNodes
        {
            get { return this.numAddedNodes; }
        }

        // *** PUBLIC ***

        // CONSTRUCTOR
        /// <summary>
        /// Initializes DawgCreator, the only way of initializing
        /// </summary>
        /// <param name="lexiconName">Name of the lexicon</param>
        /// <param name="numNodes">Number of nodes (after compression typically)</param>
        /// <param name="numWords">Number of full words in the Dawg</param>
        public DawgFileMaker(string lexiconName, uint numNodes, uint numWords, DawgType dawgType,
            uint numReverseWordlets, uint numForwardWordlets)
        {
            // initialization
            this.header = new DawgHeader();
            UpdateHeader(lexiconName, numNodes, numWords, dawgType, numReverseWordlets, numForwardWordlets);
            this.nodes = new DawgNode[numNodes];
            this.numAddedNodes = 0;
        }

        // ADD NODE
        /// <summary>
        /// Adds the given node to DAWG. Please note the nodes need to be added sequentially!
        /// </summary>
        /// <param name="dawgNode">Node to be added</param>
        public void AddNode(DawgNode dawgNode)
        {
            // validations
            Debug.Assert(this.numAddedNodes < header.NumNodes);

            nodes[this.numAddedNodes++] = dawgNode;
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
            if (this.numAddedNodes != header.NumNodes)
            {
                string exceptionMessage = string.Format(
                    "Expected number of nodes: {0}. However, only {1} nodes have been added so far!",
                    this.header.NumNodes, this.numAddedNodes);
                throw new InvalidOperationException(exceptionMessage);
            }

            // call the version specific save function
            Debug.Assert(header.VersionNumber == 1U);   // that is all is supported now!
            SaveDawgInV1Format(fileName);
        }

        // *** PRIVATE ***

        // CREATE HEADER
        private void UpdateHeader(string lexiconName, uint numNodes, uint numWords,
            DawgType dawgType, uint numReverseWordlets, uint numForwardWordlets)
        {
            // fill all the fields
            header.VersionNumber = DawgHeader.CURRENT_HEADER_VERSION;
            header.LexiconDate = DateTime.Now.ToString("dd MMMM yyyy");
            header.LexiconName = lexiconName;
            header.NumNodes = numNodes;
            header.NumWords = numWords;
            header.DawgType = dawgType;
            header.NumReverseWordlets = numReverseWordlets;
            header.NumForwardWordlets = numForwardWordlets;
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
                writer.Write(header.DawgType.ToString());
                writer.Write(header.NumReverseWordlets);
                writer.Write(header.NumForwardWordlets);

                // write the nodes
                for (uint idx = 0; idx < this.numAddedNodes; idx++)
                {
                    writer.Write(nodes[idx].ChildNodeIdx);
                    writer.Write(nodes[idx].Letter);
                    writer.Write(nodes[idx].IsTerminal);
                    writer.Write(nodes[idx].IsLastChild);
                }
            }
        }

        // DATA
        private DawgHeader header;
        private DawgNode[] nodes;
        private uint numAddedNodes;
    }
}
