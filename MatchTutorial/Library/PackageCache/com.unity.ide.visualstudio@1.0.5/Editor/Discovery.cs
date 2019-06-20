using System;
using System.Linq;
using Unity.CodeEditor;
using UnityEngine;

namespace VisualStudioEditor
{
    public interface IDiscovery {
        CodeEditor.Installation[] PathCallback();
    }

    public class Discovery : IDiscovery {
        internal static string VisualStudioVersionToNiceName(VisualStudioVersion version)
        {
            switch (version)
            {
                case VisualStudioVersion.Invalid: return "Invalid Version";
                case VisualStudioVersion.VisualStudio2008: return "Visual Studio 2008";
                case VisualStudioVersion.VisualStudio2010: return "Visual Studio 2010";
                case VisualStudioVersion.VisualStudio2012: return "Visual Studio 2012";
                case VisualStudioVersion.VisualStudio2013: return "Visual Studio 2013";
                case VisualStudioVersion.VisualStudio2015: return "Visual Studio 2015";
                case VisualStudioVersion.VisualStudio2017: return "Visual Studio 2017";
                case VisualStudioVersion.VisualStudio2019: return "Visual Studio 2019";
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }
        }

        public CodeEditor.Installation[] PathCallback()
        {
            try
            {
                return VSEditor.GetInstalledVisualStudios().Select(pair => new CodeEditor.Installation
                {
                    Path = pair.Value[0],
                    Name = VisualStudioVersionToNiceName(pair.Key)
                }).ToArray();
            }
            catch (Exception ex)
            {
                Debug.Log($"Error detecting Visual Studio installations: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return new CodeEditor.Installation[0];
            }
        }
    }
}