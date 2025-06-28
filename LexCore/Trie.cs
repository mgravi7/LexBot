namespace LeXpert.LexCore
{
	using System;
	using System.Collections;
	using System.IO;

	/// <summary>
	/// Summary description for Trie.
	/// </summary>
	public class Trie
	{
		// CONSTRUCTOR
		public Trie()
		{
			this.rootNode = new Node();
			this.nodeHashTable = new Hashtable();
		}

		public void Compress()
		{
			// generate hash codes for the Trie
			Std.TraceLine("Starting hashing ...");
			int numDescendents;
			int numSiblingDescendents;
			int maxDescendentDepth;
			int maxSiblingDescendentDepth;
			this.rootNode.ComputeHashCodeForTree(out numDescendents, out numSiblingDescendents,
				out maxDescendentDepth, out maxSiblingDescendentDepth);
			Std.TraceLine("Hashing complete.");

			// compress from the root node
			CompressNode(this.rootNode);
			//this.firstNodes = new ArrayList();
			//CompressNodeNoHashTable(this.rootNode);
			Std.TraceLine("Compression complete.");

			// number the nodes
			this.rootNode.NodeID = 1;
			this.numNodes = AssignNodeID(this.rootNode.FirstChild, 2) - 1;
			TraceSummaryInfo();
			AnalyzeHashTableUsage();
		}

		/// <summary>
		/// Reads values from a binary reader. Words need to be separated by
		/// carriage return followed by line feed (CFLF/in Hex it is 0D0A).
		/// Maximum word length is specified in the Std class. If no CR-LF combination
		/// is found before hitting the maximum length, it is assumed that the word is
		/// of maximum length and added to the TRIE. Scanning will start with the next
		/// letter as the first letter of the next word.
		/// </summary>
		/// <param name="binaryReader">
		/// Reading starts from the current
		/// position in the stream and continues till the end of stream is reached. The
		/// reader is not closed. It is up to the caller to close it.
		/// </param>
		public void Read(BinaryReader binaryReader)
		{
			Std.TraceLine("Beginning Read ...");

			// read all the words and add them to the trie
			byte[]	letters = new byte[Std.MaxWordLength];
			int		numLetters = ReadNextWord(binaryReader, letters);

			while (numLetters > 0)
			{
				AddWord(letters, numLetters);
				numLetters = ReadNextWord(binaryReader, letters);
			}

			Std.TraceLine("Finished reading");
			TraceSummaryInfo();
		}

		/// <summary>
		/// Adds the values in the byte array to the trie.
		/// </summary>
		/// <param name="letters"></param>
		/// <param name="numLetters"></param>
		public void AddWord(byte[] letters, int numLetters)
		{
			int		idx;
			Node	parentNode = this.rootNode;
			Node	curNode = null;
			bool	isWord = false;

			// book keeping
			this.numWords++;
			this.totalNumLetters += numLetters;

			// add the letters as needed
			for (idx = 0; idx < numLetters; idx++)
			{
				if (idx == numLetters - 1)
					isWord = true;

				curNode = FindChildNode(parentNode, letters[idx]);
				if (curNode == null)
				{
					// add the child node
					curNode = AddChildNode(parentNode, letters[idx], isWord);
				}
				parentNode = curNode;
			}
		}

		// IMPLEMENTATION

		private Node AddChildNode(Node parentNode, byte letter, bool isWord)
		{
			// identify where the child node needs to be inserted
			Node curChild = parentNode.FirstChild;
			Node prevSibling = null;
			while (curChild != null)
			{
				prevSibling = curChild;
				curChild = curChild.SiblingNode;
			}

			// create a new node
			curChild = new Node();
			this.numNodes++;
			if (prevSibling == null)
			{
				// this is the first child
				curChild.IsFirstChild = true;
				parentNode.FirstChild = curChild;
				this.numFirstChildNodes++;
			}
			else
			{
				// this is not a first child
				curChild.IsFirstChild = false;
				prevSibling.SiblingNode = curChild;
			}
			curChild.IsWord = isWord;
			curChild.Letter = letter;
			curChild.ParentNode = parentNode;

			return curChild;									// RETURN
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		/// <param name="id"></param>
		/// <returns>
		/// Next available id
		/// </returns>
		private int AssignNodeID(Node node, int id)
		{
			// ids need to be assigned to all the siblings
			// contiguously. Only the first child can assign
			// ids for sibling
			Node curNode;
			if (node.IsFirstChild && node.NodeID == 0)
			{
				curNode = node;
				while (curNode != null)
				{
					curNode.NodeID = id;
					id++;
					curNode = curNode.SiblingNode;
				}
			}

			// children get their id next
			curNode = node;
			while (curNode != null)
			{
				if (curNode.FirstChild != null)
					id = AssignNodeID(curNode.FirstChild, id);
				curNode = curNode.SiblingNode;
			}

			return id;											// RETURN
		}

		private void CompressNode(Node node)
		{
			// has this node been compressed already?
			if (node.IsCompressed)
				return;											// RETURN

			// skip if this is not the first child
			if (node.IsFirstChild && CompressNodeOnly(node))
				return;											// RETURN

			// compress the children
			if (node.FirstChild != null)
				CompressNode(node.FirstChild);

			// compress the sibling
			if (node.SiblingNode != null)
				CompressNode(node.SiblingNode);
			node.IsCompressed = true;	// even for non first child nodes
		}

		private bool CompressNodeOnly(Node node)
		{
			// Look up in the hash table to see if there are other nodes for that key.
			// If so, see if they are equal. If so, we can make the parent node point to
			// the existing node in the hash table. If not, add the node to the hash table.
			// is the hash code in the hash table?
			if (nodeHashTable.Contains(node.GetHashCode()))
			{
				// other nodes matching the same hash code are already there
				ArrayList nodeList = (ArrayList) nodeHashTable[node.GetHashCode()];

				// does this node equal any other node?
				foreach (object obj in nodeList)
				{
					if (node.Equals(obj))
					{
						// found the match! Modify the parent pointer and get out
						node.ParentNode.FirstChild = (Node) obj;
						return true;							// RETURN
					}
				}

				// did not find a match. Add the node to array list
				nodeList.Add(node);
			}
			else
			{
				// make an entry in the hash table
				ArrayList nodeList = new ArrayList();
				nodeList.Add(node);
				nodeHashTable.Add(node.GetHashCode(), nodeList);
			}
			node.IsCompressed = true;
			return false;										// RETURN
		}

		private void CompressNodeNoHashTable(Node node)
		{
			// has this node been compressed already?
			if (node.IsCompressed)
				return;											// RETURN

			// skip if this is not the first child
			if (node.IsFirstChild && CompressNodeOnlyNoHashTable(node))
				return;											// RETURN

			// compress the children
			if (node.FirstChild != null)
				CompressNodeNoHashTable(node.FirstChild);

			// compress the sibling
			if (node.SiblingNode != null)
				CompressNodeNoHashTable(node.SiblingNode);
			node.IsCompressed = true;	// even for non first child nodes
		}

		private bool CompressNodeOnlyNoHashTable(Node node)
		{
			// march through existing first nodes to see if there is a match
			// if not, add it
			for (int idx = 0; idx < this.firstNodes.Count; idx++)
			{
				Node curNode = (Node) this.firstNodes[idx];
				if (node.GetHashCode() == curNode.GetHashCode() &&
					node.Equals(curNode))
				{
					// found the match! Modify the parent pointer and get out
					node.ParentNode.FirstChild = curNode;
					return true;							// RETURN
				}
			}

			// did not find a match
			this.firstNodes.Add(node);
			node.IsCompressed = true;
			return false;										// RETURN
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="letter"></param>
		/// <returns>
		/// Matching node if it is found, null if not
		/// </returns>
		private Node FindChildNode(Node parentNode, byte letter)
		{
			// start with the first child
			Node curChild = parentNode.FirstChild;

			// look through the siblings
			while (curChild != null)
			{
				if (curChild.Letter == letter)
					return curChild;							// RETURN
				curChild = curChild.SiblingNode;
			}

			// didn't find a matching node
			return null;										// RETURN
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="binaryReader"></param>
		/// <param name="letters"></param>
		/// <returns>
		/// Number of letters extracted. If this value is 0, end of stream
		/// has been reached.
		/// </returns>
		private int ReadNextWord(BinaryReader binaryReader, byte[] letters)
		{
			int		numLetters = 0;
			byte	curByte = 0;

			// read until a new line is found or maximum is reached
			try
			{
				while (true)
				{
					curByte = binaryReader.ReadByte();

					// is this word termination?
					if (curByte == Std.LF_Char &&
						numLetters > 1 &&
						letters[numLetters - 1] == Std.CR_Char)
					{
						numLetters--;	// to get rid of the CR char
						if (numLetters > 0)
							return numLetters;					// RETURN
					}
					else
					{
						letters[numLetters] = curByte;
						numLetters++;

						// did we reach maximum word limit?
						if (numLetters == Std.MaxWordLength)
							return numLetters;					// RETURN
					}
				}

			}
			catch (EndOfStreamException eofException)
			{
				return numLetters;								// RETURN
			}
		}

		// DIAGNOSTICS

		private void AnalyzeHashTableUsage()
		{
			int minValuesPerBucket = 1000;
			int maxValuesPerBucket = 1;
			int numValues = 0;
			string summary;

			ArrayList valueArray;
			IDictionaryEnumerator dictEnumerator = this.nodeHashTable.GetEnumerator();
			while (dictEnumerator.MoveNext())
			{
				valueArray = (ArrayList) dictEnumerator.Value;
				numValues += valueArray.Count;
				if (valueArray.Count < minValuesPerBucket)
					minValuesPerBucket = valueArray.Count;
				if (valueArray.Count > maxValuesPerBucket)
					maxValuesPerBucket = valueArray.Count;
			}

			summary = string.Format("Number of buckets: {0}\n" +
				"Number of values: {1}\n" +
				"Min value in a bucket: {2}\n" +
				"Max value in a bucket: {3}",
				this.nodeHashTable.Count, numValues, minValuesPerBucket,
				maxValuesPerBucket);
			Std.TraceLine(summary);
		}

		private void TraceSummaryInfo()
		{
			string summaryInfo = string.Format(
				"Number of words: {0}\n" + 
				"Number of letters: {1}\n" +
				"Number of nodes: {2}\n" +
				"Number of first child nodes: {3}",
				this.numWords, this.totalNumLetters, this.numNodes,
				this.numFirstChildNodes);

			Std.TraceLine(summaryInfo);
		}

		// DECLARATION
		/// <summary>
		/// This is a data class containing information for a Trie node
		/// </summary>
		private class Node
		{
			// CONSTRUCTOR
			public Node()
			{
				this.FirstChild = null;
				this.SiblingNode = null;
				this.ParentNode = null;
				this.NodeID = 0;
				this.HashCode = base.GetHashCode();

				this.Letter = 0;
				this.IsWord = false;
				this.IsFirstChild = false;
				this.IsCompressed = false;
			}

			// DATA
			public	Node	FirstChild;		// reference to the first child
			public	Node	SiblingNode;	// reference to the next sibling
			public	Node	ParentNode;		// reference to the parent
			public	int		NodeID;			// used in the final stages of DAWG formation
			public	int		HashCode;		// needs to be filled in after the trie is read

			public	byte	Letter;			// letter value for the node
			public	bool	IsWord;			// does the node denote a word termination
			public	bool	IsFirstChild;	// is this node the first child of the parent
			public	bool	IsCompressed;	// has the node been visited for compression?

			/// <summary>
			/// Hash code is made up of the following bits
			/// 31-16 (16 bits) - bottom 16 bits of the sum of descendents and
			///					  sibling descendents
			///	15-08 ( 8 bits) - Letter value of the node
			///	07-07 ( 1 bit ) - IsWord bit of the node
			///	06-00 ( 7 bits) - Max (max descendent depth, max sibling descendent depth)
			/// </summary>
			/// <param name="numDescendents"></param>
			public void ComputeHashCode(int numDescendents, int numSiblingDescendents,
				int maxDescendentDepth, int maxSiblingDescendentDepth)
			{
				int bits31_16 = ((numDescendents + numSiblingDescendents) & 0xFFFF) << 16;
				int bits15_08 = ((int) this.Letter) << 8;
				int bits07_07 = (this.IsWord ? 1 : 0) << 7;
				int bits06_00 = (maxDescendentDepth > maxSiblingDescendentDepth ?
				maxDescendentDepth : maxSiblingDescendentDepth) & 0x7F;

				this.HashCode = bits31_16 | bits15_08 | bits07_07 | bits06_00;
			}

			/// <summary>
			/// Computes the hash code for the entire tree. This traverses the children
			/// nodes as well as the sibling nodes below.
			/// </summary>
			/// <param name="numDescendents"></param>
			/// <param name="numSiblingDescendents"></param>
			/// <param name="maxDescendentDepth"></param>
			/// <param name="maxSiblingDescendentDepth"></param>
			public void ComputeHashCodeForTree	(
				out int numDescendents,
				out int numSiblingDescendents,
				out int maxDescendentDepth,
				out int maxSiblingDescendentDepth
				)
			{
				int childNumDescendents = 0;
				int childNumSiblingDescendents = 0;
				int childMaxDescendentDepth = 0;
				int childMaxSiblingDescendentDepth = 0;
				int siblingNumDescendents = 0;
				int siblingNumSiblingDescendents = 0;
				int siblingMaxDescendentDepth = 0;
				int siblingMaxSiblingDescendentDepth = 0;

				// compute the hash code for children first
				if (this.FirstChild != null)
				{
					this.FirstChild.ComputeHashCodeForTree(out childNumDescendents,
						out childNumSiblingDescendents, out childMaxDescendentDepth,
						out childMaxSiblingDescendentDepth);

					// + 1 is for the first child which has not been counted already
					numDescendents = childNumDescendents + childNumSiblingDescendents + 1;
					maxDescendentDepth =
						childMaxDescendentDepth > childMaxSiblingDescendentDepth ?
						childMaxDescendentDepth + 1 : childMaxSiblingDescendentDepth + 1;

				}
				else
				{
					numDescendents = 0;
					maxDescendentDepth = 0;
				}

				// compute the hash code for sibling
				if (this.SiblingNode != null)
				{
					this.SiblingNode.ComputeHashCodeForTree(
						out siblingNumDescendents,
						out siblingNumSiblingDescendents,
						out siblingMaxDescendentDepth,
						out siblingMaxSiblingDescendentDepth);

					// + 1 is for the first sibling which has not been counted already
					numSiblingDescendents =
						siblingNumDescendents + siblingNumSiblingDescendents + 1;
					maxSiblingDescendentDepth =
						siblingMaxDescendentDepth > siblingMaxSiblingDescendentDepth ?
					siblingMaxDescendentDepth : siblingMaxSiblingDescendentDepth;
				}
				else
				{
					numSiblingDescendents = 0;
					maxSiblingDescendentDepth = 0;
				}

				// now... we can get the hash code for the current node
				this.ComputeHashCode(numDescendents, numSiblingDescendents,
					maxDescendentDepth, maxSiblingDescendentDepth);
			}

			public override bool Equals(object obj)
			{
				// check for null and proper type
				if (obj == null || this.GetType() != obj.GetType())
					return false;								// RETURN

				// are both objects the same?
				if (Object.ReferenceEquals(this, obj) == true)
					return true;								// RETURN
				
				// does the letter match?
				Node node = (Node)obj;
				if (this.Letter != node.Letter)
					return false;								// RETURN

				// does end of word match?
				if (this.IsWord != node.IsWord)
					return false;								// RETURN

				// does the number of children match?
				if (this.NumChildren != node.NumChildren)
					return false;								// RETURN

				// does number of siblings match?
				if (this.NumSiblings != node.NumSiblings)
					return false;								// RETURN

				// are the children equal?
				if (this.FirstChild != null && node.FirstChild != null)
				{
					if (!this.FirstChild.Equals(node.FirstChild))
						return false;							// RETURN
				}

				// are the siblings equal?
				if (this.SiblingNode != null && node.SiblingNode != null)
				{
					if (!this.SiblingNode.Equals(node.SiblingNode))
						return false;							// RETURN
				}

				// I guess they are equal!!!
				return true;									// RETURN
			}

			/// <summary>
			/// Returns the overridden hash code. When the node is constructed, it
			/// defaults to Object's hash code. After the Trie is constructed,
			/// ComputeHashCode() must be called for computing the new hash based on node
			/// value and its descendents.
			/// </summary>
			/// <returns></returns>
			public override int GetHashCode()
			{
				return this.HashCode;							// RETURN
			}

			public int NumChildren
			{
				get
				{
					if (this.FirstChild != null)
					{
						return this.FirstChild.NumSiblings + 1;	// RETURN
					}
					else
						return 0;								// RETURN
				}
			}

			public int NumSiblings
			{
				get
				{
					int numSiblings = 0;
					Node curNode = this.SiblingNode;
					while (curNode != null)
					{
						numSiblings++;
						curNode = curNode.SiblingNode;
					}

					return numSiblings;							// RETURN
				}
			}
		}

		// DATA
		Node	rootNode;
		int		numNodes;
		int		numFirstChildNodes;
		int		numWords;
		int		totalNumLetters;

		Hashtable	nodeHashTable;	// key to this table is the hash value of the node
									// value is an ArrayList that contains the nodes with
									// the same hash value. This reduces the search space
									// when we are looking for similar nodes.
		ArrayList	firstNodes;		// a list of first nodes that have gone through
									// compression
	}
}
