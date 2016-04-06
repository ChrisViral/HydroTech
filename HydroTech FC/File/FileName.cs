using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    public class FileName
    {
        public class Folder
        {
            public Folder(string folderName, params string[] names)
            {
                name = folderName;
                foreach (string n in names)
                    last = new Folder(n);
            }
            public Folder(Folder folder, params string[] folders) // copy constructor
            {
                name = folder.name;
                if (folder.subfolder != null)
                    subfolder = new Folder(folder.subfolder);
                if (folders.Count() != 0)
                    foreach (string n in folders)
                        last = new Folder(n);
            }

            public Folder subfolder = null;
            public string name = "";

            public Folder last
            {
                set
                {
                    if (subfolder == null)
                        subfolder = value;
                    else
                        subfolder.last = value;
                }
            }

            public override string ToString()
            {
                return name + "\\" + (subfolder == null ? "" : subfolder.ToString());
            }
        }
        public static readonly Folder HydroTechFolder = new Folder("GameData", "HydroTech");

        public FileName(string name, string ext)
        {
            this.name = name;
            this.ext = ext;
        }
        public FileName(string name, string ext, string rootFolder, params string[] folders)
        {
            this.name = name;
            this.ext = ext;
            folder = new Folder(rootFolder, folders);
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
            folder = new Folder(parentFolder, folders);
        }
        public FileName(FileName filename)
        {
            name = filename.name;
            ext = filename.ext;
            folder = new Folder(filename.folder);
        }

        private Folder folder = null;
        private string name = "";
        private string ext = "";

        public override string ToString()
        {
            return KSPUtil.ApplicationRootPath + "\\" + (folder == null ? "" : folder.ToString()) + name + "." + ext;
        }
        public string WWWForm { get { return "file://" + ToString().Replace('\\', '/'); } }
    }
}