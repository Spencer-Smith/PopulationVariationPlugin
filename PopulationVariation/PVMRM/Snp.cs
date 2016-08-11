namespace PVMRM
{
    public class Snp
    {
        // Auto-properties
        public int SnpID { get; set; }
        public int aa_position { get; set; }
        public string NewResidue { get; set; }
        public double MinorAlleleFrequency { get; set; }
        public double popVariation { get; set; }
        public double easMAF { get; set; }
        public double eurMAF { get; set; }
        public double afrMAF { get; set; }
        public double amrMAF { get; set; }
        public double sasMAF { get; set; }
        public string ModifiedPeptideString { get; set; }
        public string Codex { get; set; }

        /// <summary>
        /// Calculates the modified peptide string and codex after the query results are returned.
        /// </summary>
        /// <param name="origPeptide"></param>
        /// <param name="aa_position"></param>
        /// <param name="indexStart"></param>
        /// <param name="residue"></param>
        public void SetModifiedPeptideStringAndCodex(string origPeptide, int aa_position, int indexStart, string residue)
        {
            if (indexStart > -1)
            {
                ModifiedPeptideString = (origPeptide.Substring(0, (aa_position - indexStart)) + '(' +
                    residue + ')' + origPeptide.Substring(aa_position + (residue.Length) - indexStart));
                Codex = origPeptide.Substring(aa_position - indexStart, 1) + (aa_position+1) + residue;
            }
            else
            {
                ModifiedPeptideString = "";
                Codex = "";
            }
        }
   }
}
