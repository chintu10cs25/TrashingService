
public class Tree<T>
{
    public TreeNode<T> Root { get; set; }

    public Tree(T data)
    {
        Root = new TreeNode<T>(data);
    }
}
