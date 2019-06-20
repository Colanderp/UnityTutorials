using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;

namespace VSCodeEditor {
    [InitializeOnLoad]
    public class VSCodeScriptEditor : IExternalCodeEditor
    {
        IDiscovery m_Discoverability;
        IGenerator m_ProjectGeneration;
        static readonly GUIContent k_ResetArguments = EditorGUIUtility.TrTextContent("Reset argument");
        string m_Arguments;

        static readonly string[] k_SupportedFileNames = { "code.exe", "visualstudiocode.app", "visualstudiocode-insiders.app", "vscode.app", "code.app", "code.cmd", "code-insiders.cmd", "code", "com.visualstudio.code" };
        
        static bool IsOSX => Environment.OSVersion.Platform == PlatformID.Unix;

        public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
        {
            var lowerCasePath = editorPath.ToLower();
            var filename = Path.GetFileName(lowerCasePath).Replace(" ", "");
            var installations = Installations;
            if (!k_SupportedFileNames.Contains(filename))
            {
                installation = default;
                return false;
            }
            if (!installations.Any())
            {
                installation = default;
                return false;
            }
            try
            {
                installation = installations.First(inst => inst.Path == editorPath);
            }
            catch (InvalidOperationException)
            {
                installation = new CodeEditor.Installation
                {
                    Name = "Visual Studio Code",
                    Path = editorPath
                };
            }

            return true;
        }

        public void OnGUI()
        {
            Arguments = EditorGUILayout.TextField("External Script Editor Args", Arguments);
            if (GUILayout.Button(k_ResetArguments, GUILayout.Width(120)))
            {
                Arguments = DefaultArgument;
            }
        }

        public void CreateIfDoesntExist()
        {
            if (!m_ProjectGeneration.HasSolutionBeenGenerated())
            {
                m_ProjectGeneration.Sync();
            }
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
            if (line == -1)
                line = 1;
            if (column == -1)
                column = 0;

            string arguments;
            if (Arguments != DefaultArgument)
            {
                arguments = m_ProjectGeneration.ProjectDirectory != path
                    ? CodeEditor.ParseArgument(Arguments, path, line, column)
                    : m_ProjectGeneration.ProjectDirectory;
            }
            else
            {
                arguments = $@"""{m_ProjectGeneration.ProjectDirectory}""";
                if (m_ProjectGeneration.ProjectDirectory != path && path.Length != 0)
                {
                    arguments += $@" -g ""{path}"":{line}:{column}";
                }
            }

            if (IsOSX)
            {
                return OpenOSX(arguments);
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = EditorPrefs.GetString("kScriptsDefaultApp"),
                    Arguments = arguments,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = true,
                }
            };

            process.Start();
            return true;
        }

        private bool OpenOSX(string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "open",
                    Arguments = $"\"{EditorPrefs.GetString("kScriptsDefaultApp")}\" --args {arguments}",
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

        string DefaultArgument { get; } = "\"$(ProjectPath)\" -g \"$(File)\":$(Line):$(Column)";
        string Arguments
        {
            get => m_Arguments ?? (m_Arguments = EditorPrefs.GetString("vscode_arguments", DefaultArgument));
            set
            {
                m_Arguments = value;
                EditorPrefs.SetString("vscode_arguments", value);
            }
        }

        public CodeEditor.Installation[] Installations => m_Discoverability.PathCallback();

        public VSCodeScriptEditor(IDiscovery discovery, IGenerator projectGeneration)
        {
            m_Discoverability = discovery;
            m_ProjectGeneration = projectGeneration;
        }

        static VSCodeScriptEditor()
        {
            CodeEditor.Register(new VSCodeScriptEditor(new VSCodeDiscovery(), new ProjectGeneration()));
        }
    }
}
