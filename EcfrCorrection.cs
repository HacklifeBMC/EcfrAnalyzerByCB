// Author: CB
// Joke: Structuring laws into classes — it's legal OOP 

namespace EcfrAnalyzerByCB
{
    public class EcfrCorrectionResponse
    {
        public List<EcfrCorrection>? ecfr_corrections { get; set; }
    }

    public class EcfrCorrection
    {
        public int id { get; set; }
        public List<CfrReference>? cfr_references { get; set; }
        public string? corrective_action { get; set; }
        public string? error_corrected { get; set; }
        public string? error_occurred { get; set; }
        public string? fr_citation { get; set; }
        public int year { get; set; }
        public string? last_modified { get; set; }
    }

    public class CfrReference
    {
        public string? cfr_reference { get; set; }
        public Hierarchy? hierarchy { get; set; }
    }

    public class Hierarchy
    {
        public string? title { get; set; }
        public string? subtitle { get; set; }
        public string? chapter { get; set; }
        public string? subchapter { get; set; }
        public string? part { get; set; }
        public string? subpart { get; set; }
        public string? section { get; set; }
    }
}