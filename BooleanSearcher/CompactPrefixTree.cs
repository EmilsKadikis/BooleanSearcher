using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BooleanSearcher
{
    public class CompactPrefixTree
    {
        private CompactPrefixTreeNode root = new CompactPrefixTreeNode(string.Empty);

        public CompactPrefixTree()
        {
            
        }

        /// <summary>
        /// Add new term to the tree
        /// </summary>
        /// <param name="term">Term to add</param>
        /// <param name="postingListId">Id to store in the new leaf node</param>
        public void Add(string term, int postingListId)
        {
            AddRecursive(root, term, postingListId);
        }

        /// <summary>
        /// Find all ids contained in the subtree of a certain prefix. 
        /// </summary>
        /// <param name="prefix">Prefix to find in the tree. If it has $ at the end, it finds the single matching posting list id.</param>
        /// <returns>List of all posting list ids for entries that start with the given prefix</returns>
        public List<PrefixResult> FindIdsWithPrefix(string prefix)
        {
            var node = FindNodeRecursive(root, prefix);
            if (node == null)
                return new List<PrefixResult>();
            else
            {
                var result = new List<PrefixResult>();
                CollectSubtreeEntriesRecursive(node, prefix, ref result);
                return result.Distinct().OrderBy(x => x.PostingListId).ToList();
            }
        }

        /// <summary>
        /// Checks if the exact term exists in the tree (not just a term with this prefix)
        /// </summary>
        /// <param name="term">Term to look for</param>
        /// <param name="postingListId">If term is found, this will contain its posting list ID</param>
        /// <returns>True if term found, false if not.</returns>
        public bool DoesTermExist(string term, out int? postingListId)
        {
            postingListId = null;
            var node = FindNodeRecursive(root, term);
            if (node == null || !node.PostingListId.HasValue)
                return false;
            else
            {
                postingListId = node.PostingListId;
                return true;
            }
        }

        /// <summary>
        /// Recursive function for adding new terms to the tree.
        /// Steps through the tree according to the next character of the prefix until .
        /// When the appropriate leaf node is reached, sets its value to postingListId.
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="prefix"></param>
        /// <param name="postingListId">Posting list ID to store in the node</param>
        private void AddRecursive(CompactPrefixTreeNode currentNode, string prefix, int postingListId)
        {
            // We have reached the leaf node since its prefix matches our current prefix
            if (prefix == currentNode.Prefix) 
            {
                currentNode.PostingListId = postingListId;
                return;
            }

            var sharedPrefix = SharedPrefix(prefix, currentNode.Prefix);
            var remainingPrefix = prefix.Substring(sharedPrefix.Length);
            var remainingPrefixFromCurrentNode = currentNode.Prefix.Substring(sharedPrefix.Length);

            // The node's prefix contains all of the search prefix, 
            // so we just need to split it into 2 nodes for more granularity.
            if (string.IsNullOrEmpty(remainingPrefix)) 
            {
                currentNode.Split(sharedPrefix.Length);
                currentNode.PostingListId = postingListId;
                return;
            }
            // The node's prefix does not contain all of the search prefix,
            // so we need to look deeper into the tree.
            else if (string.IsNullOrEmpty(remainingPrefixFromCurrentNode))
            {
                char nextChar = remainingPrefix[0];
                if (currentNode.Children.ContainsKey(nextChar))
                    AddRecursive(currentNode.Children[nextChar], remainingPrefix, postingListId);
                else
                    AddRecursive(currentNode.AddChild(remainingPrefix), remainingPrefix, postingListId);
            }
            // The node's prefix and search prefix share the same beginning, but then both have more characters,
            // so we split the current node and create a new node to contain the last bit.
            else
            {
                currentNode.Split(sharedPrefix.Length);
                AddRecursive(currentNode.AddChild(remainingPrefix), remainingPrefix, postingListId);
            }
        }

        /// <summary>
        /// Recursively finds the node that has the exact prefix that's given.
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="prefix"></param>
        /// <returns>Node with the exact prefix or null if such a node doesn't exist.</returns>
        private CompactPrefixTreeNode FindNodeRecursive(CompactPrefixTreeNode currentNode, string prefix)
        {
            var sharedPrefix = SharedPrefix(prefix, currentNode.Prefix);
            var uniquePrefix = prefix.Substring(sharedPrefix.Length);
            var uniquePrefixFromCurrentNode = currentNode.Prefix.Substring(sharedPrefix.Length);

            // Current node's prefix contains all of the search prefix, so this node and all its children have the prefoix
            if (string.IsNullOrEmpty(uniquePrefix))
                return currentNode;

            // Current node's prefix and search prefix both differ, so there is no node with the exact prefix we are looking for.
            if (!string.IsNullOrEmpty(uniquePrefixFromCurrentNode))
                return null;

            char nextChar = uniquePrefix[0];
            if (currentNode.Children.ContainsKey(nextChar))
                return FindNodeRecursive(currentNode.Children[nextChar], uniquePrefix);
            else
                return null;
        }
        
        /// <summary>
        /// Given a start node, recursively collects all the full terms and their posting list ids contained in the subtree.
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="currentPrefix">Used to reconstruct the full term, so that we know which posting list id is for which term.</param>
        /// <param name="result">Reference to the list that will contain the result.</param>
        private void CollectSubtreeEntriesRecursive(CompactPrefixTreeNode currentNode, string currentPrefix, ref List<PrefixResult> result)
        {
            if (currentNode.PostingListId.HasValue)
                result.Add( new PrefixResult 
                { 
                    Term = currentPrefix,
                    PostingListId = currentNode.PostingListId.Value
                });

            foreach (var node in currentNode.Children.Values)
            {
                CollectSubtreeEntriesRecursive(node, currentPrefix + node.Prefix, ref result);
            }
        }

        /// <summary>
        /// Return the prefix that both strings share. For example, if a="Tester" and b="Testing",
        /// then their shared prefix is Test
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Shared prefix of a and b</returns>
        private string SharedPrefix(string a, string b)
        {
            string sharedPrefix = string.Empty;
            var firstEnumerator = a.GetEnumerator();
            var secondEnumerator = b.GetEnumerator();

            bool hasMoreCharacters = firstEnumerator.MoveNext() && secondEnumerator.MoveNext();
            while (hasMoreCharacters && firstEnumerator.Current == secondEnumerator.Current)
            {
                sharedPrefix += firstEnumerator.Current;
                hasMoreCharacters = firstEnumerator.MoveNext() && secondEnumerator.MoveNext();
            }

            return sharedPrefix;
        }
    }

    public class CompactPrefixTreeNode
    {
        public CompactPrefixTreeNode(string prefix)
        {
            Prefix = prefix;
        }

        /// <summary>
        /// Adds a new child to this node.
        /// </summary>
        /// <param name="str">Next bit of the full prefix</param>
        /// <returns>The new node</returns>
        public CompactPrefixTreeNode AddChild(string str)
        {
            var newNode = new CompactPrefixTreeNode(str);
            Children[str[0]] = newNode;
            return newNode;
        }

        public CompactPrefixTreeNode Split(int indexInPrefix)
        {
            var prefixFirstHalf = Prefix.Substring(0, indexInPrefix);
            var prefixOtherHalf = Prefix.Substring(indexInPrefix);

            Prefix = prefixFirstHalf;
            var newNode = new CompactPrefixTreeNode(prefixOtherHalf);
            newNode.PostingListId = PostingListId;
            newNode.Children = Children;
            PostingListId = null;
            Children = new Dictionary<char, CompactPrefixTreeNode>();
            Children[prefixOtherHalf[0]] = newNode;
            return newNode;
        }

        public string Prefix { get; private set; } 
        public int? PostingListId { get; set; }
        public Dictionary<char, CompactPrefixTreeNode> Children { get; private set; } = new Dictionary<char, CompactPrefixTreeNode>();
    }
}
