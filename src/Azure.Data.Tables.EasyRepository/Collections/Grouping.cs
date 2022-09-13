using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Azure.Data.Tables.EasyRepository.Collections
{
    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        private readonly List<TElement> _elements;

        public Grouping(TKey key, IEnumerable<TElement> items)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
                
            Key = key;
            _elements = items.ToList();
        }

        public TKey Key { get; }

        public IEnumerator<TElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

    }
}