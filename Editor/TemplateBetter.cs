//
// TemplateBetter.cs
//
// Copyright 2021 Ben Sherratt
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

public class TemplateBetter : UnityEditor.AssetModificationProcessor {
	public delegate bool SymbolTextGenerator(out string text, string symbol, string path);

	const string SCRIPT_TEMPLATE_DIRNAME = "ScriptTemplates";
	static string[] CSHARP_TEMPLATE_FILENAMES = new string[] { "Template.cs.txt", "81-C# Script-NewBehaviourScript.cs.txt" };

	static Dictionary<string, SymbolTextGenerator> textGeneratorBySymbol = new Dictionary<string, SymbolTextGenerator>();

	public static void RegisterSymbolTextGenerator(string symbol, SymbolTextGenerator textGenerator) {
		if (textGeneratorBySymbol.ContainsKey(symbol) == false) {
			textGeneratorBySymbol[symbol] = textGenerator;
		}
	}

	public static void OnWillCreateAsset(string path) {
		RegisterDefaultTextGenerators();

		string trueFilename = Path.GetFileNameWithoutExtension(path);
		string truePath = Path.Combine(Path.GetDirectoryName(path), trueFilename);
		string extension = Path.GetExtension(truePath);
		if (extension == ".cs") {
			AssetDatabase.StartAssetEditing();

			try {
				string assetsRootDir = Path.GetDirectoryName(Application.dataPath);
				string fullFilePath = Path.Combine(assetsRootDir, truePath);

				string baseTemplateFilePath = FindTemplateFile(truePath, assetsRootDir) ?? fullFilePath;

				string temporaryTemplateFilePath = Path.GetTempFileName();

				using (StreamReader templateReader = new StreamReader(baseTemplateFilePath, System.Text.Encoding.UTF8)) {
					using (StreamWriter fileWriter = new StreamWriter(temporaryTemplateFilePath)) {
						for (; ; ) {
							int dat = templateReader.Read();
							if (dat == -1) {
								break;
							}

							char chr = (char)dat;

							if (chr == '#') {
								string symbol = "";

								for (;;) {
									dat = templateReader.Read();
									if (dat == -1) {
										break;
									}
									chr = (char)dat;
									if (chr == '#') {
										break;
									} else {
										symbol += chr;
									}
								}

								SymbolTextGenerator generator;
								string output;
								if (textGeneratorBySymbol.TryGetValue(symbol, out generator) && generator(out output, symbol, truePath)) {
									fileWriter.Write(output);
								} else {
									fileWriter.Write('#');
									fileWriter.Write(symbol);
									fileWriter.Write('#');
								}
							} else {
								fileWriter.Write(chr);
							}
						}
					}
				}

				File.Delete(fullFilePath);
				File.Move(temporaryTemplateFilePath, fullFilePath);
			} catch (Exception e) {
				Debug.LogError($"Fatal error when making C# file: {e}");
			}

			AssetDatabase.StopAssetEditing();

			AssetDatabase.Refresh();
		}
	}

	static string FindTemplateFile(string originalFile, string assetsRootDir) {
		string templateSearchDirectory = Path.GetDirectoryName(originalFile);
		while (templateSearchDirectory.Length > 0) {
			foreach (string templateFilename in CSHARP_TEMPLATE_FILENAMES) {
				string candidateTemplatePath = Path.Combine(assetsRootDir, templateSearchDirectory, SCRIPT_TEMPLATE_DIRNAME, templateFilename);
				Debug.Log(candidateTemplatePath);
				if (File.Exists(candidateTemplatePath)) {
					return candidateTemplatePath;
				}
			}
			templateSearchDirectory = Path.GetDirectoryName(templateSearchDirectory);
		}
		return null;
	}

	static bool registeredDefaultGenerators;
	static void RegisterDefaultTextGenerators() {
		if (registeredDefaultGenerators == false) {
			registeredDefaultGenerators = true;

			RegisterSymbolTextGenerator("NOTRIM", GenerateUnityText);
			RegisterSymbolTextGenerator("NAME", GenerateUnityText);
			RegisterSymbolTextGenerator("SCRIPTNAME", GenerateUnityText);
			RegisterSymbolTextGenerator("SCRIPTNAME_LOWER", GenerateUnityText);

			RegisterSymbolTextGenerator("DAY", GenerateDateText);
			RegisterSymbolTextGenerator("MONTH", GenerateDateText);
			RegisterSymbolTextGenerator("YEAR", GenerateDateText);
			RegisterSymbolTextGenerator("DATE", GenerateDateText);
			RegisterSymbolTextGenerator("TIME", GenerateDateText);

			RegisterSymbolTextGenerator("PROJECTNAME", GenerateProjectText);
			RegisterSymbolTextGenerator("COMPANY", GenerateProjectText);

			RegisterSymbolTextGenerator("AUTHOR", GenerateEnvironmentalText);
		}
	}


	static bool GenerateUnityText(out string text, string symbol, string path) {
		if (symbol == "NOTRIM") {
			text = "";
			return true;
		} else if (symbol == "NAME") {
			text = Path.GetFileNameWithoutExtension(path);
			return true;
		} else if (symbol == "SCRIPTNAME") {
			text = Path.GetFileNameWithoutExtension(path).Replace(" ", "");
			return true;
		} else if (symbol == "SCRIPTNAME_LOWER") {
			string baseFileNoSpaces = Path.GetFileNameWithoutExtension(path).Replace(" ", "");

			// Cribbed from reference source
			if (char.IsUpper(baseFileNoSpaces, 0)) {
				text = $"{char.ToLower(baseFileNoSpaces[0])}{baseFileNoSpaces.Substring(1)}";
			} else {
				text = $"my{char.ToUpper(baseFileNoSpaces[0])}{baseFileNoSpaces.Substring(1)}";
			}

			return true;
		} else {
			text = null;
			return false;
		}
	}

	static bool GenerateDateText(out string text, string symbol, string path) {
		if (symbol == "DAY") {
			text = string.Format("{0:dd}", DateTime.Now);
			return true;
		} else if (symbol == "MONTH") {
			text = string.Format("{0:MM}", DateTime.Now);
			return true;
		} else if (symbol == "YEAR") {
			text = string.Format("{0:yyyy}", DateTime.Now);
			return true;
		} else if (symbol == "DATE") {
			text = string.Format("{0:d}", DateTime.Now);
			return true;
		} else if (symbol == "TIME") {
			text = string.Format("{0:t}", DateTime.Now);
			return true;
		} else {
			text = null;
			return false;
		}
	}

	static bool GenerateProjectText(out string text, string symbol, string path) {
		if (symbol == "PROJECTNAME") {
			text = PlayerSettings.productName;
			return true;
		} else if (symbol == "COMPANY") {
			text = PlayerSettings.companyName;
			return true;
		} else {
			text = null;
			return false;
		}
	}

	static bool GenerateEnvironmentalText(out string text, string symbol, string path) {
		if (symbol == "AUTHOR") {
			text = Environment.UserName;
			return true;
		} else {
			text = null;
			return false;
		}
	}
}
