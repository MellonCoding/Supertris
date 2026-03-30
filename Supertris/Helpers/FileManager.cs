namespace Supertris.Helpers
{
    /// <summary>
    /// Gestisce il file di comunicazione per la modalità EvE.
    /// 
    /// FUNZIONAMENTO:
    /// - Start(): crea o svuota il file all'inizio partita
    /// - Write(): appende una riga al file (formato: "X 4 5")
    /// - Nessun lock: si affida alla sincronizzazione del FileWatcher
    /// 
    /// IMPORTANTE: Start() deve essere chiamato UNA SOLA VOLTA
    /// </summary>
    
    public class FileManager
    {
        private static string percorso;

        public FileManager(string a_percorso)
        {
            percorso = a_percorso;
        }

        public bool Start()
        { 
            try
            {
                // Crea o svuota il file
                File.WriteAllText(percorso, string.Empty);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public void Write(string testo)
        {
            // MessageBox.Show(percorso);
            using (StreamWriter sw = new StreamWriter(percorso, true))
            {
                sw.WriteLine(testo);
            }
        }
    }
}