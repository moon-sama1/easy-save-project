using System;
using System.IO;
using easySave_V1.View_Model;

namespace easySave_V1.View_
{
    class View
    {
      
        private ViewModel viewModel;


        public View(ViewModel _viewModel)
        {
            this.viewModel = _viewModel;
        }

        // menu
        public int Menu()
        {
            Console.Clear();
            Console.WriteLine(
                "Menu:" +
                "\n1 - Afficher  les sauvegardes" +
                "\n2 - Ajouter une sauvegarde" +
                "\n3 - Faites un backup" +
                "\n4 - Supprimer une sauvegarde" +
                "\n5 - quitter!!!");

            return CheckChoiceMenu(Console.ReadLine(), 1, 5);
        }

        //ajouter un nom du sauvgarde 


        public string saveName()
        {
            Console.Clear();
            Console.WriteLine("parametre de sauvegarde:");
            ConsoleUpdate(2);

            Console.WriteLine("\nEnter un nom (1 to 20 characters):");
            string name = Console.ReadLine();

            //verifier le nom

            while (!CheckName(name))
            {
                name = Console.ReadLine();
            }
            return name;
        }

        private string RectifyPath(string _path)
        {
            if (_path != "0" && _path.Length >= 1)
            {
                _path += (_path.EndsWith("/") || _path.EndsWith("\\")) ? "" : "\\";
                _path = _path.Replace("/", "\\");
            }
            return _path.ToLower();
        }

        //source
        public string saveSrc()
        {
            Console.WriteLine("\nEntrez la source du répertoire ");
            string src = RectifyPath(Console.ReadLine());

        //verifier si la source est valide 

            while (!Directory.Exists(src) && src != "0")
            {
                ConsoleUpdate(211);
                src = RectifyPath(Console.ReadLine());
            }
            return src;
        }

        // destination du sauvgarde 
        public string saveDst(string _src)
        {
            Console.WriteLine("\nEntrer la destination du répertoire.");
            string dst = RectifyPath(Console.ReadLine());

            //verifier si la destination est valid

            while (!ChecksaveDst(_src, dst))
            {
                dst = RectifyPath(Console.ReadLine());
            }
            return dst;
        }

        private bool ChecksaveDst(string _src, string _dst)
        {
            if (_dst == "0")
            {
                return true;

            }
            else if (Directory.Exists(_dst))
            {
                if (_src != _dst)
                {
                    if (_dst.Length > _src.Length)
                    {
                        if (_src != _dst.Substring(0, _src.Length))
                        {
                            return true;
                        }
                        else
                        {
                            ConsoleUpdate(217);
                            return false;
                        }
                    }
                    return true;
                }
                ConsoleUpdate(212);
                return false;
            }
            ConsoleUpdate(213);
            return false;
        }

        //Ajouter un type de sauvegarde
        public int AddsaveBackupType()
        {
            Console.WriteLine(
                "\nChoose a type of Backup: " +
                "\n1.Full " +
                "\n2.Differential");
            return CheckChoiceMenu(Console.ReadLine(), 0, 2);
        }

        //Vérifier si le nom est valide
        private bool CheckName(string _name)
        {
            int length = _name.Length;

            if (length >= 1 && length <= 20)
            {
                if (!this.viewModel.model.saves.Exists(save => save.name == _name))
                {
                    return true;
                }
                ConsoleUpdate(214);
                return false;
            }
            ConsoleUpdate(215);
            return false;
        }

