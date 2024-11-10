using System.Collections.Generic;
using System.Linq;
using Godot;

public static class NodeHelper
{
    // get all scripts of type under and including node, internal children is to include sub nodes children. returns empty if none found.
    public static IEnumerable<Type> getComponents<Type>(this Node node, bool internal_children = false)
    {
        return node.GetChildren(internal_children).Append(node).OfType<Type>();
    }
    // get first script of type under and including node, internal children is to include sub nodes children. will return default if none found.
    public static Type getComponent<Type>(this Node node, bool internal_children = false)
    {
        return node.getComponents<Type>(internal_children).FirstOrDefault();
    }
}