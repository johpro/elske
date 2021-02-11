/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ElskeLib.Utils
{
    public sealed class FastClearList<T> : IList<T> // where T : IStructuralEquatable
    {
        private T[] _storage;
        private int _counter;

        public T[] Storage => _storage;

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public int Count => _counter;
        public bool IsReadOnly => false;

        public FastClearList()
        {
            _storage = new T[4];
            _counter = 0;
        }

        public void Add(T item)
        {
            if (_counter >= _storage.Length)
            {
                SetCapacity(_storage.Length << 1);
            }
            _storage[_counter] = item;
            _counter++;
        }

        public void VirtualAdd()
        {
            if (_counter >= _storage.Length)
            {
                SetCapacity(_storage.Length << 1);
            }
            _counter++;
        }



        public void SetCapacity(int capacity)
        {
            if (capacity > _storage.Length)
            {
                var newStorage = new T[capacity];
                Array.Copy(_storage, 0, newStorage, 0, _storage.Length);
                _storage = newStorage;
            }
        }

        public void FillWithValue(T val, int count)
        {
            SetCapacity(count);
            _counter = count;
            for (int i = 0; i < count; i++)
            {
                _storage[i] = val;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _counter = 0;
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if(_counter == 0)
                return;

            Array.Copy(_storage, 0, array, arrayIndex, _counter);
        }

        public T[] ToArray()
        {
            var res = new T[_counter];
            Array.Copy(_storage, 0, res, 0, _counter);
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Last()
        {
            return _storage[_counter - 1];
        }

        /// <summary>
        /// Also clear underlying array, important for potential releasing of references
        /// saved in struct
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProperClear()
        {
            Array.Clear(_storage, 0, _storage.Length);
            _counter = 0;
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _storage[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _storage[index] = value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items is IList<T> arr)
            {
                var newCount = _counter + arr.Count;
                if (newCount >= _storage.Length)
                {
                    var newStorage = new T[Math.Max(newCount, _storage.Length << 1)];
                    Array.Copy(_storage, 0, newStorage, 0, _storage.Length);
                    _storage = newStorage;
                }

                arr.CopyTo(_storage, _counter);
                _counter = newCount;
                return;
            }

            foreach (var item in items)
            {
                Add(item);
            }
        }
    }


}
