namespace Supertris.Helpers
{
    /// <summary>   
    /// Gestisce il file di comunicazione per la modalità EvE.
    /// 
    /// metodi:
    /// - Start(): crea o svuota il file all'inizio partita -> true[il file esiste e il fileWriter é utilizzabile] - false[il file non puó essere utilizzato, throw exception e ricevo diagnostica]
    /// - Write(): appende una riga al file (formato: "X 4 5") -> void
    /// </summary>

    public class FileManager
    {
        private static string path;
        private static StreamWriter sw;
        
        public FileManager(string arg_path)
        {
            path = arg_path;
        }

        public bool Start()
        {
            try
            {
                // Crea o svuota il file
                File.WriteAllText(path, string.Empty);
                sw = new StreamWriter(path, true);
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
            try 
            {
                sw.WriteLine(testo);
            }
            catch (Exception e)
            { 
                MessageBox.Show(e.Message);
            }
        }
    }
}
