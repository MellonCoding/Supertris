namespace Supertris.Helpers
{
    /// <summary>
    /// Monitora il file delle mosse e notifica quando l'avversario gioca.
    /// 
    /// FUNZIONAMENTO:
    /// - Usa FileSystemWatcher per rilevare modifiche al file
    /// - Legge l'ultima riga del file quando cambia
    /// - Se diversa dall'ultima letta -> chiama callback
    /// - Delay 500ms per assicurare scrittura completa
    /// - AggiornaUltimaRiga(): previene loop (ignora proprie mosse)
    /// 
    /// EVENTO: OnFileChanged -> leggi ultima riga -> callback
    /// </summary>

    internal class FileWatcher
    {
        private FileSystemWatcher watcher;
        private string ultimaRigaLetta;
        private string path;
        private Action<string> onMossaRicevuta;

        public FileWatcher(string arg_path, Action<string> callback)
        {
            path = arg_path;
            onMossaRicevuta = callback;
            ultimaRigaLetta = "";
        }

        public void Avvia()
        {
            // Leggi l'ultima riga attuale
            ultimaRigaLetta = LeggiUltimaRiga();

            // Configura il FileSystemWatcher
            watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(path),
                //Filter = Path.GetFileName(path),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            watcher.Changed += OnFileChanged;
        }

        public void Ferma()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs evento)
        {
            // Delay breve per assicurarsi che il file sia completamente scritto
            Thread.Sleep(500);

            string nuovaUltimaRiga = LeggiUltimaRiga();

            // Se l'ultima riga è cambiata, notifica
            if (nuovaUltimaRiga != ultimaRigaLetta && !string.IsNullOrEmpty(nuovaUltimaRiga))
            {
                ultimaRigaLetta = nuovaUltimaRiga;
                onMossaRicevuta?.Invoke(nuovaUltimaRiga);
            }
        }

        private string LeggiUltimaRiga()
        {
            try
            {
                if (!File.Exists(path))
                    return "";

                // Leggi tutte le righe e prendi l'ultima
                var righe = File.ReadAllLines(path);
                return righe.Length > 0 ? righe[righe.Length - 1] : "";
            }
            catch
            {
                return "";
            }
        }

        public void AggiornaUltimaRiga(string riga)
        {
            ultimaRigaLetta = riga;
        }
    }
}
