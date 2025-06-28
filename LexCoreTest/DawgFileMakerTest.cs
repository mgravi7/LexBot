using System;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeXpert.LexCore;

namespace LexCoreTest
{
    [TestClass]
    public class DawgFileMakerTest
    {
        public DawgFileMakerTest()
        {
            nodes = new DawgNode[numNodes];
            int idx = 0;

            nodes[idx++] = new DawgNode(1, Dawg.DEFAULT_LETTER, false, true);               // node 0

            nodes[idx++] = new DawgNode(3, Dawg.FORWARD_WORD_DAWG_SYMBOL, false, false);    // node 1
            nodes[idx++] = new DawgNode(7, Dawg.REVERSE_WORDLET_DAWG_SYMBOL, false, true); // node 2

            nodes[idx++] = new DawgNode(4, (byte)'B', false, true);    // node 3
            nodes[idx++] = new DawgNode(5, (byte)'A', false, true);    // node 4
            nodes[idx++] = new DawgNode(6, (byte)'T', false, true);    // node 5
            nodes[idx++] = new DawgNode(0, (byte)'S', true, true);     // node 6

            nodes[idx++] = new DawgNode(11, (byte)'A', false, false);  // node 7
            nodes[idx++] = new DawgNode(0, (byte)'B', true, false);    // node 8
            nodes[idx++] = new DawgNode(10, (byte)'S', false, false);  // node 9
            nodes[idx++] = new DawgNode(12, (byte)'T', false, true);   // node 10

            nodes[idx++] = new DawgNode(0, (byte)'B', true, true);     // node 11

            nodes[idx++] = new DawgNode(11, (byte)'A', false, true);   // node 12
        }

        [TestMethod]
        public void DawgFileMaker_Construct()
        {
            DawgFileMaker dawgFileMaker = new DawgFileMaker(lexiconName, numNodes, numWords, dawgType, numReverseWordlets, numForwardWordlets);
        }

        [TestMethod]
        public void DawgFileMaker_AddNodes()
        {
            // create
            DawgFileMaker dawgFileMaker = new DawgFileMaker(lexiconName, numNodes, numWords, dawgType, numReverseWordlets, numForwardWordlets);

            // add nodes
            for (int idx = 0; idx < numNodes; idx++)
            {
                dawgFileMaker.AddNode(nodes[idx]);
            }
            Assert.AreEqual<uint>(numNodes, dawgFileMaker.NumAddedNodes);
        }

        [TestMethod]
        public void DawgFileMaker_SaveDawg()
        {
            // create
            DawgFileMaker dawgFileMaker = new DawgFileMaker(lexiconName, numNodes, numWords, dawgType, numReverseWordlets, numForwardWordlets);

            // add nodes
            for (int idx = 0; idx < numNodes; idx++)
            {
                dawgFileMaker.AddNode(nodes[idx]);
            }

            // save
            dawgFileMaker.SaveDawg(lexiconFileName);
        }

        [TestMethod]
        public void DawgFileMaker_SaveDawg_ReadDawg()
        {
            // create
            DawgFileMaker dawgFileMaker = new DawgFileMaker(lexiconName, numNodes, numWords, dawgType, numReverseWordlets, numForwardWordlets);

            // add nodes
            for (int idx = 0; idx < numNodes; idx++)
            {
                dawgFileMaker.AddNode(nodes[idx]);
            }

            // save
            dawgFileMaker.SaveDawg(lexiconFileName);

            // read it back:-)
            Dawg dawg = new Dawg(lexiconFileName);

            // let us hope for no exceptions!
        }

        [TestMethod]
        public void DawgFileMaker_SaveDawg_WithLessNodes()
        {
            // create
            uint fakeNumNodes = numNodes + 1U;
            DawgFileMaker dawgFileMaker = new DawgFileMaker(lexiconName, fakeNumNodes, numWords, dawgType, numReverseWordlets, numForwardWordlets);

            // add nodes
            for (int idx = 0; idx < numNodes; idx++)
            {
                dawgFileMaker.AddNode(nodes[idx]);
            }

            // save
            try
            {
                dawgFileMaker.SaveDawg(lexiconFileName);
            }
            catch (InvalidOperationException excp)
            {
                StringAssert.Contains(excp.Message, fakeNumNodes.ToString());
                StringAssert.Contains(excp.Message, numNodes.ToString());
                return;
            }

            Assert.Fail("No exception was thrown");
        }

        // Data
        const string lexiconName = "Test Lexicon";
        const string lexiconFileName = "TestLexicon.lxdg";
        const uint numWords = 1;    // word is "BATS"
        const uint numNodes = 13;
        const uint numReverseWordlets = 4;
        const uint numForwardWordlets = 0;
        DawgType dawgType = DawgType.Reverse;
        DawgNode[] nodes;
    }
}
