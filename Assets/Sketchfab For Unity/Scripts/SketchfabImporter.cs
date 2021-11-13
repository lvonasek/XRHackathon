/*
 * Copyright(c) 2017-2018 Sketchfab Inc.
 * License: https://github.com/sketchfab/UnityGLTF/blob/master/LICENSE
 */
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Ionic.Zip;
using Siccity.GLTFUtility;

/// <summary>
/// Class to handle imports from Sketchfab
/// </summary>
namespace Sketchfab
{
	class SketchfabImporter
	{
		// Settings
		string _unzipDirectory = Application.temporaryCachePath + "/unzip";
		string _currentSampleName = "Imported";
		bool _addToCurrentScene = false;
		string _gltfInput;

		public void configure(string importDirectory, string prefabName, bool addToScene = false)
		{
			if (prefabName.Length > 0)
				_currentSampleName = prefabName;

			_addToCurrentScene = addToScene;
		}

		private string findGltfFile(string directory)
		{
			string gltfFile = "";
			DirectoryInfo info = new DirectoryInfo(directory);
			foreach (FileInfo fileInfo in info.GetFiles())
			{
				if (isSupportedFile(fileInfo.FullName))
				{
					gltfFile = fileInfo.FullName;
				}
			}

			return gltfFile;
		}

		private void deleteExistingGLTF()
		{
			string gltfFile = findGltfFile(_unzipDirectory);
			if (gltfFile != "")
			{
				File.Delete(gltfFile);
			}
		}

		private string unzipGltfArchive(string zipPath)
		{
			if (!Directory.Exists(_unzipDirectory))
				Directory.CreateDirectory(_unzipDirectory);
			else
				deleteExistingGLTF();

			// Extract archive
			Debug.Log("Unzipping " + zipPath);
			ZipFile zipfile = ZipFile.Read(zipPath);

			foreach (ZipEntry e in zipfile)
			{
				e.Extract(_unzipDirectory, ExtractExistingFileAction.OverwriteSilently);
			}


			return findGltfFile(_unzipDirectory);
		}

		private string stripProjectDirectory(string directory)
		{
			return directory.Replace(Application.dataPath, "Assets");
		}

		private void DeleteDirectory(string path)
        {
			foreach (string dir in Directory.GetDirectories(path))
			{
				DeleteDirectory(dir);
			}
			foreach (string file in Directory.GetFiles(path))
			{
				File.Delete(file);
            }
			Directory.Delete(path);
        }

		public void loadFromBuffer(byte[] data)
		{
			if (Directory.Exists(_unzipDirectory))
            {
				DeleteDirectory(_unzipDirectory);
			}
			Directory.CreateDirectory(_unzipDirectory);
			string temp = _unzipDirectory + "/temp.zip";
			using (MemoryStream stream = new MemoryStream(data))
			{
				using (FileStream fs = new FileStream(temp, FileMode.CreateNew))
				{
					stream.CopyTo(fs);
					fs.Flush();
				}
			}
			string zip = unzipGltfArchive(temp);
			string gltf = findGltfFile(_unzipDirectory);
			Importer.LoadFromFile(gltf);
		}

		private bool isSupportedFile(string filepath)
		{
			string ext = Path.GetExtension(filepath);
			return (ext == ".gltf" || ext == ".glb");			
		}
	}
}
