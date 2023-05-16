namespace DeCrawl.Primitives
{
    public interface StateSaver
    {
        public string SerializeState();

        public void DeserializeState(string json);
    }
}
