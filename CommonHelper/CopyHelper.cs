using System.Reflection;

namespace CommonHelper;

public sealed class CopyHelper
{
    private static readonly Type array_type = typeof(Array);

    private static readonly MethodInfo memberwise_clone = typeof(object)
        .GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

    private static void MakeArrayRowDeepCopy(Dictionary<object, object> state,
        Array array, int[] indices, int rank)
    {
        var next_rank = rank + 1;
        var upper_bound = array.GetUpperBound(rank);

        while (indices[rank] <= upper_bound)
        {
            var value = array.GetValue(indices);
            if (!ReferenceEquals(value, null))
                array.SetValue(CreateDeepCopyInternal(state, value), indices);

            if (next_rank < array.Rank)
                MakeArrayRowDeepCopy(state, array, indices, next_rank);

            indices[rank] += 1;
        }

        indices[rank] = array.GetLowerBound(rank);
    }

    private static Array CreateArrayDeepCopy(Dictionary<object, object> state, Array array)
    {
        var result = (Array)array.Clone();
        var indices = new int[result.Rank];
        for (var rank = 0; rank < indices.Length; ++rank)
            indices[rank] = result.GetLowerBound(rank);
        MakeArrayRowDeepCopy(state, result, indices, 0);
        return result;
    }

    private static object CreateDeepCopyInternal(Dictionary<object, object> state,
        object o)
    {
        if (state.TryGetValue(o, out var exist_object))
            return exist_object;

        if (o is Array array)
        {
            object array_copy = CreateArrayDeepCopy(state, array);
            state[array] = array_copy;
            return array_copy;
        }

        if (o is string s)
        {
            object string_copy = new string(s.ToCharArray());
            state[s] = string_copy;
            return string_copy;
        }

        var o_type = o.GetType();
        if (o_type.IsPrimitive)
            return o;
        var copy = memberwise_clone.Invoke(o, null);
        state[o] = copy ?? string.Empty;
        foreach (var f in o_type.GetFields(
                     BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            var original = f.GetValue(o);
            if (!ReferenceEquals(original, null))
                f.SetValue(copy, CreateDeepCopyInternal(state, original));
        }

        return copy ?? string.Empty;
    }

    public static T CreateDeepCopy<T>(T o)
    {
        object input = o;
        if (ReferenceEquals(o, null))
            return o;
        return (T)CreateDeepCopyInternal(new Dictionary<object, object>(), input);
    }
}
