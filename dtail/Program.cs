using System.Threading.Tasks;
using Docker.DotNet;

namespace dtail
{
    class Program
    {

        static async Task Main(string[] _)
        {
            var dockerClient = new DockerClientConfiguration()
                .CreateClient();

            var cr = new ContainerRunner(dockerClient);

            await cr.RunAsync();
        }
    }
}
