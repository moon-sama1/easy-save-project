using System;
using System.IO;
using System.Collections.Generic;
using easySave_V1.Model_;
using easySave_V1.View_;

namespace easySave_V1.View_Model
{
    class ViewModel
    {
      
        public Model model;
        public View view;

        
        public ViewModel()
        {
            
            this.model = new Model();
            this.view = new View(this);

           
            this.view.ConsoleUpdate(this.model.Loadsaves());
        }


        
        public void Run()
        {
            bool isRunning = true;

            while (isRunning)
            {
                switch (this.view.Menu())
                {
                    case 1:
                        Displaysaves();
                        break;

                    case 2:
                        Addsave();
                        break;

                    case 3:
                        LaunchBackupsave();
                        break;

                    case 4:
                        Removesave();
                        break;

                    case 5:
                        isRunning = false;
                        break;

                    default:
                        this.view.ConsoleUpdate(207);
                        break;
                }
            }
        }

        private void Displaysaves()
        {
            if (this.model.saves.Count > 0)
            {
                this.view.Displaysaves();
            }
            else
            {
                this.view.ConsoleUpdate(204);
            }
        }

        private void Addsave()
        {
            if (this.model.saves.Count < 5)
            {
                string addsaveName = view.saveName();
                if (addsaveName == "0") return;

                string addsaveSrc = view.saveSrc();
                if (addsaveSrc == "0") return;

                string addsaveDest = view.saveDst(addsaveSrc);
                if (addsaveDest == "0") return;

                BackupType addsaveBackupType;
                switch (view.AddsaveBackupType())
                {
                    case 0:
                        return;

                    case 1:
                        addsaveBackupType = BackupType.FULL;
                        break;

                    case 2:
                        addsaveBackupType = BackupType.DIFFRENTIAL;
                        break;

                    default:
                        addsaveBackupType = BackupType.FULL;
                        break;
                }
                this.view.ConsoleUpdate(model.Addsave(addsaveName, addsaveSrc, addsaveDest, addsaveBackupType));
            }
            else
            {
                this.view.ConsoleUpdate(205);
            }
        }

        private void LaunchBackupsave()
        {
            if (this.model.saves.Count > 0)
            {
                int userChoice = view.LaunchBackupChoice();

                switch (userChoice)
                {
                    // Retour au menu
                    case 0:
                        return;

                    // Exécuter toutes les sauvegardes une par une
                    case 1:
                        foreach (save save in this.model.saves)
                        {
                            this.view.ConsoleUpdate(LaunchBackupType(save));
                            this.view.ConsoleUpdate(4);
                        }
                        break;

                    // Exécuter une sauvegarde à partir de son ID dans la liste
                    default:
                        int indexsave = userChoice - 2;
                        this.view.ConsoleUpdate(LaunchBackupType(this.model.saves[indexsave]));
                        break;
                }
                this.view.ConsoleUpdate(1);
            }
            else
            {
                this.view.ConsoleUpdate(204);
            }
        }
        public int LaunchBackupType(save _save)
        {
            DirectoryInfo dir = new DirectoryInfo(_save.src);

            // Vérifiez si le dossier source et le dossier de déstionnement existent.
            if (!dir.Exists && !Directory.Exists(_save.dst))
            {
                // Code d'erreur de retour
                return 207;
            }

            // Exécuter la bonne sauvegarde (complète ou différée)
            switch (_save.backupType)
            {
                case BackupType.DIFFRENTIAL:
                    string fullBackupDir = GetFullBackupDir(_save);

                    // S'il n'y a pas de première sauvegarde complète, nous créons la première (référence de la prochaine sauvegarde différentielle).
                    if (fullBackupDir != null)
                    {
                        return DifferentialBackupSetup(_save, dir, fullBackupDir);
                    }
                    return FullBackupSetup(_save, dir);

                case BackupType.FULL:
                    return FullBackupSetup(_save, dir);

                default:
                    // Code d'erreur de retour
                    return 208;
            }
        }

        // Obtenir le répertoire de la première sauvegarde complète d'une sauvegarde différentielle
        private string GetFullBackupDir(save _save)
        {
            // Obtenir le nom de tous les répertoires du dossier dest
            DirectoryInfo[] dirs = new DirectoryInfo(_save.dst).GetDirectories();

            foreach (DirectoryInfo directory in dirs)
            {
                if (directory.Name.IndexOf("_") > 0 && _save.name == directory.Name.Substring(0, directory.Name.IndexOf("_")))
                {
                    return directory.FullName;
                }
            }
            return null;
        }

        // Full Backup
        private int FullBackupSetup(save _save, DirectoryInfo _dir)
        {
            long totalSize = 0;

            // Récupérer tous les fichiers du répertoire source
            FileInfo[] files = _dir.GetFiles("*.*", SearchOption.AllDirectories);

            // Calculer la taille de chaque fichier
            foreach (FileInfo file in files)
            {
                totalSize += file.Length;
            }
            return DoBackup(_save, files, totalSize);
        }

