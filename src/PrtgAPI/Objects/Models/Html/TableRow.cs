namespace PrtgAPI.Html
{
    internal class TableRow
    {
        public TableData[] Data { get; }

        public TableRow(TableData[] data)
        {
            Data = data;
        }
    }
}