        //Vérifier si input est integer
        private static bool CheckInt(string _input)
        {
            try
            {
                int.Parse(_input);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //Afficher toutes les sauvgardes 
        private void Loadsaves(int _shift)
        {
            var saves = this.viewModel.model.saves;

            for (int i = 0; i < saves.Count; i++)
            {
                Console.WriteLine(
                    "\n" + (i + _shift) + " - " + "Name: " + saves[i].name
                    + "\n    Source: " + saves[i].src
                    + "\n    Destination: " + saves[i].dst
                    + "\n    Type: " + saves[i].backupType);
            }
        }

        public void Displaysaves()
        {
            Console.Clear();
            Console.WriteLine("saves list :");

            //Afficher toutes les sauvgardes
            Loadsaves(1);
            ConsoleUpdate(1);
        }

        //Choisissez le travail à sauvegarder
        public int LaunchBackupChoice()
        {
            Console.Clear();
            Console.WriteLine(
                "Choisissez le travail à sauvegarder : " +
                "\n\n1 - tout");

            //Choisissez le travail à sauvegarder 
            Loadsaves(2);
            ConsoleUpdate(2);

            //Vérifier si input est un integer valide. 
            return CheckChoiceMenu(Console.ReadLine(), 0, this.viewModel.model.saves.Count + 1);
        }

        //Choisissez la sauvegarde à supprimer
        public int RemovesaveChoice()
        {
            Console.Clear();
            Console.WriteLine("supprime un sauvgarde:");

            //Afficher toutes les sauvgardes 
            Loadsaves(1);
            ConsoleUpdate(2);

            //Vérifier si input de l'utilisateur est un integer valide.
            return CheckChoiceMenu(Console.ReadLine(), 0, this.viewModel.model.saves.Count);
        }

        //Vérifier si input est integer et dans la bonne range.
        private int CheckChoiceMenu(string _inputUser, int _minEntry, int _maxEntry)
        {
            while (!(CheckInt(_inputUser) && (Int32.Parse(_inputUser) >= _minEntry && Int32.Parse(_inputUser) <= _maxEntry)))
            {
                ConsoleUpdate(206);
                _inputUser = Console.ReadLine();
            }
            return Int32.Parse(_inputUser);
        }

        public void DisplayCurrentState(string _name, int _fileLeft, long _leftSize, long _curSize, int _pourcent)
        {
            Console.Clear();
            Console.WriteLine(
                "backup actuelle : " + _name
                + "\nTaille des fichiers: " + DiplaySize(_curSize)
                + "\nNombre de fichiers restants: " + _fileLeft
                + "\nTaille des fichiers restants : " + DiplaySize(_leftSize) + "\n");
            DisplayProgressBar(_pourcent);
        }

        public void DisplayBackupRecap(string _name, double _transferTime)
        {
            Console.WriteLine("\n\n" +
                "Backup : " + _name + " terminé\n"
                + "\nDurée : " + _transferTime + " ms\n");
            DisplayProgressBar(100);
        }

        public void DisplayFiledError(string _name)
        {
            Console.WriteLine("Fichier nommé " + _name + " échec.");
        }

        private void DisplayProgressBar(int _pourcent)
        {
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("Progress: [ " + _pourcent + "%]");
            Console.ResetColor();

            Console.Write(" [");
            for (int i = 0; i < 100; i += 5)
            {
                if (_pourcent > i)
                {
                    Console.Write("#");
                }
                else
                {
                    Console.Write(".");
                }
            }
            Console.Write("]\n\n");
        }

        private string DiplaySize(long _octet)
        {
            if (_octet > 1000000000000)
            {
                return Math.Round((decimal)_octet / 1000000000000, 2) + " To";
            }
            else if (_octet > 1000000000)
            {
                return Math.Round((decimal)_octet / 1000000000, 2) + " Go";
            }
            else if (_octet > 1000000)
            {
                return Math.Round((decimal)_octet / 1000000, 2) + " Mo";
            }
            else if (_octet > 1000)
            {
                return Math.Round((decimal)_octet / 1000, 2) + " ko";
            }
            else
            {
                return _octet + " o";
            }
        }

        //Afficher le message sur la console
        public void ConsoleUpdate(int _id)
        {
            if (_id < 100)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                switch (_id)
                {
                    //Information message
                    case 1:
                        Console.WriteLine("\nAppuyez sur  Enter pour afficher le menu . . .");
                        Console.ReadLine();
                        break;

                    case 2:
                        Console.WriteLine("\n(Entrez 0 pour revenir au menu)");
                        break;

                    case 3:
                        Console.Clear();
                        Console.WriteLine("\nBackup information :");
                        break;

                    case 4:
                        Console.WriteLine("\nAppuyez sur la touche Enter pour en voir plus . . .");
                        Console.ReadLine();
                        break;
                }
            }
            else if (_id < 200)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                switch (_id)
                {
                    // Success message from 100 to 199
                    case 100:
                        Console.WriteLine("\n$$$$$$$$$$$$$$$$$$$$ EASYSAVE A3 Oran  $$$$$$$$$$$$$$$$$$$$");
                        ConsoleUpdate(1);
                        break;

                    case 101:
                        Console.WriteLine("\nLe fichier a été ajouté avec succès !!!");
                        ConsoleUpdate(1);
                        break;

                    case 102:
                        Console.WriteLine("\nLe fichier été sauvegardé avec succès !");
                        break;

                    case 103:
                        Console.WriteLine("\nLe fichier a été supprimé avec succès !");
                        ConsoleUpdate(1);
                        break;

                    case 104:
                        Console.WriteLine("\nBackup success !");
                        break;

                    case 105:
                        Console.WriteLine("\nAucune modification depuis la dernière sauvegarde complète !\n");
                        break;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                switch (_id)
                {
                    // Error message from 200 to 299
                    case 200:
                        Console.WriteLine("\nRestaurer votre fichier de sauvegarde JSON.");
                        ConsoleUpdate(1);
                        break;

                    case 201:
                        Console.WriteLine("\nÉchec de l'ajout .");
                        ConsoleUpdate(1);
                        break;

                    case 202:
                        Console.WriteLine("\nÉchec de sauvgarde .");
                        ConsoleUpdate(1);
                        break;

                    case 203:
                        Console.WriteLine("\nÉchec de la suppression");
                        ConsoleUpdate(1);
                        break;

                    case 204:
                        Console.WriteLine("\nLa list est vide");
                        ConsoleUpdate(1);
                        break;

                    case 205:
                        Console.WriteLine("\nLa list est pleine");
                        ConsoleUpdate(1);
                        break;

                    case 206:
                        Console.WriteLine("\nEntrer une option valide");
                        break;

                    case 207:
                        Console.WriteLine("\nÉchec du transfert d'un fichier, le fichier source ou de destination n'existe pas.");
                        break;

                    case 208:
                        Console.WriteLine("\nLe type de sauvegarde sélectionné n'existe pas");
                        break;

                    case 209:
                        Console.WriteLine("\nÉchec de la copie du fichier.");
                        ConsoleUpdate(1);
                        break;

                    case 210:
                        Console.WriteLine("\nÉchec de la création du dossier de sauvegarde.");
                        ConsoleUpdate(1);
                        break;
                    case 211:
                        Console.WriteLine("\nDirectory 'existe pas. Veuillez entrer une source de directory valide. ");
                        break;

                    case 212:
                        Console.WriteLine("\nChoisissez un path différent de la source. ");
                        break;

                    case 213:
                        Console.WriteLine("\nDirectory n'existe pas. Veuillez entrer une direction de directory valide. ");
                        break;

                    case 214:
                        Console.WriteLine("\nLe nom est déjà pris. Veuillez entrer un autre nom.");
                        break;

                    case 215:
                        Console.WriteLine("\nEntrez un nom VALIDE(1 to 20 characters):");
                        break;

                    case 216:
                        Console.WriteLine("\nBackup terminé avec erreur.");
                        break;

                    case 217:
                        Console.WriteLine("\nLa destination directory ne peut pas être à l'intérieur de la source directory.");
                        break;

                    default:
                        Console.WriteLine("\nFailed : Error Unknown.");
                        ConsoleUpdate(1);
                        break;
                }
            }
            Console.ResetColor();
        }
    }
}
