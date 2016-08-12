using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace PVMRM
{
    public class Pvmrm
    {
        private static SNPDatabase database { get; set; }

        static Pvmrm (){
            database = new SNPDatabase();
        }

        /// <summary>
        /// Finds SNPs within the peptides of protein accessions in the dictionary
        /// </summary>
        /// <param name="FastaFilePath"></param>
        /// <param name="SNPdbFilePath"></param>
        /// <param name="OutputFilePath"></param>
        /// <param name="accessionDictionary"></param>
        public static void Execute(string FastaFilePath, string SNPdbFilePath, string OutputFilePath,
            Dictionary<string, Protein> Proteins)
        {
            bool faqWrite = false;
            try
            {
                database.SetUpConnection(SNPdbFilePath);

                foreach (var protein in Proteins)
                {
                    try
                    {
                        Protein testProtein = protein.Value;
                        QueryforSnps(testProtein);
                    }
                    catch (Exception e)
                    {
                        if (!faqWrite)
                        {
                            Console.WriteLine("Please see F.A.Q for proper accession naming.");
                            faqWrite = true;
                        }
                        Console.WriteLine("Unable to process accession: " + protein.Key);
                        Console.WriteLine(e);
                    }
                }
                WriteResultsToFile(OutputFilePath, Proteins);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Runs a smart query on the protein
        /// </summary>
        /// <param name="lProtein"></param>
        public static void QueryforSnps(Protein lProtein)
        {
            database.FindSnps(lProtein);

            //now we have SNPs get their new sequence and codex
            foreach (Peptide lPep in lProtein.PeptideList)
            {
                foreach (Snp ls in lPep.Snps)
                {
                    ls.SetModifiedPeptideStringAndCodex(lPep.PeptideString, ls.aa_position, lPep.IndexStart, ls.NewResidue);
                    lPep.FoundVariantInQuery = true;
                }
            }
        }

        /// <summary>
        /// Outputs query results to file for read-in by viewer
        /// </summary>
        /// <param name="outputFilePath"></param>
        /// <param name="proteinInstance"></param>
        public static void WriteResultsToFile(string outputFilePath, Dictionary<string, Protein> proteinInstance)
        {
            StringBuilder sb = new StringBuilder();

            //Column headers
            sb.Append("Protein Accession\tProtein Name\tVariant Codex\tMinor Allele Frequency\tVariance\tReference");
            sb.Append(" Peptide\tModified Peptide\tEAS MAF\tEUR MAF\tAFR MAF\tAMR MAF\tSAS MAF\tdbSNP ID\n");

            foreach (var protein in proteinInstance)
                sb.Append(CreateResultsOutputString(protein.Value));

            StreamWriter fileOut = new StreamWriter(outputFilePath);
            fileOut.Write(sb.ToString());
            fileOut.Close();
        }

        /// <summary>
        /// Creates strings with information about a protein, its peptides and their SNPs
        /// </summary>
        /// <param name="proteinInstance"></param>
        /// <returns></returns>
        private static string CreateResultsOutputString(Protein proteinInstance)
        {
            StringBuilder sb = new StringBuilder();

            if (proteinInstance.FoundProteinLevelChange)
            {
                foreach (Snp ps in proteinInstance.ProteinLevelSnps)
                    sb.Append(proteinInstance.ProteinAccession + "\t" + proteinInstance.ProteinName + "\t" + ps.Codex +
                        "\t" + ps.MinorAlleleFrequency + "\t" + ps.popVariation + "\t" + "N/A" + "\t" + ps.ModifiedPeptideString
                        + "\t" + ps.easMAF + "\t" + ps.eurMAF + "\t" + ps.afrMAF + "\t" + ps.amrMAF + "\t" + ps.sasMAF + "\t"
                        + ps.SnpID + "\n");
            }

            foreach (Peptide pep in proteinInstance.PeptideList)
            {
                //peptide string was found in the fasta protein string AND found a variant in the query
                if (pep.FoundVariantInQuery)
                {
                    foreach (Snp snp in pep.Snps)
                    {
                        string MAF = "";
                        //Expression to write "N/A" if minor allele frequency is not given.
                        MAF = snp.MinorAlleleFrequency > 0 ? snp.MinorAlleleFrequency.ToString(CultureInfo.InvariantCulture) : "N/A";

                        sb.Append(proteinInstance.ProteinAccession + "\t" + proteinInstance.ProteinName + "\t" + snp.Codex
                            + "\t" + MAF + "\t" + snp.popVariation + "\t" + pep.PeptideString + "\t" + snp.ModifiedPeptideString
                            + "\t" + snp.easMAF + "\t" + snp.eurMAF + "\t" + snp.afrMAF + "\t" + snp.amrMAF + "\t" + 
                            snp.sasMAF + "\t" + snp.SnpID + "\n");
                    }
                }
            }

            return sb.ToString();
        }
    }
}
