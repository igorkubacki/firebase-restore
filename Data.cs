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

    class DataDocument
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public Dictionary<string, object> Fields { get; set; }
        public List<DataCollection> Collections { get; set; }

        public DataDocument(string id, string path)
        {
            Id = id;
            Path = path;
            Fields = new Dictionary<string, object>();
            Collections = new List<DataCollection>();
        }
    }
}