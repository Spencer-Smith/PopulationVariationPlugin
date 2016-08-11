using System.Collections.Generic;
using System.IO;
using PvmrmViewer.DataObjects;
using TopDownIQGUI.ViewModel;

namespace PvmrmViewer.ViewModels
{
    public class PvmrmViewViewModel : ViewModelBase
    {
        public string InputFileName { get; set; }
        public string Text { get; set; }
        public List<PvmrmEntry> Entries { get; set; }

        public PvmrmViewViewModel()
        {
            Entries = new List<PvmrmEntry>();
        }

        public void Open()
        {
            Text = File.ReadAllText(InputFileName);
            StreamReader reader = new StreamReader(InputFileName);
            string header = reader.ReadLine();
            while (reader.Peek() > -1)
            {
                string inputLine = reader.ReadLine();
                string[] splitLine = inputLine.Split('\t');
                    // 0. Protein Accession
                    // 1. Protein Name
                    // 2. Variant Codex
                    // 3. Minor Allele Frequency
                    // 4. Variance
                    // 5. Reference Peptide
                    // 6. Modified Peptide
                    // 7. EAS MAF
                    // 8. EUR MAF
                    // 9. AFR MAF
                    //10. AMR MAF
                    //11. SAS MAF
                    //12. dbSNP ID
                Entries.Add(new PvmrmEntry(splitLine[0], splitLine[1], splitLine[2], splitLine[3],
                    splitLine[4], splitLine[7], splitLine[8], splitLine[9], splitLine[10], splitLine[11],
                     splitLine[5], splitLine[6], splitLine[12]));
            }
        }

        public void Save(string filename)
        {
            StreamWriter output = new StreamWriter(filename);
            output.Write(Text);
            output.Close();
        }

    }
}
