using System.IO;

namespace Liquid.Interfaces
{
    public interface ILightAttachment
    {
        string Id { get; set; }
        string Name { get; set; }
        string ContentType { get; set; }
        string MediaLink { get; set; }
        Stream MediaStream { get; set; }
        string ResourceId { get; set; }
    }
}
