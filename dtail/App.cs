using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Docker.DotNet;
using dtail.Config;
using Terminal.Gui;

namespace dtail
{
    internal class App : Toplevel
    {
        public const string DTAIL_DOT_PATH = ".dtailrc.json";
        ContainerLogsView ContainerLogsView { get; } = new ContainerLogsView();
        ContainerRunner ContainerRunner { get; set; }

        public App()
        {
            var logview = new LogView(ContainerLogsView)
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            var statusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.F1, "~F1~ Help", () => Help()),
                //new StatusItem(Key.F2, "~F2~ Load", Load),
                //new StatusItem(Key.F3, "~F3~ Save", Save),
                new StatusItem(Key.ControlQ, "~^Q~ Quit", OnQuit),
            });

            Add(logview, statusBar);
            logview.EnsureFocus();

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
    }
}