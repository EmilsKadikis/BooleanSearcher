using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BooleanSearcher
{
    public class PrefixTree
    {
        private PrefixTreeNode root = new PrefixTreeNode(char.MinValue);

        public PrefixTree()
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
        /// Steps through the tree character by character, creating new nodes if needed.
        /// When the appropriate leaf node is reached, sets its value to postingListId.
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="prefix"></param>
        /// <param name="postingListId"></param>
        private void AddRecursive(PrefixTreeNode currentNode, string prefix, int postingListId)
        {
            if (string.IsNullOrEmpty(prefix)) // We have reached the leaf node
            {
                currentNode.PostingListId = postingListId;
                return;
            }

            char nextChar = prefix[0];
            if (currentNode.Children.ContainsKey(nextChar))
                AddRecursive(currentNode.Children[nextChar], prefix.Substring(1), postingListId);
            else
                AddRecursive(currentNode.AddChild(nextChar), prefix.Substring(1), postingListId);
        }


        /// <summary>
        /// Recursively finds the node that has the exact prefix that's given.
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="prefix"></param>
        /// <returns>Node with the exact prefix or null if such a node doesn't exist.</returns>
        private PrefixTreeNode FindNodeRecursive(PrefixTreeNode currentNode, string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                return currentNode;

            char nextChar = prefix[0];
            if (currentNode.Children.ContainsKey(nextChar))
                return FindNodeRecursive(currentNode.Children[nextChar], prefix.Substring(1));
            else
                return null;
        }
        
        /// <summary>
        /// Given a start node, recursively collects all the full terms and their posting list ids contained in the subtree.
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="currentPrefix">Used to reconstruct the full term, so that we know which posting list id is for which term.</param>
        /// <param name="result">Reference to the list that will contain the result.</param>
        private void CollectSubtreeEntriesRecursive(PrefixTreeNode currentNode, string currentPrefix, ref List<PrefixResult> result)
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
    }

    public class PrefixTreeNode
    {
        public PrefixTreeNode(char prefix)
        {
            Prefix = prefix;
        }

        /// <summary>
        /// Adds a new child to this node.
        /// </summary>
        /// <param name="c">Next character in the prefix</param>
        /// <returns>The new node</returns>
        public PrefixTreeNode AddChild(char c)
        {
            var newNode = new PrefixTreeNode(c);
            Children[c] = newNode;
            return newNode;
        }

        public char Prefix { get; } 
        public int? PostingListId { get; set; }
        public Dictionary<char, PrefixTreeNode> Children { get; } = new Dictionary<char, PrefixTreeNode>();
    }

    public struct PrefixResult
    {
        public string Term { get; set; }
        public int PostingListId { get; set; }
    }
}
