using System;
using System.Data;

namespace PVMRM
{
    public class Snp
    {
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
        /// Loads data from a query-returned row
        /// </summary>
        /// <param name="row"></param>
        public void LoadData(DataRow row)
        {
            NewResidue = SNPDatabase.DbCStr(row["residue"]);
            aa_position = SNPDatabase.DbCInt(row["aa_pos"]);
            MinorAlleleFrequency = SNPDatabase.DbCDouble(row["freq"]);
            easMAF = SNPDatabase.DbCDouble(row["eas"]);
            eurMAF = SNPDatabase.DbCDouble(row["eur"]);
            afrMAF = SNPDatabase.DbCDouble(row["afr"]);
            amrMAF = SNPDatabase.DbCDouble(row["amr"]);
            sasMAF = SNPDatabase.DbCDouble(row["sas"]);
            double high = Math.Max(easMAF, Math.Max(eurMAF, Math.Max(afrMAF,
                Math.Max(amrMAF, sasMAF))));
            double low = Math.Min(easMAF, Math.Min(eurMAF, Math.Min(afrMAF,
                Math.Min(amrMAF, sasMAF))));
            popVariation = high - low;
            SnpID = SNPDatabase.DbCInt(row["snp_id"]);
        }

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
