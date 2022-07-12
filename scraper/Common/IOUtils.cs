//v1
//started versioning 25-jun-2022 scraper project
//doesn't tuse ookii package anymore
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mi.Common
{
    public static class IOUtils
    {



       
        public static void PromptPickingDirectory(string title = "Chose drectory")
        {
           /* var saveDialog = new 
            {
                DefaultExt = ext,
                Title = title,
                Filter = filter,
                CheckPathExists = true,
                OverwritePrompt = true,
                RestoreDirectory = true
            };

            bool? notcanceled = saveDialog.ShowDialog();

            if (notcanceled.HasValue == false || notcanceled.Value == false) return "";
            if (string.IsNullOrWhiteSpace(saveDialog.FileName) == false)
            {
                return saveDialog.FileName;
            }
            else return "";*/
        }



        /// <summary>
        /// returns the selection path, or "" on any cancelation or failure
        /// </summary>
        /// <param name="ext">the default extension with .</param>
        /// <returns></returns>
        public static string PromptSavingPath(string ext, string title = "Save file", string filter = "Icon Files|*.ico|All Files|*")
        {
            var saveDialog = new SaveFileDialog
            {
                DefaultExt = ext,
                Title = title,
                Filter = filter,
                CheckPathExists = true,
                OverwritePrompt = true,
                RestoreDirectory = true
            };
            
            bool? notcanceled= saveDialog.ShowDialog();

            if (notcanceled.HasValue == false || notcanceled.Value == false) return "";
            if (string.IsNullOrWhiteSpace(saveDialog.FileName) == false)
            {
                return saveDialog.FileName;
            }
            else return "";
        }

        /// <summary>
        /// if not canceled, performs the ction delegateagainst the selected path
        /// NOTE: the boolean param is true if the selection was canceled, 
        /// NOTE: runs async
        /// </summary>
        /// <param name="ext">the default extension</param>
        /// <returns></returns>
        public static void PromptSavingPathAsync(string ext, Action<string,bool>CallbackAction, string title = "Save file", string filter = "Icon Files|*.ico|All Files|*")
        {
            Task.Run(() =>
            {

                var saveDialog = new SaveFileDialog
                {
                    DefaultExt = ext,
                    Title = title,
                    Filter = filter,
                    CheckPathExists = true,
                    OverwritePrompt = true,
                    RestoreDirectory = true
                };
                bool? notcanceled = saveDialog.ShowDialog();

                if (notcanceled.HasValue == false || notcanceled.Value == false) {
                    CallbackAction(saveDialog.FileName, true);
                    return;
                }
                if (string.IsNullOrWhiteSpace(saveDialog.FileName) == false)
                {
                    CallbackAction(saveDialog.FileName, false);
                    return;
                }

                else {
                    CallbackAction("", true);
                    return;
                } 
            });
            
        }


        /// <summary>
        /// if not canceled, performs the ction delegateagainst the selected path
        /// NOTE: the boolean param is true if the selection was canceled, 
        /// NOTE: runs async
        /// </summary>
        /// <param name="ext">the default extension</param>
        /// <returns></returns>
        public static void PromptOpeningPathAsync( Action<string, bool> CallbackAction, string ext=null, string title = "Open file", string filter = "All Files|*")
        {
            Task.Run(() =>
            {

                var openDialog = new OpenFileDialog
                {
                    DefaultExt = ext,
                    Title = title,
                    Filter = filter,
                    CheckPathExists = true,
                    RestoreDirectory = true
                };
                bool? notcanceled = openDialog.ShowDialog();

                if (notcanceled.HasValue == false || notcanceled.Value == false)
                {
                    CallbackAction(openDialog.FileName, true);
                    return;
                }
                if (string.IsNullOrWhiteSpace(openDialog.FileName) == false)
                {
                    CallbackAction(openDialog.FileName, false);
                    return;
                }

                else
                {
                    CallbackAction("", true);
                    return;
                }
            });

        }







        /// <summary>
        /// open file or execute program. silently does nothing if the file is missing
        /// </summary>
        /// <param name="absolutePath"></param>
        internal static void TryInvokeItemAsync(string absolutePath)
        {
            Task.Run(() => {
                if(File.Exists(absolutePath))
                Process.Start(absolutePath);
            });
        }
    }
}
