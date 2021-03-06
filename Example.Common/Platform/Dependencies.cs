/*
 * Copyright (c) 2013-2014 Tobias Schulz
 *
 * Copying, redistribution and use of the source code in this file in source
 * and binary forms, with or without modification, are permitted provided
 * that the conditions of the MIT license are met.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Windows.Forms;

using Ionic.Zip;

namespace Platform
{
    [ExcludeFromCodeCoverageAttribute]
    public static class Dependencies
    {
        public static string DOWNLOAD_URL_SDL2 = "http://www.libsdl.org/release/SDL2-2.0.2-win32-x86.zip";
        public static string DOWNLOAD_URL_SDL2_image = "https://www.libsdl.org/projects/SDL_image/release/SDL2_image-2.0.0-win32-x86.zip";
        public static string DOWNLOAD_URL_OPENAL_SOFT = "http://kcat.strangesoft.net/openal-soft-1.15.1-bin.zip";

        private static int ExtractZip (string zipFilename)
        {
            int extractedFiles = 0;
            // read the zip file
            using (ZipFile zip = ZipFile.Read (zipFilename)) {
                // iterate over files in the zip file
                foreach (ZipEntry entry in zip) {
                    CatchExtractExceptions (() => {
                        // extract the file to the current directory
                        entry.Extract (".", ExtractExistingFileAction.OverwriteSilently);
                        // downloading was obviously sucessful
                        ++ extractedFiles;
                    });
                }
            }
            return extractedFiles;
        }

        private static bool DownloadPackage (string dll, string downloadUrl, string zipFilename)
        {
            if (File.Exists (dll)) {
                return true;
            }

            bool success = false;
            try {
                int extractedFiles = 0;
                // try to download the zip file
                if (Download (downloadUrl, zipFilename)) {
                    extractedFiles += ExtractZip (zipFilename);
                }

                // if all files were extracted
                success = extractedFiles > 0;
            }
            catch (Exception ex) {
                // an error occurred
                Log.Error (ex);
                success = false;
            }

            // remove the zip file
            try {
                File.Delete (zipFilename);
            }
            catch (Exception ex) {
                Log.Error (ex);
            }

            return success;
        }

        public static bool DownloadSDL2 ()
        {
            return DownloadPackage (dll: "SDL2.dll", downloadUrl: DOWNLOAD_URL_SDL2, zipFilename: "SDL2.zip");
        }

        public static bool DownloadSDL2_image ()
        {
            return DownloadPackage (dll: "SDL2_image.dll", downloadUrl: DOWNLOAD_URL_SDL2_image, zipFilename: "SDL2_image.zip");
        }

        public static bool DownloadOpenALSoft ()
        {
            string dllFilename = "soft_oal.dll";
            if (File.Exists (dllFilename)) {
                return true;
            }
            bool success = DownloadPackage (dll: dllFilename, downloadUrl: DOWNLOAD_URL_OPENAL_SOFT, zipFilename: "openal-soft.zip");
            if (success) {
                Action<string> findDll = (filename) => {
                    if (filename.ToLower ().Contains ("win32") && filename.ToLower ().Contains (dllFilename)) {
                        Log.Message ("Found OpenAL Soft DLL in zip file: ", filename);
                        File.Copy (filename, dllFilename, true);
                    }
                };
                string[] directories = new string[] { "." };
                string[] extensions = new string[] { "dll" };
                FileUtility.SearchFiles (directories: directories, extensions: extensions, add: findDll);
            }
            return success;
        }

        private static bool Download (string httpUrl, string saveAs)
        {
            try {
                Log.Message ("Download:");
                Log.Message ("  HTTP URL: ", httpUrl);
                Log.Message ("  Save as:  ", saveAs);

                WebClient webClient = new WebClient ();
                webClient.DownloadFile (httpUrl, saveAs);

                return true;
            }
            catch (Exception ex) {
                Log.Error (ex);
                return false;
            }
        }

        public static void CatchExtractExceptions (Action action)
        {
            try {
                action ();
            }
            catch (Exception ex) {
                Log.Error (ex);
            }
        }

        private static string MessageBoxTitle = "Dependency missing";

        private static string DownloadErrorMessage (string package)
        {
            return package + " could not be downloaded. Please contact the developers.";
        }

        public static void CatchDllExceptions (Action action)
        {
            Application.EnableVisualStyles ();

            if (SystemInfo.IsRunningOnWindows ()) {
                if (!Dependencies.DownloadSDL2 ()) {
                    Log.ShowMessageBox (DownloadErrorMessage ("SDL2"), MessageBoxTitle);
                    return;
                }
                if (!Dependencies.DownloadSDL2_image ()) {
                    Log.ShowMessageBox (DownloadErrorMessage ("SDL2_image"), MessageBoxTitle);
                    return;
                }
                if (!Dependencies.DownloadOpenALSoft ()) {
                    Log.ShowMessageBox (DownloadErrorMessage ("OpenAL Soft"), MessageBoxTitle);
                    return;
                }
            }

            try {
                action ();
            }
            catch (DllNotFoundException ex) {
                Log.Message ();
                Log.Error (ex);
                Log.Message ();
                string dllMessage = ex.ToString ().Split ('(') [0].Split ('\n') [0];
                Log.ShowMessageBox (dllMessage, MessageBoxTitle);
            }
        }
    }
}
