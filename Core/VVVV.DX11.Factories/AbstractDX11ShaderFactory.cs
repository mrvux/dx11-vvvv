﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.CodeDom.Compiler;
using System.ComponentModel.Composition.Hosting;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Reflection;

using SlimDX.D3DCompiler;

using VVVV.Core;
using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.Factories;
using VVVV.Core.Model;
using VVVV.Core.Logging;
using VVVV.Core.Model.FX;
using VVVV.Hosting;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.IO;

using VVVV.DX11.Internals.Effects;
using VVVV.DX11.Nodes.Layers;
using VVVV.DX11.Lib.Effects;
using VVVV.DX11.Lib.RenderGraph.Listeners;

using FeralTic.DX11;


namespace VVVV.DX11.Factories
{

    [ComVisible(false)]
    public abstract class AbstractDX11ShaderFactory<T> : AbstractFileFactory<IInternalPluginHost> where T : IDX11ShaderNodeWrapper
    {

        [Import]
        protected ISolution FSolution;

        [Import]
        protected DotNetPluginFactory FDotNetFactory;

        [Import]
        protected IORegistry FIORegistry;

        [Import]
        protected ILogger Logger { get; set; }

        private readonly Dictionary<string, FXProject> FProjects;
        private readonly Dictionary<FXProject, INodeInfo> FProjectNodeInfo;

        public event PluginCreatedDelegate PluginCreated;
        public event PluginDeletedDelegate PluginDeleted;

        private DX11ShaderInclude FIncludeHandler;

        private CompositionContainer FParentContainer;

        private readonly Dictionary<IPluginBase, PluginContainer> FPluginContainers;

        protected abstract string NodeCategory { get; }
        protected abstract string NodeVersion { get; }
        protected virtual List<CompilerError> VerifyShader(string file,DX11Effect effect)
        {
            return new List<CompilerError>();
        }

        public AbstractDX11ShaderFactory(CompositionContainer parentContainer, string exts)
            : base(exts)
        {
            FProjects = new Dictionary<string, FXProject>();
            FProjectNodeInfo = new Dictionary<FXProject, INodeInfo>();
            FIncludeHandler = new DX11ShaderInclude();
            FParentContainer = parentContainer;
            FPluginContainers = new Dictionary<IPluginBase, PluginContainer>();
        }

        //create a node info from a filename
        protected override IEnumerable<INodeInfo> LoadNodeInfos(string filename)
        {
            var project = CreateProject(filename);
            yield return LoadNodeInfoFromEffect(filename, project);
        }

        protected override void DoAddFile(string filename)
        {
            CreateProject(filename);
            base.DoAddFile(filename);
        }

        protected override void DoRemoveFile(string filename)
        {
            FXProject project;
            if (FProjects.TryGetValue(filename, out project))
            {
                if (FSolution.Projects.CanRemove(project))
                {
                    FSolution.Projects.Remove(project);
                    project.DoCompileEvent -= project_DoCompileEvent;
                }
                FProjects.Remove(filename);
            }

            base.DoRemoveFile(filename);
        }

        private FXProject CreateProject(string filename)
        {
            FXProject project;
            if (!FProjects.TryGetValue(filename, out project))
            {
            	var isDX11 = false;
            	
                //check if this is a dx11 effect in that it does contain either "technique10 " or "technique11 "
                using (var sr = new StreamReader(filename))
                {
                	var code = sr.ReadToEnd();
                	//remove comments: between (* and *)
                	code = Regex.Replace(code, @"/\*.*?\*/", "", RegexOptions.Singleline);
                	//remove comments: from // to lineend
                	code = Regex.Replace(code, @"//.*?\n", "", RegexOptions.Singleline);
                    
                    //if the code contains now contains "technique10 " or "technique11 " this must be a dx11 effect
					if (code.Contains("technique10 ") || code.Contains("technique11 "))
                		isDX11 = true;
            	}
            	
            	if (isDX11)
	            {
	                project = new FXProject(filename, FHDEHost.ExePath);
	                if (FSolution.Projects.CanAdd(project))
	                {
	                    FSolution.Projects.Add(project);
	                }
	                else
	                {
	                    // Project was renamed
	                    project = FSolution.Projects[project.Name] as FXProject;
	                }
	
	                project.DoCompileEvent += project_DoCompileEvent;
	
	                FProjects[filename] = project;
            	}
            }

            return project;
        }

