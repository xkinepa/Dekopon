using System.Threading;

namespace Dekopon.Transaction
{
    public interface IInstanceHolder<T>
    {
        T Value { get; set; }
    }

    public class SimpleInstanceHolder<T> : IInstanceHolder<T>
    {
        public T Value { get; set; }
    }

    public class ThreadLocalInstanceHolder<T> : IInstanceHolder<T>
    {
        private readonly ThreadLocal<T> _container = new ThreadLocal<T>();

        public T Value
        {
            get => _container.Value;
            set => _container.Value = value;
        }
    }

    public class AsyncLocalInstanceHolder<T> : IInstanceHolder<T>
    {
        private readonly AsyncLocal<T> _container;

        public AsyncLocalInstanceHolder()
            : this(new AsyncLocal<T>())
        {
        }

        public AsyncLocalInstanceHolder(AsyncLocal<T> container)
        {
            _container = container;
        }

        public T Value
        {
            get => _container.Value;
            set => _container.Value = value;
        }
    }
}