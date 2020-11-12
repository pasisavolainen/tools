﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Docker.DotNet;
using dtail.Config;
using dtail.Extensions;
using Terminal.Gui;

namespace dtail
{
    internal class App : Toplevel
    {
        public const string DTAIL_DOT_PATH = ".dtailrc.json";
        ContainerLogsView ContainerLogsView { get; } = new ContainerLogsView();
        ContainerRunner ContainerRunner { get; set; }
        LogView LogViewWindow { get; set; }

        public App()
        {
            var logview = new LogView(ContainerLogsView)
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            var statusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.F1, "~F1~ Help", () => Help()),
                new StatusItem(Key.F2, "~F2~ Rename", () => RenameContainer()),
                new StatusItem(Key.F3, "~F3~ Containers", () => SelectContainers()),
                new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", OnQuit),
            }); ;

            Add(logview, statusBar);
            logview.EnsureFocus();
            LogViewWindow = logview;

            Task.Run(InitDocker);
        }

        private void OnQuit()
        {
            var dt = ContainerRunner.CollectConfig();
            var serialized = JsonSerializer.Serialize(dt, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(GetDotTailConfigPath(), serialized);
            Application.RequestStop();
        }

        public async Task InitDocker()
        {
            string fp = GetDotTailConfigPath();
            DTailConfig dt = File.Exists(fp)
                ? JsonSerializer.Deserialize<DTailConfig>(File.ReadAllBytes(fp))
                : new DTailConfig();

            var dockerClient = new DockerClientConfiguration()
                .CreateClient();

            var cr = new ContainerRunner(dockerClient, ContainerLogsView, dt);
            ContainerRunner = cr;

            await cr.RunAsync();
        }

        private static string GetDotTailConfigPath()
            =>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                DTAIL_DOT_PATH);

        static void Help()
        {
            MessageBox.Query(50, 7, "Help", "This is a small help\nBe kind.", "Ok");
        }

        private void SelectContainers()
        {
            var containerSource = ContainersAsSource();
            void OnOkClicked()
            {
                foreach (var (item, index, marked) in containerSource.GetItemMarkings())
                {
                    item.IsVisible = marked;
                }
                ContainerLogsView.Refresh();
                Application.RequestStop();
            }
            void OnCancelClicked() => Application.RequestStop();

            var dialog = new Dialog("Select visible containers",
                    new Button("Ok").OnClick(OnOkClicked),
                    new Button("Cancel").OnClick(OnCancelClicked));

            var lview = new ListView {
                // height hardcoded, bug in Gui.cs
                X = 1, Y = 1, Height = 15, Width = Dim.Fill(),
                AllowsMarking = true, AllowsMultipleSelection = true,
                Source = containerSource,
            };

            dialog.Add(lview);

            Application.Run(dialog);
        }
        private void RenameContainer()
        {
            var container = GetCurrentContainer();
            var label = new Label("Short name:") { X = 0, Y = 0, Width = 12, Height = 1 };
            var text = new TextField(container?.ShortName) { X = Pos.Right(label) + 1, Y = Pos.Top(label), Width = 30, Height = 1 };

            var dialog = new Dialog("Rename container",
                new Button("Rename").OnClick(OnRename),
                new Button("Cancel").OnClick(Application.RequestStop))
            {
                X = Pos.Center(), Y = 1,
                Width = 40, Height = 20,
            };

            var namesView = new ListView
            {
                X = 0, Y = Pos.Bottom(label), Width = Dim.Fill(), Height = Dim.Fill(3),
                AllowsMarking = true, AllowsMultipleSelection = true,
                Source = container.Aliases.AsSource(),
            };

            dialog.Add(label, text, namesView);

            Application.Run(dialog);

            void OnRename()
            {
                var result = ContainerLogsView.RenameContainer(container, text.Text.ToString());
                if (result)
                    Application.RequestStop();
                else
                    MessageBox.ErrorQuery(50, 7, "Rename", "Couldn't rename container, the short name is probably taken", "Ok");
            };
        }

        private RunningContainerInfo GetCurrentContainer()
        {
            var cid = ContainerLogsView.LogLines.Last.Value.ContainerId;
            return ContainerLogsView.Containers[cid];
        }

        private ListDataSource<RunningContainerInfo> ContainersAsSource()
        {
            var r = new ListDataSource<RunningContainerInfo>(ContainerLogsView.Containers.Values, c => c.IsVisible)
            {
                FormatItem = (index, item) => item.ShortName,
            };
            return r;
        }
    }
}