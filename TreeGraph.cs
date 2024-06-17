using System.Collections.Generic;

namespace AngelMacro
{
    public class TreeNode
    {
        public List<TreeNode> conditionIfTrue = new List<TreeNode>();
        public List<TreeNode> conditionElseFalse = new List<TreeNode>();
        public int[] nodeArgs;
        public bool isRootNode;

        public TreeNode(bool _isRootNode = false)
        {
            isRootNode = _isRootNode;
        }
    }
}