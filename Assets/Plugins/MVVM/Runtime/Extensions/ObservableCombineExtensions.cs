using System;
using R3;

namespace MVVM
{
    public static class ObservableCombineExtensions
    {
        public static Observable<TR> Combine<T1, T2, TR>(
        this Observable<T1> obs1, 
        Observable<T2> obs2, 
        Func<T1, T2, TR> selector)
    {
        return obs1.CombineLatest(obs2, selector);
    }

    public static Observable<TR> Combine<T1, T2, T3, TR>(
        this Observable<T1> obs1, 
        Observable<T2> obs2, 
        Observable<T3> obs3, 
        Func<T1, T2, T3, TR> selector)
    {
        return obs1.CombineLatest(obs2, obs3, selector);
    }

    public static Observable<TR> Combine<T1, T2, T3, T4, TR>(
        this Observable<T1> obs1, 
        Observable<T2> obs2, 
        Observable<T3> obs3, 
        Observable<T4> obs4, 
        Func<T1, T2, T3, T4, TR> selector)
    {
        return obs1.CombineLatest(obs2, obs3, obs4, selector);
    }

    public static Observable<TR> Combine<T1, T2, T3, T4, T5, TR>(
        this Observable<T1> obs1, 
        Observable<T2> obs2, 
        Observable<T3> obs3, 
        Observable<T4> obs4, 
        Observable<T5> obs5, 
        Func<T1, T2, T3, T4, T5, TR> selector)
    {
        return obs1.CombineLatest(obs2, obs3, obs4, obs5, selector);
    }

    public static Observable<TR> Combine<T1, T2, T3, T4, T5, T6, TR>(
        this Observable<T1> obs1, 
        Observable<T2> obs2, 
        Observable<T3> obs3, 
        Observable<T4> obs4, 
        Observable<T5> obs5, 
        Observable<T6> obs6, 
        Func<T1, T2, T3, T4, T5, T6, TR> selector)
    {
        return obs1.CombineLatest(obs2, obs3, obs4, obs5, obs6, selector);
    }

    public static Observable<TR> Combine<T1, T2, T3, T4, T5, T6, T7, TR>(
        this Observable<T1> obs1, 
        Observable<T2> obs2, 
        Observable<T3> obs3, 
        Observable<T4> obs4, 
        Observable<T5> obs5, 
        Observable<T6> obs6, 
        Observable<T7> obs7, 
        Func<T1, T2, T3, T4, T5, T6, T7, TR> selector)
    {
        return obs1.CombineLatest(obs2, obs3, obs4, obs5, obs6, obs7, selector);
    }

    public static Observable<TR> Combine<T1, T2, T3, T4, T5, T6, T7, T8, TR>(
        this Observable<T1> obs1, 
        Observable<T2> obs2, 
        Observable<T3> obs3, 
        Observable<T4> obs4, 
        Observable<T5> obs5, 
        Observable<T6> obs6, 
        Observable<T7> obs7, 
        Observable<T8> obs8, 
        Func<T1, T2, T3, T4, T5, T6, T7, T8, TR> selector)
    {
        return obs1.CombineLatest(obs2, obs3, obs4, obs5, obs6, obs7, obs8, selector);
    }

    public static Observable<TR> Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9, TR>(
        this Observable<T1> obs1, 
        Observable<T2> obs2, 
        Observable<T3> obs3, 
        Observable<T4> obs4, 
        Observable<T5> obs5, 
        Observable<T6> obs6, 
        Observable<T7> obs7, 
        Observable<T8> obs8, 
        Observable<T9> obs9, 
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TR> selector)
    {
        return obs1.CombineLatest(obs2, obs3, obs4, obs5, obs6, obs7, obs8, obs9, selector);
    }

    public static Observable<TR> Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TR>(
        this Observable<T1> obs1, 
        Observable<T2> obs2, 
        Observable<T3> obs3, 
        Observable<T4> obs4, 
        Observable<T5> obs5, 
        Observable<T6> obs6, 
        Observable<T7> obs7, 
        Observable<T8> obs8, 
        Observable<T9> obs9, 
        Observable<T10> obs10, 
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TR> selector)
    {
        return obs1.CombineLatest(obs2, obs3, obs4, obs5, obs6, obs7, obs8, obs9, obs10, selector);
    }
    }
}