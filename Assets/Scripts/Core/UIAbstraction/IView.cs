namespace Core.UIAbstraction
{
    public interface IView
    {
        void BindObject(object vm);
    }

    public interface IView<in TVM> : IView
    {
        void BindObject(TVM vm);
    }
}