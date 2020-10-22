using System;
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

            var conts = await dockerClient.Containers.ListContainersAsync(new Docker.DotNet.Models.ContainersListParameters { All = true });

            Console.WriteLine($"Seeing {conts.Count} containers.");
        }
    }
}
