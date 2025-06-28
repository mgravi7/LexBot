using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeXpert.LexCore
{
    public class ReverseTrie : Trie
    {
        public ReverseTrie() : base()
        {
            bool childExists = false;
            this.reverseWordletNode = AddChildNode(this.RootNode, Dawg.REVERSE_WORDLET_DAWG_SYMBOL, false, out childExists);
            Debug.Assert(childExists == false);
        }

        #region PROTECTED
        /// <summary>
        /// Adds the word to the Trie
        /// </summary>
        /// <param name="word">word to be added</param>
        protected override void AddWord(string word)
        {
            // call the base first!
            base.AddWord(word);

            // TODO add reverse wordlets and reverse word
        }
        #endregion

        private TrieNode reverseWordletNode;   // reverse wordlets and reverse words
    }
}
