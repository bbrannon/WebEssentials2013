﻿using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Threading;

namespace MadsKristensen.EditorExtensions
{
    public class Viewport : IRule
    {
        private string _file;
        private BestPractices _extension;

        public Viewport(string file, BestPractices extension)
        {
            _file = file;
            _extension = extension;
        }

        public string Message
        {
            get { return "Mobile: The 'viewport' <meta> tag is missing. Double-click to fix"; }
        }

        public string Question
        {
            get { return "Do you want to insert a viewport <meta> tag?"; }
        }

        public TaskErrorCategory Category
        {
            get { return TaskErrorCategory.Warning; }
        }

        public void Navigate(object sender, EventArgs e)
        {
            ErrorTask task = (ErrorTask)sender;

            if (MessageBox.Show(Question, "Web Essentials", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (!File.Exists(_file))
                    return;

                string html = File.ReadAllText(_file);
                int index = html.IndexOf("</head>");

                if (index > -1)
                {
                    EditorExtensionsPackage.DTE.ItemOperations.OpenFile(_file);
                    _extension._errorList.Tasks.Remove(task);

                    AddMetaTag(index);
                    return;
                }
            }
        }

        private static void AddMetaTag(int index)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                var view = ProjectHelpers.GetCurentTextView();
                var buffer = view.TextBuffer;

                EditorExtensionsPackage.DTE.UndoContext.Open("Adding <meta> viewport");

                buffer.Insert(index, "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />" + Environment.NewLine);
                view.Caret.MoveTo(new SnapshotPoint(buffer.CurrentSnapshot, index));
                EditorExtensionsPackage.DTE.ExecuteCommand("Edit.FormatSelection");

                EditorExtensionsPackage.DTE.UndoContext.Close();
                EditorExtensionsPackage.DTE.ActiveDocument.Save();

            }), DispatcherPriority.ApplicationIdle, null);
        }
    }
}