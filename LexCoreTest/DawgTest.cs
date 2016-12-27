using System;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeXpert.LexCore;

namespace LexCoreTest
{
    [TestClass]
    public class DawgCreatorTest
    {
        public DawgCreatorTest()
        {
            nodes = new DawgNode[numNodes];
            int idx = 0;

            nodes[idx++] = new DawgNode(1, Dawg.DEFAULT_LETTER, false, true);               // node 0

            nodes[idx++] = new DawgNode(3, Dawg.FORWARD_WORD_DAWG_SYMBOL, false, false);    // node 1
            nodes[idx++] = new DawgNode(7, Dawg.REVERSE_PARTWORD_DAWG_SYMBOL, false, true); // node 2

            nodes[idx++] = new DawgNode(4, (byte) 'B', false, true);    // node 3
            nodes[idx++] = new DawgNode(5, (byte) 'A', false, true);    // node 4
            nodes[idx++] = new DawgNode(6, (byte) 'T', false, true);    // node 5
            nodes[idx++] = new DawgNode(0, (byte) 'S', true, true);     // node 6

            nodes[idx++] = new DawgNode(11, (byte) 'A', false, false);  // node 7
            nodes[idx++] = new DawgNode(0, (byte) 'B', true, false);    // node 8
            nodes[idx++] = new DawgNode(10, (byte) 'S', false, false);  // node 9
            nodes[idx++] = new DawgNode(12, (byte) 'T', false, true);   // node 10

            nodes[idx++] = new DawgNode(0, (byte) 'B', true, true);     // node 11

            nodes[idx++] = new DawgNode(11, (byte) 'A', false, true);   // node 12
        }

        [TestMethod]
        public void DawgCreator_Construct()
        {
            DawgCreator dawgCreator = new DawgCreator(lexiconName, numNodes, numWords, numReversePartWords);
        }

        [TestMethod]
        public void DawgCreator_AddNodes()
        {
            // create
            DawgCreator dawgCreator = new DawgCreator(lexiconName, numNodes, numWords, numReversePartWords);

            // add nodes
            for (int idx = 0; idx < numNodes; idx++)
            {
                dawgCreator.AddNode(nodes[idx]);
            }
            Assert.AreEqual<uint>(numNodes, dawgCreator.NumAddedNodes);
        }

        [TestMethod]
        public void DawgCreator_SaveDawg()
        {
            // create
            DawgCreator dawgCreator = new DawgCreator(lexiconName, numNodes, numWords, numReversePartWords);

            // add nodes
            for (int idx = 0; idx < numNodes; idx++)
            {
                dawgCreator.AddNode(nodes[idx]);
            }

            // save
            dawgCreator.SaveDawg(lexiconFileName);
        }

        [TestMethod]
        public void DawgCreator_SaveDawg_ReadDawg()
        {
            // create
            DawgCreator dawgCreator = new DawgCreator(lexiconName, numNodes, numWords, numReversePartWords);

            // add nodes
            for (int idx = 0; idx < numNodes; idx++)
            {
                dawgCreator.AddNode(nodes[idx]);
            }

            // save
            dawgCreator.SaveDawg(lexiconFileName);

            // read it back:-)
            Dawg dawg = new Dawg(lexiconFileName);

            // let us hope for no exceptions!
        }

        [TestMethod]
        public void DawgCreator_SaveDawg_WithLessNodes()
        {
            // create
            uint fakeNumNodes = numNodes + 1U;
            DawgCreator dawgCreator = new DawgCreator(lexiconName, fakeNumNodes, numWords, numReversePartWords);

            // add nodes
            for (int idx = 0; idx < numNodes; idx++)
            {
                dawgCreator.AddNode(nodes[idx]);
            }

            // save
            try
            {
                dawgCreator.SaveDawg(lexiconFileName);
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
        const uint numReversePartWords = 4;
        DawgNode[] nodes;
    }
}
