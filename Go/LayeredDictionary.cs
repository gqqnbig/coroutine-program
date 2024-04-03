using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Go
{
	class LayeredDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private List<Dictionary<TKey, TValue>> stack = new List<Dictionary<TKey, TValue>>();

		/// <summary>
		/// Get the number of layers in this LayeredDictionary.
		/// </summary>
		public int Layers
		{
			get
			{
				return stack.Count;
			}
		}

		public void AddLayer()
		{
			stack.Add(new Dictionary<TKey, TValue>());
		}

		public void RemoveLayer()
		{
			stack.RemoveAt(Layers - 1);
		}


		public TValue this[TKey key]
		{
			get
			{
				if (TryGetValue(key, out TValue v))
					return v;
				else
					throw new KeyNotFoundException();
			}

			set
			{
				stack[Layers - 1][key] = value;
			}
		}

		public ICollection<TKey> Keys => throw new NotImplementedException();

		public ICollection<TValue> Values => throw new NotImplementedException();

		public int Count => throw new NotImplementedException();

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public void Add(TKey key, TValue value)
		{
			stack[Layers - 1].Add(key, value);
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		public bool ContainsKey(TKey key)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public bool Remove(TKey key)
		{
			throw new NotImplementedException();
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
		{
			for (int i = Layers - 1; i >= 0; i--)
			{
				if (stack[i].TryGetValue(key, out value))
					return true;
			}
			value = default(TValue);
			return false;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}
