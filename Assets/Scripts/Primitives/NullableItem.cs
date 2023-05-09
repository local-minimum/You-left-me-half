using DeCrawl.Primitives;

public class NullableItem<T>
{
    private bool itemSet = false;
    private T item;
    public T Item {
        get {
            if (itemSet) return item;
            throw new System.Exception("No value set");
        }

        set
        {
            itemSet = true;
            item = value;
        }
    }
    
    public bool HasItem
    {
        get => itemSet;
    }

    public void Clear()
    {
        itemSet = false;
    }

    public NullableItem(T item) {
        Item = item;
    }

    public NullableItem() {}

    
    public bool Equals(T other)
    {
        if (!itemSet) return false;
        return item.Equals(other);
    }

    public bool Equals(NullableItem<T> other)
    {
        if (!itemSet || !other.itemSet) return false;
        return item.Equals(other.item);
    }


    public override bool Equals(object obj)
    {
        return false;
    }
}