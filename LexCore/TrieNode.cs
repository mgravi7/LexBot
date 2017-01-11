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
    public class TrieNode
    {
        // DATA
        public TrieNode FirstChild;
        public TrieNode NextSibling;        // refers to the younger sibling
        public TrieNode OriginalParent;
        public uint NodeIdx;            // assigned after compression
        public int HashCode;           // assigned after all the words are added

        public byte Letter;
        public bool IsTerminal;
        public byte NumChildren;        // assigned after all the words are added
        public byte NumYoungerSiblings; // assigned after all the words are added

        public bool IsFirstChild;       // assigned after all the words are added
        public bool IsCompressed;       // valid only for first child (current algorithm usage)
        public bool IsCounted;          // has a pass been made for counting descendents
        public bool IsNodeIdxAssigned;  // Has NodeIdx been assigned to this node?

        // CONSTRUCTOR
        /// <summary>
        /// Set default values for all the fields
        /// </summary>
        public TrieNode()
        {
            this.FirstChild = null;
            this.NextSibling = null;
            this.OriginalParent = null;
            this.NodeIdx = 0;
            this.HashCode = base.GetHashCode();

            this.Letter = 0;
            this.IsTerminal = false;
            this.NumChildren = 0;
            this.NumYoungerSiblings = 0;

            this.IsFirstChild = false;
            this.IsCompressed = false;
            this.IsCounted = false;
            this.IsNodeIdxAssigned = false;
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

            // first child and its tree
            if (this.FirstChild != null)
                this.FirstChild.ComputeHashCodeForTree();

            // younger sibling and its tree
            if (this.NextSibling != null)
                this.NextSibling.ComputeHashCodeForTree();
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
            if (this.Letter != node.Letter)
                return false;

            // terminal match?
            if (this.IsTerminal != node.IsTerminal)
                return false;

            // number of children?
            if (this.NumChildren != node.NumChildren)
                return false;

            // number of siblings?
            if (this.NumYoungerSiblings != node.NumYoungerSiblings)
                return false;

            // first child?
            if (this.FirstChild != null)
            {
                if (node.FirstChild != null)
                {
                    if (!this.FirstChild.Equals(node.FirstChild))
                        return false;
                }
                else
                    return false;   // node.FirstChild is null whereas this.FirstChild is NOT null
            }
            else
            {
                if (node.FirstChild != null)
                    return false;   // node.FirstChild is NOT null whereas this.FirstChild is null
            }

            // next sibling?
            if (this.NextSibling != null)
            {
                if (node.NextSibling != null)
                {
                    if (!this.NextSibling.Equals(node.NextSibling))
                        return false;
                }
                else
                    return false;   // node.NextSibling is null whereas this.NextSibling is NOT null
            }
            else
            {
                if (node.NextSibling != null)
                    return false;   // node.NextSibling is NOT null whereas this.NextSibling is null
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
            return this.HashCode;
        }

        // UPDATE IS FIRST CHILD FOR TREE
        /// <summary>
        /// Starting the call for root node will take care of every node
        /// in the hierarchy except for the root node itself (which
        /// should be set to true anyway)
        /// </summary>
        public void UpdateIsFirstChildForTree()
        {
            // first child and its tree
            if (this.FirstChild != null)
            {
                this.FirstChild.IsFirstChild = true;
                this.FirstChild.UpdateIsFirstChildForTree();
            }

            // next sibling and its tree
            if (this.NextSibling != null)
            {
                this.NextSibling.IsFirstChild = false;   // just by the fact it is younger sibling to current node
                this.NextSibling.UpdateIsFirstChildForTree();
            }
        }

        // UPDATE NUM CHILDREN AND SIBLING FOR TREE
        /// <summary>
        /// Updates the value for the node itself, children and
        /// younger siblings (recursive in nature). Starting the call
        /// for root node will take care of every node in the hierarchy.
        /// </summary>
        public void UpdateNumChildrenAndSiblingForTree()
        {
            // order of this is important as we take advantage
            // of sibling count of the first child to update our child count
            UpdateNumSiblings();    // for current node

            // first child and its tree
            if (this.FirstChild != null)
            {
                this.FirstChild.UpdateNumChildrenAndSiblingForTree();
                this.NumChildren = this.FirstChild.NumYoungerSiblings;
                this.NumChildren++;    // +1 for counting the first child
            }

            // next sibling and its tree
            if (this.NextSibling != null)
            {
                this.NextSibling.UpdateNumChildrenAndSiblingForTree();
            }
        }

        // *** PRIVATE ***

        // COMPUTE HASH CODE
        /// <summary>
        /// To be called after all the words are added, number of children and
        /// number of siblings set and IsFirstChild set. These are used in computing
        /// the hash code for the current Node.
        /// </summary>
        private void ComputeHashCode()
        {
            // Hash code is made up of the following bits
            // 31-26 (6 bits) - Unused
            // 25-18 (8 bits) - Number of siblings
            // 17-10 (8 bits) - Number of Children
            // 09-09 (1 bit ) - IsFirstChild
            // 08-08 (1 bit ) - IsTerminal
            // 07-00 (8 bits) - Letter value
            int bits25_18 = ((int)NumYoungerSiblings) << 18;
            int bits17_10 = ((int)NumChildren) << 10;
            int bit09_09 = (IsFirstChild ? 1 : 0) << 9;
            int bit08_08 = (IsTerminal ? 1 : 0) << 8;
            int bits07_00 = (int)Letter;

            this.HashCode = bits25_18 | bits17_10 | bit09_09 | bit08_08 | bits07_00;
        }

        // UPDATE NUM SIBLINGS
        /// <summary>
        /// Updates it for the current Node only
        /// </summary>
        private void UpdateNumSiblings()
        {
            // walk through the NextSibling value
            this.NumYoungerSiblings = 0;
            TrieNode curNode = NextSibling;
            while (curNode != null)
            {
                this.NumYoungerSiblings++;
                curNode = curNode.NextSibling;
            }
        }
    }
}
