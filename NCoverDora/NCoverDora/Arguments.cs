namespace NCoverDora
{
    public class Arguments
    {
        public Arguments()
        {
            LogFileName = "NCoverDora.log";
        }

        public string LogFileName { get; set; }
        public string ConfigFileName { get; set; }
        public string CoverageFileName { get; set; }
    }
}