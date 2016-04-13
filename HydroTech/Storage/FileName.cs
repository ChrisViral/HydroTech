
namespace HydroTech.Storage
{
    public class FileName
    {
        public class Folder
        {
            #region Properties
            public Folder Last
            {
                set
                {
                    if (this.subfolder == null) { this.subfolder = value; }
                    else { this.subfolder.Last = value; }
                }
            }
            #endregion

            #region Fields
            public string name;
            public Folder subfolder;
            #endregion

            #region Constructors
            public Folder(string folderName, params string[] names)
            {
                this.name = folderName;
                foreach (string n in names)
                {
                    this.Last = new Folder(n);
                }
            }

            public Folder(Folder folder, params string[] folders) //Copy constructor
            {
                this.name = folder.name;
                if (folder.subfolder != null) { this.subfolder = new Folder(folder.subfolder); }
                if (folders.Length != 0)
                {
                    foreach (string n in folders)
                    {
                        this.Last = new Folder(n);
                    }
                }
            }
            #endregion

            #region Overrides
            public override string ToString()
            {
                return string.Format("{0}\\{1}", this.name, this.subfolder == null ? string.Empty : this.subfolder.ToString());
            }
            #endregion
        }

        #region Static properties
        //Temporary
        public static readonly Folder hydroTechFolder = new Folder("GameData", "HydroTech");
        public static readonly Folder autopilotSaveFolder = new Folder(hydroTechFolder, "PluginData", "rcsautopilot", "autopilots");
        public static readonly Folder panelSaveFolder = new Folder(hydroTechFolder, "PluginData", "rcsautopilot", "panels");
        #endregion

        #region Properties
        public string WWWForm
        {
            get { return "file://" + ToString().Replace('\\', '/'); }
        }
        #endregion

        #region Fields
        private readonly string ext, name;
        private readonly Folder folder;
        #endregion

        #region Constructors
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
        #endregion

        #region Overrides
        public override string ToString()
        {
            return string.Format("{0}\\{1}{2}.{3}", KSPUtil.ApplicationRootPath, this.folder == null ? string.Empty : this.folder.ToString(), this.name, this.ext);
        }
        #endregion
    }
}