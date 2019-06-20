using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Unity.CodeEditor;

namespace RiderEditor
{
    [InitializeOnLoad]
    public class RiderScriptEditor : IExternalCodeEditor
    {
        IDiscovery m_Discoverability;
        IGenerator m_ProjectGeneration;

        static RiderScriptEditor()
        {
            var projectGeneration = new ProjectGeneration();
            var editor = new RiderScriptEditor(new Discovery(), projectGeneration);
            CodeEditor.Register(editor);
            if (IsRiderInstallation(CodeEditor.CurrentEditorInstallation))
            {
                editor.CreateIfDoesntExist();
            }
        }
        static bool IsOSX => Environment.OSVersion.Platform == PlatformID.Unix;

        public RiderScriptEditor(IDiscovery discovery, IGenerator projectGeneration)
        {
            m_Discoverability = discovery;
            m_ProjectGeneration = projectGeneration;
        }

        public void OnGUI()
        {
        }

        public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
        {
            m_ProjectGeneration.SyncIfNeeded(addedFiles.Union(deletedFiles).Union(movedFiles).Union(movedFromFiles), importedFiles);
        }

        public void SyncAll()
        {
            m_ProjectGeneration.Sync();
        }

        public void Initialize(string editorInstallationPath)
        {
        }

        public bool OpenProject(string path, int line, int column)
        {
            if (IsOSX)
            {
                return OpenOSXApp(path, line, column);
            }
            var solution = GetSolutionFile(path); // TODO: If solution file doesn't exist resync.
            solution = solution == "" ? "" : $"\"{solution}\"";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = EditorPrefs.GetString("kScriptsDefaultApp"),
                    Arguments = $"{solution} -l {line} \"{path}\"",
                    UseShellExecute = true,
                }
            };

            process.Start();

            return true;
        }

        private bool OpenOSXApp(string path, int line, int column)
        {
            var solution = GetSolutionFile(path); // TODO: If solution file doesn't exist resync.
            solution = solution == "" ? "" : $"\"{solution}\"";
            var pathArguments = path == "" ? "" : $"-l {line} \"{path}\"";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "open",
                    Arguments = $"\"{EditorPrefs.GetString("kScriptsDefaultApp")}\" --args {solution} {pathArguments}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            };

            process.Start();

            while (!process.StandardOutput.EndOfStream)
            {
                UnityEngine.Debug.Log(process.StandardOutput.ReadLine());
            }
            var errorOutput = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(errorOutput))
            {
                UnityEngine.Debug.Log("Error: \n" + errorOutput);
            }

            return true;
        }

        private string GetSolutionFile(string path)
        {
            if (UnityEditor.Unsupported.IsDeveloperBuild())
            {
                var baseFolder = GetBaseUnityDeveloperFolder();
                var lowerPath = path.ToLowerInvariant();
                var isUnitySourceCode = false;

                if (lowerPath.Contains((baseFolder + "/Runtime").ToLowerInvariant()))
                {
                    isUnitySourceCode = true;
                }
                if (lowerPath.Contains((baseFolder + "/Editor").ToLowerInvariant()))
                {
                    isUnitySourceCode = true;
                }

                if (isUnitySourceCode)
                {
                    return Path.Combine(baseFolder, "Projects/CSharp/Unity.CSharpProjects.gen.sln");
                }
            }
            var solutionFile = m_ProjectGeneration.SolutionFile();
            if (File.Exists(solutionFile))
            {
                return solutionFile;
            }
            return "";
        }

        private string GetBaseUnityDeveloperFolder()
        {
            return Directory.GetParent(EditorApplication.applicationPath).Parent.Parent.FullName;
        }

        public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
        {
            if (IsRiderInstallation(editorPath))
            {
                try
                {
                    installation = Installations.First(inst => inst.Path == editorPath);
                }
                catch (InvalidOperationException)
                {
                    installation = new CodeEditor.Installation { Name = editorPath, Path = editorPath };
                }
                return true;
            }

            installation = default;
            return false;
        }

        private static bool IsRiderInstallation(string path) {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            var lowerCasePath = path.ToLower();
            var filename = Path.GetFileName(lowerCasePath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar)).Replace(" ", "");
            return filename.StartsWith("rider");
        }

        public CodeEditor.Installation[] Installations => m_Discoverability.PathCallback();

        public void CreateIfDoesntExist()
        {
            if (!m_ProjectGeneration.HasSolutionBeenGenerated())
            {
                m_ProjectGeneration.Sync();
            }
        }
    }
}
