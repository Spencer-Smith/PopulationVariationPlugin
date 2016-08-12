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
        public void FindSnps(Protein protein)
        {
            //Find SNPs in database related to protein
            DataTable QueryResults = ProteinQuery(protein.ProteinAccession);

            //Process and store results in the protein structure, and its substructures
            foreach (DataRow currentResult in QueryResults.Rows)
            {
                string function = DbCStr(currentResult["function"]);
                if (function == "42" || function == "8")
                    ProcessMissenseResult(currentResult, protein.PeptideList);
                else
                {
                    ProcessSequenceChangeResult(currentResult, function, protein.ProteinLevelSnps);
                    protein.FoundProteinLevelChange = true;
                }
            }
        }

        /// <summary>
        /// Queries for data on a protein
        /// </summary>
        /// <param name="protAcc"></param>
        /// <returns>Data regarded passsed protein accession</returns>
        public DataTable ProteinQuery(string protAcc)
        {
            //Look up SNP information for the given Accession
            StringBuilder sqlStr = new StringBuilder();
            sqlStr.Append(" SELECT distinct *");
            sqlStr.Append(" FROM minor01var05");
            sqlStr.Append(" WHERE prot_acc = '" + protAcc + "'");
            sqlStr.Append(" ORDER BY aa_pos ");
            SQLiteCommand command = new SQLiteCommand(sqlStr.ToString(), conn);

            //Get a table to hold the results of the query
            DataTable dt = null;
            SQLiteDataAdapter Da = new SQLiteDataAdapter(command);
            DataSet Ds = new DataSet();
            Da.Fill(Ds);
            dt = Ds.Tables[0];
            return dt;
        }

        /// <summary>
        /// Reads information returned from query and stores it in the peptide it corresponds to
        /// </summary>
        /// <param name="curRow"></param>
        /// <param name="plist"></param>
        public void ProcessMissenseResult(DataRow curRow, List<Peptide> plist)
        {
            //Find the SNP's position...
            int pos = DbCInt(curRow["aa_pos"]);
            foreach (Peptide pep in plist)
            {
                //...see if it lies within a peptide in our list...
                if (pos > pep.IndexStart && pos < pep.IndexStop)
                {
                   //...then make a Snp object for it...
                    Snp result = new Snp();
                    result.LoadData(curRow);

                    //...and add that object to the Snp List for that peptide
                    pep.Snps.Add(result);
                    pep.FoundVariantInQuery = true;
                }
            }
        }

        /// <summary>
        /// Reads information returned from query and stores it as a protein level SNP
        /// </summary>
        /// <param name="curRow"></param>
        /// <param name="function"></param>
        /// <param name="snplist"></param>
        public void ProcessSequenceChangeResult(DataRow curRow, string function, List<Snp> snplist)
        {
            Snp result = new Snp();
            result.LoadData(curRow);

            if (function == "41")
                result.ModifiedPeptideString = "Stop-Gain";
            else if (function == "43")
                result.ModifiedPeptideString = "Stop-Loss";
            else if (function == "44")
                result.ModifiedPeptideString = "Frameshift";
            else if (function == "45")
                result.ModifiedPeptideString = "CDS-Indel";
            else
                result.ModifiedPeptideString = "Unknown";

            if (result.MinorAlleleFrequency != -1)
                snplist.Add(result);
        }

        /// <summary>
        ///  Converts a database field value to a string, checking for null values
        /// </summary>
        /// <param name="inpObj"></param>
        /// <returns></returns>
        public static string DbCStr(object inpObj)
        {
            //If input object is DbNull, returns "", otherwise returns String representation of object
            if (ReferenceEquals(inpObj, DBNull.Value))
                return string.Empty;
            else
                return Convert.ToString(inpObj);
        }

        /// <summary>
        ///  Converts a database field value to an integer (int32), checking for null values
        /// </summary>
        /// <param name="inpObj"></param>
        /// <returns></returns>
        public static int DbCInt(object inpObj)
        {
            //If input object is DbNull, returns -1, otherwise returns Integer representation of object
            if (ReferenceEquals(inpObj, DBNull.Value))
                return -1;
            else
                return Convert.ToInt32(inpObj);
        }

        /// <summary>
        ///  Converts a database field value to a Double, checking for null values
        /// </summary>
        /// <param name="inpObj"></param>
        /// <returns></returns>
        public static double DbCDouble(object inpObj)
        {
            //If input object is DbNull, returns -1, otherwise returns Double representation of object
            if (ReferenceEquals(inpObj, DBNull.Value))
                return -1;
            else
                return Convert.ToDouble(inpObj);
        }
    }
}
