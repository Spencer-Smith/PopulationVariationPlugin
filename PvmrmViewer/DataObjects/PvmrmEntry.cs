using System;

namespace PvmrmViewer.DataObjects
{
    public class PvmrmEntry
    {
        public string ProteinAccession { get; set; }
        public string ProteinName { get; set; }
        public string VariantCodex { get; set; }
        public string MinorAlleleFrequency { get; set; }
        public string Variance { get; set; }
        public string EASmaf { get; set; }
        public string EURmaf { get; set; }
        public string AFRmaf { get; set; }
        public string AMRmaf { get; set; }
        public string SASmaf { get; set; }
        public string ReferencePeptide { get; set; }
        public string ModifiedPeptide { get; set; }
        public string SnpId { get; set; }
        public Uri SnpUri { get; set; }

        public PvmrmEntry(string accession, string name, string variant, string maf, string var, string eas,
            string eur, string afr, string amr, string sas, string refPeptide, string modPeptide, string snpId)
        {
            ProteinAccession = accession;
            ProteinName = name;
            VariantCodex = variant;
            MinorAlleleFrequency = MakePercentage(maf);
            Variance = MakePercentage(var);
            EASmaf = MakePercentage(eas);
            EURmaf = MakePercentage(eur);
            AFRmaf = MakePercentage(afr);
            AMRmaf = MakePercentage(amr);
            SASmaf = MakePercentage(sas);
            ReferencePeptide = refPeptide;
            ModifiedPeptide = modPeptide;
            SnpId = "rs" + snpId;
            SnpUri = new Uri("http://www.ncbi.nlm.nih.gov/projects/SNP/snp_ref.cgi?rs=" + snpId);
        }

        /// <summary>
        /// Format a long decimal string to a short percentage string
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private string MakePercentage(string number)
        {
            double Decimal = Convert.ToDouble(number);
            double Percent = 100 * Math.Round(Decimal, 4);
            string percentString = Percent.ToString() + "%";

            return percentString;
        }
    }
}
