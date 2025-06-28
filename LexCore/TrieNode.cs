using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeXpert.LexCore
{
    /// <summary>
    /// This is primarily a data class used for building the Trie
    /// </summary>
    public class TrieNode : IEquatable<TrieNode>
    {
        #region PUBLIC_DATA
        internal SortedList<byte, TrieNode> children;
        internal TrieNode                   originalParent;
        internal uint?                      nodeIdx;            // assigned after compression
        internal int                        hashCode;           // assigned after all the words are added

        internal byte letter;
        internal bool isTerminal;
        internal bool isFirstChild;       // assigned after all the words are added
        internal bool isCompressed;       // valid only for first child (current algorithm usage)
        
        const int CHILDREN_LIST_INITIAL_CAPACITY = 5;
        #endregion PUBLIC_DATA

        #region PUBLIC_METHODS
        // CONSTRUCTOR
        /// <summary>
        /// Set default values for all the fields
        /// </summary>
        public TrieNode()
        {
            this.children = new SortedList<byte, TrieNode>(CHILDREN_LIST_INITIAL_CAPACITY);
            this.originalParent = null;
            this.nodeIdx = null;
            this.hashCode = base.GetHashCode();

            this.letter = 0;
            this.isTerminal = false;
            this.isFirstChild = false;
            this.isCompressed = false;
        }

        // COMPUTE HASH CODE FOR TREE
        /// <summary>
        /// Computes the hash code for the node itself, children and
        /// younger siblings (recursive in nature). Starting the call
        /// for root node will take care of every node in the hierarchy.
        /// </summary>
        public void ComputeHashCodeForTree()
        {
            // need to do this for self first
            ComputeHashCode();

            // for all the children
            foreach (var item in this.children)
                item.Value.ComputeHashCodeForTree();    
        }

        // EQUALS
        public override bool Equals(object obj)
        {
            // check for null and proper type
            if (obj == null || this.GetType() != obj.GetType())
                return false;

            // are they the same instances?
            if (Object.ReferenceEquals(this, obj))
                return true;

            // letter match?
            TrieNode node = (TrieNode)obj;
            return this.Equals(node);
        }

        // EQUALS (IEQUATABLE)
        public bool Equals(TrieNode other)
        {
            if (this.letter != other.letter)
                return false;

            // terminal match?
            if (this.isTerminal != other.isTerminal)
                return false;

            // number of children?
            if (this.children.Count != other.children.Count)
                return false;

            // all the children?
            var otherItr = other.children.GetEnumerator();
            otherItr.MoveNext();
            foreach (var item in this.children)
            {
                var otherItem = otherItr.Current;
                if (!item.Value.Equals(otherItem.Value))
                    return false;
                otherItr.MoveNext();
            }

            // I guess they are equal!
            return true;
        }

        // GET HASH CODE
        /// <summary>
        /// When the node is contructed, this defaults to base's hash code.
        /// After all the words are added to Trie, calling <see cref="ComputeHashCodeForTree"/>
        /// will generate better hash code!
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.hashCode;
        }

        // UPDATE IS FIRST CHILD FOR TREE
        /// <summary>
        /// Starting the call for root node will take care of every node
        /// in the hierarchy except for the root node itself (which
        /// should be set to true anyway)
        /// </summary>
        public void UpdateIsFirstChildForTree()
        {
            int idx = 0;
            foreach (var item in this.children)
            {
                item.Value.UpdateIsFirstChildForTree();
                if (idx++ == 0)   // first child
                {
                    item.Value.isFirstChild = true;
                }
            }
        }
        #endregion PUBLIC_METHODS

        #region PRIVATE_METHODS
        // COMPUTE HASH CODE
        /// <summary>
        /// To be called after all the words are added, and IsFirstChild set.
        /// These are used in computing the hash code for the current Node.
        /// </summary>
        private void ComputeHashCode()
        {
            // Hash code is made up of the following bits
            // 31-16 (16 bits) - Sum of Child letter values (cheap way of hashing)
            // 15-10 ( 6 bits) - Number of Children
            // 09-09 ( 1 bit ) - IsFirstChild
            // 08-08 ( 1 bit ) - IsTerminal
            // 07-00 ( 8 bits) - Letter value

            // sum of child Letters
            int sumOfChildLetters = 0;
            foreach (var item in this.children)
                sumOfChildLetters += (int)item.Value.letter;

            int bits31_16 = sumOfChildLetters << 16;
            int bits15_10 = ((int)this.children.Count) << 10;
            int bit09_09 = (this.isFirstChild ? 1 : 0) << 9;
            int bit08_08 = (this.isTerminal ? 1 : 0) << 8;
            int bits07_00 = (int)this.letter;

            this.hashCode = bits31_16 | bits15_10 | bit09_09 | bit08_08 | bits07_00;
        }
        #endregion PRIVATE_METHODS
    }
}
