namespace firebase_restore
{
    class DataCollection
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public List<DataDocument> Documents { get; set; }

        public DataCollection(string id, string path)
        {
            Id = id;
            Path = path;
            Documents = new List<DataDocument>();
        }
    }
}