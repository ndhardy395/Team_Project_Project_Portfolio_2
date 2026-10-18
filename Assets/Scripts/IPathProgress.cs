// Anything that walks the path and knows how far it still has to go.
public interface IPathProgress
{
    float DistanceRemaining { get; }
}