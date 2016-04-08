using System.Linq;

namespace HydroTech.File
{
    public class FileName
    {
        public class Folder
        {
            public string name = "";

            public Folder subfolder;

            public Folder Last
            {
                set
                {
                    if (this.subfolder == null) { this.subfolder = value; }
                    else
                    {
                        this.subfolder.Last = value;
                    }
                }
            }

            public Folder(string folderName, params string[] names)
            {
                this.name = folderName;
                foreach (string n in names) { this.Last = new Folder(n); }
            }

            public Folder(Folder folder, params string[] folders) // copy constructor
            {
                this.name = folder.name;
                if (folder.subfolder != null) { this.subfolder = new Folder(folder.subfolder); }
                if (folders.Count() != 0) { foreach (string n in folders) { this.Last = new Folder(n); } }
            }

            public override string ToString()
            {
                return this.name + "\\" + (this.subfolder == null ? "" : this.subfolder.ToString());
            }
        }

        public static readonly Folder hydroTechFolder = new Folder("GameData", "HydroTech");
        private string ext = "";

        private Folder folder;
        private string name = "";

        public string WwwForm
        {
            get { return "file://" + ToString().Replace('\\', '/'); }
        }

        public FileName(string name, string ext)
        {
            this.name = name;
            this.ext = ext;
        }

        public FileName(string name, string ext, string rootFolder, params string[] folders)
        {
            this.name = name;
            this.ext = ext;
            this.folder = new Folder(rootFolder, folders);
        }

        public FileName(string name, string ext, Folder folder)
        {
            this.name = name;
            this.ext = ext;
            this.folder = new Folder(folder);
        }

        public FileName(string name, string ext, Folder parentFolder, params string[] folders)
        {
            this.name = name;
            this.ext = ext;
            this.folder = new Folder(parentFolder, folders);
        }

        public FileName(FileName filename)
        {
            this.name = filename.name;
            this.ext = filename.ext;
            this.folder = new Folder(filename.folder);
        }

        public override string ToString()
        {
            return KSPUtil.ApplicationRootPath + "\\" + (this.folder == null ? "" : this.folder.ToString()) + this.name + "." + this.ext;
        }
    }
}