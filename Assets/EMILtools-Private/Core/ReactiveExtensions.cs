namespace EMILtools.Core
{
    public static class ReactiveExtensions
    {
        public static T ApplyAllIntercepts<T>(this ReactiveIntercept<T> ri, T val)
            where T : struct
        {
            if (ri.Intercepts == null) return val;
            foreach (var intercept in ri.Intercepts)
                val = intercept(val);
            return val;
        }
    }
}