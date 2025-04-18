﻿using CBRE.Common.Mediator;
using CBRE.DataStructures.Geometric;
using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Brushes;
using CBRE.Editor.Documents;
using CBRE.Editor.Menu;
using CBRE.Editor.Rendering;
using CBRE.Editor.Settings;
using CBRE.Editor.Tools;
using CBRE.Editor.UI;
using CBRE.Editor.UI.Sidebar;
using CBRE.Graphics.Helpers;
using CBRE.Providers;
using CBRE.Providers.Map;
using CBRE.Providers.Model;
using CBRE.Providers.Texture;
using CBRE.QuickForms;
using CBRE.Settings;
using CBRE.Settings.Models;
using CBRE.UI;
using Microsoft.WindowsAPICodePack.Taskbar;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using LayoutSettings = CBRE.Editor.UI.Layout.LayoutSettings;

namespace CBRE.Editor
{
	public partial class Editor : HotkeyForm, IMediatorListener
	{
		private JumpList _jumpList;
		private DiscordManager _DiscordManager;
		
		public static Editor Instance { get; private set; }

		private const string API_RELEASES_URL = "https://api.github.com/repos/AnalogFeelings/cbre-ex/releases/latest";
		private const string GIT_LATEST_RELEASE_URL = "https://github.com/AnalogFeelings/cbre-ex/releases/latest";

		public const string GITHUB_REPORT_BUG_URL = "https://github.com/AnalogFeelings/cbre-ex/issues/new?assignees=AnalogFeelings&labels=bug&template=bug_report.md&title=";

		public bool CaptureAltPresses { get; set; }
		public bool ShowEntityErrorForm = true;

		public Editor()
		{
			PreventSimpleHotkeyPassthrough = false;
			InitializeComponent();
			Instance = this;
		}

		public void SelectTool(BaseTool t)
		{
			ToolManager.Activate(t);
		}

		public static void ProcessArguments(IEnumerable<string> args)
		{
			foreach (string file in args.Skip(1).Where(File.Exists))
			{
				Mediator.Publish(EditorMediator.LoadFile, file);
			}
		}

