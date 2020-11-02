using System;
using System.IO;
using System.Linq;
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
                new StatusItem(Key.F2, "~F2~ Containers", () => SelectContainers()),
                //new StatusItem(Key.F2, "~F2~ Load", Load),
                //new StatusItem(Key.F3, "~F3~ Save", Save),
                new StatusItem(Key.ControlQ, "~^Q~ Quit", OnQuit),
            });

            Add(logview, statusBar);
            logview.EnsureFocus();
            LogViewWindow = logview;

            Task.Run(InitDocker);
        }

        private void OnQuit()
        {
            var dt = ContainerRunner.CollectConfig();
            var serialized = JsonSerializer.Serialize(dt);
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
            Action OnOkClicked = () =>
            {
                foreach(var (item, index, marked) in containerSource.GetItemMarkings())
                {
                    item.IsVisible = marked;
                }
                ContainerLogsView.Refresh();
                Application.RequestStop();
            };
            Action OnCancelClicked = () => Application.RequestStop();

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

        private ListDataSource<ContainerInfo> ContainersAsSource()
        {
            var r = new ListDataSource<ContainerInfo>(ContainerRunner.ContainerLogsView.Containers.Values, c => c.IsVisible)
            {
                FormatItem = (index, item) => item.ShortName,
            };
            return r;
        }
    }
}