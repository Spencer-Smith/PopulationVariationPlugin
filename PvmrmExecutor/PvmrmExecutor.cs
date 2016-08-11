using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Windows;
using CommandLine;
using PVMRM;
using PvmrmViewer.Views;

namespace PvmrmExecutor
{
    public class PvmrmExecutor
    {

        #region Properties

        public static Options Options { get; set; }
        public static Dictionary<string, Protein> Proteins { get; set; }
        public static PvmrmView Viewer { get; set; }
        private static Dictionary<string, string> KnownConversions;

        #endregion

        #region Constructor

        static PvmrmExecutor()
        {
            Options = new Options();
            Proteins = new Dictionary<string, Protein>();
            KnownConversions = new Dictionary<string, string>();
            Options.FastaPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\\DataFiles\protein.fa";
            Options.DatabasePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\\DataFiles\SNP.db";
            Options.OutfilePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\\temp.txt";
        }

        #endregion

        [STAThread]
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments(args, Options);

            Console.WriteLine(@"Importing Data From Skyline...");
            try
            {
                Import();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine(@"Querying Database...");

            Pvmrm.Execute(Options.FastaPath, Options.DatabasePath, Options.OutfilePath, Proteins);

            Console.WriteLine(@"Processing Complete.");

            Application app = new Application();
            app.Run(new PvmrmView(Options.OutfilePath));
        }

        /// <summary>
        /// Imports and stores data from Skyline's Input Report
        /// </summary>
        public static void Import()
        {
            StreamReader input = new StreamReader(Options.InputReportPath);
            int proteinIndex = 0, peptideIndex = 1, nameIndex = 2, startIndex = 3, endIndex = 4;

            //Go through each entry following the header. 
            while (input.Peek() > -1)
            {
                string entry = input.ReadLine();
                if (entry != null)
                {
                    string[] entrySplit = entry.Split(',');
                    string[] accessionSplit = entrySplit[proteinIndex].Split('|');

                    //First find out what the protein accession is
                    string accession = "";
                    for (int i = 0; i < accessionSplit.Length; i++)
                    {
                        if (accessionSplit[i] == "ref")
                            accession = accessionSplit[i + 1];
                        if (accessionSplit[i] == "sp" || accessionSplit[0] == "tr")
                            accession = ConvertToRefSeq(accessionSplit[i + 1]);
                    }

                    //Remove version number
                    string[] accessionParts = accession.Split('.');
                    accession = accessionParts[0];

                    //Prepare other information to make a peptide
                    string peptide = entrySplit[peptideIndex];
                    int startPos = 0;
                    int.TryParse(entrySplit[startIndex], out startPos);
                    int endPos = 0;
                    int.TryParse(entrySplit[endIndex], out endPos);

                    //Either add the peptide to an existing protein, or make a new one
                    if (Proteins.ContainsKey(accession))
                        Proteins[accession].AddPeptide(peptide, startPos, endPos);
                    else
                    {
                        Protein protein = new Protein(accession);
                        protein.ProteinName = entrySplit[nameIndex];
                        protein.AddPeptide(peptide, startPos, endPos);

                        Proteins.Add(accession, protein);
                    }
                }
            }
            input.Close();
        }

        /// <summary>
        /// Queries the database to find the equivalent refseqId
        /// </summary>
        /// <param name="sharedId">ID found in UniProt fasta files</param>
        /// <returns>RefSeqId, recognized by local database</returns>
        public static string ConvertToRefSeq(string sharedId)
        {
            string result = "";

            //If we've already seen this Id, don't bother querying, just return what we already know
            if (KnownConversions.ContainsKey(sharedId))
            {
                result = KnownConversions[sharedId];
                return result;
            }

            // Looks up refseqId for the given sharedId
            StringBuilder sqlStr = new StringBuilder();
            sqlStr.Append("SELECT RefSeqId FROM AccessionMap ");
            sqlStr.Append("WHERE SharedId = '" + sharedId + "';");

            //Get a table to hold the results of the query
            DataTable dt = null;
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Options.DatabasePath);
            using (SQLiteDataAdapter Da = new SQLiteDataAdapter(sqlStr.ToString(), connection))
            {
                using (DataSet Ds = new DataSet())
                {
                    Da.Fill(Ds);
                    dt = Ds.Tables[0];
                }
            }

            result =  DbCStr(dt.Rows[0]["RefSeqId"]);
            dt.Dispose();

            //Save each conversion we find, to prevent querying excessively
            KnownConversions[sharedId] = result;

            return result;
        }

        /// <summary>
        /// Converts a database field value to a string, checking for null values
        /// </summary>
        /// <param name="inpObj">An object returned from a database</param>
        /// <returns>String representation of database object</returns>
        public static string DbCStr(object inpObj)
        {
            //If input object is null, returns "", otherwise returns string representation of object
            if (object.ReferenceEquals(inpObj, DBNull.Value))
                return string.Empty;
            else
                return Convert.ToString(inpObj);
        }
    }
}
