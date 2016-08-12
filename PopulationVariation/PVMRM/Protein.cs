using System.Collections.Generic;

namespace PVMRM
{
    public class Protein
    {
        public string ProteinName { get; set; } //Refseq protein name, can be returned to the user as a way of validating what was found
        public string ProteinFullString { get; set; } //Full protein string from Fasta file
        public string ProteinAccession { get; private set; } //NP_ number (RefSeq) from user
        public List<Peptide> PeptideList { get; set; } //One protein, many possible peptide entries.
        public bool FoundProteinLevelChange { get; set; } //Stop-Gain or a frameshift was found in the query
        public List<Snp> ProteinLevelSnps { get; set; } //these are for SNPs that affect the sequence as a whole, like a frame shift of stop-Gain.

        public Protein(string protAccession)
        {
            this.ProteinAccession = protAccession;
            PeptideList = new List<Peptide>();
            ProteinLevelSnps = new List<Snp>();
        }

        /// <summary>
        /// Creates a peptide object with the given information
        /// </summary>
        /// <param name="peptideString"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        public void AddPeptide(string peptideString, int startPos, int endPos)
        {
            Peptide peptideInstance = new Peptide(peptideString); //new and not fully-formed peptide
            peptideInstance.IndexStart = startPos;
            peptideInstance.IndexStop = endPos;
            PeptideList.Add(peptideInstance);
        }

        /// <summary>
        /// Stores a peptide object
        /// </summary>
        /// <param name="pep"></param>
        public void DirectAddPeptide(Peptide pep){
            PeptideList.Add(pep);
        }
    }
}