        void project_DoCompileEvent(object sender, EventArgs e)
        {
            var project = sender as FXProject;
            var filename = project.LocalPath;

            LoadNodeInfoFromEffect(filename, project);
        }

        private INodeInfo LoadNodeInfoFromEffect(string filename, FXProject project)
        {
            var nodeInfo = FNodeInfoFactory.CreateNodeInfo(
                Path.GetFileNameWithoutExtension(filename),
                this.NodeCategory,this.NodeVersion,
                filename,
                true);

            nodeInfo.BeginUpdate();
            nodeInfo.Type = NodeType.Dynamic;
            nodeInfo.Factory = this;
            nodeInfo.UserData = project;

            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(filename))
                {
                    string line;
                    string author = @"//@author:";
                    string desc = @"//@help:";
                    string tags = @"//@tags:";
                    string credits = @"//@credits:";

                    // Parse lines from the file until the end of
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith(author))
                            nodeInfo.Author = line.Replace(author, "").Trim();

                        else if (line.StartsWith(desc))
                            nodeInfo.Help = line.Replace(desc, "").Trim();

                        else if (line.StartsWith(tags))
                            nodeInfo.Tags = line.Replace(tags, "").Trim();

                        else if (line.StartsWith(credits))
                            nodeInfo.Credits = line.Replace(credits, "").Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, "Effect does not contain detailed info");
                Logger.Log(ex);
            }

            try
            {
                nodeInfo.CommitUpdate();
            }
            catch { }

            return nodeInfo;
        }

        private void ParseErrors(string e,FXProject project, DX11Effect shader)
        {
            var compilerResults = new CompilerResults(null);
            //now parse errors to CompilerResults
            //split errorstring linewise
            var errorlines = e.Split(new char[1] { '\n' });
            foreach (var line in errorlines)
            {
                string filePath = project.LocalPath;
                string eCoords = string.Empty;
                int eLine = 0;
                int eChar = 0;
                string eNumber = string.Empty;
                string eText = string.Empty;
                bool isWarning = false;

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                //split the line at ": "
                //which results in 3 or 4 lines:
                //[0] filename (line, character)
                //[1] error/warning code
                //[2] (optional) erroneous character
                //[3] error description
                var eItems = line.Split(new string[1] { ": " }, StringSplitOptions.None);

                //extract line/char substring
                int start = eItems[0].LastIndexOf('(');
                int end = eItems[0].LastIndexOf(')');

                if (start > 0)
                {
                    string relativePath = eItems[0].Substring(0, start);

                    //if this is a path to an include..
                    if (relativePath != Path.Combine(FHDEHost.ExePath, "memory"))
                    {
                        // we need to guess here. shader compiler outputs relative paths.
                        // we don't know if the include was "local" or <global>

                        filePath = Path.Combine(Path.GetDirectoryName(project.LocalPath), relativePath);
                        if (!File.Exists(filePath))
                        {
                            string fileName = Path.GetFileName(relativePath);

                            foreach (var reference in project.References)
                            {
                                var referenceFileName = Path.GetFileName((reference as FXReference).ReferencedDocument.LocalPath);
                                if (referenceFileName.ToLower() == fileName.ToLower())
                                {
                                    filePath = reference.AssemblyLocation;
                                }
                            }
                        }
                    }
                }

                if (start > -1 && end > 0)
                {
                    eCoords = eItems[0].Substring(start + 1, end - start - 1);
                    var eLineChar = eCoords.Split(new char[1] { ',' });
                    eLine = Convert.ToInt32(eLineChar[0]);
                    eChar = Convert.ToInt32(eLineChar[1]);

                    if (eItems[1].StartsWith("warning"))
                    {
                        isWarning = true;
                        eNumber = eItems[1].Substring(8, 5);
                    }
                    else
                        eNumber = eItems[1].Substring(6, 5);

                    if (eItems.Length == 2)
                    {
                        isWarning = false;
                        eNumber = "-1";
                        eText = eItems[1];
                    }
                    else
                    {
                        eText = eItems[2];
                        if (eItems.Length > 3)
                            eText += ": " + eItems[3];
                    }
                }
                else
                {
                    eText = line;
                }

                var error = new CompilerError(filePath, eLine, eChar, eNumber, eText);
                error.IsWarning = isWarning;
                compilerResults.Errors.Add(error);
            }
            
            //Add some extra error from reflection
            if (shader.IsCompiled)
            {
                compilerResults.Errors.AddRange(this.VerifyShader(project.LocalPath,shader).ToArray());
            }

            project.CompilerResults = compilerResults;
        }

        protected override bool CreateNode(INodeInfo nodeInfo, IInternalPluginHost pluginHost)
        {
            if (nodeInfo.Type != NodeType.Dynamic)
                return false;

            var project = nodeInfo.UserData as FXProject;
            /*if (!project.IsLoaded)
                project.Load();*/

            //compile shader
            FIncludeHandler.ParentPath = Path.GetDirectoryName(nodeInfo.Filename);
            string code = File.ReadAllText(nodeInfo.Filename);

            DX11Effect shader;

            //create or update plugin
            if (pluginHost.Plugin == null)
            {
                

                nodeInfo.AutoEvaluate = false;
                nodeInfo.Arguments = typeof(T).ToString();

                var pluginContainer = new PluginContainer(pluginHost, FIORegistry, FParentContainer, FNodeInfoFactory, FDotNetFactory, typeof(T), nodeInfo);
                pluginHost.Plugin = pluginContainer;

                FPluginContainers[pluginContainer.PluginBase] = pluginContainer;

                IDX11ShaderNodeWrapper shadernode = pluginContainer.PluginBase as IDX11ShaderNodeWrapper;
                shadernode.Source = nodeInfo;
                shadernode.WantRecompile += new EventHandler(shadernode_WantRecompile);

                shader = DX11Effect.FromString(code, FIncludeHandler,shadernode.Macros);

                shadernode.SetShader(shader, true);

                if (this.PluginCreated != null)
                {
                    this.PluginCreated(pluginContainer, pluginHost);
                }
            }
            else
            {
                PluginContainer container = pluginHost.Plugin as PluginContainer;
                var shaderNode = container.PluginBase as IDX11ShaderNodeWrapper;
                shader = DX11Effect.FromString(code, FIncludeHandler, shaderNode.Macros);
                shaderNode.SetShader(shader, false);
            }

            //now the effect is compiled in vvvv and we can access the errors
            string e = shader.ErrorMessage;//effectHost.GetErrors();
            if (string.IsNullOrEmpty(e))
                e = "";

            this.ParseErrors(e, project,shader);



            //and the input pins
            string f = "";// effectHost.GetParameterDescription();
            if (string.IsNullOrEmpty(f))
                f = "";
            project.ParameterDescription = f;

            return true;
        }

        void shadernode_WantRecompile(object sender, EventArgs e)
        {
            IDX11ShaderNodeWrapper wrp = (IDX11ShaderNodeWrapper)sender;
            FIncludeHandler.ParentPath = Path.GetDirectoryName(wrp.Source.Filename);
            string code = File.ReadAllText(wrp.Source.Filename);

            var shader = DX11Effect.FromString(code, FIncludeHandler,wrp.Macros);
            wrp.SetShader(shader, false);
        }

        protected override bool DeleteNode(INodeInfo nodeInfo, IInternalPluginHost host)
        {
            var plugin = host.Plugin;

            var disposablePlugin = plugin as IDisposable;
            if (FPluginContainers.ContainsKey(plugin))
            {
                FPluginContainers[plugin].Dispose();
                FPluginContainers.Remove(plugin);
            }

            if (this.PluginDeleted != null)
            {
                this.PluginDeleted(host.Plugin);
            }
            return true;
        }

        protected override bool CloneNode(INodeInfo nodeInfo, string path, string name, string category, string version, out string filename)
        {
            if (nodeInfo.Type == NodeType.Dynamic)
            {
                var project = nodeInfo.UserData as FXProject;
                /*if (!project.IsLoaded)
                    project.Load();*/

                var projectDir = path;
                var newProjectName = name + this.FileExtension[0];
                var newLocation = new Uri(projectDir.ConcatPath(newProjectName));

                filename = projectDir.ConcatPath(newProjectName);

                project.SaveTo(filename);

                return true;
            }

            return base.CloneNode(nodeInfo, path, name, category, version, out filename);
        }

    }
}
