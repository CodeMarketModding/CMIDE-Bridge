using System;
using System.Linq;
using JavaMarket.App;
using JavaMarket.UI;

namespace CodeMarket.LiveIDE
{
    internal static class FileOperations
    {
        public static void FillProject(Response response, CodeProject project)
        {
            response.files = project.Files.Select(f => f.Path).ToArray();
            response.folders = project.Folders.Select(f => f.Path).ToArray();
        }

        private static string NameError(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Enter a name.";
            if (name.Length > CodeProject.MaxNameLength) return "Names can contain at most 32 characters.";
            if (name.IndexOfAny(new[] { '/', '\\', '\r', '\n', '\0' }) >= 0) return "Names cannot contain slashes or line breaks.";
            return null;
        }
        private static Response Error(Response response, string error) { response.ok = false; response.error = error; return response; }
        private static bool Changed(Request request, CodeFile file) => request.expected != file.Text;
        public static Response Create(Request request, Response response, CodeProject project)
        {
            string name = (request.name ?? "").Trim();
            string error = NameError(name);
            if (error != null) return Error(response, error);
            string folderPath = request.path ?? "";
            CodeFolder folder = folderPath.Length == 0 ? project.Root : project.Folders.FirstOrDefault(x => x.Path == folderPath);
            if (folder == null) return Error(response, "That folder no longer exists. Refresh the Explorer.");
            if (folder.Children.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))) return Error(response, "A file or folder with that name already exists.");
            CodeFile file = project.CreateFile(folder, name, "");
            response.path = file.Path;response.text = file.Text;FillProject(response, project);return response;
        }
        public static Response CreateFolder(Request request, Response response, CodeProject project)
        {
            string name = (request.name ?? "").Trim();
            string error = NameError(name);
            if (error != null) return Error(response, error);
            string folderPath = request.path ?? "";
            CodeFolder parent = folderPath.Length == 0 ? project.Root : project.Folders.FirstOrDefault(x => x.Path == folderPath);
            if (parent == null) return Error(response, "That parent folder no longer exists. Refresh the Explorer.");
            if (parent.Children.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))) return Error(response, "A file or folder with that name already exists.");
            CodeFolder folder = project.CreateFolder(parent, name);
            response.path = folder.Path;FillProject(response, project);return response;
        }
        public static Response Rename(Request request, Response response, CodeProject project, CodeFile file)
        {
            if (Changed(request,file)) return Error(response, "The script changed in the game. Reload before renaming.");
            string name = (request.name ?? "").Trim();
            string error = NameError(name);
            if (error != null) return Error(response, error);
            error = project.Rename(file, name);
            if (error != null) return Error(response, error);
            response.path = file.Path;response.text = file.Text;FillProject(response, project);return response;
        }
        public static Response Delete(Request request, Response response, CodeWorkspace workspace, CodeFile file)
        {
            if (Changed(request,file)) return Error(response, "The script changed in the game. Reload before deleting.");
            workspace.Delete(file);
            FillProject(response, workspace.Project);return response;
        }
    }
}
