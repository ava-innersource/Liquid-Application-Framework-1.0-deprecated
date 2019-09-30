namespace Liquid.Domain
{
    public abstract class LightCommand<T> : LightViewModel<T> where T : LightCommand<T>, new()
    {
    }
}