        // Sauvegarde différentielle
        private int DifferentialBackupSetup(save _save, DirectoryInfo _dir, string _fullBackupDir)
        {
            long totalSize = 0;

            // Récupérer tous les fichiers du répertoire source
            FileInfo[] srcFiles = _dir.GetFiles("*.*", SearchOption.AllDirectories);
            List<FileInfo> filesToCopy = new List<FileInfo>();

            // Vérifier s'il y a une modification entre le fichier courant et la dernière sauvegarde complète
            foreach (FileInfo file in srcFiles)
            {
                string currFullBackPath = _fullBackupDir + "\\" + Path.GetRelativePath(_save.src, file.FullName);

                if (!File.Exists(currFullBackPath) || !IsSameFile(currFullBackPath, file.FullName))
                {
                    // Calculer la taille de chaque fichier
                    totalSize += file.Length;

                    // Ajouter le fichier à la liste
                    filesToCopy.Add(file);
                }
            }

            // Tester s'il y a un fichier à copier
            if (filesToCopy.Count == 0)
            {
                _save.lastBackupDate = DateTime.Now.ToString("yyyy/MM/dd_HH:mm:ss");
                this.model.Savesaves();
                this.view.ConsoleUpdate(3);
                this.view.DisplayBackupRecap(_save.name, 0);
                return 105;
            }
            return DoBackup(_save, filesToCopy.ToArray(), totalSize);
        }

        // Vérifiez si le fichier ou le src est le même que celui de la sauvegarde complète pour savoir si les fichiers doivent être copiés ou non.
        private bool IsSameFile(string path1, string path2)
        {
            byte[] file1 = File.ReadAllBytes(path1);
            byte[] file2 = File.ReadAllBytes(path2);

            if (file1.Length == file2.Length)
            {
                for (int i = 0; i < file1.Length; i++)
                {
                    if (file1[i] != file2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        // Faire Backup
        private int DoBackup(save _save, FileInfo[] _files, long _totalSize)
        {
            // Créer le fichier d'état
            DateTime startTime = DateTime.Now;
            string dst = _save.dst + _save.name + "_" + startTime.ToString("yyyy-MM-dd_HH-mm-ss") + "\\";

            // Mettre à jour l'état actuel de la sauvegarde
            _save.state = new State(_files.Length, _totalSize, _save.src, dst);
            _save.lastBackupDate = startTime.ToString("yyyy/MM/dd_HH:mm:ss");

            // Créer le dossier dst
            try
            {
                Directory.CreateDirectory(dst);
            }
            catch
            {
                return 210;
            }
            List<string> failedFiles = CopyFiles(_save, _files, _totalSize, dst);

            // Calculer le temps de l'ensemble du processus de copie
            DateTime endTime = DateTime.Now;
            TimeSpan saveTime = endTime - startTime;
            double transferTime = saveTime.TotalMilliseconds;

            // Mettre à jour l'état actuel de la sauvegarde
            _save.state = null;
            this.model.Savesaves();
            this.view.ConsoleUpdate(3);

            foreach (string failedFile in failedFiles)
            {
                this.view.DisplayFiledError(failedFile);
            }
            this.view.DisplayBackupRecap(_save.name, transferTime);

            if (failedFiles.Count == 0)
            {
                // Return Success Code
                return 104;
            }
            else
            {
                // Return Error Code
                return 216;
            }
        }

        private void Removesave()
        {
            if (this.model.saves.Count > 0)
            {
                int RemoveChoice = this.view.RemovesaveChoice() - 1;
                if (RemoveChoice == -1) return;

                this.view.ConsoleUpdate(this.model.Removesave(RemoveChoice));
            }
            else
            {
                this.view.ConsoleUpdate(204);
            }
        }

        private List<string> CopyFiles(save _save, FileInfo[] _files, long _totalSize, string _dst)
        {
            // Taille des fichiers
            long leftSize = _totalSize;
            // Num de fichier
            int totalFile = _files.Length;
            List<string> failedFiles = new List<string>();

            // Copy tous les fichiers
            for (int i = 0; i < _files.Length; i++)
            {
                // Mise à jour de la taille restante à copier (octet)
                int pourcent = (i * 100 / totalFile);
                long curSize = _files[i].Length;
                leftSize -= curSize;

                if (this.model.CopyFile(_save, _files[i], curSize, _dst, leftSize, totalFile, i, pourcent))
                {
                    this.view.DisplayCurrentState(_save.name, (totalFile - i), leftSize, curSize, pourcent);
                }
                else
                {
                    failedFiles.Add(_files[i].Name);
                }
            }
            return failedFiles;
        }
    }
}