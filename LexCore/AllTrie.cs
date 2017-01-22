using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeXpert.LexCore
{
    class AllTrie : ReverseTrie
    {
        public AllTrie() : base()
        {
            bool childExists = false;
            this.forwardWordletNode = AddChildNode(this.RootNode, Dawg.FORWARD_WORDLET_DAWG_SYMBOL, false, out childExists);
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

            // TODO add forward wordlets only
        }
        #endregion

        private TrieNode forwardWordletNode;   // forward wordlets only
    }
}
