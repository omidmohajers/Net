namespace PA.Net.Core
{
    public class Box<K,V>
    {
        public Box(K k, V v)
        {
            Key = k;
            Value = v;
        }

        public K Key { get; private set; }
        public V Value { get; private set; }
    }
}
