namespace FileRenamer
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Forms;

    /// <summary>
    ///     Metodi di elaboraizone di FileRenamer
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        ///     Contatore interno che rileva errori durante l'elaborazione
        /// </summary>
        private int errori;

        /// <summary>
        ///     Esplora risorse
        /// </summary>
        private FolderBrowserDialog folderExlporer = new FolderBrowserDialog();

        /// <summary>
        ///     Inizializzazionw
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Esplora risorse per il caricamento del path di origine
        /// </summary>
        /// <param name="sender">Mittente del click</param>
        /// <param name="e">RoutedEventArgs Evento</param>
        private void btnSfoglia_Click(object sender, RoutedEventArgs e)
        {
            DialogResult path = folderExlporer.ShowDialog();
            tbPercorso.Text = folderExlporer.SelectedPath;
        }

        /// <summary>
        ///     Cuore di Filerenamer, in cui si esegue ogni passo di elaborazione
        /// </summary>
        /// <param name="sender">Mittente del click</param>
        /// <param name="e">RoutedEventArgs Evento</param>
        private void btnEsegui_Click(object sender, RoutedEventArgs e)
        {
            ControlloErroriInput();

            if (errori == 0)
            {
                var files = Directory.GetFiles(tbPercorso.Text);
                foreach(string f in files)
                {
                    if (string.IsNullOrWhiteSpace(tbSottostringa.Text)
                        || (!string.IsNullOrWhiteSpace(tbSottostringa.Text) && f.Contains(tbSottostringa.Text)))
                    {
                        string fileResult = f;
                        string newFileName = Path.GetFileName(f);
                        string destPath = string.IsNullOrWhiteSpace(tbDestinazione.Text) ? Path.GetDirectoryName(f) : tbDestinazione.Text;

                        newFileName = Path.ChangeExtension(newFileName, string.IsNullOrWhiteSpace(tbEstensione.Text) ? Path.GetExtension(newFileName) : tbEstensione.Text);
                        newFileName = SostituzioneSottostringa(newFileName);
                        newFileName = !string.IsNullOrWhiteSpace(tbPrefisso.Text) ? string.Format("{0}{1}", tbPrefisso.Text, newFileName) : newFileName;
                        newFileName = AggiuntaPostfisso(newFileName);

                        File.Move(f, Path.Combine(destPath, newFileName));
                    }
                }

                System.Windows.Forms.MessageBox.Show("Operazione eseguita !");
            }
        }

        /// <summary>
        ///     Verifica degli input per poter procedere con l'elaborazione
        /// </summary>
        private void ControlloErroriInput()
        {
            errori = 0;
            if (tbPercorso.Text.Contains("Scegliere") || String.IsNullOrEmpty(tbPercorso.Text))
            {
                System.Windows.Forms.MessageBox.Show("Scegliere una cartella !", "ERRORE", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                errori++;
            }
            else
            {

                if (string.IsNullOrWhiteSpace(tbEstensione.Text)
                     && string.IsNullOrWhiteSpace(tbSostituisciOld.Text)
                     && string.IsNullOrWhiteSpace(tbSostituisciNew.Text)
                     && string.IsNullOrWhiteSpace(tbPrefisso.Text)
                     && string.IsNullOrWhiteSpace(tbPostfisso.Text)
                     && string.IsNullOrWhiteSpace(tbSottostringa.Text)
                     && string.IsNullOrWhiteSpace(tbDestinazione.Text))
                {
                    System.Windows.Forms.MessageBox.Show("Inserire almeno una variazione !", "ERRORE", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    errori++;
                }
            }


        }

        /// <summary>
        ///     Nel file name in input sostituisce la stringa in tbSostituisciOld con la stringa in tbSostituisciNew
        /// </summary>
        /// <param name="nomeFile">Nome del file originale</param>
        /// <returns>nome del file sostituito</returns>
        private string SostituzioneSottostringa(string nomeFile)
        {
            string result = nomeFile;

            if (tbSostituisciOld.Text.Contains("*"))
            {
                result = SostituzioneConAsterisco(nomeFile);
            }
            else
            {
                if (!string.IsNullOrEmpty(tbSostituisciOld.Text))
                {
                    result = nomeFile.Replace(tbSostituisciOld.Text, tbSostituisciNew.Text);
                }
            }

            return result;
        }

        /// <summary>
        ///     Sostituisce tutto il nome file (compresa estensione) con tbSostituisciNew
        /// </summary>
        /// <param name="nomeFile">Nome del file originale</param>
        /// <returns>nome del file sostituito</returns>
        private string SostituzioneConAsterisco(string nomeFile)
        {
            string result = nomeFile;
            int index = 0;
            if (!String.IsNullOrEmpty(tbSostituisciOld.Text))
            {
                //Sostituzione di * (di tutto, compreso l'estensione)
                string[] delimitatore = tbSostituisciOld.Text.Split('*');
                if (String.IsNullOrEmpty(delimitatore[0]) && String.IsNullOrEmpty(delimitatore[1]))
                {
                    result = nomeFile.Replace(nomeFile, tbSostituisciNew.Text);
                }

                //Sostituzione di *[
                if (String.IsNullOrEmpty(delimitatore[0]) && !String.IsNullOrEmpty(delimitatore[1]))
                {
                    index = nomeFile.IndexOf(delimitatore[1]);

                    if (index >= 0)
                    {
                        result = result.Remove(0, index);
                        result = tbSostituisciNew.Text + result;
                    }
                    else { return nomeFile; }
                }

                //Sostituzione di [*
                if (!String.IsNullOrEmpty(delimitatore[0]) && String.IsNullOrEmpty(delimitatore[1]))
                {
                    index = nomeFile.IndexOf(delimitatore[0]);

                    if (index >= 0)
                    {
                        result = result.Remove(index, result.Length);
                        result = result + tbSostituisciNew.Text;
                    }
                    else { return nomeFile; }

                }

                //Sostituzione di [*]
                if (!String.IsNullOrEmpty(delimitatore[0]) && !String.IsNullOrEmpty(delimitatore[1]))
                {
                    index = nomeFile.IndexOf(delimitatore[0]);
                    int index1 = nomeFile.IndexOf(delimitatore[1]);

                    if (index >= 0 && index1 >= 0)
                    {
                        string result1 = result.Substring(0, index + 1);
                        string result2 = result.Substring(index1);

                        result = result1 + tbSostituisciNew.Text + result2;
                    }
                    else { return nomeFile; }
                }
            }
            return result;
        }

        /// <summary>
        ///     Aggiunge al nome del file in input tbPostfisso in coda al nome e prima dell'estensione
        /// </summary>
        /// <param name="nomeFile">Nome del file originale</param>
        /// <returns>nome del file sostituito</returns>
        private string AggiuntaPostfisso(string nomeFile)
        {
            string result = nomeFile;
            if (!String.IsNullOrEmpty(tbPostfisso.Text))
            {
                string soloNome = Path.GetFileNameWithoutExtension(nomeFile);
                string estensione = Path.GetExtension(nomeFile);

                result = String.Format("{0}{1}{2}", soloNome, tbPostfisso.Text, estensione);
            }
            return result;
        }
    }
}
