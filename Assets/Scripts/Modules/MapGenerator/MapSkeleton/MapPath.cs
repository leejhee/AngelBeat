
public class MapPath
{
    int _from;
    int _to;
    EventNodeData _data = null;

    public MapPath(int from, int to)
    {
        _from = from; _to = to;
    }

    public void SetEventNode(EventNodeData data) => _data = data;

}