		private static void LoadFileGame(string fileName, Game game)
		{
			try
			{
				Image[] lightmaps;
				Map map = MapProvider.GetMapFromFile(fileName, Directories.ModelDirs, out lightmaps);
				if (MapProvider.warnings != "")
				{
					MessageBox.Show(MapProvider.warnings, "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				Document doc = new Document(fileName, map, game);
				DocumentManager.AddAndSwitch(doc);
				if (lightmaps != null)
				{
					lock (doc.TextureCollection.Lightmaps)
					{
						for (int i = 0; i < lightmaps.Length; i++)
						{
							doc.TextureCollection.Lightmaps[i]?.Dispose();
							doc.TextureCollection.Lightmaps[i] = new Bitmap(lightmaps[i]);
							lightmaps[i].Dispose();
						}
						doc.TextureCollection.LightmapTextureOutdated = true;
					}
					foreach (Viewport3D viewport in ViewportManager.Viewports.Where(vp => vp is Viewport3D).Select(vp => vp as Viewport3D))
					{
						viewport.Type = Viewport3D.ViewType.Lightmapped;
						ViewportLabelListener listener = viewport.Listeners.Find(l => l is ViewportLabelListener) as ViewportLabelListener;
						listener?.Rebuild();
					}
				}
			}
			catch (ProviderException e)
			{
				Error.Warning("The map file could not be opened:\n" + e.Message);
			}
		}

		private static void LoadFile(string fileName)
		{
			LoadFileGame(fileName, SettingsManager.Game);
		}

		private void EditorLoad(object sender, EventArgs e)
		{
			FileTypeRegistration.RegisterFileTypes();

			SettingsManager.Read();

			if (TaskbarManager.IsPlatformSupported)
			{
				TaskbarManager.Instance.ApplicationId = FileTypeRegistration.ProgramId;

				_jumpList = JumpList.CreateJumpList();
				_jumpList.KnownCategoryToDisplay = JumpListKnownCategoryType.Recent;
				_jumpList.Refresh();
			}

			UpdateDocumentTabs();
			UpdateRecentFiles();

			MenuManager.Init(mnuMain, tscToolStrip);
			MenuManager.Rebuild();

			BrushManager.Init();
			SidebarManager.Init(RightSidebar);

			ViewportManager.Init(TableSplitView);
			ToolManager.Init();

			foreach (BaseTool tool in ToolManager.Tools)
			{
				BaseTool tl = tool;
				HotkeyImplementation hotkey = CBRE.Settings.Hotkeys.GetHotkeyForMessage(HotkeysMediator.SwitchTool, tool.GetHotkeyToolType());
				tspTools.Items.Add(new ToolStripButton(
					"",
					tl.GetIcon(),
					(s, ea) => Mediator.Publish(HotkeysMediator.SwitchTool, tl.GetHotkeyToolType()),
					tl.GetName())
				{
					Checked = (tl == ToolManager.ActiveTool),
					ToolTipText = tl.GetName() + (hotkey != null ? " (" + hotkey.Hotkey + ")" : ""),
					DisplayStyle = ToolStripItemDisplayStyle.Image,
					ImageScaling = ToolStripItemImageScaling.None,
					AutoSize = false,
					Width = 36,
					Height = 36
				});
			}

			TextureProvider.SetCachePath(SettingsManager.GetTextureCachePath());
			MapProvider.Register(new CBRProvider());
			MapProvider.Register(new VmfProvider());
			MapProvider.Register(new L3DWProvider());
			MapProvider.Register(new MSL2Provider());
			TextureProvider.Register(new MiscTexProvider());
			ModelProvider.Register(new AssimpProvider());

			TextureHelper.EnableTransparency = !CBRE.Settings.View.GloballyDisableTransparency;

			Subscribe();

			Mediator.MediatorException += (mthd, ex) => Logging.Logger.ShowException(ex.Exception, "Mediator Error: " + ex.Message);

			if (!Directories.TextureDirs.Any())
			{
				OpenSettings(4);
			}

			if (CBRE.Settings.View.LoadSession)
			{
				foreach (string session in SettingsManager.LoadSession())
				{
					LoadFileGame(session, SettingsManager.Game);
				}
			}

			ProcessArguments(System.Environment.GetCommandLineArgs());

			ViewportManager.RefreshClearColour(DocumentTabs.TabPages.Count == 0);

			if (CBRE.Settings.General.CheckUpdatesOnStartup) CheckForUpdates(true);
			
			ToggleDiscord(CBRE.Settings.General.EnableDiscordPresence);
		}

		public void ToggleDiscord(bool Enabled)
		{
			if (Enabled)
			{
				if (_DiscordManager == null) _DiscordManager = new DiscordManager();
			}
			else if (_DiscordManager != null)
			{
				_DiscordManager.Dispose();
				_DiscordManager = null;
			}
		}

		#region Updates
		private Version GetCurrentVersion()
		{
			Version info = typeof(Editor).Assembly.GetName().Version;
			return info;
		}

		private void CheckForUpdates(bool notFromMenu)
		{
			using (HttpClient Client = new HttpClient())
			{
				try
				{
					Version ParsedNewVersion;
					Version CurrentVersion = GetCurrentVersion();

					//Github wants me to set a user agent, sure!
					Client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
					Client.DefaultRequestHeaders.Add("User-Agent", "AnalogFeelings/cbre-ex");

					JsonSerializerSettings DeserializeSettings = new JsonSerializerSettings
					{
						MissingMemberHandling = MissingMemberHandling.Ignore
					};

					ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

					string ServerResponse = Task.Run(() => Client.GetStringAsync(API_RELEASES_URL)).Result;

					UpdaterResponse Response = JsonConvert.DeserializeObject<UpdaterResponse>(ServerResponse, DeserializeSettings);

					//Version is invalid? Die
					if (!Version.TryParse(Response.VersionTag, out ParsedNewVersion)) return;

					ReleaseAsset PackageAsset = Response.Assets.FirstOrDefault(x => x.Filename.EndsWith(".zip"));
					ReleaseAsset ChecksumAsset = Response.Assets.FirstOrDefault(x => x.Filename.EndsWith(".sha256"));

					if (ParsedNewVersion > CurrentVersion)
					{
						//Missing required files? The update must have changed the structure!
						if (PackageAsset == default(ReleaseAsset) || ChecksumAsset == default(ReleaseAsset))
						{
							DialogResult Result = MessageBox.Show("There is a new update available, but the required files are missing. This may mean that you need to update CBRE-EX manually.\n\n" +
																  "Do you want to open the latest GitHub release?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

							if (Result == DialogResult.Yes) Process.Start(GIT_LATEST_RELEASE_URL);

							return;
						}

						UpdaterForm Form = new UpdaterForm(ParsedNewVersion, Response.Description, PackageAsset, ChecksumAsset);
						Form.ShowDialog();
					}
					else
					{
						//Kinda ugly maybe?
						if (!notFromMenu)
						{
							MessageBox.Show("There are no updates available.", "Information", MessageBoxButtons.OK,
								MessageBoxIcon.Information);
						}
					}
				}
				catch (Exception)
				{
					return; //Do nothing.
				}
			}
		}
		#endregion

		private bool PromptForChanges(Document doc)
		{
			if (doc.History.TotalActionsSinceLastSave > 0)
			{
				DialogResult result = MessageBox.Show("Would you like to save your changes to " + doc.MapFileName + "?", "Changes Detected",
					MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
				if (result == DialogResult.Cancel)
				{
					return false;
				}
				if (result == DialogResult.Yes)
				{
					if (!doc.SaveFile())
					{
						return false;
					}
				}
			}
			return true;
		}

		private void EditorClosing(object sender, FormClosingEventArgs e)
		{
			foreach (Document doc in DocumentManager.Documents.ToArray())
			{
				if (!PromptForChanges(doc))
				{
					e.Cancel = true;
					return;
				}
			}
			SidebarManager.SaveLayout();
			ViewportManager.SaveLayout();
			SettingsManager.SaveSession(DocumentManager.Documents.Select(x => Tuple.Create(x.MapFile, x.Game)));
			SettingsManager.Write();
		}

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
			{
				FileType[] supported = FileTypeRegistration.GetSupportedExtensions();
				List<string> files = (drgevent.Data.GetData(DataFormats.FileDrop) as IEnumerable<string> ?? new string[0])
					.Where(x => supported.Any(f => x.EndsWith(f.Extension, StringComparison.OrdinalIgnoreCase)))
					.ToList();
				foreach (string file in files) LoadFile(file);
			}
			base.OnDragDrop(drgevent);
		}

		protected override void OnDragEnter(DragEventArgs drgevent)
		{
			if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
			{
				FileType[] supported = FileTypeRegistration.GetSupportedExtensions();
				List<string> files = (drgevent.Data.GetData(DataFormats.FileDrop) as IEnumerable<string> ?? new string[0])
					.Where(x => supported.Any(f => x.EndsWith(f.Extension, StringComparison.OrdinalIgnoreCase)))
					.ToList();
				drgevent.Effect = files.Any() ? DragDropEffects.Link : DragDropEffects.None;
			}
			base.OnDragEnter(drgevent);
		}

		#region Mediator

		private void Subscribe()
		{
			Mediator.Subscribe(HotkeysMediator.ViewportAutosize, this);
			Mediator.Subscribe(HotkeysMediator.FourViewFocusBottomLeft, this);
			Mediator.Subscribe(HotkeysMediator.FourViewFocusBottomRight, this);
			Mediator.Subscribe(HotkeysMediator.FourViewFocusTopLeft, this);
			Mediator.Subscribe(HotkeysMediator.FourViewFocusTopRight, this);
			Mediator.Subscribe(HotkeysMediator.FourViewFocusCurrent, this);

			Mediator.Subscribe(HotkeysMediator.ScreenshotViewport, this);

			Mediator.Subscribe(HotkeysMediator.FileNew, this);
			Mediator.Subscribe(HotkeysMediator.FileOpen, this);

			Mediator.Subscribe(HotkeysMediator.PreviousTab, this);
			Mediator.Subscribe(HotkeysMediator.NextTab, this);

			Mediator.Subscribe(EditorMediator.FileOpened, this);
			Mediator.Subscribe(EditorMediator.FileSaved, this);

			Mediator.Subscribe(EditorMediator.LoadFile, this);

			Mediator.Subscribe(EditorMediator.Exit, this);

			Mediator.Subscribe(EditorMediator.OpenSettings, this);
			Mediator.Subscribe(EditorMediator.SettingsChanged, this);

			Mediator.Subscribe(EditorMediator.CreateNewLayoutWindow, this);
			Mediator.Subscribe(EditorMediator.OpenLayoutSettings, this);

			Mediator.Subscribe(EditorMediator.DocumentActivated, this);
			Mediator.Subscribe(EditorMediator.DocumentSaved, this);
			Mediator.Subscribe(EditorMediator.DocumentOpened, this);
			Mediator.Subscribe(EditorMediator.DocumentClosed, this);
			Mediator.Subscribe(EditorMediator.DocumentAllClosed, this);
			Mediator.Subscribe(EditorMediator.HistoryChanged, this);

			Mediator.Subscribe(EditorMediator.CompileStarted, this);
			Mediator.Subscribe(EditorMediator.CompileFinished, this);
			Mediator.Subscribe(EditorMediator.CompileFailed, this);

			Mediator.Subscribe(EditorMediator.MouseCoordinatesChanged, this);
			Mediator.Subscribe(EditorMediator.SelectionBoxChanged, this);
			Mediator.Subscribe(EditorMediator.SelectionChanged, this);
			Mediator.Subscribe(EditorMediator.ViewZoomChanged, this);
			Mediator.Subscribe(EditorMediator.ViewFocused, this);
			Mediator.Subscribe(EditorMediator.ViewUnfocused, this);
			Mediator.Subscribe(EditorMediator.DocumentGridSpacingChanged, this);

			Mediator.Subscribe(EditorMediator.ToolSelected, this);

			Mediator.Subscribe(EditorMediator.OpenWebsite, this);
			Mediator.Subscribe(EditorMediator.CheckForUpdates, this);
			Mediator.Subscribe(EditorMediator.About, this);
		}

		private void OpenWebsite(string url)
		{
			Process.Start(url);
		}

		private void About()
		{
			using (AboutDialog ad = new AboutDialog())
			{
				ad.ShowDialog();
			}
		}

		public static void FileNew()
		{
			DocumentManager.AddAndSwitch(new Document(null, new Map(), SettingsManager.Game));
		}

		private static void FileOpen()
		{
			using (OpenFileDialog ofd = new OpenFileDialog())
			{
				string filter = String.Join("|", FileTypeRegistration.GetSupportedExtensions().Where(x => x.CanLoad)
						.Select(x => x.Description + " (*" + x.Extension + ")|*" + x.Extension));
				string[] all = FileTypeRegistration.GetSupportedExtensions().Where(x => x.CanLoad).Select(x => "*" + x.Extension).ToArray();
				ofd.Filter = "All supported formats (" + String.Join(", ", all) + ")|" + String.Join(";", all) + "|" + filter;

				if (ofd.ShowDialog() != DialogResult.OK) return;
				LoadFile(ofd.FileName);
			}
		}

		private void PreviousTab()
		{
			int count = DocumentTabs.TabCount;
			if (count <= 1) return;
			int sel = DocumentTabs.SelectedIndex;
			int prev = sel - 1;
			if (prev < 0) prev = count - 1;
			DocumentTabs.SelectedIndex = prev;
		}

		private void NextTab()
		{
			int count = DocumentTabs.TabCount;
			if (count <= 1) return;
			int sel = DocumentTabs.SelectedIndex;
			int next = sel + 1;
			if (next >= count) next = 0;
			DocumentTabs.SelectedIndex = next;
		}

		private static void OpenSettings(int selectTab = -1)
		{
			using (SettingsForm sf = new SettingsForm())
			{
				if (selectTab >= 0) { sf.SelectTab(selectTab); }
				sf.ShowDialog();
			}
		}

		private static void CreateNewLayoutWindow()
		{
			ViewportManager.CreateNewWindow();
		}

		private static void OpenLayoutSettings()
		{
			using (LayoutSettings dlg = new LayoutSettings(ViewportManager.GetWindowConfigurations()))
			{
				dlg.ShowDialog();
			}
		}

		private static void SettingsChanged()
		{
			foreach (Viewport3D vp in ViewportManager.Viewports.OfType<CBRE.UI.Viewport3D>())
			{
				vp.Camera.FOV = CBRE.Settings.View.CameraFOV;
				vp.Camera.ClipDistance = CBRE.Settings.View.BackClippingPane;
			}
			ViewportManager.RefreshClearColour(Instance.DocumentTabs.TabPages.Count == 0);
			TextureHelper.EnableTransparency = !CBRE.Settings.View.GloballyDisableTransparency;
			
			Editor.Instance.ToggleDiscord(CBRE.Settings.General.EnableDiscordPresence);
		}

		private void Exit()
		{
			Close();
		}

		private void DocumentActivated(Document doc)
		{
			// Status bar
			StatusSelectionLabel.Text = "";
			StatusCoordinatesLabel.Text = "";
			StatusBoxLabel.Text = "";
			StatusZoomLabel.Text = "";
			StatusSnapLabel.Text = "";
			StatusTextLabel.Text = "";

			SelectionChanged();
			DocumentGridSpacingChanged(doc.Map.GridSpacing);

			DocumentTabs.SelectedIndex = DocumentManager.Documents.IndexOf(doc);

			UpdateTitle();
		}

		private void UpdateDocumentTabs()
		{
			if (DocumentTabs.TabPages.Count != DocumentManager.Documents.Count)
			{
				DocumentTabs.TabPages.Clear();
				foreach (Document doc in DocumentManager.Documents)
				{
					DocumentTabs.TabPages.Add(doc.MapFileName);
				}
			}
			else
			{
				for (int i = 0; i < DocumentManager.Documents.Count; i++)
				{
					Document doc = DocumentManager.Documents[i];
					DocumentTabs.TabPages[i].Text = doc.MapFileName + (doc.History.TotalActionsSinceLastSave > 0 ? " *" : "");
				}
			}
			if (DocumentManager.CurrentDocument != null)
			{
				int si = DocumentManager.Documents.IndexOf(DocumentManager.CurrentDocument);
				if (si >= 0 && si != DocumentTabs.SelectedIndex) DocumentTabs.SelectedIndex = si;
			}
			ViewportManager.RefreshClearColour(DocumentTabs.TabPages.Count == 0);
		}

		private void DocumentTabsSelectedIndexChanged(object sender, EventArgs e)
		{
			if (_closingDocumentTab) return;
			int si = DocumentTabs.SelectedIndex;
			if (si >= 0 && si < DocumentManager.Documents.Count)
			{
				DocumentManager.SwitchTo(DocumentManager.Documents[si]);
				if (DocumentManager.Documents[si].History.TotalActionsSinceLastSave > 0)
				{
					this.Text += " *UNSAVED CHANGES*";
				}
			}
		}

		private bool _closingDocumentTab = false;

		private void DocumentTabsRequestClose(object sender, int index)
		{
			if (index < 0 || index >= DocumentManager.Documents.Count) return;

			Document doc = DocumentManager.Documents[index];
			if (!PromptForChanges(doc))
			{
				return;
			}
			_closingDocumentTab = true;
			DocumentManager.Remove(doc);
			_closingDocumentTab = false;
		}

		private void DocumentOpened(Document doc)
		{
			UpdateDocumentTabs();
		}

		private void DocumentSaved(Document doc)
		{
			FileOpened(doc.MapFile);
			UpdateDocumentTabs();
			UpdateTitle();
		}

		private static readonly Version Version = Assembly.GetEntryAssembly().GetName().Version;

		private string titleStart
			=> $"CBRE-EX v{Version.ToString(3)}";

		private void UpdateTitle()
		{
			if (DocumentManager.CurrentDocument != null)
			{
				Document doc = DocumentManager.CurrentDocument;
				Text = $"{titleStart} - {(String.IsNullOrWhiteSpace(doc.MapFile) ? "Untitled" : System.IO.Path.GetFileName(doc.MapFile))}";
			}
			else
			{
				Text = titleStart;
			}
		}

		private void DocumentClosed(Document doc)
		{
			UpdateDocumentTabs();
		}

		private void DocumentAllClosed()
		{
			StatusSelectionLabel.Text = "";
			StatusCoordinatesLabel.Text = "";
			StatusBoxLabel.Text = "";
			StatusZoomLabel.Text = "";
			StatusSnapLabel.Text = "";
			StatusTextLabel.Text = "";
			Text = titleStart;
		}

		private void MouseCoordinatesChanged(Coordinate coord)
		{
			if (DocumentManager.CurrentDocument != null)
			{
				coord = DocumentManager.CurrentDocument.Snap(coord);
			}
			StatusCoordinatesLabel.Text = coord.X.ToString("0") + " " + coord.Y.ToString("0") + " " + coord.Z.ToString("0");
		}

		private void SelectionBoxChanged(Box box)
		{
			if (box == null || box.IsEmpty()) StatusBoxLabel.Text = "";
			else StatusBoxLabel.Text = box.Width.ToString("0") + " x " + box.Length.ToString("0") + " x " + box.Height.ToString("0");
		}

		private void SelectionChanged()
		{
			StatusSelectionLabel.Text = "";
			if (DocumentManager.CurrentDocument == null) return;

			List<MapObject> sel = DocumentManager.CurrentDocument.Selection.GetSelectedParents().ToList();
			int count = sel.Count;
			if (count == 0)
			{
				StatusSelectionLabel.Text = "No Objects Selected";
			}
			else if (count == 1)
			{
				MapObject obj = sel[0];
				EntityData ed = obj.GetEntityData();
				if (ed != null)
				{
					string name = ed.GetPropertyValue("targetname");
					StatusSelectionLabel.Text = ed.Name + (String.IsNullOrWhiteSpace(name) ? "" : " - " + name);
				}
				else
				{
					StatusSelectionLabel.Text = sel[0].GetType().Name;
				}
			}
			else
			{
				StatusSelectionLabel.Text = count.ToString() + " Objects Selected";
			}
		}

		private void ViewZoomChanged(decimal zoom)
		{
			StatusZoomLabel.Text = "Zoom: " + zoom.ToString("0.00");
		}

		private void ViewFocused()
		{

		}

		private void ViewUnfocused()
		{
			StatusCoordinatesLabel.Text = "";
			StatusZoomLabel.Text = "";
		}

		private void DocumentGridSpacingChanged(decimal spacing)
		{
			StatusSnapLabel.Text = "Grid: " + spacing.ToString("0.##");
		}

		public void ToolSelected()
		{
			BaseTool at = ToolManager.ActiveTool;
			if (at == null) return;
			foreach (ToolStripButton tsb in from object item in tspTools.Items select ((ToolStripButton)item))
			{
				tsb.Checked = (tsb.Name == at.GetName());
			}
		}

		public void FileOpened(string path)
		{
			RecentFile(path);
		}

		public void FileSaved(string path)
		{
			RecentFile(path);
		}

		public void ViewportAutosize()
		{
			TableSplitView.ResetViews();
		}

		public void FourViewFocusTopLeft()
		{
			TableSplitView.FocusOn(0, 0);
		}

		public void FourViewFocusTopRight()
		{
			TableSplitView.FocusOn(0, 1);
		}

		public void FourViewFocusBottomLeft()
		{
			TableSplitView.FocusOn(1, 0);
		}

		public void FourViewFocusBottomRight()
		{
			TableSplitView.FocusOn(1, 1);
		}

		public void FourViewFocusCurrent()
		{
			if (TableSplitView.IsFocusing())
			{
				TableSplitView.Unfocus();
			}
			else
			{
				ViewportBase focused = ViewportManager.Viewports.FirstOrDefault(x => x.IsFocused);
				if (focused != null)
				{
					TableSplitView.FocusOn(focused);
				}
			}
		}

		public void ScreenshotViewport(object parameter)
		{
			ViewportBase focused = (parameter as ViewportBase) ?? ViewportManager.Viewports.FirstOrDefault(x => x.IsFocused);
			if (focused == null) return;

			Screen screen = Screen.FromControl(this);
			Rectangle area = screen.Bounds;

			using (QuickForm qf = new QuickForm("Select Screenshot Size") { UseShortcutKeys = true }
				.NumericUpDown("Width", 640, 5000, 0, area.Width)
				.NumericUpDown("Height", 480, 5000, 0, area.Height)
				.CheckBox("Copy to Clipboard", false)
				.OkCancel())
			{
				if (qf.ShowDialog() != DialogResult.OK) return;

				Image shot = ViewportManager.CreateScreenshot(focused, (int)qf.Decimal("Width"), (int)qf.Decimal("Height"));
				if (shot == null) return;

				string ext = focused is Viewport2D || (focused is Viewport3D && ((Viewport3D)focused).Type != Viewport3D.ViewType.Textured) ? ".png" : ".jpg";

				using (SaveFileDialog sfd = new SaveFileDialog())
				{
					sfd.FileName = $"{titleStart} - "
								   + (DocumentManager.CurrentDocument != null ? DocumentManager.CurrentDocument.MapFileName : "untitled")
								   + " - " + DateTime.Now.ToString("yyyy-MM-ddThh-mm-ss") + ext;
					sfd.Filter = "Image Files (*.png, *.jpg, *.bmp)|*.png;*.jpg;*.bmp";

					if (sfd.ShowDialog() == DialogResult.OK)
					{
						if (sfd.FileName.EndsWith("jpg"))
						{
							ImageCodecInfo encoder = GetJpegEncoder();
							if (encoder != null)
							{
								EncoderParameter p = new EncoderParameter(Encoder.Quality, 100L);
								EncoderParameters ep = new EncoderParameters(1);
								ep.Param[0] = p;
								shot.Save(sfd.FileName, encoder, ep);
							}
							else
							{
								shot.Save(sfd.FileName);
							}
						}
						else
						{
							shot.Save(sfd.FileName);
						}
						if (qf.Bool("Copy to Clipboard"))
						{
							System.Windows.Forms.Clipboard.SetFileDropList(new StringCollection { sfd.FileName });
						}
					}
				}
				shot.Dispose();
			}
		}

		private ImageCodecInfo GetJpegEncoder()
		{
			return ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == ImageFormat.Jpeg.Guid);
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			// Suppress presses of the alt key if required
			if (CaptureAltPresses && (keyData & Keys.Alt) == Keys.Alt)
			{
				return true;
			}

			return base.ProcessDialogKey(keyData);
		}

		public void Notify(string message, object data)
		{
			Mediator.ExecuteDefault(this, message, data);
		}

		#endregion

		private void RecentFile(string path)
		{
			if (TaskbarManager.IsPlatformSupported)
			{
				//Elevation.RegisterFileType(System.IO.Path.GetExtension(path));
				JumpList.AddToRecent(path);
				_jumpList.Refresh();
			}
			List<RecentFile> recents = SettingsManager.RecentFiles.OrderBy(x => x.Order).Where(x => x.Location != path).Take(9).ToList();
			recents.Insert(0, new RecentFile { Location = path });
			for (int i = 0; i < recents.Count; i++)
			{
				recents[i].Order = i;
			}
			SettingsManager.RecentFiles.Clear();
			SettingsManager.RecentFiles.AddRange(recents);
			SettingsManager.Write();
			UpdateRecentFiles();
		}

		private void UpdateRecentFiles()
		{
			List<RecentFile> recents = SettingsManager.RecentFiles;
			MenuManager.RecentFiles.Clear();
			MenuManager.RecentFiles.AddRange(recents);
			MenuManager.UpdateRecentFilesMenu();
		}

		private void TextureReplaceButtonClicked(object sender, EventArgs e)
		{
			Mediator.Publish(HotkeysMediator.ReplaceTextures);
		}

		private void MoveToWorldClicked(object sender, EventArgs e)
		{
			Mediator.Publish(HotkeysMediator.TieToWorld);
		}

		private void MoveToEntityClicked(object sender, EventArgs e)
		{
			Mediator.Publish(HotkeysMediator.TieToEntity);
		}

		private void EditorShown(object sender, EventArgs e)
		{

		}
	}
}
