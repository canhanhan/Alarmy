namespace Alarmy.Common
{
    internal class ImportContext
    {
        public string Path { get; set; }
        public string DateFormat { get; set; }
        public string CaptionFormat { get; set; }
        public string[] CaptionPatterns { get; set; }
        public bool DeleteExisting { get; set; }
    }
}
