namespace Microarea.Snap.Core
{
    public interface IInversionOfControlFactory
    {
        T GetInstance<T, TOfParameter>(TOfParameter parameter);
        T GetInstance<T>();
    }
}