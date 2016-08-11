using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;

namespace PVMRM
{
    /// <summary>
    /// The sole purpose of this class is to query the trimmed database I created from dbSNP's data.
    /// </summary>
    public class SNPDatabase
    {
        private SQLiteConnection conn;

        /// <summary>
        /// LITE db connection
        /// Set up database connection for SQLite using trimmed database dbSNPdatabase.db
        /// </summary>
        /// <param name="dbFilePath"></param>
        public void SetUpConnection(string dbFilePath)
        {
            string ConnectionString = "Data Source=" + dbFilePath;
            conn = new SQLiteConnection(ConnectionString);
        }

        /// <summary>
        /// Quicker database query than the other function by querying for all peptides at once, then comparing
        /// data gathered to peptide lengths
        /// </summary>
        /// <param name="protein"></param>
        public void FindSnpsSmart(Protein protein)
        {
            MissenseQuerySmart(protein.ProteinAccession, protein.PeptideList);

            //now do a call to a new method that looks for variants that affect the whole protein sequence
            protein.ProteinLevelSnps = ProteinLevelQueryDbSNPLite(protein.ProteinAccession); //does query and attaches to the protein
            foreach (Snp s in protein.ProteinLevelSnps)
            {
                if (s.ModifiedPeptideString == "41")
                    s.ModifiedPeptideString = "Stop-Gain";
                else if (s.ModifiedPeptideString == "43")
                    s.ModifiedPeptideString = "Stop-Loss";
                else if (s.ModifiedPeptideString == "44")
                    s.ModifiedPeptideString = "Frameshift";
                else if (s.ModifiedPeptideString == "45")
                    s.ModifiedPeptideString = "CDS-Indel";

                protein.FoundProteinLevelChange = true;
            }
        }

        /// <summary>
        /// Searches for all peptides in a protein at once
        /// </summary>
        /// <param name="protAcc"></param>
        /// <param name="peptideStart"></param>
        /// <param name="peptideStop"></param>
        /// <returns></returns>
        public void MissenseQuerySmart(string protAcc, List<Peptide> plist)
        {
            // Looks up SNP information for the given Accession
            StringBuilder sqlStr = new StringBuilder();
            sqlStr.Append(" SELECT distinct residue, aa_pos, snp_id, freq, eas, eur, afr, amr, sas");
            sqlStr.Append(" FROM minor01var05");
            sqlStr.Append(" WHERE prot_acc = '" + protAcc + "'");
            sqlStr.Append(" AND function = 42");
            sqlStr.Append(" ORDER BY aa_pos ");

            SQLiteCommand command = new SQLiteCommand(sqlStr.ToString(), conn);

            //Get a table to hold the results of the query
            DataTable dt = null;
            SQLiteDataAdapter Da = new SQLiteDataAdapter(command);
            DataSet Ds = new DataSet();
            Da.Fill(Ds);
            dt = Ds.Tables[0];

            //For each snp found...
            foreach (DataRow curRow in dt.Rows)
            {
                int pos = DbCInt(curRow["aa_pos"]);
                foreach (Peptide pep in plist)
                {
                    //... see if it lies within a peptide in our list...
                    if (pos > pep.IndexStart && pos < pep.IndexStop)
                    {
                        //... then make a Snp object for it...
                        Snp result = new Snp();

                        result.NewResidue = DbCStr(curRow["residue"]);
                        result.aa_position = pos;
                        result.MinorAlleleFrequency = DbCDouble(curRow["freq"]);
                        result.easMAF = DbCDouble(curRow["eas"]);
                        result.eurMAF = DbCDouble(curRow["eur"]);
                        result.afrMAF = DbCDouble(curRow["afr"]);
                        result.amrMAF = DbCDouble(curRow["amr"]);
                        result.sasMAF = DbCDouble(curRow["sas"]);
                        double high = Math.Max(result.easMAF, Math.Max(result.eurMAF, Math.Max(result.afrMAF,
                            Math.Max(result.amrMAF, result.sasMAF))));
                        double low = Math.Min(result.easMAF, Math.Min(result.eurMAF, Math.Min(result.afrMAF,
                            Math.Min(result.amrMAF, result.sasMAF))));
                        result.popVariation = high - low;
                        result.SnpID = DbCInt(curRow["snp_id"]);

                        //... and add that object to the Snp List for that peptide
                        pep.Snps.Add(result);
                        pep.FoundVariantInQuery = true;
                    }
                }
            }

            dt.Dispose();
        }

