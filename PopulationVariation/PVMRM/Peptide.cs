using System.Collections.Generic;

namespace PVMRM
{
    public class Peptide
    {
        public string PeptideString { get; set; } //user-given peptide
        public int IndexStart { get; set; } //0-based location of where peptide begins on protein string
        public int IndexStop { get; set; } //0-based location of where peptide ends on protein string
        public List<Snp> Snps { get; set; } //query may return multiple SNPs per peptide string
        public bool FoundVariantInQuery { get; set; } //there was a variant returned by the dbSNP query.  Should be 1 iff len(Snps) > 0
        // Modified peptide string is part of the Snps list

        public Peptide(string peptide)
        {
            PeptideString = peptide;
            Snps = new List<Snp>();
        }
    }
}
