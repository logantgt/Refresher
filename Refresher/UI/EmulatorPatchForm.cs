using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.GtkSharp.Drawing;
using Refresher.Patching;
using Refresher.Verification;

namespace Refresher.UI;

public class EmulatorPatchForm : PatchForm<Patcher>
{
    private readonly FilePicker _folderField;
    private readonly DropDown _gameDropdown;
    private readonly TextBox _outputField;
    
    protected override TableLayout FormPanel { get; }
    
    public EmulatorPatchForm() : base("RPCS3 Patch")
    {
        this.FormPanel = new TableLayout(new List<TableRow>
        {
            AddField("RPCS3 dev_hdd0 folder", out this._folderField),
            AddField("Game to patch", out this._gameDropdown),
            AddField("Server URL", out this.UrlField),
            AddField("Output identifier (e.g. refresh)", out this._outputField),
        });

        this._folderField.FileAction = FileAction.SelectFolder;
        this._folderField.FilePathChanged += this.PathChanged;

        // RPCS3 builds for Windows are portable
        if (!OperatingSystem.IsWindows())
        {
            // ~/.config/rpcs3/dev_hdd0
            string folder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "rpcs3", "dev_hdd0");
            if (Directory.Exists(folder))
            {
                this._folderField.FilePath = folder;
                this.PathChanged(this, EventArgs.Empty);
            }
        }

        this.ClientSize = new Size(600, -1);
        this.InitializePatcher();
    }

    private void PathChanged(object? sender, EventArgs ev)
    {
        string path = this._folderField.FilePath;
        this._gameDropdown.Items.Clear();

        string gamesPath = Path.Join(path, "game");
        if (!Directory.Exists(gamesPath)) return;
            
        string[] games = Directory.GetDirectories(Path.Join(path, "game"));
        
        foreach (string gamePath in games)
        {
            string game = Path.GetFileName(gamePath);
            
            // Example TitleID: BCUS98208, must be 9 chars
            if(game.Length != 9) continue; // Skip over profiles/save data/other garbage

            ImageListItem item = new();
            
            string iconPath = Path.Combine(gamePath, "ICON0.PNG");
            if (File.Exists(iconPath))
            {
                item.Image = new Bitmap(iconPath).WithSize(new Size(64, 64));
            }
            string sfoPath = Path.Combine(gamePath, "PARAM.SFO");
            try
            {
                ParamSfo sfo = new(File.OpenRead(sfoPath));
                item.Text = $"{sfo.Table["TITLE"]} [{game}]";
            }
            catch
            {
                item.Text = game;
            }
            
            this._gameDropdown.Items.Add(item);
        }
    }

    private void GameChanged(object? sender, EventArgs ev)
    {
        this.Reverify(sender, ev);
    }
}