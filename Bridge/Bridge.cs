using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using BepInEx;
using JavaMarket.App;
using JavaMarket.UI;
using JavaMarket.Scripting;
using UnityEngine;

namespace CodeMarket.LiveIDE
{
    [BepInPlugin("local.codemarket.liveide", "Code Market Live IDE Bridge", "1.0.7")]
    public sealed class Bridge : BaseUnityPlugin
    {
        private string folder;
        private readonly string session = Guid.NewGuid().ToString("N");
        private float next;
        void Awake() {
            Application.runInBackground = true;
            try { HiddenFolderView.Install(); }
            catch (Exception e) { Logger.LogWarning("Could not hide underscore folders in the game explorer: " + e); }
            folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CodeMarketLiveIDE", "bridge");
            Directory.CreateDirectory(folder);
            Logger.LogInfo("Live IDE bridge active at " + folder);
        }
        void Update() {
            if (Time.realtimeSinceStartup < next) return;
            next = Time.realtimeSinceStartup + 0.1f;
            try {
                foreach (var file in Directory.GetFiles(folder, "*.request").Take(15)) Process(file);
            } catch (Exception e) { Logger.LogWarning("Bridge polling: " + e.Message); }
        }
        void Process(string file) {
            string response = Path.ChangeExtension(file, ".response");
            try {
                var req = JsonUtility.FromJson<Request>(File.ReadAllText(file));
                var result = Handle(req);
                string temp = response + ".tmp";
                File.WriteAllText(temp, JsonUtility.ToJson(result), new System.Text.UTF8Encoding(false));
                if (File.Exists(response)) File.Delete(response);
                File.Move(temp, response);
            } catch (Exception e) {
                try { File.WriteAllText(response, JsonUtility.ToJson(new Response { ok=false, error=e.Message })); } catch { }
                Logger.LogWarning("Bridge request: " + e);
            } finally { try { File.Delete(file); } catch { } }
        }
        Response Handle(Request req) {
            var app = UnityEngine.Object.FindObjectOfType<GameApp>();
            if (app == null || app.Workspace == null || app.Upgrades == null || app.Saves == null)
                return new Response { ok=false, error="Open a game world first.", session=session };
            var project = app.Workspace.Project;
            var r = new Response { ok=true, session=session, world=app.CurrentWorld, language=CodeText.Key(CodeText.Current) };
            if (req.command == "list") { FileOperations.FillProject(r, project); return r; }
            if (req.session != session || req.world != app.CurrentWorld)
                return new Response { ok=false, error="The game session or world changed. Reconnect before editing.", session=session, world=app.CurrentWorld };
            if (req.command == "referenceIndex") return ReferenceOperations.Index(r, app);
            if (req.command == "referencePage") return ReferenceOperations.Page(req, r, app);
            if (req.command == "create") return FileOperations.Create(req, r, project);
            if (req.command == "createFolder") return FileOperations.CreateFolder(req, r, project);
            var codeFile = project.Files.FirstOrDefault(f => f.Path == req.path);
            if (codeFile == null) { r.ok=false; r.error="Script no longer exists. Refresh the file list."; return r; }
            if (req.command == "rename") return FileOperations.Rename(req, r, project, codeFile);
            if (req.command == "delete") return FileOperations.Delete(req, r, app.Workspace, codeFile);
            if (req.command == "read") { r.text=codeFile.Text; return r; }
            if (req.command == "write") {
                if (codeFile.Text != req.expected) { r.ok=false; r.error="This script changed in the game or another editor. Reload before writing."; r.text=codeFile.Text; return r; }
                var window=app.Workspace.WindowOf(codeFile);
                if (window == null) {
                    var field = typeof(CodeWorkspace).GetField("windows", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var windows = field == null ? null : field.GetValue(app.Workspace) as Dictionary<CodeFile, CodeWindow>;
                    if (windows != null) windows.TryGetValue(codeFile, out window);
                }
                if (window != null) window.Code=req.text ?? "";
                project.SetText(codeFile, req.text ?? "");
                r.text=codeFile.Text; return r;
            }
            if (req.command == "complete") {
                var result=app.CodeHelp.Suggest(req.text ?? "", req.caret, req.forced);
                r.from=result == null ? req.caret : result.From;
                r.items=result == null ? new string[0] : result.Items.Select(x => JsonUtility.ToJson(new Item { label=x.Label, insert=x.Insert, detail=x.Detail, doc=x.Doc, caret=x.Caret })).ToArray();
                return r;
            }
            r.ok=false; r.error="Unknown request."; return r;
        }
    }
}
