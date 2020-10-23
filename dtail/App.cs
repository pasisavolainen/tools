using System.Threading.Tasks;
using Docker.DotNet;
using Terminal.Gui;

namespace dtail
{
    internal class App : Toplevel
    {
        ContainerLogsView ContainerLogsView { get; } = new ContainerLogsView();
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
                new StatusItem(Key.ControlQ, "~^Q~ Quit", () => { Application.RequestStop(); }),
            });

            Add(logview, statusBar);
            logview.EnsureFocus();

            Task.Run(InitDocker);
        }

        public async Task InitDocker()
        {
            var dockerClient = new DockerClientConfiguration()
                .CreateClient();

            var cr = new ContainerRunner(dockerClient, ContainerLogsView);

            await cr.RunAsync();
        }

        static void Help()
        {
            MessageBox.Query(50, 7, "Help", "This is a small help\nBe kind.", "Ok");
        }
    }
}