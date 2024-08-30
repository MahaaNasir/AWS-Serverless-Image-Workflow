namespace Lab4
{
    /// <summary>
    /// The state passed between the step function executions.
    /// </summary>
    public class State
    {
      
        public string Bucket { get; set; }

       
        public string Key { get; set; }

        
        public string Message { get; set; }

     
        public int WaitInSeconds { get; set; }

      
        public bool IsProcessingSuccessful { get; set; }

   
        public Dictionary<string, string> Metadata { get; set; }

        public string Id { get; set; }

    }
}
