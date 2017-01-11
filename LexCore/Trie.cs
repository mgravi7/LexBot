using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeXpert.LexCore
{
    /// <summary>
    /// Class for constructing Trie structure, compressing it and saving it as DAWG file.
    /// Initially, Trie is in ADDING_WORDS state when words are added. When Compress is
    /// called, it is in COMPRESSING state followed by COMPRESSED when finished.
    /// </summary>
    public class Trie
    {
        public enum TrieState {ADDING_WORDS, COMPRESSING, COMPRESSED};

        // CONSTRUCTOR
        /// <summary>
        /// Initializes diagnostics, special nodes and sets the Trie state.
        /// </summary>
        public Trie()
        {
            // diagnostics initialization
            this.diagnostics = new TrieDiagnostics();

            // state
            this.state = TrieState.ADDING_WORDS;
            
            // initialize the special nodes
            this.rootNode = new TrieNode();
            this.diagnostics.NumNodesBeforeCompression++;

            bool childExists;
            this.forwardWordNode = AddChildNode(this.rootNode, Dawg.FORWARD_WORD_DAWG_SYMBOL, false, out childExists);
            Debug.Assert(childExists == false);

            this.reversePartWordNode = AddChildNode(this.rootNode, Dawg.REVERSE_WORDLET_DAWG_SYMBOL, false, out childExists);
            Debug.Assert(childExists == false);
        }

        // Interface IDawgFileMaker
        /// <summary>
        /// Constructs Dawg from a given lexicon file.
        /// Lines starting with '#' are ignored.
        /// One word per line and all letters will be converted to UPPERCASE.
        /// </summary>
        /// <param name="wordFileName">Path of the file where lexicon words are present</param>
        public async Task ConstructDawg(string wordFileName, DawgType dawgType)
        {
            throw new NotImplementedException();
        }

        public async Task SaveDawg(string dawgFileName, Dawg.FileFormat fileFormat)
        {
            throw new NotImplementedException();
        }

        // *** PRIVATE ***

        // ADD CHILD NODE
        /// <summary>
        /// Adds a new child node if needed. The addition/insertion takes place by alphabetical order.
        /// If the child node already exists and its isTerminal is true, the incoming isTerminal value is ignored.
        /// </summary>
        /// <param name="parentNode">Parent node where the child will be inserted</param>
        /// <param name="letter">Letter value of the child</param>
        /// <param name="isTerminal">Is this a terminal node (end of word or end of word fragment)</param>
        /// <param name="childAlreadyExists">Set to true if the child already exists, false otherwise</param>
        /// <returns>Newly created node or existing node if the child already exists</returns>
        private TrieNode AddChildNode(TrieNode parentNode, byte letter, bool isTerminal, out bool childAlreadyExists)
        {
            Debug.Assert(this.state == TrieState.ADDING_WORDS);
            Debug.Assert(parentNode != null);

            // initialize
            childAlreadyExists = false;

            // need to find the right place for insertion
            TrieNode curChild = parentNode.FirstChild;
            TrieNode prevChild = null;

            while (curChild != null)
            {
                // is there a matching node?
                if (letter == curChild.Letter)
                {
                    // isTerminal = true always wins for setting
                    if (isTerminal == true && curChild.IsTerminal == false)
                        curChild.IsTerminal = true;

                    childAlreadyExists = true;
                    return curChild;
                }

                // should the new child be inserted before the current child?
                if (letter < curChild.Letter)
                    break;

                // next child
                prevChild = curChild;
                curChild = curChild.NextSibling;
            }

            // create a new node
            TrieNode newChild = new TrieNode();
            this.diagnostics.NumNodesBeforeCompression++;

            // initialize the node
            newChild.OriginalParent = parentNode;
            newChild.Letter = letter;
            newChild.IsTerminal = isTerminal;
            newChild.NextSibling = curChild;    // this works regardless of if curChild is null or not

            if (prevChild == null)
            {
                // this is the first child
                newChild.IsFirstChild = true;

                // if the parent already had a first child, its property should change!
                if (parentNode.FirstChild != null)
                    parentNode.FirstChild.IsFirstChild = false;

                // set the newChild as FirstChild for parent
                parentNode.FirstChild = newChild;
            }
            else
            {
                // this is not the first child
                // link it to the previous child
                prevChild.NextSibling = newChild;
            }

            return newChild;
        }

       

        // DATA
        TrieState       state;
        TrieNode        rootNode;
        TrieNode        forwardWordNode;       // forward words (no partials)
        TrieNode        reversePartWordNode;   // reverse partials and reverse words
        TrieDiagnostics diagnostics;
    }
}
