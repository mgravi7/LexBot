using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeXpert.LexCore
{
    /// <summary>
    /// Base class for constructing Trie structure, compressing it and saving it as DAWG file.
    /// This class creates a Dawg of type <see cref="DawgType"/> "Forward".
    /// Initially, Trie is in ADDING_WORDS state when words are added. When Compress is
    /// called, it is in COMPRESSING state followed by COMPRESSED when finished.
    /// 
    /// Call <see cref="AddWordsAsync(string)"/> followed by
    /// <see cref="CompressAsync"/> followed by
    /// <see cref="SaveTrieAsDawgAsync(string, Dawg.FileFormat)"/> to complete the process.
    /// Calling <see cref="ValidateDawgAsync(string, string)"/> is recommended though
    /// it is not necessary.
    /// </summary>
    public class Trie
    {
        #region PUBLIC
        public enum TrieState {ADDING_WORDS, COMPRESSING, COMPRESSED};

        // CONSTRUCTOR
        /// <summary>
        /// Initializes diagnostics, special nodes and sets the Trie state.
        /// </summary>
        public Trie()
        {
            // initialize the special nodes
            this.rootNode = new TrieNode();
            this.diagnostics.NumNodesBeforeCompression++;

            bool childExists;
            this.forwardWordNode = AddChildNode(this.rootNode, Dawg.FORWARD_WORD_DAWG_SYMBOL, false, out childExists);
            Debug.Assert(childExists == false);
        }

        /// <summary>
        /// Adds words to Trie from a given lexicon file.
        /// Lines starting with '#' are ignored.
        /// One word per line and all letters will be converted to UPPERCASE.
        /// It is recommended to use the async version: <see cref="AddWordsAsync(string)"/>.
        /// </summary>
        /// <param name="wordFileName">Path of the file where lexicon words are present</param>
        /// <returns>Number of words read</returns>
        public uint AddWords(string wordFileName)
        {
            uint numWords = 0;
            return numWords;
        }

        /// <summary>
        /// Adds words to Trie from a given lexicon file.
        /// Lines starting with '#' are ignored.
        /// One word per line and all letters will be converted to UPPERCASE.
        /// </summary>
        /// <param name="wordFileName">Path of the file where lexicon words are present</param>
        /// <returns>Number of words read</returns>
        public async Task<uint> AddWordsAsync(string wordFileName)
        {
            return await Task.Run(() =>
            {
                return AddWords(wordFileName);
            });
        }

        /// <summary>
        /// Compresses the Trie.
        /// All the words must have been added already.
        /// It is recommended to use the async version: <see cref="CompressAsync()"/>.
        /// </summary>
        /// <returns>Percents of nodes reduced through compression</returns>
        public uint Compress()
        {
            // diagnostics
            this.diagnostics.CompressStartTime = DateTime.UtcNow;

            uint percentNodeReduction = 0;

            // return
            this.diagnostics.CompressEndTime = DateTime.UtcNow;
            return percentNodeReduction;
        }

        /// <summary>
        /// Compresses the Trie.
        /// All the words must have been added already.
        /// </summary>
        /// <returns>Percents of nodes reduced through compression</returns>
        public async Task<uint> CompressAsync()
        {
            return await Task.Run(() =>
            {
                return Compress();
            });
        }

        /// <summary>
        /// Saves the Trie as Dawg to a file.
        /// All the words must have been added already.
        /// It is recommended to use the async version: <see cref="SaveTrieAsDawgAsync(string, Dawg.FileFormat)"/>.
        /// </summary>
        /// <param name="dawgFileName">Path of the Dawg file to be created/overwritten</param>
        /// <param name="fileFormat">Dawg file format</param>
        /// <returns>Number of nodes saved</returns>
        public uint SaveTrieAsDawg(string dawgFileName, Dawg.FileFormat fileFormat)
        {
            // diagnostics
            this.diagnostics.SaveDawgStartTime = DateTime.UtcNow;

            uint numNodes = 0;

            // return
            this.diagnostics.SaveDawgEndTime = DateTime.UtcNow;
            return numNodes;
        }

        /// <summary>
        /// Saves the Trie as Dawg to a file.
        /// All the words must have been added already.
        /// </summary>
        /// <param name="dawgFileName">Path of the Dawg file to be created/overwritten</param>
        /// <param name="fileFormat">Dawg file format</param>
        /// <returns>Number of nodes saved</returns>
        public async Task<uint> SaveTrieAsDawgAsync(string dawgFileName, Dawg.FileFormat fileFormat)
        {
            return await Task.Run(() =>
            {
                return SaveTrieAsDawg(dawgFileName, fileFormat);
            });
        }

        /// <summary>
        /// Reads each word from the wordFileName and ensures it is
        /// represented in the Dawg properly. Also, it makes sure there
        /// no unnecessary representations within the Dawg!
        /// It is recommended to use the async version: <see cref="ValidateDawgAsync(string, string)"/>.
        /// </summary>
        /// <param name="dawgFileName">Path of the Dawg file</param>
        /// <param name="wordFileName">Path of the file where lexicon words are present</param>
        /// <returns>true if the validation is successful, false otherwise</returns>
        public bool ValidateDawg(string dawgFileName, string wordFileName)
        {
            // diagnostics
            this.diagnostics.ValidateDawgStartTime = DateTime.UtcNow;
            bool isValid = false;

            // return
            this.diagnostics.ValidateDawgEndTime = DateTime.UtcNow;
            return isValid;
        }

        /// <summary>
        /// Reads each word from the wordFileName and ensures it is
        /// represented in the Dawg properly. Also, it makes sure there
        /// no unnecessary representations within the Dawg!
        /// </summary>
        /// <param name="dawgFileName">Path of the Dawg file</param>
        /// <param name="wordFileName">Path of the file where lexicon words are present</param>
        /// <returns>true if the validation is successful, false otherwise</returns>
        public async Task<bool> ValidateDawgAsync(string dawgFileName, string wordFileName)
        {
            return await Task.Run(() =>
            {
                return ValidateDawg(dawgFileName, wordFileName);
            });
        }
        #endregion PUBLIC

        #region PROTECTED
        protected TrieNode RootNode
        {
            get { return this.rootNode; }
        }

        /// <summary>
        /// Adds the word to the Trie
        /// </summary>
        /// <param name="word">word to be added</param>
        protected virtual void AddWord(string word)
        {
            // diagnostics
            this.diagnostics.NumAttemptedWords++;

            // add just the letters making up a given word
        }
        
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
        protected TrieNode AddChildNode(TrieNode parentNode, byte letter, bool isTerminal, out bool childAlreadyExists)
        {
            Debug.Assert(this.state == TrieState.ADDING_WORDS);
            Debug.Assert(parentNode != null);

            // does the child exist?
            childAlreadyExists = false;
            TrieNode childNode;
            if (parentNode.children.TryGetValue(letter, out childNode))
            {
                childAlreadyExists = true;
                if (childNode.isTerminal == false)
                    childNode.isTerminal = isTerminal;
                return childNode;
            }

            // create a new node and initialize it
            childNode = new TrieNode();
            this.diagnostics.NumNodesBeforeCompression++;

            childNode.originalParent = parentNode;
            childNode.letter = letter;
            childNode.isTerminal = isTerminal;
            
            return childNode;
        }
        #endregion PROTECTED


        #region DATA
        private TrieState       state = TrieState.ADDING_WORDS;
        private TrieNode        rootNode;
        private TrieNode        forwardWordNode;      // forward words (no wordlets)
        
        protected TrieDiagnostics diagnostics = new TrieDiagnostics();
        #endregion DATA
    }
}