        /// <summary>
        /// ProteinLevel query: query dbSNP database for protein-level variants for a given accession
        /// </summary>
        /// <param name="protAcc"></param>
        /// <returns>List of Snps that populates the Snp list in the Protein class (not the Peptide class for this one).</returns>
        public List<Snp> ProteinLevelQueryDbSNPLite(string protAcc)
        {
            // Looks up SNP information for the given Accession
            List<Snp> results = new List<Snp>();

            StringBuilder sqlStr = new StringBuilder();
            sqlStr.Append(" SELECT distinct residue, aa_pos, snp_id, freq, eas,");
            sqlStr.Append(" eur, afr, amr, sas, function from minor01var05");
            sqlStr.Append(" WHERE prot_acc = '" + protAcc + "'");
            sqlStr.Append(" AND function in (41,43,44,45)");
            sqlStr.Append(" ORDER BY aa_pos;");

            DataTable dt = null;

            //Get a table to hold the results of the query

            using (SQLiteDataAdapter Da = new SQLiteDataAdapter(sqlStr.ToString(), conn))
            {
                using (DataSet Ds = new DataSet())
                {
                    Da.Fill(Ds);
                    dt = Ds.Tables[0];
                }
            }

            //Verify at least one row returned
            if (dt.Rows.Count < 1)
            {
                // No data was returned
                dt.Dispose();
                return results;
            }
            else
            {
                foreach (DataRow curRow in dt.Rows)
                {
                    Snp result = new Snp();

                    result.NewResidue = DbCStr(curRow["residue"]);
                    result.aa_position = DbCInt(curRow["aa_pos"]);
                    result.MinorAlleleFrequency = DbCDouble(curRow["freq"]);
                    result.easMAF = DbCDouble(curRow["eas"]);
                    result.eurMAF = DbCDouble(curRow["eur"]);
                    result.afrMAF = DbCDouble(curRow["afr"]);
                    result.amrMAF = DbCDouble(curRow["amr"]);
                    result.sasMAF = DbCDouble(curRow["sas"]);
                    double high = Math.Max(result.easMAF, Math.Max(result.eurMAF, Math.Max(result.afrMAF, 
                        Math.Max(result.amrMAF, result.sasMAF))));
                    double low = Math.Min(result.easMAF, Math.Min(result.eurMAF, Math.Min(result.afrMAF, 
                        Math.Min(result.amrMAF, result.sasMAF))));
                    result.popVariation = high - low;
                    result.SnpID = DbCInt(curRow["snp_id"]);
                    result.ModifiedPeptideString = DbCInt(curRow["function"]).ToString();

                    if (result.MinorAlleleFrequency != -1)
                        results.Add(result);
                }
                dt.Dispose();
            }
            return results;
        }

        /// <summary>
        ///  Converts a database field value to a string, checking for null values
        /// </summary>
        /// <param name="inpObj"></param>
        /// <returns></returns>
        public string DbCStr(object inpObj)
        {
            //If input object is DbNull, returns "", otherwise returns String representation of object
            if (ReferenceEquals(inpObj, DBNull.Value))
            {
                return string.Empty;
            }
            else
            {
                return Convert.ToString(inpObj);
            }
        }

        /// <summary>
        ///  Converts a database field value to an integer (int32), checking for null values
        /// </summary>
        /// <param name="inpObj"></param>
        /// <returns></returns>
        public int DbCInt(object inpObj)
        {
            //If input object is DbNull, returns -1, otherwise returns Integer representation of object
            if (ReferenceEquals(inpObj, DBNull.Value))
            {
                return -1;
            }
            else
            {
                return Convert.ToInt32(inpObj);
            }
        }

        /// <summary>
        ///  Converts a database field value to a Double, checking for null values
        /// </summary>
        /// <param name="inpObj"></param>
        /// <returns></returns>
        public double DbCDouble(object inpObj)
        {
            //If input object is DbNull, returns -1, otherwise returns Double representation of object
            if (ReferenceEquals(inpObj, DBNull.Value))
            {
                return -1;
            }
            else
            {
                return Convert.ToDouble(inpObj);
            }
        }
    }
}
